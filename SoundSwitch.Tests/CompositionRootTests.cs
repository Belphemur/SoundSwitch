using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio.Lister;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Model;
using SoundSwitch.Services;
using SoundSwitch.UI.Component;

namespace SoundSwitch.Tests;

/// <summary>
///     Composition tests for the DI registration graph (ADR
///     docs/design/architecture/2026-11-21-appmodel-decomposition-di.md §D): the wiring
///     itself is testable for the first time. The registration graph is asserted without
///     resolving the UI-thread services; the resolvable subset proves the four
///     sub-interfaces map to their single implementations, that disposal is a single
///     container-owned path, and that the startup mute-banner reconcile is a
///     device-service step gated by initialization. The fake lister composed with the
///     real <see cref="DeviceService" /> exercises the mute-notification wiring the
///     bootstrapper installs at construction time.
/// </summary>
[TestFixture]
public class CompositionRootTests
{
    /// <summary>
    ///     A lister fake the real DeviceService can subscribe to, so the composition the
    ///     bootstrapper wires can be driven without audio infrastructure.
    /// </summary>
    private class FakeAudioDeviceLister : IAudioDeviceLister
    {
        public Subject<DefaultDevicePayload> DefaultDeviceChangedSubject { get; } = new();
        public Subject<DeviceVolumeChangedPayload> DeviceVolumeChangedSubject { get; } = new();
        public Subject<Unit> DeviceListRefreshedSubject { get; } = new();

        public bool Refreshing => false;
        public IObservable<DefaultDevicePayload> DefaultDeviceChanged => DefaultDeviceChangedSubject;
        public IObservable<DeviceVolumeChangedPayload> DeviceVolumeChanged => DeviceVolumeChangedSubject;
        public IObservable<Unit> DeviceListRefreshed => DeviceListRefreshedSubject;

        public void Refresh(CancellationToken token) => DeviceListRefreshedSubject.OnNext(Unit.Default);

        public virtual DeviceReadOnlyCollection<DeviceFullInfo> GetDevices(EDataFlow type, EDeviceState state) => new(Array.Empty<DeviceFullInfo>(), type);

        public void ProcessDeviceUpdates(IEnumerable<DeviceChangedEvent> deviceChangedEvents) { }

        public void Dispose()
        {
        }
    }

    /// <summary>
    ///     A lister whose device read fails, proving the startup reconcile propagates a
    ///     failure to its caller: the bootstrapper's never-abort-init try/catch is the guard.
    /// </summary>
    private sealed class ThrowingLister : FakeAudioDeviceLister
    {
        public override DeviceReadOnlyCollection<DeviceFullInfo> GetDevices(EDataFlow type, EDeviceState state) => throw new InvalidOperationException("list read failed");
    }

    private static ServiceDescriptor SingletonDescriptor(ServiceCollection services, Type serviceType)
    {
        services.Should().ContainSingle(descriptor => descriptor.ServiceType == serviceType,
            $"the registry must hold exactly one {serviceType.Name} registration");
        return services.Single(descriptor => descriptor.ServiceType == serviceType);
    }

    private static (DeviceService DeviceService, SettingsService SettingsService, NotificationSettingsService NotificationSettingsService) ComposeDeviceService(IAudioDeviceLister lister)
    {
        var errorEvents = new ErrorEvents();
        var settingsService = new SettingsService(errorEvents);
        var notificationSettingsService = new NotificationSettingsService();
        return (new DeviceService(lister, errorEvents, settingsService, settingsService, notificationSettingsService), settingsService, notificationSettingsService);
    }

    [Test]
    public void RegisterServices_MatchesTheExpectedRegistrationGraph()
    {
        var services = new ServiceCollection();
        CompositionRoot.RegisterServices(services);

        // Process-level singletons registered for container-owned teardown.
        SingletonDescriptor(services, typeof(AudioSwitcher)).ImplementationInstance.Should().BeSameAs(AudioSwitcher.Instance);
        SingletonDescriptor(services, typeof(MMNotificationClient)).ImplementationInstance.Should().BeSameAs(MMNotificationClient.Instance);

        // The cached audio device lister and the shared error event.
        SingletonDescriptor(services, typeof(IAudioDeviceLister)).ImplementationFactory.Should().NotBeNull("the lister is factory-registered with the active+unplugged mask");
        SingletonDescriptor(services, typeof(ErrorEvents)).ImplementationType.Should().Be(typeof(ErrorEvents));

        // The four sub-interfaces, each mapped to its single implementation.
        SingletonDescriptor(services, typeof(SettingsService)).ImplementationType.Should().Be(typeof(SettingsService));
        SingletonDescriptor(services, typeof(IAppSettings)).ImplementationFactory.Should().NotBeNull("IAppSettings forwards to SettingsService");
        SingletonDescriptor(services, typeof(NotificationSettingsService)).ImplementationType.Should().Be(typeof(NotificationSettingsService));
        SingletonDescriptor(services, typeof(INotificationSettings)).ImplementationFactory.Should().NotBeNull("INotificationSettings forwards to NotificationSettingsService");
        SingletonDescriptor(services, typeof(DeviceService)).ImplementationType.Should().Be(typeof(DeviceService));
        SingletonDescriptor(services, typeof(IDeviceService)).ImplementationFactory.Should().NotBeNull("IDeviceService forwards to DeviceService");
        SingletonDescriptor(services, typeof(AppInfrastructure)).ImplementationType.Should().Be(typeof(AppInfrastructure));
        SingletonDescriptor(services, typeof(IAppInfrastructure)).ImplementationFactory.Should().NotBeNull("IAppInfrastructure forwards to AppInfrastructure");

        // The managers and the startup sequence.
        SingletonDescriptor(services, typeof(NotificationManager)).ImplementationType.Should().Be(typeof(NotificationManager));
        SingletonDescriptor(services, typeof(TrayIcon)).ImplementationType.Should().Be(typeof(TrayIcon));
        SingletonDescriptor(services, typeof(ProfileManager)).ImplementationFactory.Should().NotBeNull("ProfileManager is factory-registered with its collaborators");
        SingletonDescriptor(services, typeof(AppSoundLockManager)).ImplementationFactory.Should().NotBeNull("AppSoundLockManager is factory-registered with its collaborators");
        SingletonDescriptor(services, typeof(AppBootstrapper)).ImplementationType.Should().Be(typeof(AppBootstrapper));

        // The Lazy bridges that give the façade and the bootstrapper deferred access to
        // the managers (MS.DI resolves Lazy<T> only when registered).
        SingletonDescriptor(services, typeof(Lazy<TrayIcon>)).ImplementationFactory.Should().NotBeNull();
        SingletonDescriptor(services, typeof(Lazy<ProfileManager>)).ImplementationFactory.Should().NotBeNull();
        SingletonDescriptor(services, typeof(Lazy<AppSoundLockManager>)).ImplementationFactory.Should().NotBeNull();

        // Everything in the graph is a singleton: one instance per service for the
        // whole application lifetime.
        services.Should().OnlyContain(descriptor => descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [Test]
    public void TheFourSubInterfaces_ResolveToTheirSingleImplementations()
    {
        using var provider = CompositionRoot.BuildServiceProvider();

        var deviceService = provider.GetRequiredService<IDeviceService>();
        deviceService.Should().BeAssignableTo<DeviceService>();
        provider.GetRequiredService<DeviceService>().Should().BeSameAs(deviceService,
            "resolving the interface and the class yields the same instance");

        var appSettings = provider.GetRequiredService<IAppSettings>();
        appSettings.Should().BeAssignableTo<SettingsService>();
        provider.GetRequiredService<SettingsService>().Should().BeSameAs(appSettings);

        var notificationSettings = provider.GetRequiredService<INotificationSettings>();
        notificationSettings.Should().BeAssignableTo<NotificationSettingsService>();
        provider.GetRequiredService<NotificationSettingsService>().Should().BeSameAs(notificationSettings);

        var infrastructure = provider.GetRequiredService<IAppInfrastructure>();
        infrastructure.Should().BeAssignableTo<AppInfrastructure>();
        provider.GetRequiredService<AppInfrastructure>().Should().BeSameAs(infrastructure);
    }

    [Test]
    public void DisposingTheProvider_IsTheSingleTeardownPath()
    {
        var provider = CompositionRoot.BuildServiceProvider();
        // Resolving the non-UI subset: the manager ctors must be constructible without a
        // message pump or audio registration, since the container constructs them in
        // dependency order.
        provider.GetRequiredService<IAudioDeviceLister>().Should().NotBeNull();
        provider.GetRequiredService<NotificationManager>().Should().NotBeNull();

        var act = () => provider.Dispose();

        // Container-owned reverse disposal: no throw, and the provider is finalized.
        act.Should().NotThrow();
        var resolve = () => provider.GetService<IAppSettings>();
        resolve.Should().Throw<ObjectDisposedException>("a disposed provider refuses further resolution");
    }

    [Test]
    public void MuteBannerStartupReconcile_IsADeviceServiceStepGatedByInitialization()
    {
        using var lister = new FakeAudioDeviceLister();
        var (deviceService, _, _) = ComposeDeviceService(lister);

        var reconciles = 0;
        deviceService.MuteBannersSyncRequested += (_, _) => reconciles++;

        // Before the startup sequence completes, the reconcile is suppressed exactly as
        // the pre-refactor guard was.
        deviceService.RefreshMicrophoneMuteBanners();
        reconciles.Should().Be(0);

        // MarkInitialized is the bootstrapper's statement; the startup reconcile that
        // follows it (inside its never-abort-init try/catch) invokes the step once.
        deviceService.MarkInitialized();
        deviceService.RefreshMicrophoneMuteBanners();
        reconciles.Should().Be(1);

        // The composition root wires the sync event to the notification manager with the
        // service's scoped-reader; a failing lister read then propagates to the caller of
        // the step — the bootstrapper's never-abort try/catch is the guard. The subscriber
        // here mirrors that wiring.
        using var throwingLister = new ThrowingLister();
        var (failingService, _, _) = ComposeDeviceService(throwingLister);
        failingService.MarkInitialized();
        Exception propagated = null;
        failingService.MuteBannersSyncRequested += (_, _) =>
        {
            try
            {
                failingService.GetInScopeMutedMicrophones();
            }
            catch (Exception ex)
            {
                propagated = ex;
            }
        };
        failingService.RefreshMicrophoneMuteBanners();
        propagated.Should().BeAssignableTo<InvalidOperationException>("the reconcile step must surface lister failures to its caller, where the bootstrapper's try/catch keeps initialization alive");
    }

    [Test]
    public void MuteNotificationFlow_ThroughTheBootstrapperWiring_IsSkippedWithAWarningBeforeInit()
    {
        // SOUNDSWITCH-41T, now exercised through the real composition: the lister's
        // volume event reaches the device service, which publishes the pending mute
        // notification; the bootstrapper's construction-time wiring routes it to the
        // notification manager, whose not-ready-yet guard skips it instead of crashing.
        using var lister = new FakeAudioDeviceLister();
        var (deviceService, settingsService, notificationSettingsService) = ComposeDeviceService(lister);
        using var notificationManager = new NotificationManager(notificationSettingsService, new FakeInfrastructure(), new FakeDeviceService());
        using var bootstrapper = new AppBootstrapper(
            lister,
            deviceService,
            settingsService,
            notificationManager,
            new FakeInfrastructure(),
            new Lazy<TrayIcon>(() => throw new InvalidOperationException("the UI-thread managers are not constructed by this test")),
            new Lazy<ProfileManager>(() => throw new InvalidOperationException("the UI-thread managers are not constructed by this test")),
            new Lazy<AppSoundLockManager>(() => throw new InvalidOperationException("the UI-thread managers are not constructed by this test")));

        deviceService.SubscribeAudioDeviceEvents();

        var device = new DeviceFullInfo("Test microphone (USB Audio)", "{0.0.1.00000000}.{59e4df6f-d9a6-4ce0-9899-49dede000000}", EDataFlow.eCapture, "icon", EDeviceState.Active, false);
        var act = () => lister.DeviceVolumeChangedSubject.OnNext(new DeviceVolumeChangedPayload(
            device, new VolumeChangedEventArgs(50, 50, isMuted: true, wasMuted: false)));

        act.Should().NotThrow("a mute event before NotificationManager.Init must be skipped with a warning, not crash the audio thread (SOUNDSWITCH-41T)");
    }
}

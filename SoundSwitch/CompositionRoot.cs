/********************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
 * Copyright (C) 2015-2024 Antoine Aflalo
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 ********************************************************************/

using System;

using Microsoft.Extensions.DependencyInjection;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio.Lister;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Model;
using SoundSwitch.Services;
using SoundSwitch.UI.Component;

namespace SoundSwitch;

/// <summary>
///     The single registry for the application's service graph (ADR
///     docs/design/architecture/2026-11-21-appmodel-decomposition-di.md §C):
///     one file, explicit registrations, no convention scanning. The registry
///     is the graph's source of truth; <see cref="AppBootstrapper" /> owns when
///     each service is resolved, preserving the pre-refactor startup order.
/// </summary>
public static class CompositionRoot
{
    /// <summary>
    ///     Registers every application service. Callers build the provider with
    ///     <see cref="BuildServiceProvider"/> or their own <c>BuildServiceProvider</c>.
    /// </summary>
    public static void RegisterServices(IServiceCollection services)
    {
        // Process-level singletons, registered so the container owns their teardown.
        // Their call sites keep using the static instances; JobScheduler and
        // QuickMenuManager deliberately stay outside the graph (ADR non-goals).
        services.AddSingleton(AudioSwitcher.Instance);
        services.AddSingleton(MMNotificationClient.Instance);

        // The cached audio device lister (constructed with the same device-state
        // mask the pre-refactor context used) and the application error event.
        services.AddSingleton<IAudioDeviceLister>(_ => new CachedAudioDeviceLister(EDeviceState.Active | EDeviceState.Unplugged));
        services.AddSingleton<ErrorEvents>();

        // Model services: the four sub-interfaces, each mapped to its single
        // implementation so resolving the interface and the concrete class yields
        // the same instance.
        services.AddSingleton<SettingsService>();
        services.AddSingleton<IAppSettings>(serviceProvider => serviceProvider.GetRequiredService<SettingsService>());
        services.AddSingleton<NotificationSettingsService>();
        services.AddSingleton<INotificationSettings>(serviceProvider => serviceProvider.GetRequiredService<NotificationSettingsService>());
        services.AddSingleton<DeviceService>();
        services.AddSingleton<IDeviceService>(serviceProvider => serviceProvider.GetRequiredService<DeviceService>());

        // The IAppInfrastructure façade. The tray-adjacent managers resolve lazily:
        // the notification manager (built before them) reads TrayIcon only after the
        // startup sequence constructs it, as the pre-refactor property timing allowed.
        services.AddSingleton<AppInfrastructure>();
        services.AddSingleton<IAppInfrastructure>(serviceProvider => serviceProvider.GetRequiredService<AppInfrastructure>());

        // Managers. ProfileManager and AppSoundLockManager take their collaborators
        // exactly as the pre-refactor InitializeMain supplied them.
        services.AddSingleton<NotificationManager>();
        services.AddSingleton<TrayIcon>();
        services.AddSingleton<ProfileManager>(serviceProvider => new ProfileManager(
            new WindowMonitor(),
            serviceProvider.GetRequiredService<AudioSwitcher>(),
            serviceProvider.GetRequiredService<IAudioDeviceLister>(),
            serviceProvider.GetRequiredService<IDeviceService>(),
            serviceProvider.GetRequiredService<IAppSettings>(),
            serviceProvider.GetRequiredService<TrayIcon>().ShowError,
            new TriggerFactory(),
            serviceProvider.GetRequiredService<NotificationManager>()));
        services.AddSingleton<AppSoundLockManager>(serviceProvider => new AppSoundLockManager(
            AppConfigs.Configuration,
            serviceProvider.GetRequiredService<AudioSwitcher>(),
            new WindowMonitor(),
            new ProcessMonitor(),
            serviceProvider.GetRequiredService<NotificationManager>(),
            serviceProvider.GetRequiredService<IAudioDeviceLister>()));

        // Lazy bridges for the deferred manager access (MS.DI resolves Lazy<T> only
        // when it is registered): the IAppInfrastructure façade and the bootstrapper
        // hold these, and the managers themselves are constructed at the bootstrapper's
        // step, preserving the pre-refactor construction order.
        services.AddSingleton(serviceProvider => new Lazy<TrayIcon>(() => serviceProvider.GetRequiredService<TrayIcon>()));
        services.AddSingleton(serviceProvider => new Lazy<ProfileManager>(() => serviceProvider.GetRequiredService<ProfileManager>()));
        services.AddSingleton(serviceProvider => new Lazy<AppSoundLockManager>(() => serviceProvider.GetRequiredService<AppSoundLockManager>()));

        // The startup sequence.
        services.AddSingleton<AppBootstrapper>();
    }

    /// <summary>
    ///     Builds the application's service provider.
    /// </summary>
    public static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        RegisterServices(services);
        return services.BuildServiceProvider();
    }
}

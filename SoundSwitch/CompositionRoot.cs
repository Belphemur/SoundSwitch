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

using Microsoft.Extensions.DependencyInjection;

using SoundSwitch.Model;

namespace SoundSwitch;

/// <summary>
///     The single registry for the application's service graph (ADR
///     docs/design/architecture/2026-11-21-appmodel-decomposition-di.md §C):
///     one file, explicit registrations, no convention scanning.
/// </summary>
public static class CompositionRoot
{
    /// <summary>
    ///     Registers every application service. Callers build the provider with
    ///     <see cref="BuildServiceProvider"/> or their own <c>BuildServiceProvider</c>.
    /// </summary>
    public static void RegisterServices(IServiceCollection services)
    {
        // The AppModel singleton is today's implementation of all four sub-interfaces
        // (IDeviceService, INotificationSettings, IAppSettings, IAppInfrastructure).
        // The registrations below give the application a single place to resolve them
        // while the implementations are extracted into dedicated service classes; the
        // manager registrations (TrayIcon, ProfileManager, AppSoundLockManager,
        // NotificationManager) join the graph when their construction moves into it.
        var appModel = AppModel.Instance;
        services.AddSingleton(appModel);
        services.AddSingleton<IDeviceService>(serviceProvider => serviceProvider.GetRequiredService<IAppModel>());
        services.AddSingleton<INotificationSettings>(serviceProvider => serviceProvider.GetRequiredService<IAppModel>());
        services.AddSingleton<IAppSettings>(serviceProvider => serviceProvider.GetRequiredService<IAppModel>());
        services.AddSingleton<IAppInfrastructure>(serviceProvider => serviceProvider.GetRequiredService<IAppModel>());
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

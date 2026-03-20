# SoundSwitch App Agents

## Scope

This directory is the main Windows application project. Use the domain-specific AGENTS files in the subfolders for detailed guidance.

## Domain Map

- `Framework/`: app infrastructure, OS integration, configuration, notifications, profiles, updater, tray, WinAPI.
- `Model/`: application state, events, interfaces, and app context wiring.
- `UI/`: WinForms forms and components for the main app.
- `Localization/`: `.resx` resources and localization factories.
- `Services/`: service-level integrations used by the app.
- `Util/`: shared helpers and extensions for the main app.

## Cross-Cutting Rules

- Keep user-facing text in localization resources, not inline strings.
- Preserve backward compatibility for configuration changes and migrations.
- Use `DeviceFullInfo.NameClean` for display and comparison logic, never `Name`.
- Prefer `Serilog` for diagnostics and `RailSharp.Result<T>` patterns where the surrounding code already uses them.
- Validate changes with `dotnet build SoundSwitch.sln -c Debug` at minimum.

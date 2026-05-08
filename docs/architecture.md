# Architecture Overview

SoundSwitch is a Windows desktop application built on .NET with a WinForms UI. It provides fast switching between audio playback and recording devices via global hotkeys, profiles, and per-application rules.

## Project Structure

| Project | Responsibility |
|---------|----------------|
| `SoundSwitch/` | Main WinForms application, UI, localization, framework, services, and application model |
| `SoundSwitch.Audio.Manager/` | Windows audio device management, policy switching, and Core Audio API integration |
| `SoundSwitch.Common/` | Shared utilities and components used across multiple projects |
| `SoundSwitch.CLI/` | Command-line interface and automation entry points |
| `SoundSwitch.IPC/` | Inter-process communication components |
| `SoundSwitch.UI.Menu/` | Reusable WinForms menu components |
| `SoundSwitch.UI.UserControls/` | Reusable WinForms user controls |
| `SoundSwitch.Tests/` | Unit and integration tests for the main application |
| `SoundSwitch.Audio.Manager.Tests/` | Tests focused on audio management logic |

## Key Layers

- **Framework**: Infrastructure including banner notifications, configuration, updater, tray integration, and WinAPI adapters.
- **Model**: Application state, events, interfaces, and context wiring.
- **UI**: WinForms forms and components for user interaction.
- **Services**: External integrations and business logic services.
- **Localization**: Resource-based translations for user-facing text.

## Threading

Several UI-facing managers (such as banner notifications) require initialization on the UI thread. Always respect thread-affinity rules when working with WinForms controls or synchronization contexts.

## Dependencies

- **.NET** (version defined in `global.json`)
- **Serilog** for structured logging
- **RailSharp** for functional result patterns (where adopted)
- **VuePress** (in `website/`) for public-facing documentation

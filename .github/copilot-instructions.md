# SoundSwitch GitHub Copilot Instructions

## Project Architecture

SoundSwitch is a Windows application for switching audio playback and recording devices using hotkeys. It's built with .NET on the Windows platform.

### Core Components

- **SoundSwitch (Main Project)**: The core application handling UI, notifications, and user interaction
- **SoundSwitch.Audio.Manager**: Manages audio device switching using NAudio and Windows APIs
- **SoundSwitch.Common**: Shared utilities and framework components
- **SoundSwitch.CLI**: Command-line interface for controlling SoundSwitch
- **SoundSwitch.IPC**: Inter-process communication for integration with other applications
- **SoundSwitch.UI.Menu**: UI menu components for the system tray and application
- **SoundSwitch.Bluetooth**: Bluetooth device management
- **SoundSwitch.UI.UserControls**: Reusable UI controls

### Architectural Patterns

- **Model-View-Controller**: The application follows MVC pattern with separation of concerns
- **Event-driven architecture**: Heavy use of event handling for device changes and hotkey detection
- **Dependency Injection**: Used to manage component dependencies
- **Job Scheduling**: Used for background tasks and recurring operations

## Development Guidelines

### Coding Standards

1. Classes should use proper XML documentation
2. Follow C# naming conventions (PascalCase for public members, camelCase for private)
3. Extract interfaces for testable components
4. Include copyright notices at the top of source files

### Error Handling

- Use structured exception handling with appropriate logging
- Prefer using the `Result<T>` pattern from RailSharp for indicating success/failure
- Log errors with Serilog

### Localization

- All user-facing strings should be localized
- Use resource files (.resx) for localization
- Support for right-to-left languages

### Audio Device Management

- Use NAudio for audio device enumeration and control
- Handle device addition/removal events
- Support both playback and recording devices

### Settings Management

- Use AppConfigs.Configuration for persistent settings
- Settings should be automatically saved when changed

### Hotkey Registration

- Use the WindowsAPIAdapter for hotkey registration
- Handle conflicts with other applications
- Support customizable hotkeys

## Project Structure

- Use separate assembly projects for distinct functionality
- Tests should be in corresponding test projects
- UI components should be separated from business logic

## Important Interfaces

- `IAudioDeviceLister`: For enumerating audio devices
- `IAppModel`: Main model interface for application state
- `INotificationManager`: Handles user notifications
- `IProfileManager`: Manages device switching profiles

## Testing

- Use unit tests for core functionality
- Mock external dependencies
- Test all device operations

## Deployment

- Support multiple release channels (Stable, Beta, Nightly)
- Use GitHub Actions for CI/CD
- Sign executables with certificate

## Installer (InnoSetup 6.4.2)

SoundSwitch uses InnoSetup for its installer. The main script is located in the `Installer/setup.iss` file.

### Installer Features

- **Multi-language Support**: The installer supports multiple languages (English, German, French, Spanish, Italian, Portuguese, Russian, Polish, Dutch, Chinese, and Korean)
- **Command Line Options**:
  - `/VERYSILENT`: Runs installer without showing the interface
  - `/NODONATE`: Disables the donation page from showing after installation
  - Supports standard InnoSetup command line parameters

### Installation Options

- **Desktop Icon**: Optional desktop shortcut creation (disabled in silent mode)
- **Remove Existing Settings**: Option to clean up previous installation settings (unchecked by default)
- **Add to PATH**: Adds SoundSwitch CLI to system or user PATH based on installation privileges (checked by default)

### Code Signing

- Uses digital signature for the installer and application executables
- Configured with `SignTool` directive in the setup script
- Certificates are properly cleaned up during uninstallation

### Dependencies

The installer handles the following dependencies:

- MSI 4.5 or higher
- KB835732 update (if needed for the OS)
- Various .NET Framework versions through preprocessor directives
- Visual C++ Redistributables (when required)

### Uninstallation

- Verifies that SoundSwitch is not running before uninstallation
- Removes installed files and registry entries
- Removes PATH environment variable entries
- Optionally removes user settings (prompted during uninstallation)
- Removes certificates from the system

### Path Operations

- `ModifyPath`: Adds SoundSwitch CLI to PATH
- `RemoveFromPath`: Cleans up PATH entries during uninstallation
- Both support system-wide and user-level PATH modifications

### Script Organization

- Main setup configuration in `setup.iss`
- Helper scripts in the `scripts/` directory
- `path_operations.iss`: Handles PATH environment variable modifications
- `checkMutex.iss`: Ensures the application is closed before installation/uninstallation
- Various product detection and installation scripts for dependencies

## When Contributing

1. Target the latest .NET version supported by the project
2. Test changes thoroughly across different Windows versions
3. Use GitHub flow for contributions (feature branches and PRs)
4. Update documentation and CHANGELOG.md with significant changes

## DeviceFullInfo

Never use the Name property. Always use the NameClean property instead. Ensure that any handling of device names is done using NameClean to avoid potential issues with formatting or invalid characters.

# SoundSwitch.UI.Menu Agents

## Scope

This project contains the separate quick-menu UI assembly and its supporting components/utilities.

## Responsibilities

- Keep this project focused on quick-menu rendering and interaction.
- Preserve UI-thread synchronization and menu lifecycle behavior.

## Change Rules

- Do not move app configuration or device orchestration logic into this project.
- Keep generic menu types reusable and payload-driven.
- Maintain `SynchronizationContext` safety for menu creation and updates.

## Validation

- Build the solution.
- Manually verify menu lifecycle behavior if interaction code changes.

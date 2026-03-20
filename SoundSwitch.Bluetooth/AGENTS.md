# SoundSwitch.Bluetooth Agents

## Scope

This project is reserved for Bluetooth-related integration used by SoundSwitch.

## Responsibilities

- Keep Bluetooth-specific logic isolated from the main app.
- Introduce focused abstractions instead of leaking platform details outward.

## Change Rules

- Avoid UI and localization logic here.
- Keep new APIs small and testable.
- If substantial functionality is added, add or extend matching tests in a dedicated test project.

## Validation

- Build the solution.

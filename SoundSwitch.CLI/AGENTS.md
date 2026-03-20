# SoundSwitch.CLI Agents

## Scope

This project contains the command-line interface built on `Spectre.Console.Cli`.

## Responsibilities

- Provide scriptable entry points for switching devices, profile actions, settings, and mute control.
- Reuse existing app/runtime behavior through IPC instead of duplicating business logic.

## Change Rules

- Keep commands concise, explicit, and automation-friendly.
- Prefer extending the `Commands/` folder over inflating `Program.cs`.
- Keep argument names stable where possible.
- If a command needs app interaction, route it through `SoundSwitch.IPC` contracts.

## Validation

- Build the solution.
- Run the CLI command path manually for changed commands when practical.

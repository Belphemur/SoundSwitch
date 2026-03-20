# SoundSwitch.UI.UserControls Agents

## Scope

This project is intended for reusable WinForms user controls shared across the solution.

## Responsibilities

- Keep controls reusable, composable, and presentation-focused.
- Avoid embedding main-app orchestration or persistence logic.

## Change Rules

- Prefer self-contained controls with explicit inputs/outputs.
- Localize visible text through resource files where applicable.
- Keep form-specific assumptions out of shared controls.

## Validation

- Build the solution.
- Verify consuming projects still compile after public control changes.

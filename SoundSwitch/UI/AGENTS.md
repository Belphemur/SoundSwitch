# UI Agents

## Scope

This domain contains the main WinForms UI, including forms and reusable components specific to the main app.

## Responsibilities

- Keep UI code focused on presentation, data binding, and user interaction.
- Delegate device, configuration, and notification behavior to the model/framework layers.

## Change Rules

- Use localization resources for all visible text.
- Keep form code-behind thin; prefer `AppModel` and dedicated managers for behavior.
- Avoid editing `*.Designer.cs` manually unless the change is specifically layout/designer wiring.
- Preserve WinForms threading assumptions and avoid background-thread UI updates.
- Keep hotkey, notification, and settings interactions routed through the existing model APIs.

## Validation

- Build the solution.
- If the change affects a settings or banner workflow, validate the corresponding UI interaction manually.

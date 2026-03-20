# Model Agents

## Scope

This domain contains `AppModel`, application events, interfaces, hotkey actions, and the application context.

## Responsibilities

- Keep the model as the state and event boundary between framework code and UI.
- Expose state changes through events and stable properties.
- Coordinate settings persistence without embedding form logic.

## Change Rules

- Preserve event semantics and avoid silent behavior changes.
- Keep `IAppModel` and `AppModel` aligned.
- When adding new settings, wire all of: model property, persistence, migration, and runtime consumers.
- Prefer minimal logic in event DTOs and keep them serialization-agnostic.

## Validation

- Build the solution.
- For settings changes, verify initialization, save behavior, and migration paths.

# Framework Agents

## Scope

This domain contains app infrastructure: audio orchestration, banner notifications, configuration, profiles, updater, tray integration, job scheduling, and WinAPI adapters.

## Responsibilities

- Keep this layer as the runtime backbone of the app.
- Centralize OS and device integration here rather than pushing it into UI code.
- Route persistent settings through `AppConfigs.Configuration` and keep migrations in sync.

## Change Rules

- Any configuration schema change must remain backward compatible and be migrated in `SoundSwitchConfiguration`.
- Keep UI-facing behavior exposed through model/events instead of direct form coupling.
- Preserve thread-affinity rules for UI-thread managers such as banners, menus, and notifications.
- Use localized strings for notification text and prompts.
- Do not bypass existing factories/managers when adding a new banner, notification, or profile behavior.

## Validation

- Build the solution.
- If audio or notification behavior changes, verify the related tests and event flows.

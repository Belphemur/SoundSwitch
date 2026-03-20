# SoundSwitch.Tests Agents

## Scope

This project contains application-level tests such as configuration, language, refresh-device, download, and version behavior.

## Responsibilities

- Protect app behavior that crosses domains.
- Add regression coverage for configuration, localization, and orchestration bugs.

## Change Rules

- Prefer deterministic tests with minimal environment assumptions.
- Add focused regression tests when changing migrations, settings flows, or public app behaviors.
- Keep test names descriptive and tied to the behavior under test.

## Validation

- Run the changed tests or the full test project.

# SoundSwitch Development Documentation

Welcome to the SoundSwitch development documentation. This section is intended for contributors and maintainers who want to understand the codebase, architecture, and key components.

## Getting Started

- [Building SoundSwitch](./building.md) — Setup, build commands, CI/CD pipeline, and debugging tips.
- [Contributing](./contributing.md) — Standards, conventions, commit format, PR process, and translation workflow.

## Architecture

- [Architecture Overview](./architecture.md) — Project structure, key layers, threading model, and dependencies.
- [Microphone Mute Banner Manager](./banner-manager.md) — Deep dive into the persistent banner subsystem.

## Architecture Decision Records (ADRs)

Significant design decisions are recorded under [`docs/design/`](./design/) in dated, topic-scoped markdown files (e.g. `design/app-rules/2026-10-22-app-rule-modes.md`). Write new ones following the `design-docs` skill (`.opencode/skills/design-docs/SKILL.md`) — see the root [AGENTS.md](../AGENTS.md) for the trigger rules.

## Contributing

Please refer to the root [AGENTS.md](../AGENTS.md) for repository-wide guidelines, coding standards, and validation rules before making changes. Also see [Contributing](./contributing.md) for detailed contribution guidelines.

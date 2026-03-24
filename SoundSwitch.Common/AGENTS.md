# SoundSwitch.Common Agents

## Scope

This project contains shared framework primitives, reusable resources, and low-level helpers shared across projects.

## Responsibilities

- Keep common code broadly reusable and dependency-light.
- Avoid main-app-specific workflow logic in this project.

## Change Rules

- Prefer stable APIs because many projects may depend on them.
- Keep serialization, icon, and shared utility code generic.
- Avoid introducing dependencies on WinForms application state unless that dependency already exists by design.

## Icon Infrastructure

All icon extraction goes through `Framework/Icon/IconExtractor` (see `Framework/Icon/AGENTS.md`).
The extractor provides caching, GDI reference counting via `IconHandle`, and proper disposal.
`Framework/Audio/Icon/AudioDeviceIconExtractor` wraps it and adds audio-specific fallback defaults.

Key rules:
- `IconExtractor.Extract` and `ExtractFromPath` return `IconHandle` — callers must dispose.
- `DeviceFullInfo.LargeIcon` / `SmallIcon` return `IconHandle` — same disposal rule applies.
- Use `IconExtractor.CreatePermanent` for static, application-lifetime fallback icons.

## Validation

- Build the solution.
- Check downstream compile impact when changing shared public types.

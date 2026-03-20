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

## Validation

- Build the solution.
- Check downstream compile impact when changing shared public types.

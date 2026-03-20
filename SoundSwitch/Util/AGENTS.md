# Util Agents

## Scope

This domain contains helper methods, extensions, timers, and small utilities used by the main app.

## Responsibilities

- Keep helpers generic, reusable, and low-risk.
- Avoid leaking business rules into utility code.

## Change Rules

- Prefer pure functions and side-effect-light helpers.
- Do not add app orchestration, persistence, or UI workflow logic here.
- Keep naming concrete and avoid overly broad helper classes.

## Validation

- Build the solution.
- Check call sites if a utility API shape changes.

# Services Agents

## Scope

This domain contains service-oriented integrations used by the main app.

## Responsibilities

- Encapsulate external or subsystem interactions behind focused services.
- Keep services reusable by framework/model code without presentation concerns.

## Change Rules

- Avoid UI logic and localization decisions in this layer.
- Prefer clear boundaries, cancellation-aware APIs, and deterministic side effects.
- Keep service contracts small and explicit.

## Validation

- Build the solution.
- If a service affects devices or background workflows, verify the upstream caller behavior too.

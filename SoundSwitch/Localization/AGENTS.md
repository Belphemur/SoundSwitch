# Localization Agents

## Scope

This domain contains localized resource files and localization factories for the main app.

## Responsibilities

- Store all user-facing text in `.resx` resources.
- Keep resource keys stable and descriptive.
- Maintain culture-specific resources consistently.

## Change Rules

- Edit `.resx` files, not generated `*.Designer.cs` files.
- Add new keys to the base resource first; update localized resources when possible.
- Preserve formatting tokens, line breaks, and access-key semantics.
- For multi-line UI text, encode the line break explicitly in the resource value.

## Validation

- Build the solution to regenerate resource accessors if needed.
- Check that changed strings still fit the intended UI.

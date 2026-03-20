# SoundSwitch.Audio.Manager.Tests Agents

## Scope

This project contains tests for the audio manager and policy/config behavior.

## Responsibilities

- Cover audio-manager logic with deterministic tests.
- Keep tests focused on observable behavior and contract boundaries.

## Change Rules

- Prefer targeted tests over broad fixture-heavy coverage.
- Mock or isolate external dependencies where practical.
- Add tests when changing audio policy, switching behavior, or serialization of audio-related payloads.

## Validation

- Run the relevant test file or the full project after updates.

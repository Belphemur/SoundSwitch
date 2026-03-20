# SoundSwitch.IPC Agents

## Scope

This project contains the named-pipe transport and message contracts used for inter-process communication.

## Responsibilities

- Keep IPC contracts version-tolerant and explicit.
- Isolate transport concerns from CLI and main app behavior.
- Preserve request/response clarity and cancellation behavior.

## Change Rules

- Use MessagePack for IPC serialization.
- Prefer MessagePack source generation for new or expanded message contracts.
- Keep pipe messages small, strongly typed, and stable.
- Avoid embedding business logic in transport classes; message handling belongs in the caller/consumer.

## Validation

- Build the solution.
- Verify both sender and receiver sides when changing contracts.

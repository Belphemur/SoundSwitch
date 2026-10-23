---
name: Answer pull request reviews
description: Answer GitHub pull-request review threads (CodeRabbit, Kody, human) by reading each thread through the gh CLI, deciding fix-vs-push-back against the current code, then replying and resolving threads until zero remain. Use when the user wants to triage or close out review threads on a PR, or after an automated review bot re-reviews a push.
---

# Answer pull request reviews

Close out GitHub review threads by treating every comment critically, fixing what
is still valid against the current code, and politely but precisely declining
the rest — then resolving each thread so the PR is ready to merge.

## Workflow

The loop is: **read → assess → fix or push back → reply → resolve → verify → re-review**. Bots re-review on every push, so the last step may hand control back to the start.

### 1. Gather the threads

Use `tools/Review-Threads.ps1`, which wraps the gh GraphQL API (never `gh pr
view` — it has no review-thread data):

```powershell
pwsh tools\Review-Threads.ps1 -Repo Belphemur/SoundSwitch -Pr 2532 -Json review.json
```

- The `-Json` file holds the structured thread list (ids, paths, lines,
  resolved/outdated flags, first comment author and a body snippet). Start from
  the first comment of each thread; the review text is the actionable part.
- `-Thread <id>` prints one thread in full (path, line, all comments, diff
  hunks) — use it before deciding anything on a substantive thread.
- Resolved or outdated threads still deserve a note when they overlap a live
  concern, but GitHub only lets you **resolve** live ones; plan `leave` for
  outdated threads.

### 2. Assess critically

For every unresolved thread:

1. Read the comment and the **current** code it points at (the thread's path and
   line, not just the diff hunk). A reworked feature often already addresses it.
2. Verify the claim: reproduce the scenario in the code path, not in your head.
3. Decide:
   - **Fix** — the concern is real and the fix is small, safe, and in scope.
   - **Push back** — the concern is already handled, is a deliberate documented
     limitation, or the fix would be a disproportionate change (then say so with
     the evidence, e.g. the code that already handles it or a test name).

Load the `dry-solid-kiss` skill when choosing how to implement a fix (smallest
correct change, one source of truth, match surrounding style).

### 3. Fix

- Implement the fix on the PR branch, add the relevant unit test if practical,
  build, and run the targeted tests:
  ```powershell
  dotnet build SoundSwitch.sln -c Debug
  dotnet test SoundSwitch.Tests\SoundSwitch.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~<Area>"
  ```
- Commit with a conventional message scoped to the area, e.g.
  `fix(app-rules): reserve the pair index atomically per press`. Push.

### 4. Reply

Every reply must be actionable and evidence-based. State the commit SHA when
something was fixed, or the concrete reason (with proof) when it was not:

> Fixed in `abc1234`: `IsHotkeyAvailable` now returns `owner.Id == ruleId`
> before probing the OS, and the probe is a strict native registration that no
> longer falls back to the keyboard hook, so a shortcut owned by a profile or
> another application is rejected. Verified by `dotnet build` and the
> `AppSoundRule*` tests.

> Not changing this: `ApplyRuleToRunningProcesses` matches the process's main
> window title by design — enumerating every top-level window per process would
> be a separate feature (needs a Win32 window scan) and is tracked in the design
> doc instead.

Rules of thumb:

- Acknowledge the specific point; do not reply with a generic "thanks".
- If the bot's premise does not hold on the current code, say exactly that and
  point at the lines/tests that prove it.
- Keep it short; no need to restate the whole comment.
- Mention that you verified with a build/test when you did.

### 5. Resolve

Post the replies and resolve the threads in one pass with the plan file. The
plan is the well-described JSON payload `Review-Threads.ps1 -ResolveAll`
consumes:

```json
{
  "repo": "Belphemur/SoundSwitch",
  "pullRequest": 2532,
  "threads": [
    { "threadId": "PRRT_...", "action": "leave", "body": "..." },
    { "threadId": "PRRT_...", "action": "unresolve", "body": "..." }
  ]
}
```

- `action`: `resolve` (reply then resolve), `unresolve` (reply then re-open),
  or `leave` (reply only — for outdated threads GitHub refuses to resolve).
- Every entry with a `body` posts a threaded reply first; the action then
  resolves/unresolves the thread.

```powershell
pwsh tools\Review-Threads.ps1 -Repo Belphemur/SoundSwitch -Pr 2532 -ResolveAll -Plan plan.json
```

The script prints one `OK/FAIL` line per thread and ends with the list of
threads still open. `-DryRun` previews the plan without touching the API.

Resolving without replying (or vice versa) is the classic mistake: reviewers see
no justification, or the merge gate stays blocked.

### 6. Verify, then hand back to the bots

```powershell
pwsh tools\Review-Threads.ps1 -Repo Belphemur/SoundSwitch -Pr 2532 | jsonq '[.threads[] | select(.isResolved == false)] | length'
gh pr view 2532 --json mergeStateStatus,statusCheckRollup,headRefOid
```

- If zero live threads remain and checks are green, the PR can merge.
- If the push triggered another bot review, loop back to step 1 with the new head
  — do not "resolve all" a second review that re-raises a fixed concern.

## Script reference

`tools/Review-Threads.ps1` (PowerShell 7+, `gh` CLI authenticated):

| Flag | Effect |
| --- | --- |
| `-List` (default) | Print a compact thread summary (id, resolved, outdated, path, line, author, snippet). |
| `-Json <file>` | Also write the full structured thread payload for the answering model. |
| `-Thread <id>` | Print one thread in full (all comments, diff hunks). |
| `-Reply <id> -Body "..."` | Post a threaded reply. |
| `-Resolve <id>` / `-Unresolve <id>` | Resolve / re-open one thread. |
| `-ResolveAll -Plan <file.json>` | Reply + resolve/unresolve per the plan, then report what is still open. |
| `-DryRun` | Validate the plan and preview it without API calls. |

All GraphQL requests go through `gh api graphql --input -` with a JSON payload,
so quoting is never a problem; success is judged by `$LASTEXITCODE`.
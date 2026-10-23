<#
.SYNOPSIS
    Reads and resolves GitHub pull-request review threads through the gh CLI.

.DESCRIPTION
    Wraps the gh GraphQL API for the pull-request review-thread workflow. All
    requests are POSTed via `gh api graphql --input -` with a
    ConvertTo-Json -Compress payload, so PowerShell quoting can never mangle the
    query. Success is judged by $LASTEXITCODE, never by empty output (gh writes
    progress to stderr).

    Commands
      -List
          Print every review thread of the PR as JSON: thread id, resolved /
          outdated flags, path/line, first comment author, and a short body
          snippet. Combine with -Jq to post-process; with -Json <file> to also
          write the raw JSON payload (the full bodies and diff hunks) for the
          answering model.

      -Thread <id>
          Print a single thread in full (path, line, all comments with author
          and body). Use this to read the complete bot report before deciding
          fix-vs-push-back.

      -Reply <id> -Body "text"
          Reply to the thread with the given Markdown body. The reply is
          threaded under the thread's first comment.

      -Resolve <id>
          Resolve the thread (merge-gate checkmark). Can fail for outdated
          threads; check the output.

      -Unresolve <id>
          Re-open a resolved thread.

      -ResolveAll -Plan <file.json>
          Read a plan file (see JSON payload below) and, for every entry, post
          the reply and then resolve / unresolve the thread in one pass. Prints
          a per-thread result and a final list of threads still open.

    JSON PLAN PAYLOAD (for -ResolveAll / hand-rolled loops)
    {
      "repo": "Belphemur/SoundSwitch",
      "pullRequest": 2532,
      "threads": [
        {
          "threadId": "PRRT_...",
          "action": "resolve",   // resolve | unresolve | leave
          "body": "Fixed in <sha>: ..."
        }
      ]
    }
    Every thread with a `body` gets the reply posted first; `action` then
    resolves, unresolves, or leaves the thread. `leave` is useful for outdated
    threads that cannot be resolved via the API.

.PARAMETER Repo
    GitHub repository `owner/name`. Defaults to the current checkout.

.PARAMETER Pr
    Pull request number.

.PARAMETER Json
    (List) also write the full raw thread payload to this file (all comments,
    bodies, diff hunks). Intended to feed the answering model.

.PARAMETER Jq
    (List) jq/JSONPath filter applied to the thread summary. Defaults to a
    compact per-thread line. Set to `.` for the raw array.

.PARAMETER Thread
    Fetch one thread by id (full body).

.PARAMETER Reply / Body
    Reply to Thread with Body.

.PARAMETER Resolve / Unresolve
    Resolve / re-open Thread.

.PARAMETER ResolveAll / Plan
    Apply a plan file; see the JSON PAYLOAD section.

.PARAMETER DryRun
    (ResolveAll) validate the plan and print what would happen, no API calls.

.PARAMETER MaxThreads
    (List) page size for reviewThreads. Defaults to 100.

#>
[CmdletBinding()]
param(
    [string]$Repo,
    [int]$Pr,
    [string]$Json,
    [string]$Jq,
    [switch]$List,
    [string]$Thread,
    [string]$Reply,
    [string]$Body,
    [string]$Resolve,
    [string]$Unresolve,
    [switch]$ResolveAll,
    [string]$Plan,
    [switch]$DryRun,
    [int]$MaxThreads = 100
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Fail($msg) {
    Write-Error $msg
    exit 1
}

# --- repo resolution ---------------------------------------------------------

if ([string]::IsNullOrWhiteSpace($Repo)) {
    try {
        $Repo = (gh repo view --json nameWithOwner --jq .nameWithOwner 2>$null | Out-String).Trim()
    } catch { $Repo = '' }
}
if ([string]::IsNullOrWhiteSpace($Repo)) { Fail "-Repo is required (could not infer it)." }
if ($Pr -le 0) { Fail "-Pr is required and must be a positive number." }

# --- gh GraphQL helper --------------------------------------------------------

function Graphql($query, $variables) {
    $payload = @{ query = $query; variables = $variables } | ConvertTo-Json -Depth 20 -Compress
    $out = ($payload | gh api graphql --input - 2>$null | Out-String).Trim()
    if ($LASTEXITCODE -ne 0) {
        # Throw (not exit) so a -ResolveAll batch can catch a failed entry and
        # keep going; earlier replies in the batch have already been posted.
        throw "gh api graphql failed (exit $LASTEXITCODE).`n$out"
    }
    return ($out | ConvertFrom-Json)
}

# --- queries -----------------------------------------------------------------

$ListQuery = @'
query($owner: String!, $repo: String!, $number: Int!, $max: Int!, $after: String) {
  repository(owner: $owner, name: $repo) {
    pullRequest(number: $number) {
      headRefOid
      reviewThreads(first: $max, after: $after) {
        pageInfo { hasNextPage endCursor }
        nodes {
          id
          isResolved
          isOutdated
          path
          line
          comments(first: 100) {
            nodes { id author { login } createdAt body diffHunk }
          }
        }
      }
    }
  }
}
'@

$ThreadQuery = @'
query($threadId: ID!) {
  node(id: $threadId) {
    ... on PullRequestReviewThread {
      path
      line
      isResolved
      isOutdated
      comments(first: 100) { nodes { id author { login } createdAt body diffHunk } }
    }
  }
}
'@

$ReplyMutation = @'
mutation($threadId: ID!, $body: String!) {
  addPullRequestReviewThreadReply(
    input: { pullRequestReviewThreadId: $threadId, body: $body }
  ) { comment { id url } }
}
'@

$ResolveMutation = @'
mutation($threadId: ID!) {
  resolveReviewThread(input: { threadId: $threadId }) {
    thread { id isResolved }
  }
}
'@

$UnresolveMutation = @'
mutation($threadId: ID!) {
  unresolveReviewThread(input: { threadId: $threadId }) {
    thread { id isResolved }
  }
}
'@

$ReviewsQuery = @'
query($owner: String!, $repo: String!, $number: Int!, $max: Int!, $after: String) {
  repository(owner: $owner, name: $repo) {
    pullRequest(number: $number) {
      reviews(first: $max, after: $after) {
        pageInfo { hasNextPage endCursor }
        nodes { author { login } state submittedAt body }
      }
    }
  }
}
'@

function Get-Threads {
    $split = $Repo.Split('/')
    $all = @()
    $pageInfo = $null
    $after = $null
    do {
        $data = Graphql $ListQuery @{ owner = $split[0]; repo = $split[1]; number = $Pr; max = $MaxThreads; after = $after }
        $pr = $data.data.repository.pullRequest
        $all += $pr.reviewThreads.nodes
        $pageInfo = $pr.reviewThreads.pageInfo
        $after = $pageInfo.endCursor
    } while ($pageInfo.hasNextPage)

    return @{
        headRefOid = $pr.headRefOid
        pageInfo   = $pageInfo
        threads    = $all
    }
}

function Get-Reviews {
    $split = $Repo.Split('/')
    $all = @()
    $pageInfo = $null
    $after = $null
    do {
        $data = Graphql $ReviewsQuery @{ owner = $split[0]; repo = $split[1]; number = $Pr; max = $MaxThreads; after = $after }
        $pr = $data.data.repository.pullRequest
        foreach ($n in $pr.reviews.nodes) { if (-not [string]::IsNullOrWhiteSpace($n.body)) { $all += $n } }
        $pageInfo = $pr.reviews.pageInfo
        $after = $pageInfo.endCursor
    } while ($pageInfo.hasNextPage)

    return $all
}

function Read-Thread($id) {
    $data = Graphql $ThreadQuery @{ threadId = $id }
    return $data.data.node
}

function Post-Reply($id, $body) {
    $data = Graphql $ReplyMutation @{ threadId = $id; body = $body }
    return $data.data.addPullRequestReviewThreadReply.comment
}

function Set-Resolved($id, $state) {
    $data = Graphql ($state ? $ResolveMutation : $UnresolveMutation) @{ threadId = $id }
    return $data.data.($state ? 'resolveReviewThread' : 'unresolveReviewThread').thread
}

# --- commands ----------------------------------------------------------------

if ($Thread) {
    $t = Read-Thread $Thread
    if ($null -eq $t) { Fail "Thread not found: $Thread" }
    Write-Host (($t | ConvertTo-Json -Depth 20))
    exit 0
}

if ($Reply) {
    if ([string]::IsNullOrWhiteSpace($Body)) { Fail "-Body is required with -Reply." }
    $c = Post-Reply $Reply $Body
    Write-Host "Replied: $($c.url)"
    exit 0
}

if ($Resolve) {
    $t = Set-Resolved $Resolve $true
    Write-Host "Resolved: $($t.id) (isResolved=$($t.isResolved))"
    exit 0
}

if ($Unresolve) {
    $t = Set-Resolved $Unresolve $false
    Write-Host "Unresolved: $($t.id) (isResolved=$($t.isResolved))"
    exit 0
}

if ($ResolveAll) {
    if ([string]::IsNullOrWhiteSpace($Plan)) { Fail "-Plan <file.json> is required with -ResolveAll." }
    if (-not (Test-Path $Plan)) { Fail "Plan file not found: $Plan" }
    $planObject = ConvertFrom-Json ([System.IO.File]::ReadAllText($Plan))

    if ($planObject.repo -and $planObject.repo -ne $Repo) { Fail "Plan targets $($planObject.repo), script was called with $Repo." }
    if ($planObject.pullRequest -and $planObject.pullRequest -ne $Pr) { Fail "Plan targets pull request $($planObject.pullRequest), script was called with $Pr." }

    $ids = @{}
    foreach ($entry in $planObject.threads) {
        if ([string]::IsNullOrWhiteSpace($entry.threadId)) { Fail "Plan entry missing threadId." }
        if ($ids.ContainsKey($entry.threadId)) { Fail "Duplicate threadId in plan: $($entry.threadId)" }
        $ids[$entry.threadId] = $true
        if ($entry.action -notin @('resolve', 'unresolve', 'leave')) { Fail "action '$($entry.action)' invalid." }
    }

    if ($DryRun) {
        Write-Host "DRY RUN - $($planObject.threads.Count) thread(s):"
        foreach ($entry in $planObject.threads) {
            $preview = ([string]::IsNullOrWhiteSpace($entry.body) ? '(no reply)' : "$($entry.body.Substring(0, [Math]::Min(50, $entry.body.Length)))…")
            Write-Host "  $($entry.threadId) [$($entry.action)] $preview"
        }
        exit 0
    }

    $results = @()
    foreach ($entry in $planObject.threads) {
        $r = @{ threadId = $entry.threadId; action = $entry.action }
        try {
            if (-not [string]::IsNullOrWhiteSpace($entry.body)) {
                $c = Post-Reply $entry.threadId $entry.body
                $r.replyUrl = $c.url
            }
            if ($entry.action -in @('resolve', 'unresolve')) {
                $t = Set-Resolved $entry.threadId ($entry.action -eq 'resolve')
                $r.isResolved = $t.isResolved
            }
            $r.ok = $true
        } catch {
            $r.ok = $false
            $r.error = $_.Exception.Message
        }
        $results += $r
    }

    foreach ($r in $results) {
        $status = $r.ok ? 'OK  ' : 'FAIL'
        $replyUrl = $r.Contains('replyUrl') ? $r.replyUrl : ''
        $errorMessage = $r.Contains('error') ? $r.error : ''
        Write-Host "[$status] $($r.threadId) $($r.action) $replyUrl $errorMessage"
    }

    $anyFailed = $false
    foreach ($r in $results) {
        if (-not $r.ok) { $anyFailed = $true }
    }
    if ($anyFailed) {
        Write-Error "One or more plan entries failed; see the report above."
        exit 1
    }

    Write-Host ''
    Write-Host 'Threads still open:'
    foreach ($t in (Get-Threads).threads) {
        if (-not $t.isResolved) {
            Write-Host "  - $($t.id) $($t.path):$($t.line) $($t.isOutdated ? '[outdated]' : '')"
        }
    }
    exit 0
}

# --- default: -List ----------------------------------------------------------

$data = Get-Threads
$summary = @{ repo = $Repo; pullRequest = $Pr; headRefOid = $data.headRefOid; pageInfo = $data.pageInfo; threads = @() }
foreach ($t in $data.threads) {
    $first = $t.comments.nodes.Count -gt 0 ? $t.comments.nodes[0] : $null
    $summary.threads += @{
        threadId   = $t.id
        isResolved = $t.isResolved
        isOutdated = $t.isOutdated
        path       = $t.path
        line       = $t.line
        author     = $first ? $first.author.login : $null
        snippet    = $first ? ($first.body | ConvertTo-Json -Compress) : $null
    }
}

if (-not [string]::IsNullOrWhiteSpace($Json)) {
    $full = @{
        repo        = $Repo
        pullRequest = $Pr
        headRefOid  = $data.headRefOid
        pageInfo    = $data.pageInfo
        threads     = $data.threads
        reviews     = (Get-Reviews)
    }
    [System.IO.File]::WriteAllText($Json, ($full | ConvertTo-Json -Depth 20), [System.Text.Encoding]::UTF8)
    Write-Host "Wrote $Json" -ForegroundColor Green
}

$jqExpr = -not [string]::IsNullOrWhiteSpace($Jq) ? $Jq : ''
$summaryJson = $summary | ConvertTo-Json -Depth 12
if (-not [string]::IsNullOrWhiteSpace($jqExpr)) {
    $jqBin = (Get-Command jq -ErrorAction SilentlyContinue)
    if ($null -ne $jqBin) {
        $summaryJson = ($summaryJson | & $jqBin.Source $jqExpr | Out-String).Trim()
    }
}
Write-Host $summaryJson
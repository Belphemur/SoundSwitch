from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
from dataclasses import asdict, dataclass
from datetime import datetime, timezone
from typing import Any, cast
from urllib import error, request

import boto3  # type: ignore[import-not-found]


@dataclass
class Artifact:
    key: str
    version: str
    published: str
    url: str


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--file", required=True)
    parser.add_argument("--version", required=True)
    parser.add_argument("--bucket", required=True)
    parser.add_argument("--account-id", required=True)
    parser.add_argument("--access-key-id", required=True)
    parser.add_argument("--secret-access-key", required=True)
    parser.add_argument("--public-base-url", default="")
    parser.add_argument("--prefix", default="nightly")
    parser.add_argument("--metadata-file", default="")
    parser.add_argument("--discord-webhook", default="")
    parser.add_argument("--repository", default="")
    parser.add_argument("--commit-count", type=int, default=10)
    return parser.parse_args()


def create_client(args: argparse.Namespace) -> Any:
    boto3_module = cast(Any, boto3)
    return boto3_module.client(
        service_name="s3",
        endpoint_url=f"https://{args.account_id}.r2.cloudflarestorage.com",
        aws_access_key_id=args.access_key_id,
        aws_secret_access_key=args.secret_access_key,
        region_name="auto",
    )


def load_existing_artifacts(
    client: Any, bucket: str, version_key: str, public_base_url: str
) -> list[Artifact]:
    try:
        response = cast(
            dict[str, Any], client.get_object(Bucket=bucket, Key=version_key)
        )
    except Exception as error:
        error_response = getattr(error, "response", None)
        error_code: str | None = None
        if isinstance(error_response, dict):
            typed_error_response = cast(dict[str, object], error_response)
            error_details = (
                typed_error_response["Error"]
                if "Error" in typed_error_response
                else None
            )
            if isinstance(error_details, dict):
                typed_error_details = cast(dict[str, object], error_details)
                error_code_value = (
                    typed_error_details["Code"]
                    if "Code" in typed_error_details
                    else None
                )
                if isinstance(error_code_value, str):
                    error_code = error_code_value
        if error_code in {"NoSuchKey", "404", "NotFound"}:
            return []
        raise

    body_stream = response["Body"]
    body = cast(bytes, body_stream.read()).decode("utf-8")
    metadata = cast(dict[str, Any], json.loads(body))

    artifacts = cast(list[object] | None, metadata.get("artifacts"))
    if isinstance(artifacts, list):
        result: list[Artifact] = []
        for artifact in artifacts:
            if not isinstance(artifact, dict):
                continue

            artifact_map = cast(dict[str, Any], artifact)
            key = str(artifact_map.get("key", "")).strip()
            if not key:
                continue

            result.append(
                Artifact(
                    key=key,
                    version=str(artifact_map.get("version", "")),
                    published=str(artifact_map.get("published", "")),
                    url=str(artifact_map.get("url", "")),
                )
            )
        return result

    url = str(metadata.get("url", "")).strip()
    if public_base_url and url.startswith(f"{public_base_url}/"):
        return [
            Artifact(
                key=url[len(public_base_url) + 1 :],
                version=str(metadata.get("latest", "")),
                published=str(metadata.get("published", "")),
                url=url,
            )
        ]

    return []


def write_github_output(version: str, download_url: str) -> None:
    github_output = os.getenv("GITHUB_OUTPUT")
    if not github_output:
        return

    with open(github_output, "a", encoding="utf-8") as output_file:
        output_file.write(f"version={version}\n")
        output_file.write(f"download-url={download_url}\n")


def write_metadata_file(metadata_file: str, version_payload: dict[str, Any]) -> None:
    if not metadata_file:
        return

    with open(metadata_file, "w", encoding="utf-8") as output_file:
        json.dump(version_payload, output_file, separators=(",", ":"))


def get_recent_commits(commit_count: int) -> list[str]:
    if commit_count <= 0:
        return []

    result = subprocess.run(
        ["git", "log", "--no-merges", "--oneline", f"-{commit_count}"],
        check=False,
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        return []

    return [line.strip() for line in result.stdout.splitlines() if line.strip()]


def format_commit_lines(commits: list[str], repository: str) -> str:
    if not commits:
        return "* No recent commits found"

    formatted_commits: list[str] = []
    pattern = re.compile(r"^([\da-f]{7,})\s(\w+)\((.+)\)(.+)$")
    for line in commits:
        match = pattern.match(line)
        if match and repository:
            sha = match.group(1)
            commit_type = match.group(2)
            scope = match.group(3)
            subject = match.group(4).strip()
            formatted_commits.append(
                f"* **{commit_type}**(*{scope}*) {subject} [{sha}](https://github.com/{repository}/commit/{sha})"
            )
            continue

        formatted_commits.append(f"* {line}")

    return "\n".join(formatted_commits)


def send_discord_notification(
    webhook_url: str,
    version: str,
    download_url: str,
    repository: str,
    commit_count: int,
    timestamp: str,
) -> None:
    if not webhook_url:
        print("DISCORD_WEBHOOK is not configured, skipping notification.")
        return

    description = "**Last 10 commits**\n\n" + format_commit_lines(
        get_recent_commits(commit_count), repository
    )
    payload: dict[str, Any] = {
        "embeds": [
            {
                "author": {
                    "name": "Beta Build",
                    "icon_url": "https://soundswitch.aaflalo.me/img/beta.png",
                },
                "title": f"New Build: {version}",
                "url": download_url,
                "color": 255,
                "description": description,
                "timestamp": timestamp,
            }
        ]
    }

    payload_bytes = json.dumps(payload).encode("utf-8")
    http_request = request.Request(
        webhook_url,
        data=payload_bytes,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with request.urlopen(http_request) as response:
        if response.status >= 400:
            raise error.HTTPError(
                webhook_url,
                response.status,
                "Discord webhook failed",
                response.headers,
                None,
            )


def main() -> None:
    args = parse_args()
    client = create_client(args)

    prefix = args.prefix.strip("/") or "nightly"
    archive_name = os.path.basename(args.file)
    archive_key = f"{prefix}/{archive_name}"
    version_key = f"{prefix}/version.json"
    public_base_url = args.public_base_url.rstrip("/")
    download_url = f"{public_base_url}/{archive_key}" if public_base_url else ""

    existing_artifacts = load_existing_artifacts(
        client, args.bucket, version_key, public_base_url
    )

    published_at = datetime.now(timezone.utc).isoformat()
    current_artifact = Artifact(
        key=archive_key,
        version=args.version,
        published=published_at,
        url=download_url,
    )

    retained_artifacts = [current_artifact]
    retained_artifacts.extend(
        artifact for artifact in existing_artifacts if artifact.key != archive_key
    )
    retained_artifacts = retained_artifacts[:9]
    retained_keys = {artifact.key for artifact in retained_artifacts}
    delete_keys = [
        artifact.key
        for artifact in existing_artifacts
        if artifact.key and artifact.key not in retained_keys
    ]

    with open(args.file, "rb") as archive_stream:
        response = cast(
            dict[str, Any],
            client.put_object(
                Bucket=args.bucket,
                Key=archive_key,
                Body=archive_stream,
                ContentType="application/zip",
            ),
        )
        print(f"Uploaded successfully. ETag: {response['ETag']}")

    for delete_key in delete_keys:
        client.delete_object(Bucket=args.bucket, Key=delete_key)

    version_payload: dict[str, Any] = {
        "latest": args.version,
        "published": published_at,
        "url": download_url,
        "artifacts": [asdict(artifact) for artifact in retained_artifacts],
    }

    client.put_object(
        Bucket=args.bucket,
        Key=version_key,
        Body=json.dumps(version_payload, separators=(",", ":")).encode("utf-8"),
        ContentType="application/json",
    )

    write_metadata_file(args.metadata_file, version_payload)
    send_discord_notification(
        webhook_url=args.discord_webhook,
        version=args.version,
        download_url=download_url,
        repository=args.repository,
        commit_count=args.commit_count,
        timestamp=published_at,
    )
    write_github_output(args.version, download_url)


if __name__ == "__main__":
    main()

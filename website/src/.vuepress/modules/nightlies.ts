export const NIGHTLYS_ENDPOINT = 'https://nightly.soundswitch.aaflalo.me/nightly/version.json'

interface NightlyArtifactPayload {
  key?: string
  version?: string
  published?: string
  url?: string
}

interface NightliesPayload {
  latest?: string
  url?: string
  artifacts?: NightlyArtifactPayload[]
}

export interface AvailableNightly {
  id: string
  filename: string
  version: string
  published: string
  url: string
  isLatest: boolean
}

function parsePublished(published: string): number {
  const timestamp = Date.parse(published)
  return Number.isNaN(timestamp) ? 0 : timestamp
}

function fileNameFromUrl(url: string): string {
  const [path] = url.split('?')
  const segments = path.split('/').filter(Boolean)
  return decodeURIComponent(segments[segments.length - 1] ?? url)
}

export async function fetchAvailableNightlies(limit = 5): Promise<AvailableNightly[]> {
  const response = await fetch(NIGHTLYS_ENDPOINT, {
    headers: { accept: 'application/json' },
  })

  if (!response.ok) {
    throw new Error(`Nightly endpoint responded with ${response.status}`)
  }

  const payload = (await response.json()) as NightliesPayload
  const artifacts = Array.isArray(payload.artifacts) ? payload.artifacts : []

  return artifacts
    .filter(
      (artifact): artifact is Required<Pick<NightlyArtifactPayload, 'version' | 'published' | 'url'>> &
        NightlyArtifactPayload =>
        typeof artifact.version === 'string' &&
        artifact.version.length > 0 &&
        typeof artifact.published === 'string' &&
        artifact.published.length > 0 &&
        typeof artifact.url === 'string' &&
        artifact.url.length > 0,
    )
    .sort((left, right) => parsePublished(right.published) - parsePublished(left.published))
    .slice(0, limit)
    .map((artifact) => ({
      id: `${artifact.version}-${artifact.url}`,
      filename: fileNameFromUrl(artifact.url),
      version: artifact.version,
      published: artifact.published,
      url: artifact.url,
      isLatest: artifact.version === payload.latest || artifact.url === payload.url,
    }))
}

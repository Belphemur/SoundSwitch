<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { fetchAvailableNightlies, NIGHTLYS_ENDPOINT, type AvailableNightly } from '../modules/nightlies'

const nightlies = ref<AvailableNightly[]>([])
const isLoading = ref(true)
const hasError = ref(false)

const hasNightlies = computed(() => nightlies.value.length > 0)

function formatPublishedDate(value: string): string {
  const published = new Date(value)
  if (Number.isNaN(published.getTime())) {
    return value
  }

  return published.toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    timeZoneName: 'short',
  })
}

onMounted(async () => {
  try {
    nightlies.value = await fetchAvailableNightlies()
  } catch {
    hasError.value = true
  } finally {
    isLoading.value = false
  }
})
</script>

<template>
  <section class="nightly-builds" aria-label="Latest nightly builds">
    <div v-if="isLoading" class="nightly-builds__status">Loading nightly builds…</div>
    <div v-else-if="hasError" class="nightly-builds__status nightly-builds__status--error">
      Unable to load nightly builds right now.
      <a :href="NIGHTLYS_ENDPOINT" target="_blank" rel="noopener">Open the nightly feed directly</a>.
    </div>
    <div v-else-if="!hasNightlies" class="nightly-builds__status">
      No nightly builds are currently available.
    </div>
    <ol v-else class="nightly-builds__list">
      <li v-for="nightly in nightlies" :key="nightly.id" class="nightly-builds__item">
        <div class="nightly-builds__content">
          <div class="nightly-builds__heading">
            <strong>{{ nightly.version }}</strong>
            <span v-if="nightly.isLatest" class="nightly-builds__badge">Latest</span>
          </div>
          <div class="nightly-builds__filename">{{ nightly.filename }}</div>
          <div class="nightly-builds__meta">Published {{ formatPublishedDate(nightly.published) }}</div>
        </div>
        <a class="nightly-builds__link" :href="nightly.url" target="_blank" rel="noopener">
          Download
        </a>
      </li>
    </ol>
  </section>
</template>

<style scoped>
.nightly-builds {
  margin: 1.5rem 0 2rem;
}

.nightly-builds__status {
  padding: 1rem;
  border-radius: 0.75rem;
  background: var(--vp-c-bg-alt, #f6f8fa);
  color: var(--vp-c-text-mute, #5a6878);
  text-align: center;
}

.nightly-builds__status--error {
  background: rgba(244, 67, 54, 0.08);
  color: #b71c1c;
}

.nightly-builds__status a {
  color: inherit;
}

.nightly-builds__list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.85rem;
}

.nightly-builds__item {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: center;
  padding: 1rem 1.1rem;
  border-radius: 0.9rem;
  background: var(--vp-c-bg-alt, #f6f8fa);
  border: 1px solid var(--vp-c-border, rgba(60, 60, 67, 0.12));
}

.nightly-builds__content {
  min-width: 0;
}

.nightly-builds__heading {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  align-items: center;
}

.nightly-builds__heading strong {
  font-size: 1rem;
}

.nightly-builds__badge {
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
  background: rgba(25, 118, 210, 0.12);
  color: #1565c0;
  font-size: 0.78rem;
  font-weight: 700;
  letter-spacing: 0.02em;
  text-transform: uppercase;
}

.nightly-builds__filename,
.nightly-builds__meta {
  color: var(--vp-c-text-mute, #5a6878);
}

.nightly-builds__filename {
  margin-top: 0.2rem;
  overflow-wrap: anywhere;
}

.nightly-builds__meta {
  margin-top: 0.25rem;
  font-size: 0.9rem;
}

.nightly-builds__link {
  flex-shrink: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 7rem;
  padding: 0.7rem 1rem;
  border-radius: 999px;
  background: linear-gradient(135deg, #1976d2 0%, #2196f3 100%);
  color: #fff;
  font-weight: 700;
  text-decoration: none;
  box-shadow: 0 4px 14px rgba(33, 150, 243, 0.2);
}

.nightly-builds__link:hover {
  text-decoration: none;
}

[data-theme='dark'] .nightly-builds__badge {
  background: rgba(100, 181, 246, 0.18);
  color: #90caf9;
}

@media (max-width: 640px) {
  .nightly-builds__item {
    flex-direction: column;
    align-items: stretch;
  }

  .nightly-builds__link {
    width: 100%;
  }
}
</style>

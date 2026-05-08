<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'

interface DownloadsPayload {
  total: number
    formatted?: string
    asOf?: string
    fallback?: boolean
}

// Floor displayed when the API can't be reached. Mirrors the Worker's own
// fallback so the page never shows a missing or "0" value.
const FALLBACK_TOTAL = 3_000_000
const ANIMATION_DURATION_MS = 1600

const total = ref(0)
const isLoading = ref(true)
const isFallback = ref(false)
let rafHandle: number | null = null

function animateTo(target: number) {
    const start = performance.now()
    const from = total.value

    function tick(now: number) {
        const elapsed = now - start
        const progress = Math.min(1, elapsed / ANIMATION_DURATION_MS)
        // easeOutCubic — fast at first then settles cleanly on the final value.
        const eased = 1 - Math.pow(1 - progress, 3)
        total.value = Math.round(from + (target - from) * eased)
        if (progress < 1) {
            rafHandle = requestAnimationFrame(tick)
        } else {
            rafHandle = null
        }
    }

    if (rafHandle !== null) cancelAnimationFrame(rafHandle)
    rafHandle = requestAnimationFrame(tick)
}

const groupedDigits = computed(() => {
    // Render as comma-separated thousands groups so the styling can highlight
    // each group individually (more readable for very large numbers).
    return total.value.toLocaleString('en-US').split(',')
})

onMounted(async () => {
    let target = FALLBACK_TOTAL
  try {
    const response = await fetch('/api/downloads.json', {
      headers: { accept: 'application/json' },
    })
      if (response.ok) {
          const payload = (await response.json()) as DownloadsPayload
          if (typeof payload.total === 'number' && payload.total > 0) {
          target = payload.total
          isFallback.value = Boolean(payload.fallback)
      } else {
            isFallback.value = true
        }
    } else {
        isFallback.value = true
    }
  } catch {
      isFallback.value = true
  } finally {
    isLoading.value = false
      animateTo(target)
  }
})

onBeforeUnmount(() => {
    if (rafHandle !== null) cancelAnimationFrame(rafHandle)
})
</script>

<template>
    <section class="download-count" aria-label="SoundSwitch total downloads">
   
    <div class="download-count__inner">
            <div class="download-count__label">
                <svg class="download-count__icon" viewBox="0 0 24 24" width="18" height="18" fill="none"
                    stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                    aria-hidden="true">
                    <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
                    <polyline points="7 10 12 15 17 10" />
                    <line x1="12" y1="15" x2="12" y2="3" />
                </svg>
                <span>Total downloads</span>
            </div>

     
      <div class="download-count__value" :class="{ 'is-loading': isLoading }">
                <template v-for="(group, idx) in groupedDigits" :key="idx">
                    <span v-if="idx > 0" class="separator">,</span>
                    <span class="digit-group">
                        <span v-for="(char, i) in group" :key="i" class="digit">{{ char }}</span>
                    </span>
       
        </template>
      </div>

     
      <div class="download-count__caption">
                Aggregated across every
                <a href="https://github.com/Belphemur/SoundSwitch/releases" target="_blank" rel="noopener">GitHub
                    release</a>
                <template v-if="isFallback"> · live count temporarily unavailable</template>
     
      </div>
    </div>
  </section>
</template>

<style scoped>
.download-count {
  display: flex;
  justify-content: center;
margin: 2.5rem auto 3rem;
  padding: 0 1rem;
}

.download-count__inner {
position: relative;
  width: 100%;
max-width: 36rem;
    padding: 2rem 2.25rem;
    border-radius: 1.25rem;
    text-align: center;
    background:
        radial-gradient(circle at 0% 0%, rgba(33, 150, 243, 0.18), transparent 60%),
        radial-gradient(circle at 100% 100%, rgba(13, 71, 161, 0.22), transparent 60%),
        linear-gradient(135deg, rgba(255, 255, 255, 0.9), rgba(245, 250, 255, 0.85));
  border: 1px solid rgba(33, 150, 243, 0.25);
box-shadow:
        0 12px 32px rgba(13, 71, 161, 0.12),
        0 2px 4px rgba(13, 71, 161, 0.06);
    overflow: hidden;
}

.download-count__label {
display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.78rem;
    font-weight: 600;
  text-transform: uppercase;
letter-spacing: 0.16em;
    color: var(--vp-c-text-mute, #4a5568);
}

.download-count__icon {
    color: #2196f3;
}

.download-count__value {
display: flex;
    align-items: baseline;
    justify-content: center;
    margin-top: 0.85rem;
    font-size: clamp(2.5rem, 6vw, 4rem);
    font-weight: 800;
  font-variant-numeric: tabular-nums;
letter-spacing: -0.02em;
    line-height: 1;
    background: linear-gradient(135deg, #2196f3 0%, #0d47a1 100%);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
transition: opacity 0.2s ease;
}

.download-count__value.is-loading {
    opacity: 0.55;
}

.digit-group {
    display: inline-flex;
}

.digit {
    display: inline-block;
    min-width: 0.62em;
    text-align: center;
}

.separator {
    display: inline-block;
    margin: 0 0.05em;
}

.download-count__caption {
margin-top: 0.85rem;
  font-size: 0.85rem;
color: var(--vp-c-text-mute, #5a6878);
}

.download-count__caption a {
    color: #1976d2;
    text-decoration: none;
    border-bottom: 1px solid currentColor;
}

[data-theme="dark"] .download-count__inner {
background:
        radial-gradient(circle at 0% 0%, rgba(33, 150, 243, 0.22), transparent 60%),
        radial-gradient(circle at 100% 100%, rgba(13, 71, 161, 0.32), transparent 60%),
        linear-gradient(135deg, rgba(20, 28, 48, 0.85), rgba(15, 22, 40, 0.9));
    border-color: rgba(100, 181, 246, 0.4);
    box-shadow:
        0 12px 32px rgba(0, 0, 0, 0.4),
        0 2px 4px rgba(0, 0, 0, 0.3);
}

[data-theme="dark"] .download-count__label {
    color: #b0bec5;
}

[data-theme="dark"] .download-count__icon {
    color: #64b5f6;
}

[data-theme="dark"] .download-count__value {
background: linear-gradient(135deg, #64b5f6 0%, #90caf9 100%);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

[data-theme="dark"] .download-count__caption {
    color: #90a4ae;
}

[data-theme="dark"] .download-count__caption a {
    color: #64b5f6;
}
</style>

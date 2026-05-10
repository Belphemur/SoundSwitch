<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'

interface DownloadsPayload {
  total: number
    formatted?: string
    asOf?: string
    fallback?: boolean
}

interface DownloadsHistoryPoint {
    date: string
    total: number
    formatted?: string
}

interface DownloadsHistoryPayload {
    asOf?: string
    retentionDays?: number
    history: DownloadsHistoryPoint[]
}

const FALLBACK_TOTAL = 3_000_000
const ANIMATION_DURATION_MS = 1600
const CHART_WIDTH = 860
const CHART_HEIGHT = 250
const CHART_PADDING_X = 28
const CHART_PADDING_Y = 20

const total = ref(0)
const isLoading = ref(true)
const isFallback = ref(false)
const isChartVisible = ref(false)
const isHistoryLoading = ref(false)
const historyError = ref<string | null>(null)
const historyPoints = ref<DownloadsHistoryPoint[]>([])
let rafHandle: number | null = null

function animateTo(target: number) {
    const start = performance.now()
    const from = total.value

    function tick(now: number) {
        const elapsed = now - start
        const progress = Math.min(1, elapsed / ANIMATION_DURATION_MS)
        const eased = 1 - Math.pow(1 - progress, 3)
        total.value = Math.round(from + (target - from) * eased)
        if (progress < 1) {
            rafHandle = requestAnimationFrame(tick)
        } else {
            rafHandle = null
        }
    }

    if (rafHandle !== null) {
        cancelAnimationFrame(rafHandle)
    }
    rafHandle = requestAnimationFrame(tick)
}

const groupedDigits = computed(() => total.value.toLocaleString('en-US').split(','))

const chartDomain = computed(() => {
    const totals = historyPoints.value.map((point) => point.total)
    if (totals.length === 0) {
        return { min: 0, max: 1 }
    }

    const min = Math.min(...totals)
    const max = Math.max(...totals)
    if (min === max) {
        const padded = Math.max(10_000, Math.round(min * 0.02))
        return { min: Math.max(0, min - padded), max: max + padded }
    }

    return {
        min,
        max,
    }
})

const chartCoordinates = computed(() => {
    const points = historyPoints.value
    const count = points.length
    if (count === 0) {
        return [] as Array<{ x: number; y: number }>
    }

    const innerWidth = CHART_WIDTH - CHART_PADDING_X * 2
    const innerHeight = CHART_HEIGHT - CHART_PADDING_Y * 2
    const range = Math.max(1, chartDomain.value.max - chartDomain.value.min)

    return points.map((point, index) => {
        const x =
            count === 1
                ? CHART_PADDING_X + innerWidth / 2
                : CHART_PADDING_X + (index / (count - 1)) * innerWidth
        const y =
            CHART_PADDING_Y +
            (1 - (point.total - chartDomain.value.min) / range) * innerHeight

        return { x, y }
    })
})

const chartPath = computed(() => {
    if (chartCoordinates.value.length === 0) {
        return ''
    }
    return chartCoordinates.value
        .map((point, index) => `${index === 0 ? 'M' : 'L'}${point.x.toFixed(2)},${point.y.toFixed(2)}`)
        .join(' ')
})

const chartAreaPath = computed(() => {
    if (chartCoordinates.value.length === 0) {
        return ''
    }
    const baseY = CHART_HEIGHT - CHART_PADDING_Y
    const first = chartCoordinates.value[0]
    const last = chartCoordinates.value[chartCoordinates.value.length - 1]
    return [
        `M${first.x.toFixed(2)},${baseY.toFixed(2)}`,
        ...chartCoordinates.value.map((point) => `L${point.x.toFixed(2)},${point.y.toFixed(2)}`),
        `L${last.x.toFixed(2)},${baseY.toFixed(2)}`,
        'Z',
    ].join(' ')
})

const growthText = computed(() => {
    const points = historyPoints.value
    if (points.length < 2) {
        return 'Need more daily snapshots to show growth.'
    }

    const first = points[0]
    const last = points[points.length - 1]
    const delta = last.total - first.total
    const direction = delta >= 0 ? '+' : '-'
    return `${direction}${Math.abs(delta).toLocaleString('en-US')} downloads in the visible period.`
})

const firstDateLabel = computed(() => {
    if (historyPoints.value.length === 0) return ''
    return new Date(historyPoints.value[0].date).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
    })
})

const lastDateLabel = computed(() => {
    if (historyPoints.value.length === 0) return ''
    return new Date(historyPoints.value[historyPoints.value.length - 1].date).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
    })
})

async function loadHistory() {
    isHistoryLoading.value = true
    historyError.value = null

    try {
        const response = await fetch('/api/downloads-history.json', {
            headers: { accept: 'application/json' },
        })
        if (!response.ok) {
            throw new Error('History endpoint unavailable')
        }

        const payload = (await response.json()) as DownloadsHistoryPayload
        historyPoints.value = Array.isArray(payload.history)
            ? payload.history
                .filter((point) => typeof point.total === 'number' && point.total > 0 && typeof point.date === 'string')
                .sort((a, b) => a.date.localeCompare(b.date))
            : []
    } catch {
        historyError.value = 'History is temporarily unavailable.'
    } finally {
        isHistoryLoading.value = false
    }
}

async function toggleChart() {
    isChartVisible.value = !isChartVisible.value
    if (isChartVisible.value && historyPoints.value.length === 0 && !isHistoryLoading.value) {
        await loadHistory()
    }
}

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
    if (rafHandle !== null) {
        cancelAnimationFrame(rafHandle)
    }
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


         <button type="button" class="download-count__value" :class="{ 'is-loading': isLoading }"
                :aria-expanded="isChartVisible" aria-controls="downloads-history-chart" @click="toggleChart">
                <template v-for="(group, idx) in groupedDigits" :key="idx">
                    <span v-if="idx > 0" class="separator">,</span>
                    <span class="digit-group">
                        <span v-for="(char, i) in group" :key="i" class="digit">{{ char }}</span>
                    </span>
       
        </template>
            </button>


     
      <div class="download-count__caption">
                Aggregated across every

                <a href="https://github.com/Belphemur/SoundSwitch/releases" target="_blank" rel="noopener">GitHub
                    release</a>
                · click number for 3-month trend
                <template v-if="isFallback"> · live count temporarily unavailable</template>
     
       </div>

            <transition name="history-panel">
                <div v-if="isChartVisible" id="downloads-history-chart" class="history-panel">
                    <div v-if="isHistoryLoading" class="history-panel__state">Loading history...</div>
                    <div v-else-if="historyError" class="history-panel__state history-panel__state--error">
                        {{ historyError }}</div>
                    <div v-else-if="historyPoints.length < 2" class="history-panel__state">
                        Download history is collecting. Check back tomorrow for a full trend line.
                    </div>
                    <div v-else class="history-panel__chart-wrap">
                        <svg class="history-chart" :viewBox="`0 0 ${CHART_WIDTH} ${CHART_HEIGHT}`" role="img"
                            aria-label="Downloads over the last 3 months">
                            <defs>
                                <linearGradient id="historyAreaGradient" x1="0" y1="0" x2="0" y2="1">
                                    <stop offset="0%" stop-color="rgba(33, 150, 243, 0.42)" />
                                    <stop offset="100%" stop-color="rgba(33, 150, 243, 0.04)" />
                                </linearGradient>
                            </defs>

                            <path v-if="chartAreaPath" :d="chartAreaPath" fill="url(#historyAreaGradient)" />
                            <path v-if="chartPath" :d="chartPath" fill="none" stroke="currentColor" stroke-width="4"
                                stroke-linecap="round" stroke-linejoin="round" />

                            <circle v-for="(point, idx) in chartCoordinates" :key="idx" :cx="point.x" :cy="point.y"
                                r="4" fill="currentColor" />
                        </svg>




                        <div class="history-panel__axis">
                            <span>{{ firstDateLabel }}</span>
                            <span>{{ lastDateLabel }}</span>

                        </div>

                        <p class="history-panel__growth">{{ growthText }}</p>
                    </div>
                </div>
            </transition>

   
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
    display: flex;
        flex-direction: column;
        align-items: center;
    position: relative;
  width: 100%;
    max-width: 44rem;
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
    display: inline-flex;
    align-items: baseline;
    justify-content: center;
    margin-top: 0.85rem;
    padding: 0;
        border: 0;
        background: linear-gradient(135deg, #2196f3 0%, #0d47a1 100%);
        -webkit-background-clip: text;
        background-clip: text;
        color: transparent;
        cursor: pointer;
    font-size: clamp(2.5rem, 6vw, 4rem);
    font-weight: 800;
  font-variant-numeric: tabular-nums;
    letter-spacing: -0.02em;
    line-height: 1;
    transition: opacity 0.2s ease, transform 0.2s ease;
    }
    
    .download-count__value:hover {
        transform: translateY(-1px);
    }
    
    .download-count__value:focus-visible {
        outline: 2px solid rgba(25, 118, 210, 0.6);
        outline-offset: 8px;
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

.history-panel {
    align-self: stretch;
    margin-top: 1.15rem;
    padding: 1rem;
    border-radius: 1rem;
    text-align: left;
    background: linear-gradient(180deg, rgba(255, 255, 255, 0.74), rgba(248, 252, 255, 0.88));
    border: 1px solid rgba(33, 150, 243, 0.2);
}

.history-panel__state {
    color: var(--vp-c-text-mute, #5a6878);
    font-size: 0.9rem;
    text-align: center;
    padding: 0.8rem 0;
}

.history-panel__state--error {
    color: #c0392b;
}

.history-panel__chart-wrap {
    color: #0d47a1;
}

.history-chart {
    display: block;
    width: 100%;
    height: auto;
}

.history-panel__axis {
    margin-top: 0.25rem;
    display: flex;
    justify-content: space-between;
    color: var(--vp-c-text-mute, #5a6878);
    font-size: 0.8rem;
}

.history-panel__growth {
    margin: 0.5rem 0 0;
    color: #0d47a1;
    font-size: 0.86rem;
}

.history-panel-enter-active,
.history-panel-leave-active {
    transition: opacity 0.22s ease, transform 0.22s ease;
}

.history-panel-enter-from,
.history-panel-leave-to {
    opacity: 0;
    transform: translateY(8px);
}

[data-theme='dark'] .download-count__inner {
    background:
        radial-gradient(circle at 0% 0%, rgba(33, 150, 243, 0.22), transparent 60%),
        radial-gradient(circle at 100% 100%, rgba(13, 71, 161, 0.32), transparent 60%),
        linear-gradient(135deg, rgba(20, 28, 48, 0.85), rgba(15, 22, 40, 0.9));
    border-color: rgba(100, 181, 246, 0.4);
    box-shadow:
        0 12px 32px rgba(0, 0, 0, 0.4),
        0 2px 4px rgba(0, 0, 0, 0.3);
}

[data-theme='dark'] .download-count__label {
    color: #b0bec5;
}

[data-theme='dark'] .download-count__icon {
    color: #64b5f6;
}

[data-theme='dark'] .download-count__value {
    background: linear-gradient(135deg, #64b5f6 0%, #90caf9 100%);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

[data-theme='dark'] .download-count__caption {
    color: #90a4ae;
}

[data-theme='dark'] .download-count__caption a {
    color: #64b5f6;
}
[data-theme='dark'] .history-panel {
    background: linear-gradient(180deg, rgba(13, 28, 52, 0.72), rgba(10, 20, 40, 0.9));
    border-color: rgba(100, 181, 246, 0.34);
}

[data-theme='dark'] .history-panel__state {
    color: #b0bec5;
}

[data-theme='dark'] .history-panel__state--error {
    color: #ef9a9a;
}

[data-theme='dark'] .history-panel__chart-wrap {
    color: #90caf9;
}

[data-theme='dark'] .history-panel__axis {
    color: #90a4ae;
}

[data-theme='dark'] .history-panel__growth {
    color: #bbdefb;
}

@media (max-width: 600px) {
    .download-count__inner {
        padding: 1.4rem 1.1rem;
    }

    .history-panel {
        padding: 0.72rem;
    }
}
</style>

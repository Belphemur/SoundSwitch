<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'

interface WeblateLanguage {
    code: string
    name: string
    translated_percent?: number
    translated?: number
    total?: number
}

const languages = ref<WeblateLanguage[]>([])
const isLoading = ref(true)
const hasError = ref(false)

const sorted = computed(() =>
    [...languages.value].sort((a, b) => {
        const pa = a.translated_percent ?? 0
        const pb = b.translated_percent ?? 0
        if (pb !== pa) return pb - pa
        return a.name.localeCompare(b.name)
    }),
)

function percentClass(p: number | undefined): string {
    const v = p ?? 0
    if (v >= 95) return 'is-complete'
    if (v >= 70) return 'is-good'
    if (v >= 40) return 'is-partial'
    return 'is-low'
}

function formatPercent(p: number | undefined): string {
    return `${Math.round(p ?? 0)}%`
}

onMounted(async () => {
    try {
        const response = await fetch('/api/weblate-languages.json', {
            headers: { accept: 'application/json' },
        })
        if (!response.ok) {
            hasError.value = true
            return
        }
        const payload = await response.json()
        // Weblate may return either an array or a paginated `{ results: [...] }`
        // envelope depending on the endpoint version.
        const list: WeblateLanguage[] = Array.isArray(payload)
            ? payload
            : Array.isArray(payload?.results)
                ? payload.results
                : []
        languages.value = list
    } catch {
        hasError.value = true
    } finally {
        isLoading.value = false
    }
})
</script>

<template>
    <section class="weblate-languages" aria-label="Available languages">
        <div v-if="isLoading" class="weblate-languages__status">Loading languages…</div>
        <div v-else-if="hasError" class="weblate-languages__status is-error">
            Unable to load the language list right now. Please check
            <a href="https://hosted.weblate.org/projects/soundswitch/#languages" target="_blank"
                rel="noopener">Weblate</a>
            directly.
        </div>
        <template v-else>
            <div class="weblate-languages__summary">
                <strong>{{ sorted.length }}</strong> languages available
            </div>
            <ul class="weblate-languages__grid">
                <li v-for="lang in sorted" :key="lang.code" class="weblate-languages__item"
                    :class="percentClass(lang.translated_percent)">
                    <div class="weblate-languages__name">
                        <span class="weblate-languages__label">{{ lang.name }}</span>
                        <span class="weblate-languages__code">{{ lang.code }}</span>
                    </div>
                    <div class="weblate-languages__bar" :title="`${lang.translated ?? 0} / ${lang.total ?? 0} strings`">
                        <div class="weblate-languages__bar-fill"
                            :style="{ width: `${Math.min(100, Math.max(0, lang.translated_percent ?? 0))}%` }" />
                    </div>
                    <div class="weblate-languages__percent">{{ formatPercent(lang.translated_percent) }}</div>
                </li>
            </ul>
        </template>
    </section>
</template>

<style scoped>
.weblate-languages {
    margin: 1.5rem 0 2rem;
}

.weblate-languages__status {
    padding: 1rem;
    border-radius: 0.5rem;
    background: var(--vp-c-bg-alt, #f6f8fa);
    color: var(--vp-c-text-mute, #5a6878);
    text-align: center;
    font-size: 0.95rem;
}

.weblate-languages__status.is-error {
    background: rgba(244, 67, 54, 0.08);
    color: #b71c1c;
}

.weblate-languages__summary {
    margin-bottom: 0.75rem;
    font-size: 0.9rem;
    color: var(--vp-c-text-mute, #5a6878);
}

.weblate-languages__grid {
    list-style: none;
    margin: 0;
    padding: 0;
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(15rem, 1fr));
    gap: 0.6rem;
}

.weblate-languages__item {
    display: grid;
    grid-template-columns: 1fr auto;
    grid-template-rows: auto auto;
    grid-template-areas:
        'name percent'
        'bar  bar';
    gap: 0.35rem 0.75rem;
    padding: 0.65rem 0.85rem;
    border-radius: 0.5rem;
    background: var(--vp-c-bg-alt, #f6f8fa);
    border: 1px solid var(--vp-c-border, rgba(60, 60, 67, 0.12));
}

.weblate-languages__name {
    grid-area: name;
    display: flex;
    align-items: baseline;
    gap: 0.5rem;
    min-width: 0;
}

.weblate-languages__label {
    font-weight: 600;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.weblate-languages__code {
    font-size: 0.75rem;
    color: var(--vp-c-text-mute, #5a6878);
    font-variant-numeric: tabular-nums;
    text-transform: uppercase;
    letter-spacing: 0.04em;
}

.weblate-languages__percent {
    grid-area: percent;
    font-size: 0.85rem;
    font-weight: 600;
    font-variant-numeric: tabular-nums;
    color: var(--vp-c-text-mute, #5a6878);
}

.weblate-languages__bar {
    grid-area: bar;
    height: 0.35rem;
    border-radius: 999px;
    background: rgba(127, 127, 127, 0.18);
    overflow: hidden;
}

.weblate-languages__bar-fill {
    height: 100%;
    border-radius: inherit;
    background: linear-gradient(90deg, #2196f3, #0d47a1);
    transition: width 0.4s ease;
}

.weblate-languages__item.is-complete .weblate-languages__bar-fill {
    background: linear-gradient(90deg, #43a047, #1b5e20);
}

.weblate-languages__item.is-complete .weblate-languages__percent {
    color: #1b5e20;
}

.weblate-languages__item.is-good .weblate-languages__bar-fill {
    background: linear-gradient(90deg, #66bb6a, #2e7d32);
}

.weblate-languages__item.is-partial .weblate-languages__bar-fill {
    background: linear-gradient(90deg, #ffb74d, #ef6c00);
}

.weblate-languages__item.is-low .weblate-languages__bar-fill {
    background: linear-gradient(90deg, #ef5350, #b71c1c);
}
</style>

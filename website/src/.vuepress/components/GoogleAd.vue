<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref } from 'vue'

const adRef = ref<HTMLElement | null>(null)
let observer: ResizeObserver | null = null
let pushed = false

function pushAd() {
  if (pushed) return
  try {
    const w = window as unknown as Record<string, unknown>
    const adsbygoogle = (w.adsbygoogle = w.adsbygoogle || []) as Array<Record<string, unknown>>
    adsbygoogle.push({})
    pushed = true
  } catch {
    // Silently ignore ad initialization failures so the page remains
    // functional when an ad blocker is present or the script fails to load.
  }
}

onMounted(() => {
  if (!adRef.value) return

  // Google's adsbygoogle.push() throws "No slot size for availableWidth=0"
  // when the <ins> is mounted before its container has been laid out
  // (common with SSR-hydrated pages). Wait until we observe a non-zero
  // width before pushing.
  if (adRef.value.offsetWidth > 0) {
    pushAd()
    return
  }

  if (typeof ResizeObserver !== 'undefined') {
    observer = new ResizeObserver((entries) => {
      for (const entry of entries) {
        if (entry.contentRect.width > 0) {
          pushAd()
          observer?.disconnect()
          observer = null
          break
        }
      }
    })
    observer.observe(adRef.value)
  } else {
    // Fallback: push on next frame.
    requestAnimationFrame(pushAd)
  }
})

onBeforeUnmount(() => {
  observer?.disconnect()
  observer = null
})
</script>

<template>
  <div class="google-ad">
    <ins ref="adRef" class="adsbygoogle" style="display:block" data-ad-client="ca-pub-7284443005140816"
      data-ad-slot="3897699789" data-ad-format="auto" data-full-width-responsive="true"></ins>
  </div>
</template>

<style scoped>
.google-ad {
  margin: 2rem 0;
  min-height: 90px;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>

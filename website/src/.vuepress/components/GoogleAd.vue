<script setup lang="ts">
import { nextTick, onMounted, ref } from 'vue'

type AdSlotPushItem = Record<string, unknown>
type AdSenseWindow = Window & {
  adsbygoogle?: AdSlotPushItem[]
}

const adSlotRef = ref<HTMLElement | null>(null)

const waitForVisibleSlot = async (): Promise<HTMLElement | null> => {
  for (let attempt = 0; attempt < 5; attempt += 1) {
    await nextTick()
    const adSlot = adSlotRef.value
    if (adSlot && adSlot.offsetWidth > 0 && adSlot.offsetHeight > 0) {
      return adSlot
    }

    await new Promise<void>((resolve) => requestAnimationFrame(() => resolve()))
  }

  return adSlotRef.value
}

onMounted(async () => {
  try {
    const adSlot = await waitForVisibleSlot()
    if (!adSlot || adSlot.getAttribute('data-adsbygoogle-status')) {
      return
    }

    const adSenseWindow = window as AdSenseWindow
    ;(adSenseWindow.adsbygoogle = adSenseWindow.adsbygoogle || []).push({})
  } catch (error) {
    console.error('[GoogleAd] Failed to initialize AdSense slot', error)
  }
})
</script>

<template>
  <div class="google-ad">
    <ins ref="adSlotRef" class="adsbygoogle" style="display:block" data-ad-client="ca-pub-7284443005140816" data-ad-slot="3897699789"
      data-ad-format="auto" data-full-width-responsive="true"></ins>
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

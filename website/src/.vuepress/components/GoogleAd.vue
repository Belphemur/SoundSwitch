<script setup lang="ts">
import { onMounted } from 'vue'

type AdSlotPushItem = Record<string, unknown>
type AdSenseWindow = Window & {
  adsbygoogle?: AdSlotPushItem[]
}

onMounted(() => {
  try {
    const adSenseWindow = window as AdSenseWindow
    ;(adSenseWindow.adsbygoogle = adSenseWindow.adsbygoogle || []).push({})
  } catch {
    // Silently ignore ad initialization failures so the page remains
    // functional when an ad blocker is present or the script fails to load.
  }
})
</script>

<template>
  <div class="google-ad">
    <ins class="adsbygoogle" style="display:block" data-ad-client="ca-pub-7284443005140816" data-ad-slot="3897699789"
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

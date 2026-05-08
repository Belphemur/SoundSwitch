<script setup lang="ts">
import { onMounted } from 'vue'

onMounted(() => {
  try {
    const w = window as unknown as Record<string, unknown>
    const adsbygoogle = (w.adsbygoogle = w.adsbygoogle || []) as Array<Record<string, unknown>>
    adsbygoogle.push({})
  } catch {
    // Silently ignore ad initialization failures so the page remains functional
    // when ad blockers are present or the script fails to load.
  }
})
</script>

<template>
  <div class="google-ad">
    <ins
      class="adsbygoogle"
      style="display:block"
      data-ad-client="ca-pub-7284443005140816"
      data-ad-slot="8613510554"
      data-ad-format="auto"
      data-full-width-responsive="true"
    ></ins>
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

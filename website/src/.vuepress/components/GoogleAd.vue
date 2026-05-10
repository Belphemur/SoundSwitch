<script setup lang="ts">
import { nextTick, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'

type AdSlotPushItem = Record<string, unknown>
type AdSenseWindow = Window & {
  adsbygoogle?: AdSlotPushItem[]
}

const props = withDefaults(defineProps<{
  adClient?: string
  adSlot?: string
  adFormat?: string
  fullWidthResponsive?: boolean
}>(), {
  adClient: 'ca-pub-7284443005140816',
  adSlot: '3897699789',
  adFormat: 'auto',
  fullWidthResponsive: true,
})

const route = useRoute()
const adKey = ref(0)
const adSlotRef = ref<HTMLElement | null>(null)

const MAX_PUSH_RETRIES = 10
const PUSH_RETRY_DELAY_MS = 200

const pushAd = (attempt = 1) => {
  try {
    const adSlot = adSlotRef.value
    if (!adSlot || adSlot.getAttribute('data-adsbygoogle-status')) {
      return
    }

    if (adSlot.offsetWidth <= 0) {
      if (attempt <= MAX_PUSH_RETRIES) {
        setTimeout(() => pushAd(attempt + 1), PUSH_RETRY_DELAY_MS)
      }
      return
    }

    const adSenseWindow = window as AdSenseWindow
      ; (adSenseWindow.adsbygoogle = adSenseWindow.adsbygoogle || []).push({})
  } catch (error) {
    console.error('[GoogleAd] Failed to initialize AdSense slot', error)
  }
}

onMounted(() => nextTick(pushAd))

watch(() => route.path, () => {
  adKey.value += 1
  nextTick(pushAd)
})
</script>

<template>
  <div class="google-ad">
    <ins :key="adKey" ref="adSlotRef" class="adsbygoogle" :data-ad-client="props.adClient" :data-ad-slot="props.adSlot"
      :data-ad-format="props.adFormat" :data-full-width-responsive="props.fullWidthResponsive ? 'true' : 'false'"></ins>
  </div>
</template>

<style scoped>
.google-ad {
  position: relative;
  z-index: 1;
  max-width: 960px;
  margin: 1.5rem auto 0;
  padding: 0 2rem;
  min-height: 90px;
  min-width: 100%;
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.adsbygoogle {
  display: block;
  min-width: 100%;
  min-height: 90px;
  width: 100%;
}
</style>

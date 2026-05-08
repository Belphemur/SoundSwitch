import { defineClientConfig } from 'vuepress/client'
import HomeHero from './components/HomeHero.vue'
import GoogleAd from './components/GoogleAd.vue'
import DownloadCount from './components/DownloadCount.vue'

export default defineClientConfig({
  enhance({ app }) {
    app.component('HomeHero', HomeHero)
    app.component('GoogleAd', GoogleAd)
    app.component('DownloadCount', DownloadCount)
  },
})
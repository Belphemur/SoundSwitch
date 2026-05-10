import { defineClientConfig } from "vuepress/client";
import HomeHero from "./components/HomeHero.vue";
import DownloadCount from "./components/DownloadCount.vue";
import WeblateLanguages from "./components/WeblateLanguages.vue";

export default defineClientConfig({
  enhance({ app }) {
    app.component("HomeHero", HomeHero);
    app.component("DownloadCount", DownloadCount);
    app.component("WeblateLanguages", WeblateLanguages);
  },
});

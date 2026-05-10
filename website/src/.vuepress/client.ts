import { defineClientConfig } from "vuepress/client";
import HomeHero from "./components/HomeHero.vue";
import GoogleAd from "./components/GoogleAd.vue";
import DownloadCount from "./components/DownloadCount.vue";
import WeblateLanguages from "./components/WeblateLanguages.vue";
import CustomLayout from "./layouts/Layout.vue";
import HomeLayout from "./layouts/Home.vue";

export default defineClientConfig({
  layouts: {
    Layout: CustomLayout,
    Home: HomeLayout,
  },
  enhance({ app }) {
    app.component("HomeHero", HomeHero);
    app.component("GoogleAd", GoogleAd);
    app.component("DownloadCount", DownloadCount);
    app.component("WeblateLanguages", WeblateLanguages);
  },
});

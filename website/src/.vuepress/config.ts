import { defineUserConfig } from "vuepress";
import { viteBundler } from "@vuepress/bundler-vite";
import { hopeTheme } from "vuepress-theme-hope";

export default defineUserConfig({
  base: "/",
  title: "SoundSwitch",
  head: [
    [
      "script",
      {
        async: true,
        src: "https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-7284443005140816",
        crossorigin: "anonymous",
      },
    ],
    [
      "script",
      {},
      `
      var _mtm = window._mtm = window._mtm || [];
      var _paq = window._paq = window._paq || [];
      (window._mtm || []).push({'mtm.startTime': (new Date().getTime()), 'event': 'mtm.Start'});
      var d=document, g=d.createElement('script'), s=d.getElementsByTagName('script')[0];
      g.type='text/javascript'; g.async=true; g.src='https://unami.newmail.app/js/container_dmLA4CEF.js'; s.parentNode.insertBefore(g,s);
      `,
    ],
  ],
  bundler: viteBundler(),
  theme: hopeTheme({
    logo: "/logo.svg",
    navbar: [
      { text: "Home", link: "/" },
      { text: "Getting Started", link: "/getting-started.md" },
      {
        text: "Configuration",
        children: [
          { text: "General", link: "/configuration/general.md" },
          { text: "Playback", link: "/configuration/playback.md" },
          { text: "Recording", link: "/configuration/recording.md" },
          { text: "Notifications", link: "/configuration/notifications.md" },
        ],
      },
      {
        text: "Usage",
        children: [
          { text: "Hotkeys", link: "/usage/hotkeys.md" },
          { text: "Profiles", link: "/usage/profiles.md" },
          { text: "App Rules", link: "/usage/app-rules.md" },
          { text: "Communications", link: "/usage/communications.md" },
          { text: "CLI", link: "/usage/cli.md" },
        ],
      },
      {
        text: "Advanced",
        children: [
          { text: "Auto Updater", link: "/advanced/auto-updater.md" },
          { text: "Multi Language", link: "/advanced/multi-language.md" },
          { text: "Troubleshooting", link: "/advanced/troubleshooting.md" },
        ],
      },
    ],
    sidebar: {
      "/configuration/": [
        {
          text: "Configuration",
          children: [
            { text: "Overview", link: "/configuration/README.md" },
            { text: "General", link: "/configuration/general.md" },
            { text: "Playback", link: "/configuration/playback.md" },
            { text: "Recording", link: "/configuration/recording.md" },
            { text: "Notifications", link: "/configuration/notifications.md" },
          ],
        },
      ],
      "/usage/": [
        {
          text: "Usage",
          children: [
            { text: "Overview", link: "/usage/README.md" },
            { text: "Hotkeys", link: "/usage/hotkeys.md" },
            { text: "Profiles", link: "/usage/profiles.md" },
            { text: "App Rules", link: "/usage/app-rules.md" },
            { text: "Communications", link: "/usage/communications.md" },
            { text: "CLI", link: "/usage/cli.md" },
          ],
        },
      ],
      "/advanced/": [
        {
          text: "Advanced",
          children: [
            { text: "Overview", link: "/advanced/README.md" },
            { text: "Auto Updater", link: "/advanced/auto-updater.md" },
            { text: "Multi Language", link: "/advanced/multi-language.md" },
            { text: "Troubleshooting", link: "/advanced/troubleshooting.md" },
          ],
        },
      ],
      "/": [
        {
          text: "Introduction",
          children: [{ text: "Getting Started", link: "/getting-started.md" }],
        },
      ],
    },
    editLink: false,
    lastUpdated: true,
    contributors: false,
    footer:
      'Released under the <a href="https://github.com/Belphemur/SoundSwitch/blob/master/LICENSE" target="_blank" rel="noopener noreferrer">GNU GPL v2 License</a>. ' +
      'Copyright © 2014–present <a href="https://github.com/Belphemur" target="_blank" rel="noopener noreferrer">Antoine Aflalo</a> & contributors.',
    displayFooter: true,
    plugins: {
      icon: {
        assets: "fontawesome-with-brands",
      },
    },
  }),
});

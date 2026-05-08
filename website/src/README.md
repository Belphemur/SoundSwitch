---
home: true
title: Home
heroText: false
tagline: false
---

<HomeHero />

<GoogleAd />

## Features

<div class="features-grid">

<div class="feature-card">
<h3>🎧 Instant Switching</h3>
<p>Toggle between playback and recording devices with a customizable global hotkey — no more digging through Windows sound settings.</p>
</div>

<div class="feature-card">
<h3>⚡ Lightweight</h3>
<p>Runs quietly in the system tray with minimal resource usage. Start it once and forget it’s there until you need it.</p>
</div>

<div class="feature-card">
<h3>🔧 Per-App Rules</h3>
<p>Automatically switch audio devices when specific applications start or gain focus. Perfect for streamers and power users.</p>
</div>

<div class="feature-card">
<h3>📋 Profiles</h3>
<p>Save and recall complete audio configurations — playback device, recording device, and volume levels — all in one click.</p>
</div>

<div class="feature-card">
<h3>🔔 Smart Notifications</h3>
<p>Choose between Windows toast, banner, or sound notifications so you always know when a switch happens.</p>
</div>

<div class="feature-card">
<h3>🎮 Streamer Friendly</h3>
<p>Seamlessly switch between headphones, speakers, and streaming mixers without interrupting your broadcast.</p>
</div>

</div>

<style scoped>
.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 1.5rem;
  margin-top: 2rem;
}

.feature-card {
  background: linear-gradient(180deg, #ffffff 0%, #f8fbff 100%);
  border: 1px solid rgba(33, 150, 243, 0.12);
  border-radius: 16px;
  padding: 1.75rem;
  transition: all 0.3s cubic-bezier(0.22, 1, 0.36, 1);
  box-shadow: 0 2px 12px rgba(33, 150, 243, 0.06);
}

.feature-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 32px rgba(33, 150, 243, 0.14);
  border-color: rgba(33, 150, 243, 0.25);
}

.feature-card h3 {
  font-family: 'Space Grotesk', sans-serif;
  margin: 0 0 0.75rem;
  font-size: 1.15rem;
  color: #1565c0;
}

.feature-card p {
  margin: 0;
  color: #546e7a;
  line-height: 1.6;
}
</style>

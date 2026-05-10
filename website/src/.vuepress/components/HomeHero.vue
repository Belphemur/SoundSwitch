<script setup lang="ts">
import { ref, onMounted } from 'vue'

interface GitHubAsset {
  name: string
  browser_download_url: string
}

interface GitHubRelease {
  tag_name: string
  prerelease: boolean
  assets: GitHubAsset[]
}

declare global {
  interface Window {
    _paq?: unknown[]
    _mtm?: unknown[]
  }
}

const downloadUrl = ref('https://github.com/Belphemur/SoundSwitch/releases/latest')
const downloadText = ref('Download')
const isLoading = ref(true)
const showDonateModal = ref(false)
const showThanksModal = ref(false)
const donatedAmount = ref('')
// Tracks whether we're running on the client. Used to avoid SSR/CSR
// hydration mismatches caused by <Teleport> (which renders inline during
// SSR but moves to <body> on the client) and by client-only state derived
// from `window.location`.
const isMounted = ref(false)

const donateAmounts = [5, 10, 25, 50]

function buildDonateUrl(amount: number): string {
  const base = 'https://www.paypal.com/donate'
  // Fallback for uuidv4
  const uuid = typeof crypto !== 'undefined' && crypto.randomUUID
    ? crypto.randomUUID()
    : Math.random().toString(36).substring(2) + Date.now().toString(36)
  const custom = 'SoundSwitch-' + uuid

  const params = new URLSearchParams({
    business: 'W4ZWXP7LSL29C',
    item_name: 'SoundSwitch Donation',
    currency_code: 'USD',
    notify_url: 'https://www.aaflalo.me/?PAYPALIPN=1',
    custom: custom,
    return: window.location.href.split('#')[0].split('?')[0] + '#thanks',
    bn: 'SeamlessDonations_SP',
    amount: String(amount),
  })
  return `${base}?${params.toString()}`
}

function trackDownload() {
  window._paq?.push(['trackEvent', 'download', 'Click', downloadText.value.replace('Download ', '')])
}

function openDonateModal() {
  window._paq?.push(['trackEvent', 'donate', 'Click'])
  showDonateModal.value = true
}

function closeDonateModal() {
  window._paq?.push(['trackEvent', 'donate', 'Close'])
  showDonateModal.value = false
}

function closeThanksModal() {
  showThanksModal.value = false
}

function trackDonationAmount(amount: number) {
  window._mtm?.push({
    event: 'paypal-donation',
    donation: {
      amount: amount,
      name: String(amount) + '$',
    },
  })
}

onMounted(async () => {
  isMounted.value = true
  try {
    const response = await fetch('https://api.github.com/repos/Belphemur/SoundSwitch/releases')
    if (!response.ok) {
      throw new Error(`GitHub API responded with ${response.status}`)
    }

    const releases: GitHubRelease[] = await response.json()
    const stableRelease = releases.find((r) => r.prerelease === false)

    if (!stableRelease) {
      throw new Error('No stable release found')
    }

    const exeAsset = stableRelease.assets.find((a) => a.name.endsWith('.exe'))

    if (exeAsset) {
      downloadUrl.value = exeAsset.browser_download_url
    }
    downloadText.value = `Download ${stableRelease.tag_name}`
  } catch {
    // Fallback already set in refs
  } finally {
    isLoading.value = false
  }

  const url = window.location.href
  const pageSearchParams = new URLSearchParams(window.location.search)
  if (url.includes('#thanks')) {
    let searchParamsString = window.location.search
    if (!searchParamsString && url.includes('?')) {
      searchParamsString = url.substring(url.indexOf('?'))
    }
    const urlParams = new URLSearchParams(searchParamsString)
    const amount = parseFloat(urlParams.get('amt') || '0')
    const currency = urlParams.get('cc')

    if (amount > 0) {
      donatedAmount.value = amount.toFixed(2) + ' ' + (currency ?? 'USD')
      window._mtm?.push({
        event: 'paypal-donation-thanks',
        donation: { amount: amount, currency: currency }
      })
    }
    showThanksModal.value = true
  } else if (url.includes('#donate') || pageSearchParams.has('donate')) {
    openDonateModal()
  }
})
</script>

<template>
  <div class="home-wrapper">
    <div class="home-hero">
      <div class="hero-background">
        <div class="gradient-mesh"></div>
        <div class="noise-overlay"></div>
      </div>

      <div class="hero-content">
        <h1 class="hero-title">
          <span class="title-line">SoundSwitch</span>
        </h1>
        <p class="hero-tagline">
          Switch your audio devices with a single keystroke
        </p>

        <div class="hero-actions">
          <a :href="downloadUrl" class="action-button action-button--primary" :class="{ 'is-loading': isLoading }"
            @click="trackDownload">
            <span class="button-icon">
              <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
                <polyline points="7 10 12 15 17 10" />
                <line x1="12" y1="15" x2="12" y2="3" />
              </svg>
            </span>
            <span class="button-text">{{ downloadText }}</span>
          </a>

          <button class="action-button action-button--secondary" @click="openDonateModal">
            <span class="button-icon">
              <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2">
                <path
                  d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
              </svg>
            </span>
            <span class="button-text">Donate</span>
          </button>
        </div>
      </div>
    </div>

    <div class="features-section">
      <div class="features-grid">
        <div class="feature-card">
          <VPIcon icon="fa-solid:headphones" class="feature-icon" />
          <h3>Instant Switching</h3>
          <p>Toggle between playback and recording devices with a customizable global hotkey.</p>
        </div>
        <div class="feature-card">
          <VPIcon icon="fa-solid:feather" class="feature-icon" />
          <h3>Lightweight</h3>
          <p>Runs quietly in the system tray with minimal resource usage.</p>
        </div>
        <div class="feature-card">
          <VPIcon icon="fa-solid:gears" class="feature-icon" />
          <h3>Per-App Rules</h3>
          <p>Automatically switch devices when specific applications start or gain focus.</p>
        </div>
        <div class="feature-card">
          <VPIcon icon="fa-solid:clone" class="feature-icon" />
          <h3>Profiles</h3>
          <p>Save and recall complete audio configurations in one click.</p>
        </div>
        <div class="feature-card">
          <VPIcon icon="fa-solid:bell" class="feature-icon" />
          <h3>Smart Notifications</h3>
          <p>Choose between toast, banner, or sound notifications for switches.</p>
        </div>
        <div class="feature-card">
          <VPIcon icon="fa-solid:video" class="feature-icon" />
          <h3>Streamer Friendly</h3>
          <p>Switch devices without interrupting your broadcast.</p>
        </div>
      </div>
    </div>

    <Teleport v-if="isMounted" to="body">
      <Transition name="modal">
        <div v-if="showDonateModal" class="donate-modal-overlay" @click.self="closeDonateModal">
          <div class="donate-modal">
            <button class="modal-close" @click="closeDonateModal" aria-label="Close">
              <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="18" y1="6" x2="6" y2="18" />
                <line x1="6" y1="6" x2="18" y2="18" />
              </svg>
            </button>
            <h3 class="modal-title">
              <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="#e53935" stroke-width="2"
                style="vertical-align: middle; margin-right: 0.4rem;">
                <path
                  d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
              </svg>
              Donation
            </h3>
            <p class="modal-desc">Thank you very much for considering donating to SoundSwitch!<br>Once you choose the
              amount you'll be redirected to PayPal.</p>
            <div class="donate-amounts">
              <a v-for="amount in donateAmounts" :key="amount" :href="buildDonateUrl(amount)" target="_blank"
                rel="noopener noreferrer" class="amount-button" @click="trackDonationAmount(amount)">
                ${{ amount }}
              </a>
            </div>
            <a href="https://www.paypal.com/donate/?business=W4ZWXP7LSL29C&item_name=SoundSwitch+Donation&currency_code=USD"
              target="_blank" rel="noopener noreferrer" class="amount-button amount-button--other">
              Other $ Amount
            </a>
          </div>
        </div>
      </Transition>
    </Teleport>

    <Teleport v-if="isMounted" to="body">
      <Transition name="modal">
        <div v-if="showThanksModal" class="donate-modal-overlay" @click.self="closeThanksModal">
          <div class="donate-modal">
            <button class="modal-close" @click="closeThanksModal" aria-label="Close">
              <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="18" y1="6" x2="6" y2="18" />
                <line x1="6" y1="6" x2="18" y2="18" />
              </svg>
            </button>
            <h3 class="modal-title">Thank You!</h3>
            <p class="modal-desc">Thank you very much for your donation to SoundSwitch!</p>
            <p v-if="donatedAmount" class="thanks-amount">{{ donatedAmount }}</p>
            <button class="action-button action-button--primary" @click="closeThanksModal">Close</button>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Space+Grotesk:wght@500;700&family=Outfit:wght@400;500&display=swap');

.home-wrapper {
  position: relative;
  margin: -2rem -2rem 0;
  padding-bottom: 2rem;
  /* Background gradient is applied to .vp-project-home (in styles/index.scss)
     so the entire home page shares the same blue tint. */
  background: transparent;
}

.home-hero {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 3rem 2rem 1rem;
}

.hero-background {
  display: none;
}

.hero-content {
  position: relative;
  z-index: 1;
  text-align: center;
  max-width: 720px;
}

.hero-title {
  margin: 0 0 0.5rem;
  font-family: 'Space Grotesk', sans-serif;
  font-size: clamp(2rem, 5vw, 3.5rem);
  font-weight: 700;
  line-height: 1.1;
  letter-spacing: -0.02em;
  color: #1a1a2e;
}

.title-line {
  display: inline-block;
  background: linear-gradient(135deg, #1565c0 0%, #2196f3 50%, #42a5f5 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.hero-tagline {
  margin: 0 0 1.5rem;
  font-family: 'Outfit', sans-serif;
  font-size: clamp(1.125rem, 2.5vw, 1.5rem);
  font-weight: 400;
  line-height: 1.5;
  color: #546e7a;
}

[data-theme="dark"] .hero-tagline {
  color: #cfd8dc;
}

.hero-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  justify-content: center;
}

.action-button {
  display: inline-flex;
  align-items: center;
  gap: 0.6rem;
  padding: 0.85rem 1.75rem;
  border-radius: 12px;
  font-family: 'Outfit', sans-serif;
  font-size: 1rem;
  font-weight: 500;
  text-decoration: none;
  border: none;
  cursor: pointer;
  transition: box-shadow 0.2s ease;
  position: relative;
  overflow: hidden;
}

.action-button--primary {
  background: linear-gradient(135deg, #1976d2 0%, #2196f3 100%);
  color: #fff;
  box-shadow: 0 4px 20px rgba(33, 150, 243, 0.35), 0 1px 3px rgba(0, 0, 0, 0.1);
}

.action-button--primary:hover {
  box-shadow: 0 6px 24px rgba(33, 150, 243, 0.45), 0 2px 6px rgba(0, 0, 0, 0.12);
}

.action-button--secondary {
  background: linear-gradient(135deg, #e53935 0%, #ef5350 100%);
  color: #fff;
  border: none;
  box-shadow: 0 4px 20px rgba(229, 57, 53, 0.3), 0 1px 3px rgba(0, 0, 0, 0.1);
}

.action-button--secondary:hover {
  box-shadow: 0 6px 24px rgba(229, 57, 53, 0.4), 0 2px 6px rgba(0, 0, 0, 0.12);
}

.action-button.is-loading {
  opacity: 0.8;
  pointer-events: none;
}

.button-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.features-section {
  position: relative;
  z-index: 1;
  max-width: 960px;
  margin: 0 auto;
  padding: 0 2rem 2rem;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 1rem;
}

.feature-card {
  background: rgba(255, 255, 255, 0.75);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(33, 150, 243, 0.1);
  border-radius: 12px;
  padding: 1.5rem;
  text-align: center;
  transition: box-shadow 0.2s ease, border-color 0.2s ease;
}

.feature-card:hover {
  box-shadow: 0 4px 16px rgba(33, 150, 243, 0.12);
  border-color: rgba(33, 150, 243, 0.22);
}

.feature-icon {
  display: block;
  text-align: center;
  font-size: 2.5rem;
  color: #2196f3;
  margin: 0 auto 0.35rem;
}

.feature-card h3 {
  margin: 0 0 0.4rem;
  padding-top: 0 !important;
  font-size: 1.05rem;
  font-weight: 600;
  color: #1565c0;
  border-bottom: none !important;
}

.feature-card p {
  margin: 0;
  color: #546e7a;
  line-height: 1.5;
  font-size: 0.88rem;
}

/* Modal styles */
.donate-modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(13, 27, 42, 0.55);
  backdrop-filter: blur(6px);
  padding: 1rem;
}

.donate-modal {
  position: relative;
  background: #fff;
  border-radius: 20px;
  padding: 2.5rem 2rem;
  max-width: 400px;
  width: 100%;
  text-align: center;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.2);
}

.modal-close {
  position: absolute;
  top: 1rem;
  right: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border: none;
  background: rgba(0, 0, 0, 0.04);
  color: #546e7a;
  cursor: pointer;
  transition: all 0.2s ease;
}

.modal-close:hover {
  background: rgba(0, 0, 0, 0.08);
  color: #263238;
}

.modal-title {
  font-family: 'Space Grotesk', sans-serif;
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0 0 0.5rem;
  color: #c62828;
}

.modal-desc {
  font-family: 'Outfit', sans-serif;
  font-size: 0.95rem;
  color: #78909c;
  margin: 0 0 1.25rem;
  line-height: 1.5;
}

.donate-amounts {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}

.amount-button {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0.75rem 1rem;
  border-radius: 10px;
  background: linear-gradient(135deg, #e53935 0%, #ef5350 100%);
  color: #fff;
  font-family: 'Space Grotesk', sans-serif;
  font-size: 1.1rem;
  font-weight: 700;
  text-decoration: none;
  transition: box-shadow 0.2s ease;
  box-shadow: 0 2px 8px rgba(229, 57, 53, 0.25);
}

.amount-button:hover {
  box-shadow: 0 4px 14px rgba(229, 57, 53, 0.35);
}

.amount-button--other {
  display: inline-flex;
  grid-column: 1 / -1;
  justify-self: center;
  background: #fff;
  color: #e53935;
  border: 2px solid rgba(229, 57, 53, 0.3);
  box-shadow: none;
  font-weight: 600;
  font-size: 0.9rem;
  padding: 0.5rem 1.5rem;
  border-radius: 8px;
}

.amount-button--other:hover {
  background: rgba(229, 57, 53, 0.04);
  border-color: rgba(229, 57, 53, 0.5);
  box-shadow: 0 4px 12px rgba(229, 57, 53, 0.12);
}

.thanks-amount {
  font-family: 'Space Grotesk', sans-serif;
  font-size: 1.25rem;
  font-weight: 700;
  color: #1976d2;
  margin: 0 0 1.5rem;
}

/* Vue transition classes */
.modal-enter-active,
.modal-leave-active {
  transition: all 0.35s cubic-bezier(0.22, 1, 0.36, 1);
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .donate-modal,
.modal-leave-to .donate-modal {
  transform: scale(0.92) translateY(10px);
}

.modal-enter-active .donate-modal,
.modal-leave-active .donate-modal {
  transition: all 0.35s cubic-bezier(0.22, 1, 0.36, 1);
}

.modal-enter-to .donate-modal,
.modal-leave-from .donate-modal {
  transform: scale(1) translateY(0);
}
</style>

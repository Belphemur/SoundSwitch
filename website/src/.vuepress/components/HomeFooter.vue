<template>
  <footer class="home-footer">
    <div class="home-footer__inner">
      <div class="home-footer__links">
        <a
          href="https://github.com/Belphemur/SoundSwitch"
          target="_blank"
          rel="noopener noreferrer"
          class="home-footer__link"
          aria-label="GitHub repository"
        >
          <svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor" aria-hidden="true">
            <path d="M12 2C6.477 2 2 6.484 2 12.021c0 4.428 2.865 8.184 6.839 9.504.5.092.682-.217.682-.483 0-.237-.009-.868-.013-1.703-2.782.605-3.369-1.342-3.369-1.342-.454-1.154-1.11-1.462-1.11-1.462-.908-.62.069-.607.069-.607 1.003.07 1.531 1.031 1.531 1.031.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0 1 12 6.844a9.59 9.59 0 0 1 2.504.337c1.909-1.296 2.747-1.026 2.747-1.026.546 1.378.202 2.397.1 2.65.64.7 1.028 1.595 1.028 2.688 0 3.848-2.338 4.695-4.566 4.944.359.309.678.919.678 1.852 0 1.336-.012 2.415-.012 2.743 0 .268.18.579.688.481C19.138 20.2 22 16.447 22 12.021 22 6.484 17.523 2 12 2z"/>
          </svg>
          <span>GitHub</span>
        </a>

        <a
          href="https://github.com/Belphemur/SoundSwitch/releases/latest"
          target="_blank"
          rel="noopener noreferrer"
          class="home-footer__link"
        >
          <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/>
            <polyline points="7 10 12 15 17 10"/>
            <line x1="12" y1="15" x2="12" y2="3"/>
          </svg>
          <span>Releases</span>
        </a>

        <a
          href="https://github.com/Belphemur/SoundSwitch/blob/master/LICENSE"
          target="_blank"
          rel="noopener noreferrer"
          class="home-footer__link"
        >
          <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
            <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
          </svg>
          <span>License</span>
        </a>

        <a
          href="https://github.com/Belphemur/SoundSwitch/issues"
          target="_blank"
          rel="noopener noreferrer"
          class="home-footer__link"
        >
          <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
            <circle cx="12" cy="12" r="10"/>
            <line x1="12" y1="8" x2="12" y2="12"/>
            <line x1="12" y1="16" x2="12.01" y2="16"/>
          </svg>
          <span>Issues</span>
        </a>
      </div>

      <p class="home-footer__copy">
        Released under the
        <a
          href="https://github.com/Belphemur/SoundSwitch/blob/master/LICENSE"
          target="_blank"
          rel="noopener noreferrer"
        >GNU GPL v2 License</a>.
        &copy; {{ new Date().getFullYear() }}
        <a
          href="https://github.com/Belphemur"
          target="_blank"
          rel="noopener noreferrer"
        >Antoine Aflalo</a>
        &amp; contributors.
      </p>
    </div>
  </footer>
</template>

<style scoped>
.home-footer {
  border-top: 1px solid rgba(33, 150, 243, 0.12);
  margin: 0 -2rem;
  padding: 2rem 2rem 1.5rem;
  background: rgba(255, 255, 255, 0.6);
  backdrop-filter: blur(8px);
}

.home-footer__inner {
  max-width: 960px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.home-footer__links {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 1.5rem;
  justify-content: center;
}

.home-footer__link {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.9rem;
  font-weight: 500;
  color: #546e7a;
  text-decoration: none;
  transition: color 0.2s ease;
}

.home-footer__link:hover {
  color: #1976d2;
}

.home-footer__copy {
  margin: 0;
  font-size: 0.82rem;
  color: #90a4ae;
  text-align: center;
  line-height: 1.6;
}

.home-footer__copy a {
  color: #78909c;
  text-decoration: none;
  transition: color 0.2s ease;
}

.home-footer__copy a:hover {
  color: #1976d2;
}
</style>

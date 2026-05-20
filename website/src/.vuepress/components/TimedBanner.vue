<script setup lang="ts">
import { computed } from "vue";

const props = withDefaults(
  defineProps<{
    title: string;
    description: string;
    linkText: string;
    linkHref: string;
    expiresAt: string;
  }>(),
  {
    expiresAt: "2026-06-20T00:00:00Z",
  },
);

const isVisible = computed(() => Date.now() < new Date(props.expiresAt).getTime());
</script>

<template>
  <div v-if="isVisible" class="timed-banner" role="status" aria-live="polite">
    <strong>{{ title }}</strong>
    <span>{{ description }}</span>
    <a :href="linkHref">{{ linkText }}</a>
  </div>
</template>

<style scoped>
.timed-banner {
  margin: 1rem 0 1.25rem;
  padding: 0.8rem 1rem;
  border-radius: 0.5rem;
  border: 1px solid #1976d2;
  background: #e3f2fd;
  color: #0d47a1;
  font-size: 0.95rem;
  line-height: 1.4;
}

.timed-banner strong {
  margin-right: 0.35rem;
}

.timed-banner span {
  margin-right: 0.35rem;
}

.timed-banner a {
  color: #0d47a1;
  text-decoration: underline;
  font-weight: 600;
}

[data-theme="dark"] .timed-banner {
  border-color: #64b5f6;
  background: rgba(25, 118, 210, 0.2);
  color: #bbdefb;
}

[data-theme="dark"] .timed-banner a {
  color: #bbdefb;
}
</style>

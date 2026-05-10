<script setup lang="ts">
import { Layout } from "vuepress-theme-hope/client";
import GoogleAd from "../components/GoogleAd.vue";
</script>

<template>
    <Layout>
        <template #contentBefore>
            <GoogleAd />
        </template>
    </Layout>
</template>

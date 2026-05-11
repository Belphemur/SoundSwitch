import { copyFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const distDir = path.resolve(__dirname, "../src/.vuepress/dist");
const sourcePath = path.join(distDir, "llms.txt");
const aliasPath = path.join(distDir, "llm.txt");

await copyFile(sourcePath, aliasPath);
console.log(`[sync-llm-alias] copied ${sourcePath} -> ${aliasPath}`);

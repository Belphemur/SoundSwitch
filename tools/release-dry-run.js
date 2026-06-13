import { appendFileSync, readFileSync } from 'fs';
import semanticRelease from 'semantic-release';

let currentBranch = process.env.GITHUB_HEAD_REF || process.env.GITHUB_REF_NAME || 'master';
// For PRs, GITHUB_REF_NAME is like '123/merge' so we prefer GITHUB_HEAD_REF
if (currentBranch.includes('/merge')) {
  currentBranch = process.env.GITHUB_HEAD_REF || 'master';
}

const branches = ['master', 'dev', 'beta'];
if (!branches.includes(currentBranch)) {
  branches.push(currentBranch);
}

// Read package.json to get plugins, but remove github/git plugins to avoid authentication/network errors in dry run
const pkg = JSON.parse(readFileSync('package.json', 'utf8'));
const plugins = (pkg.release.plugins || []).filter(p => {
  const pluginName = Array.isArray(p) ? p[0] : p;
  return pluginName !== '@semantic-release/github' && pluginName !== '@semantic-release/git';
});

const result = await semanticRelease(
  {
    dryRun: true,
    ci: false,
    branches,
    plugins,
    repositoryUrl: `file://${process.cwd()}`
  },
  {
    cwd: process.cwd(),
    env: process.env,
    stdout: process.stdout,
    stderr: process.stderr,
  }
);

if (result && result.nextRelease && result.nextRelease.version) {
  const version = result.nextRelease.version;
  appendFileSync(process.env.GITHUB_OUTPUT, `version=${version}\n`);
  appendFileSync(process.env.GITHUB_OUTPUT, 'released=true\n');
  console.log(`Next release version: ${version}`);
} else {
  appendFileSync(process.env.GITHUB_OUTPUT, 'released=false\n');
  console.log('No release will be created');
}

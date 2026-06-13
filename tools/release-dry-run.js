import { appendFileSync } from 'fs';
import semanticRelease from 'semantic-release';

const result = await semanticRelease(
  { dryRun: true, ci: false },
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

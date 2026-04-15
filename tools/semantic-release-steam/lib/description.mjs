import { access, readFile } from 'node:fs/promises';
import { constants } from 'node:fs';
import { join } from 'node:path';
import { spawn } from 'node:child_process';
import { rewriteAssetLinksForSteam } from './readme.mjs';

export async function buildSteamDescription({ modPath, assetBaseUrl = '' }) {
  const readmePath = join(modPath, 'README.md');

  try {
    await access(readmePath, constants.F_OK);
  } catch {
    return 'No description available.';
  }

  const markdown = rewriteAssetLinksForSteam(await readFile(readmePath, 'utf8'), assetBaseUrl);

  return await new Promise((resolve, reject) => {
    const child = spawn('steamdown', [], { stdio: ['pipe', 'pipe', 'pipe'] });
    let stdout = '';
    let stderr = '';

    child.stdout.on('data', chunk => {
      stdout += chunk;
    });

    child.stderr.on('data', chunk => {
      stderr += chunk;
    });

    child.on('error', reject);
    child.on('close', code => {
      if (code === 0) {
        resolve(stdout || 'No description available.');
        return;
      }

      reject(new Error(stderr || `steamdown exited with code ${code}`));
    });

    child.stdin.end(markdown);
  });
}

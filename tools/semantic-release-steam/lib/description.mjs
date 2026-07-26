import { access, readFile } from 'node:fs/promises';
import { constants } from 'node:fs';
import { delimiter, isAbsolute, join } from 'node:path';
import { spawn } from 'node:child_process';
import { rewriteAssetLinksForSteam } from './readme.mjs';

/// Resolve a command to an absolute path rather than handing a bare name to
/// spawn and letting the OS search PATH implicitly. Same directories, but the
/// lookup is ours and a missing binary fails with a clear message instead of a
/// bare ENOENT.
async function resolveExecutable(name) {
  if (isAbsolute(name)) return name;

  const dirs = (process.env.PATH ?? '').split(delimiter).filter(Boolean);

  for (const dir of dirs) {
    const candidate = join(dir, name);

    try {
      await access(candidate, constants.X_OK);
      return candidate;
    } catch {
      // Not here, keep looking.
    }
  }

  throw new Error(`Could not find "${name}" on PATH.`);
}

export async function buildSteamDescription({ modPath, assetBaseUrl = '' }) {
  const readmePath = join(modPath, 'README.md');

  try {
    await access(readmePath, constants.F_OK);
  } catch {
    return 'No description available.';
  }

  const markdown = rewriteAssetLinksForSteam(await readFile(readmePath, 'utf8'), assetBaseUrl);

  const steamdown = await resolveExecutable('steamdown');

  return await new Promise((resolve, reject) => {
    const child = spawn(steamdown, [], { stdio: ['pipe', 'pipe', 'pipe'] });
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

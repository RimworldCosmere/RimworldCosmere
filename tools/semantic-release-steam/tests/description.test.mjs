import test from 'node:test';
import assert from 'node:assert/strict';
import { chmodSync, mkdirSync, mkdtempSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { buildSteamDescription } from '../lib/description.mjs';

test('returns fallback description when README is missing', async () => {
  const description = await buildSteamDescription({ modPath: 'tools/semantic-release-steam/tests/fixtures/mod' });
  assert.equal(description, 'No description available.');
});

test('buildSteamDescription composes provided header, mod README, and footer before steamdown', async () => {
  const rootPath = mkdtempSync(join(tmpdir(), 'steam-description-'));
  const modPath = join(rootPath, 'CosmereCore');
  const binPath = join(rootPath, 'bin');
  const originalPath = process.env.PATH;

  mkdirSync(modPath, { recursive: true });
  mkdirSync(binPath, { recursive: true });

  writeFileSync(join(modPath, 'README.md'), 'Body\n\n');
  writeFileSync(join(binPath, 'steamdown'), '#!/usr/bin/env node\nprocess.stdin.pipe(process.stdout);\n');
  chmodSync(join(binPath, 'steamdown'), 0o755);

  process.env.PATH = `${binPath}:${originalPath}`;

  try {
    const description = await buildSteamDescription({
      modPath,
      header: 'Header\n\n',
      footer: 'Footer\n',
    });
    assert.equal(description, 'Header\n\nBody\n\nFooter\n');
  } finally {
    process.env.PATH = originalPath;
  }
});

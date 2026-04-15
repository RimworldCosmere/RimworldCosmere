import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdirSync, mkdtempSync, readFileSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { writeCompiledReadme } from '../lib/readme.mjs';

test('writeCompiledReadme composes header template and footer with ZWSP separators', async () => {
  const rootPath = mkdtempSync(join(tmpdir(), 'readme-compile-'));
  const modPath = join(rootPath, 'CosmereScadrial');

  mkdirSync(join(rootPath, '.github', 'assets', 'scadrial'), { recursive: true });
  mkdirSync(modPath, { recursive: true });

  writeFileSync(join(modPath, 'README.template.md'), 'Body\n');
  writeFileSync(join(rootPath, '.github', 'assets', 'scadrial', 'intro.png'), '');
  writeFileSync(join(rootPath, '.github', 'assets', 'scadrial', 'support_us.png'), '');

  await writeCompiledReadme({
    modPath,
    header: '![Introduction](../.github/assets/fallback/intro.png)',
    footer: '![Support](../.github/assets/fallback/support us.png)',
  });

  assert.equal(
    readFileSync(join(modPath, 'README.md'), 'utf8'),
    '![Introduction](../.github/assets/scadrial/intro.png)\nZWSP\nBody\n\nZWSP\n![Support](../.github/assets/scadrial/support_us.png)',
  );
});

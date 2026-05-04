import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, writeFileSync, mkdirSync, existsSync, readFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { stageModContent } from '../lib/stage-content.mjs';

test('stages mod content while excluding .steamignore matches', async () => {
  const root = mkdtempSync(join(tmpdir(), 'steam-stage-'));
  const modDir = join(root, 'CosmereCore');
  mkdirSync(join(modDir, 'About'), { recursive: true });
  writeFileSync(join(modDir, 'About', 'About.xml'), '<ModMetaData />');
  writeFileSync(join(modDir, '.steamignore'), 'Secrets.txt\n');
  writeFileSync(join(modDir, 'Secrets.txt'), 'skip me');
  writeFileSync(join(modDir, 'Keep.txt'), 'keep me');

  const stagedPath = await stageModContent({ modPath: modDir });

  assert.equal(existsSync(join(stagedPath, 'Keep.txt')), true);
  assert.equal(existsSync(join(stagedPath, 'Secrets.txt')), false);
});

test('roshar steamignore keeps CombatExtended compatibility patches publishable', () => {
  const ignorePath = new URL('../../../CosmereRoshar/.steamignore', import.meta.url).pathname;
  const ignoreEntries = readFileSync(ignorePath, 'utf8').split(/\r?\n/).filter(Boolean);

  assert.equal(ignoreEntries.includes('CombatExtended'), false);
});

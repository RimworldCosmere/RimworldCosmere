import test from 'node:test';
import assert from 'node:assert/strict';
import { buildSteamDescription } from '../lib/description.mjs';

test('returns fallback description when README is missing', async () => {
  const description = await buildSteamDescription({ modPath: 'tools/semantic-release-steam/tests/fixtures/mod' });
  assert.equal(description, 'No description available.');
});

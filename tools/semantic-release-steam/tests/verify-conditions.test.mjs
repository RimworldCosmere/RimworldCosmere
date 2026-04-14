import test from 'node:test';
import assert from 'node:assert/strict';
import { verifySteamPublishConfig } from '../lib/config.mjs';

test('returns no publishable mods when beta branch lacks beta workshop id', async () => {
  const result = await verifySteamPublishConfig({
    env: { STEAM_USERNAME: 'builder' },
    branchName: 'beta',
    branchTargets: { main: 'stable', beta: 'beta' },
    mods: [{ name: 'CosmereCore', path: 'CosmereCore', workshopIds: { stable: '1' } }],
    steamConfigPath: '/tmp/config.vdf',
  });

  assert.deepEqual(result, { shouldPublish: false, target: 'beta', mods: [] });
});

test('returns no target for unsupported branch', async () => {
  const result = await verifySteamPublishConfig({
    env: { STEAM_USERNAME: 'builder' },
    branchName: 'alpha',
    branchTargets: { main: 'stable', beta: 'beta' },
    mods: [],
    steamConfigPath: '/tmp/config.vdf',
  });

  assert.deepEqual(result, { shouldPublish: false, target: null, mods: [] });
});

test('skips mods missing workshop id for the current target', async () => {
  const result = await verifySteamPublishConfig({
    env: { STEAM_USERNAME: 'builder' },
    branchName: 'main',
    branchTargets: { main: 'stable', beta: 'beta' },
    mods: [
      { name: 'CosmereCore', path: 'CosmereCore', workshopIds: { stable: '1' } },
      { name: 'CosmereRoshar', path: 'CosmereRoshar', workshopIds: {} },
    ],
    steamConfigPath: '/tmp/config.vdf',
  });

  assert.deepEqual(result, {
    shouldPublish: true,
    target: 'stable',
    mods: [
      { name: 'CosmereCore', path: 'CosmereCore', workshopIds: { stable: '1' } },
    ],
  });
});

import test from 'node:test';
import assert from 'node:assert/strict';
import plugin from '../index.mjs';

test('publish skips unsupported branches', async () => {
  const result = await plugin.publish(
    {
      branchTargets: { main: 'stable', beta: 'beta' },
      mods: [],
    },
    {
      branch: { name: 'alpha' },
      env: {},
      nextRelease: { version: '2.0.0-alpha.1', notes: '' },
      logger: { log() {} },
    },
  );

  assert.equal(result, undefined);
});

test('publish passes configured description parts to the description builder', async () => {
  const calls = [];

  const result = await plugin.publish(
    {
      branchTargets: { beta: 'beta' },
      descriptionHeader: 'Header\n\n',
      descriptionFooter: 'Footer\n',
      mods: [
        {
          name: 'CosmereCore',
          path: 'CosmereCore',
          workshopIds: { beta: '123' },
        },
      ],
    },
    {
      branch: { name: 'beta' },
      cwd: '/repo',
      env: {
        STEAM_USERNAME: 'steam-user',
        STEAM_CONFIG_VDF: '/tmp/config.vdf',
      },
      nextRelease: { version: '2.0.0-beta.1', notes: '' },
      logger: { log() {} },
      buildSteamDescription: async options => {
        calls.push(options);
        return 'built description';
      },
      stageModContent: async () => '/tmp/stage',
      uploadWorkshopItem: async () => undefined,
    },
  );

  assert.equal(result, undefined);
  assert.deepEqual(calls, [
    {
      modPath: '/repo/CosmereCore',
      header: 'Header\n\n',
      footer: 'Footer\n',
    },
  ]);
});

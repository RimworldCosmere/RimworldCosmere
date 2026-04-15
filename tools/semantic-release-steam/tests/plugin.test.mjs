import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import plugin from '../index.mjs';

const releaseWorkflow = readFileSync(new URL('../../../.github/workflows/release.yml', import.meta.url), 'utf8');

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

test('publish compiles README and passes the branch asset base URL to the description builder', async () => {
  const descriptionCalls = [];
  const readmeCalls = [];

  const result = await plugin.publish(
    {
      branchTargets: { beta: 'beta' },
      descriptionHeader: 'Header',
      descriptionFooter: 'Footer',
      assetBaseUrlTemplate: 'https://raw.githubusercontent.com/RimworldCosmere/RimworldCosmere/{branch}',
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
      writeCompiledReadme: async options => {
        readmeCalls.push(options);
      },
      buildSteamDescription: async options => {
        descriptionCalls.push(options);
        return 'built description';
      },
      stageModContent: async () => '/tmp/stage',
      uploadWorkshopItem: async () => undefined,
    },
  );

  assert.equal(result, undefined);
  assert.deepEqual(readmeCalls, [
    {
      modPath: '/repo/CosmereCore',
      header: 'Header',
      footer: 'Footer',
    },
  ]);
  assert.deepEqual(descriptionCalls, [
    {
      modPath: '/repo/CosmereCore',
      assetBaseUrl: 'https://raw.githubusercontent.com/RimworldCosmere/RimworldCosmere/beta',
    },
  ]);
});

test('release workflow pins steamdown cli beta 2 for image rendering', () => {
  assert.match(releaseWorkflow, /npm install -g @steamdown\/cli@1\.0\.0-beta\.2 tsx/);
  assert.match(
    releaseWorkflow,
    /npm install @semantic-release\/git semantic-release-replace-plugin @steamdown\/cli@1\.0\.0-beta\.2 -D/,
  );
});


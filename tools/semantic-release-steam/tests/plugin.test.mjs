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

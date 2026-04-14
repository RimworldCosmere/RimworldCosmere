import test from 'node:test';
import assert from 'node:assert/strict';
import { verifySteamPublishConfig } from '../lib/config.mjs';
import plugin from '../index.mjs';

test('fails when beta branch lacks beta workshop id', async () => {
    await assert.rejects(
        () => verifySteamPublishConfig({
            env: { STEAM_USERNAME: 'builder' },
            branchName: 'beta',
            branchTargets: { main: 'stable', beta: 'beta' },
            mods: [{ name: 'CosmereCore', path: 'CosmereCore', workshopIds: { stable: '1' } }],
            steamConfigPath: '/tmp/config.vdf',
        }),
        /beta workshop id/i,
    );
});

test('returns no target for unsupported branch', async () => {
    const result = await verifySteamPublishConfig({
        env: { STEAM_USERNAME: 'builder' },
        branchName: 'alpha',
        branchTargets: { main: 'stable', beta: 'beta' },
        mods: [],
        steamConfigPath: '/tmp/config.vdf',
    });

    assert.deepEqual(result, { shouldPublish: false, target: null });
});

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

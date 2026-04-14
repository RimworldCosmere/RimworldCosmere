import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { uploadWorkshopItem } from '../lib/steamcmd.mjs';

test('uploadWorkshopItem passes steam config path, captures stdout, and sets timeout', async () => {
  const calls = [];
  const stagePath = mkdtempSync(join(tmpdir(), 'steamcmd-stage-'));

  await uploadWorkshopItem({
    steamCmdPath: '/tmp/steamcmd.sh',
    steamUsername: 'steam-user',
    steamConfigPath: '/tmp/config.vdf',
    stagePath,
    publishedFileId: '123456',
    changenote: '2.0.0-beta.3',
    description: 'Steam description',
    execFileAsync: async (command, args, options) => {
      calls.push({ command, args, options });
      return { stdout: 'steam ok', stderr: '' };
    },
    logger: { log() {} },
  });

  assert.equal(calls.length, 1);
  assert.equal(calls[0].command, '/tmp/steamcmd.sh');
  assert.deepEqual(calls[0].args, [
    '+@ShutdownOnFailedCommand', '1',
    '+@NoPromptForPassword', '1',
    '+login', 'steam-user',
    '+workshop_build_item', join(stagePath, 'workshop.vdf'),
    '+quit',
  ]);
  assert.equal(calls[0].options.timeout, 120000);
  assert.equal(calls[0].options.env.STEAM_CONFIG_VDF, '/tmp/config.vdf');
});

test('uploadWorkshopItem includes stdout and stderr in timeout errors', async () => {
  const stagePath = mkdtempSync(join(tmpdir(), 'steamcmd-stage-'));

  await assert.rejects(
    uploadWorkshopItem({
      steamCmdPath: '/tmp/steamcmd.sh',
      steamUsername: 'builder',
      steamConfigPath: '/tmp/config.vdf',
      stagePath,
      publishedFileId: '123456',
      changenote: '2.0.0-beta.3',
      description: 'Steam description',
      execFileAsync: async () => {
        const error = new Error('timed out');
        error.code = 'ETIMEDOUT';
        error.stdout = 'steam stdout';
        error.stderr = 'steam stderr';
        throw error;
      },
      logger: { log() {} },
    }),
    error => {
      assert.match(error.message, /SteamCMD upload timed out/);
      assert.match(error.message, /steam stdout/);
      assert.match(error.message, /steam stderr/);
      return true;
    },
  );
});

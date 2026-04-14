import { writeFile } from 'node:fs/promises';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
import { homedir } from 'node:os';
import { join } from 'node:path';
import { createWorkshopVdf } from './vdf.mjs';

const defaultExecFileAsync = promisify(execFile);

function expandHomePath(path) {
  if (!path?.startsWith('~/')) {
    return path;
  }

  return join(homedir(), path.slice(2));
}

function formatSteamCmdError(error) {
  const prefix = error?.code === 'ETIMEDOUT'
    ? 'SteamCMD upload timed out'
    : 'SteamCMD upload failed';
  const stdout = error?.stdout ? `\nstdout:\n${error.stdout}` : '';
  const stderr = error?.stderr ? `\nstderr:\n${error.stderr}` : '';

  return new Error(`${prefix}: ${error.message}${stdout}${stderr}`);
}

export async function uploadWorkshopItem({
  steamCmdPath,
  steamUsername,
  steamConfigPath,
  stagePath,
  publishedFileId,
  changenote,
  description,
  appId = '294100',
  execFileAsync = defaultExecFileAsync,
  logger,
}) {
  const vdfPath = join(stagePath, 'workshop.vdf');
  await writeFile(vdfPath, createWorkshopVdf({
    appId,
    publishedFileId,
    contentFolder: stagePath,
    changenote,
    description,
  }));

  try {
    const result = await execFileAsync(expandHomePath(steamCmdPath), [
      '+@ShutdownOnFailedCommand', '1',
      '+@NoPromptForPassword', '1',
      '+login', steamUsername,
      '+workshop_build_item', join(stagePath, 'workshop.vdf'),
      '+quit',
    ], {
      timeout: 120000,
      env: {
        ...process.env,
        STEAM_CONFIG_VDF: steamConfigPath,
      },
    });

    if (logger && result?.stdout) {
      logger.log(result.stdout);
    }

    if (logger && result?.stderr) {
      logger.log(result.stderr);
    }
  } catch (error) {
    throw formatSteamCmdError(error);
  }

  return vdfPath;
}

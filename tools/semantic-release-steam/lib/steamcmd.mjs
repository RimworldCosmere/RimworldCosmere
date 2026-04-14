import { writeFile } from 'node:fs/promises';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
import { homedir } from 'node:os';
import { join } from 'node:path';
import { createWorkshopVdf } from './vdf.mjs';

const execFileAsync = promisify(execFile);

function expandHomePath(path) {
  if (!path?.startsWith('~/')) {
    return path;
  }

  return join(homedir(), path.slice(2));
}

export async function uploadWorkshopItem({
  steamCmdPath,
  steamUsername,
  stagePath,
  publishedFileId,
  changenote,
  description,
  appId = '294100',
}) {
  const vdfPath = join(stagePath, 'workshop.vdf');
  await writeFile(vdfPath, createWorkshopVdf({
    appId,
    publishedFileId,
    contentFolder: stagePath,
    changenote,
    description,
  }));

  await execFileAsync(expandHomePath(steamCmdPath), [
    '+login',
    steamUsername,
    '+workshop_build_item',
    vdfPath,
    '+quit',
  ]);

  return vdfPath;
}

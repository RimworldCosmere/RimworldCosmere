import { access, mkdtemp } from 'node:fs/promises';
import { constants } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';

const execFileAsync = promisify(execFile);

export async function stageModContent({ modPath }) {
  const stagePath = await mkdtemp(join(tmpdir(), 'steam-release-'));
  const ignorePath = join(modPath, '.steamignore');
  const args = ['-av'];

  try {
    await access(ignorePath, constants.F_OK);
    args.push(`--exclude-from=${ignorePath}`, '--exclude=.steamignore');
  } catch {
  }

  args.push(`${modPath}/`, stagePath);
  await execFileAsync('rsync', args);

  return stagePath;
}

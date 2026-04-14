import { mkdtemp, access } from 'node:fs/promises';
import { constants } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { execa } from 'execa';

export async function stageModContent({ modPath }) {
    const stagePath = await mkdtemp(join(tmpdir(), 'steam-release-'));
    const ignorePath = join(modPath, '.steamignore');

    const args = ['-av'];
    try {
        await access(ignorePath, constants.F_OK);
        args.push(`--exclude-from=${ignorePath}`, '--exclude=.steamignore');
    } catch {
        // no .steamignore, proceed without exclude
    }

    args.push(`${modPath}/`, stagePath);
    await execa('rsync', args);
    return stagePath;
}

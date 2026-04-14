import { access, readFile } from 'node:fs/promises';
import { constants } from 'node:fs';
import { join } from 'node:path';
import { execa } from 'execa';

export async function buildSteamDescription({ modPath }) {
    const readmePath = join(modPath, 'README.md');

    try {
        await access(readmePath, constants.F_OK);
    } catch {
        return 'No description available.';
    }

    const markdown = await readFile(readmePath, 'utf8');
    const { stdout } = await execa('steamdown', { input: markdown });
    return stdout || 'No description available.';
}

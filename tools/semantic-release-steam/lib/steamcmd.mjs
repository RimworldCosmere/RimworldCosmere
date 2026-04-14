import { writeFile } from 'node:fs/promises';
import { join } from 'node:path';
import { execa } from 'execa';
import { createWorkshopVdf } from './vdf.mjs';

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

    await execa(steamCmdPath, ['+login', steamUsername, '+workshop_build_item', vdfPath, '+quit']);
    return vdfPath;
}

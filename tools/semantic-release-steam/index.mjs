import { verifySteamPublishConfig } from './lib/config.mjs';
import { buildSteamDescription } from './lib/description.mjs';
import { stageModContent } from './lib/stage-content.mjs';
import { uploadWorkshopItem } from './lib/steamcmd.mjs';

async function verifyConditions(pluginConfig, context) {
    await verifySteamPublishConfig({
        env: context.env,
        branchName: context.branch.name,
        branchTargets: pluginConfig.branchTargets,
        mods: pluginConfig.mods,
        steamConfigPath: context.env.STEAM_CONFIG_VDF,
    });
}

async function publish(pluginConfig, context) {
    const state = await verifySteamPublishConfig({
        env: context.env,
        branchName: context.branch.name,
        branchTargets: pluginConfig.branchTargets,
        mods: pluginConfig.mods,
        steamConfigPath: context.env.STEAM_CONFIG_VDF,
    });

    if (!state.shouldPublish) {
        return undefined;
    }

    for (const mod of pluginConfig.mods) {
        const stagePath = await stageModContent({ modPath: mod.path });
        const description = await buildSteamDescription({ modPath: mod.path });
        const publishedFileId = mod.workshopIds[state.target];

        await uploadWorkshopItem({
            steamCmdPath: context.env.STEAMCMD_PATH ?? '~/steamcmd/steamcmd.sh',
            steamUsername: context.env.STEAM_USERNAME,
            stagePath,
            publishedFileId,
            changenote: context.nextRelease.notes || context.nextRelease.version,
            description,
        });
    }
}

export default { verifyConditions, publish };

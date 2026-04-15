import { resolve } from 'node:path';
import { verifySteamPublishConfig } from './lib/config.mjs';
import { buildSteamDescription } from './lib/description.mjs';
import { writeCompiledReadme } from './lib/readme.mjs';
import { stageModContent } from './lib/stage-content.mjs';
import { uploadWorkshopItem } from './lib/steamcmd.mjs';

export async function verifyConditions(pluginConfig, context) {
  await verifySteamPublishConfig({
    env: context.env,
    branchName: context.branch.name,
    branchTargets: pluginConfig.branchTargets,
    mods: pluginConfig.mods,
    steamConfigPath: context.env.STEAM_CONFIG_VDF,
  });
}

export async function publish(pluginConfig, context) {
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

  const cwd = context.cwd ?? process.cwd();
  const buildDescription = context.buildSteamDescription ?? buildSteamDescription;
  const stageContent = context.stageModContent ?? stageModContent;
  const uploadItem = context.uploadWorkshopItem ?? uploadWorkshopItem;
  const compileReadme = context.writeCompiledReadme ?? writeCompiledReadme;
  const assetBaseUrl = pluginConfig.assetBaseUrlTemplate
    ? pluginConfig.assetBaseUrlTemplate.replace('{branch}', context.branch.name)
    : '';

  for (const mod of state.mods) {
    const modPath = resolve(cwd, mod.path);
    await compileReadme({
      modPath,
      header: pluginConfig.descriptionHeader ?? '',
      footer: pluginConfig.descriptionFooter ?? '',
    });
    const stagePath = await stageContent({ modPath });
    const description = await buildDescription({
      modPath,
      assetBaseUrl,
    });

    await uploadItem({
      steamCmdPath: context.env.STEAMCMD_PATH ?? '~/steamcmd/steamcmd.sh',
      steamUsername: context.env.STEAM_USERNAME,
      steamConfigPath: context.env.STEAM_CONFIG_VDF,
      stagePath,
      publishedFileId: mod.workshopIds[state.target],
      changenote: context.nextRelease.notes || context.nextRelease.version,
      description,
      logger: context.logger,
    });

    context.logger.log(`Published ${mod.name} to ${state.target} workshop item ${mod.workshopIds[state.target]}`);
  }

  return undefined;
}

export default { verifyConditions, publish };

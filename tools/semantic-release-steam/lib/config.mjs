export function resolveBranchTarget(branchName, branchTargets) {
  return branchTargets[branchName] ?? null;
}

export async function verifySteamPublishConfig({
  env,
  branchName,
  branchTargets,
  mods,
  steamConfigPath,
}) {
  const target = resolveBranchTarget(branchName, branchTargets);
  if (!target) {
    return { shouldPublish: false, target: null, mods: [] };
  }

  if (!env.STEAM_USERNAME) {
    throw new Error('STEAM_USERNAME is required for Steam publishing');
  }

  if (!steamConfigPath) {
    throw new Error('Steam config.vdf path is required for Steam publishing');
  }

  const targetMods = mods.filter(mod => mod.workshopIds?.[target]);

  if (targetMods.length === 0) {
    return { shouldPublish: false, target, mods: [] };
  }

  return { shouldPublish: true, target, mods: targetMods };
}

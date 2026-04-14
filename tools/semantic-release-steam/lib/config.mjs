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
        return { shouldPublish: false, target: null };
    }

    if (!env.STEAM_USERNAME) {
        throw new Error('STEAM_USERNAME is required for Steam publishing');
    }

    if (!steamConfigPath) {
        throw new Error('Steam config.vdf path is required for Steam publishing');
    }

    for (const mod of mods) {
        if (!mod.workshopIds?.[target]) {
            throw new Error(`${target} workshop id is required for ${mod.name}`);
        }
    }

    return { shouldPublish: true, target };
}

export function resolveBetaHubPublishState({ env, branchName, branches }) {
  if (!branches.includes(branchName)) {
    return { shouldPublish: false, token: null };
  }

  if (!env.BETA_HUB_API_KEY) {
    throw new Error('BETA_HUB_API_KEY is required to publish a BetaHub release');
  }

  return { shouldPublish: true, token: env.BETA_HUB_API_KEY };
}

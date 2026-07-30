export function resolveBetaHubPublishState({ env, branchName, branches }) {
  if (!branches.includes(branchName)) {
    return { shouldPublish: false, token: null };
  }

  if (!env.BETAHUB_PAT) {
    throw new Error('BETAHUB_PAT is required to publish a BetaHub release');
  }

  return { shouldPublish: true, token: env.BETAHUB_PAT };
}

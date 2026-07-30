import { createRelease, publishRelease } from './lib/api.mjs';
import { resolveBetaHubPublishState } from './lib/config.mjs';

export async function verifyConditions(pluginConfig, context) {
  resolveBetaHubPublishState({
    env: context.env,
    branchName: context.branch.name,
    branches: pluginConfig.branches,
  });
}

export async function publish(pluginConfig, context) {
  const state = resolveBetaHubPublishState({
    env: context.env,
    branchName: context.branch.name,
    branches: pluginConfig.branches,
  });

  if (!state.shouldPublish) {
    return undefined;
  }

  const fetchImpl = context.fetchImpl ?? fetch;
  const release = await createRelease({
    token: state.token,
    projectId: pluginConfig.projectId,
    label: context.nextRelease.version,
    description: context.nextRelease.notes || context.nextRelease.version,
    fetchImpl,
  });

  if (release.status !== 'published') {
    await publishRelease({
      token: state.token,
      projectId: pluginConfig.projectId,
      releaseId: release.id,
      fetchImpl,
    });
  }

  context.logger.log(`Published BetaHub release ${release.label} (${release.id})`);

  return undefined;
}

export default { verifyConditions, publish };

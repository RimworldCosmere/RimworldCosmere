import test from 'node:test';
import assert from 'node:assert/strict';
import { resolveBetaHubPublishState } from '../lib/config.mjs';

const branches = ['beta'];

test('publishes on beta', () => {
  const state = resolveBetaHubPublishState({
    env: { BETAHUB_PAT: 'pat-x' },
    branchName: 'beta',
    branches,
  });
  assert.equal(state.shouldPublish, true);
});

test('does not publish on main or alpha', () => {
  for (const branchName of ['main', 'alpha', 'feat/whatever']) {
    const state = resolveBetaHubPublishState({
      env: { BETAHUB_PAT: 'pat-x' },
      branchName,
      branches,
    });
    assert.equal(state.shouldPublish, false, branchName);
  }
});

test('a missing PAT on a publishing branch throws rather than skipping', () => {
  assert.throws(
    () => resolveBetaHubPublishState({ env: {}, branchName: 'beta', branches }),
    /BETAHUB_PAT/,
  );
});

test('a missing PAT on a non publishing branch is fine', () => {
  const state = resolveBetaHubPublishState({ env: {}, branchName: 'main', branches });
  assert.equal(state.shouldPublish, false);
});

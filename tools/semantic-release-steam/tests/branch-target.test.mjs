import test from 'node:test';
import assert from 'node:assert/strict';
import { resolveBranchTarget } from '../lib/config.mjs';

test('maps main to stable and beta to beta', () => {
  assert.equal(resolveBranchTarget('main', { main: 'stable', beta: 'beta' }), 'stable');
  assert.equal(resolveBranchTarget('beta', { main: 'stable', beta: 'beta' }), 'beta');
});

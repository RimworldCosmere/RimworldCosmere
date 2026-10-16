import test from 'node:test';
import assert from 'node:assert/strict';
import { createRelease, publishRelease } from '../lib/api.mjs';

function stubFetch(response) {
  const calls = [];
  const fetchImpl = async (url, init) => {
    calls.push({ url, init });
    return {
      ok: response.status >= 200 && response.status < 400,
      status: response.status,
      text: async () => JSON.stringify(response.body ?? {}),
      json: async () => response.body ?? {},
    };
  };
  return { fetchImpl, calls };
}

test('createRelease posts the label and description under a release root', async () => {
  const { fetchImpl, calls } = stubFetch({ status: 201, body: { id: 4853, status: 'published' } });

  const release = await createRelease({
    token: 'pat-x',
    projectId: 'pr-4628785616',
    label: '2.0.0-beta.24',
    description: 'notes here',
    fetchImpl,
  });

  assert.equal(release.id, 4853);
  assert.equal(release.status, 'published');
  assert.equal(calls.length, 1);
  assert.match(calls[0].url, /\/projects\/pr-4628785616\/releases\.json$/);
  assert.equal(calls[0].init.headers.Authorization, 'Bearer pat-x');
  assert.equal(calls[0].init.headers['BetaHub-Project-ID'], 'pr-4628785616');
  assert.deepEqual(JSON.parse(calls[0].init.body), {
    release: { label: '2.0.0-beta.24', description: 'notes here' },
  });
});

test('createRelease throws on a rejection', async () => {
  const { fetchImpl } = stubFetch({ status: 422, body: { error: 'label taken' } });

  await assert.rejects(
    () => createRelease({ token: 'pat-x', projectId: 'pr-1', label: 'x', description: '', fetchImpl }),
    /422/,
  );
});

test('publishRelease treats a redirect as success', async () => {
  const { fetchImpl, calls } = stubFetch({ status: 302 });

  await publishRelease({ token: 'pat-x', projectId: 'pr-1', releaseId: 7, fetchImpl });

  assert.match(calls[0].url, /\/releases\/7\/publish\.json$/);
});

const BASE_URL = 'https://app.betahub.io';

function headers(token, projectId) {
  return {
    Authorization: `Bearer ${token}`,
    'BetaHub-Project-ID': projectId,
    Accept: 'application/json',
    'Content-Type': 'application/json',
  };
}

async function call(fetchImpl, url, token, projectId, body) {
  const response = await fetchImpl(url, {
    method: 'POST',
    headers: headers(token, projectId),
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  // A release delete answers 302, and publish can redirect too. Anything below 400 worked.
  if (response.status >= 400) {
    throw new Error(`BetaHub returned ${response.status} for ${url}: ${await response.text()}`);
  }

  return response;
}

export async function createRelease({ token, projectId, label, description, fetchImpl = fetch }) {
  const url = `${BASE_URL}/projects/${projectId}/releases.json`;
  const response = await call(fetchImpl, url, token, projectId, {
    release: { label, description },
  });

  return response.json();
}

export async function publishRelease({ token, projectId, releaseId, fetchImpl = fetch }) {
  const url = `${BASE_URL}/projects/${projectId}/releases/${releaseId}/publish.json`;
  await call(fetchImpl, url, token, projectId);
}

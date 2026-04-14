import test from 'node:test';
import assert from 'node:assert/strict';
import { createWorkshopVdf } from '../lib/vdf.mjs';

test('renders workshop.vdf with expected fields', () => {
  const vdf = createWorkshopVdf({
    appId: '294100',
    publishedFileId: '123456',
    contentFolder: '/tmp/CosmereCore',
    changenote: '2.0.0-beta.1',
    description: 'Steam description',
  });

  assert.match(vdf, /"appid" "294100"/);
  assert.match(vdf, /"publishedfileid" "123456"/);
  assert.match(vdf, /"contentfolder" "\/tmp\/CosmereCore"/);
  assert.match(vdf, /"changenote" "2.0.0-beta.1"/);
  assert.match(vdf, /"description" "Steam description"/);
});

test('escapes quotes in changenote and description', () => {
  const vdf = createWorkshopVdf({
    appId: '294100',
    publishedFileId: '123456',
    contentFolder: '/tmp/CosmereCore',
    changenote: 'note "quoted"',
    description: 'desc "quoted"',
  });

  assert.match(vdf, /note \\"quoted\\"/);
  assert.match(vdf, /desc \\"quoted\\"/);
});

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

test('replaces quotes in changenote and description', () => {
  const vdf = createWorkshopVdf({
    appId: '294100',
    publishedFileId: '123456',
    contentFolder: '/tmp/CosmereCore',
    changenote: 'note "quoted"',
    description: 'desc "quoted"',
  });

  assert.match(vdf, /"changenote" "note 'quoted'"/);
  assert.match(vdf, /"description" "desc 'quoted'"/);
});

test('leaves every value delimited by exactly one pair of quotes', () => {
  // A backslash-escaped quote used to slip past the old escaper and terminate the
  // value early, which steamcmd reported as "key name too long" before failing to
  // parse the file at all. Guards the shape: two quotes for the key, two for the
  // value, on every line.
  const vdf = createWorkshopVdf({
    appId: '294100',
    publishedFileId: '123456',
    contentFolder: '/tmp/CosmereCore',
    changenote: 'Revert "fix(settings): remove the vanilla window margin"',
    description: 'A "quoted" phrase and a trailing quote"',
  });

  assert.doesNotMatch(vdf, /\\/);

  for (const line of vdf.split('\n')) {
    if (!line.includes('" "')) continue;
    assert.equal(line.split('"').length - 1, 4, `expected 4 quotes on: ${line}`);
  }
});

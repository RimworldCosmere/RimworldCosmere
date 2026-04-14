function escapeVdfValue(value) {
  return String(value ?? '').replaceAll('"', '\\"');
}

export function createWorkshopVdf({
  appId,
  publishedFileId,
  contentFolder,
  changenote,
  description,
}) {
  return [
    '"workshopitem"',
    '{',
    `  "appid" "${escapeVdfValue(appId)}"`,
    `  "publishedfileid" "${escapeVdfValue(publishedFileId)}"`,
    `  "contentfolder" "${escapeVdfValue(contentFolder)}"`,
    `  "changenote" "${escapeVdfValue(changenote)}"`,
    `  "description" "${escapeVdfValue(description)}"`,
    '}',
    '',
  ].join('\n');
}

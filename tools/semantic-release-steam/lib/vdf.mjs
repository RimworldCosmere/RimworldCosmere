/// steamcmd reads workshop.vdf through Valve's KeyValues parser with escape
/// sequences disabled, so a backslash is a literal character and the quote after it
/// still terminates the string. Quotes cannot be escaped here, only replaced - a
/// changenote carrying one used to end the value early and leave the rest of the file
/// to be read as a key name.
function escapeVdfValue(value) {
  return String(value ?? '').replaceAll('"', "'");
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

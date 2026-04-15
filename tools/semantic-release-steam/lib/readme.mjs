import { mkdir, readFile, readdir, writeFile } from 'node:fs/promises';
import { basename, join, resolve } from 'node:path';

function resolveAssetDirEntries(modPath) {
  const modName = basename(modPath);

  return [
    modName.replace(/^Cosmere/, '').toLowerCase(),
    'fallback',
  ];
}

async function replaceFallbackAssets(markdown, modPath) {
  const repoRoot = resolve(modPath, '..');

  for (const assetDir of resolveAssetDirEntries(modPath)) {
    const assetPath = join(repoRoot, '.github', 'assets', assetDir);

    try {
      const entries = await readdir(assetPath);
      let nextMarkdown = markdown;

      for (const entry of entries) {
        const fallbackNames = new Set([
          entry,
          entry.replaceAll('_', ' '),
          entry.replaceAll(' ', '_'),
        ]);

        for (const fallbackName of fallbackNames) {
          nextMarkdown = nextMarkdown.replaceAll(
            `../.github/assets/fallback/${fallbackName}`,
            `../.github/assets/${assetDir}/${entry}`,
          );
        }
      }

      markdown = nextMarkdown;
    } catch {
      continue;
    }
  }

  return markdown;
}

const zeroWidthSpace = '\u200B';
const separator = `\n${zeroWidthSpace}\n\n`;

export async function compileReadme({ modPath, header, footer }) {
  const template = (await readFile(join(modPath, 'README.template.md'), 'utf8')).replace(/^\uFEFF/, '');
  const markdown = `${header}${separator}${template}${separator}${footer}`;
  return await replaceFallbackAssets(markdown, modPath);
}

export async function writeCompiledReadme({ modPath, header, footer }) {
  const markdown = await compileReadme({ modPath, header, footer });
  await mkdir(modPath, { recursive: true });
  await writeFile(join(modPath, 'README.md'), markdown);
  return markdown;
}

export function rewriteAssetLinksForSteam(markdown, assetBaseUrl) {
  if (!assetBaseUrl) {
    return markdown;
  }

  return markdown.replaceAll('../.github/assets/', `${assetBaseUrl}/.github/assets/`);
}

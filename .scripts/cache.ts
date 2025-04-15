import fs from 'fs';
import path from 'path';
import crypto from 'crypto';

const CACHE_FILE = './.generator_cache.json';

type CacheRecord = { [filePath: string]: string };

function getFileHash(filePath: string): string {
  const data = fs.readFileSync(filePath);
  return crypto.createHash('sha256').update(data).digest('hex');
}

function getTemplateFilesFor(generator: string): string[] {
  const baseDir = path.join('./Generators', generator);
  if (!fs.existsSync(baseDir)) return [];

  const walk = (dir: string): string[] => {
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    return entries.flatMap(entry =>
      entry.isDirectory()
        ? walk(path.join(dir, entry.name))
        : entry.name.endsWith('.template')
          ? [path.join(dir, entry.name)]
          : []
    );
  };

  return walk(baseDir);
}

function loadCache(): CacheRecord {
  try {
    return JSON.parse(fs.readFileSync(CACHE_FILE, 'utf8'));
  } catch {
    return {};
  }
}

function saveCache(cache: CacheRecord) {
  fs.writeFileSync(CACHE_FILE, JSON.stringify(cache, null, 2));
}

export function shouldSkipGeneration(generator: string): boolean {
  const files = [
    './Resources/MetalRegistry.json',
    `./Generators/${generator}/index.ts`,
    ...getTemplateFilesFor(generator)
  ];

  const prevCache = loadCache();
  const newCache: CacheRecord = {};

  let changed = false;
  for (const file of files) {
    const hash = getFileHash(file);
    newCache[file] = hash;
    if (prevCache[file] !== hash) {
      changed = true;
    }
  }

  if (!changed) {
    return true;
  }

  saveCache({ ...prevCache, ...newCache });
  return false;
}

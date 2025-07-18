import fs, {mkdirSync, readdirSync} from 'fs';
import path, {resolve} from 'path';
import crypto from 'crypto';

const CACHE_FILE = (generator: string) => path.resolve(__dirname, '.cache', `${generator}.json`);
mkdirSync(path.resolve(__dirname, '.cache'), {recursive: true});

type CacheRecord = { [filePath: string]: string };

function getFileHash(filePath: string): string {
    const data = fs.readFileSync(filePath);
    return crypto.createHash('sha256').update(data).digest('hex');
}

function getTemplateFilesFor(generator: string): string[] {
    const baseDir = path.resolve(__dirname, 'Generators', generator);
    if (!fs.existsSync(baseDir)) return [];

    const walk = (dir: string): string[] => {
        const entries = fs.readdirSync(dir, {withFileTypes: true});
        return entries.flatMap(entry =>
            entry.isDirectory()
                ? walk(path.resolve(dir, entry.name))
                : entry.name.endsWith('.template')
                    ? [path.resolve(dir, entry.name)]
                    : []
        );
    };

    return walk(baseDir);
}

function loadCache(generator: string): CacheRecord {
    try {
        return JSON.parse(fs.readFileSync(CACHE_FILE(generator), 'utf8'));
    } catch {
        return {};
    }
}

function saveCache(generator: string, cache: CacheRecord) {
    fs.writeFileSync(CACHE_FILE(generator), JSON.stringify(cache, null, 2));
}

function getMetalFiles() {
    const directoryPath = resolve(__dirname, 'Resources', 'Metals');
    const files = readdirSync(directoryPath);

    return files.filter(x => x.endsWith('.json')).map(x => resolve(directoryPath, x));
}

export function shouldSkipGeneration(generator: string): boolean {
    const files = [
        ...getMetalFiles(),
        path.resolve(__dirname, 'Generators', generator, 'index.ts'),
        ...getTemplateFilesFor(generator)
    ];

    const prevCache = loadCache(generator);
    const newCache: CacheRecord = {};

    let changed = false;
    for (const file of files) {
        const hash = getFileHash(file);
        newCache[file] = hash;
        if (prevCache[file] !== hash) {
            console.log(`${file} has changed.`);
            changed = true;
        }
    }

    if (!changed) {
        return true;
    }

    saveCache(generator, {...prevCache, ...newCache});
    return false;
}

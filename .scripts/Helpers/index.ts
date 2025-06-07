import Handlebars from 'handlebars';
import { mkdirSync, readFileSync, writeFileSync } from 'fs';
import { resolve, dirname } from 'path';

export function compileTemplate(baseDir: string, template: string) {
    return Handlebars.compile(readFileSync(resolve(baseDir, template), 'utf8'));
}

export function writeGeneratedFile(dir: string, fileName: string, content: string) {
    const fullPath = resolve(dir, fileName);
    mkdirSync(dirname(fullPath), { recursive: true });
    writeFileSync(fullPath, content, 'utf8');
}
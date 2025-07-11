import Handlebars from 'handlebars';
import {mkdirSync, readdirSync, readFileSync, writeFileSync} from 'fs';
import {dirname, resolve} from 'path';
import {options} from "../index";

export const projectDir = resolve(__dirname, '..', '..') + '\\';

export function compileTemplate(baseDir: string, template: string) {
    return Handlebars.compile(readFileSync(resolve(baseDir, template), 'utf8'));
}

export function writeGeneratedFile(dir: string, fileName: string, content: string) {
    const fullPath = resolve(dir, fileName);
    if (options.verbose) console.log((options.dryRun ? '[DRY-RUN] ' : '') + 'Generated file: ' + fullPath.replace(projectDir, ''));
    if (options.dryRun) return;

    mkdirSync(dirname(fullPath), {recursive: true});
    writeFileSync(fullPath, content, 'utf8');
}

export function loadAllJsonSync(directoryPath: string): Record<string, any>[] {
    const files = readdirSync(directoryPath);
    const jsonObjects = [];

    for (const file of files) {
        if (file.endsWith('.json')) {
            const fullPath = resolve(directoryPath, file);
            const content = readFileSync(fullPath, 'utf-8');
            jsonObjects.push(JSON.parse(content));
        }
    }

    return jsonObjects;
}
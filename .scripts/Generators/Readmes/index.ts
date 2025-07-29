import {resolve} from 'node:path';
import {compileTemplate, writeGeneratedFile} from '../../Helpers';

const attributions = compileTemplate(__dirname, 'attributions.md.template');
const donate = compileTemplate(__dirname, 'donate.md.template');
const introduction = compileTemplate(__dirname, 'introduction.md.template');
const recommendedMods = compileTemplate(__dirname, 'recommendedMods.md.template');
const support = compileTemplate(__dirname, 'support.md.template');


export default function () {
    const mods = ['framework', 'resources', 'core', 'scadrial', 'roshar'];
    for (const mod of mods) {
        const dir = resolve(__dirname, '..', '..', '..', 'Cosmere' + mod.capitalize());
        const baseReadme = compileTemplate(resolve(__dirname, 'mods'), mod + '.md.template');
        
        let compiledReadme = [introduction({mod})];
        compiledReadme.push(baseReadme({mod}).trimStart());
        compiledReadme.push(recommendedMods({mod}).trimStart());
        compiledReadme.push(support({mod}).trimStart());
        compiledReadme.push(attributions({mod}).trimStart());
        compiledReadme.push(donate({mod}).trimStart());
        
        writeGeneratedFile(dir, 'README.md', compiledReadme.join('\n\n'));
    }
}
import {resolve} from 'node:path';

import {compileTemplate, writeGeneratedFile} from '../../Helpers';
import {GemRegistry} from '../../Gems/GemRegistry';
import {ROSHAR_MOD_DIR} from '../../constants';

const gemTemplate = compileTemplate(__dirname, 'ItemGemDef.xml.template');
const gemOutputDir = resolve(ROSHAR_MOD_DIR, 'Defs', 'Things', 'Gems');

const thingDefOfTemplate = compileTemplate(__dirname, 'ThingDefOf.Item.cs.template');
const defOfOutputDir = resolve(ROSHAR_MOD_DIR, 'CosmereRoshar');


export default function () {
    const gems = Object.values(GemRegistry.Gems);
    for (const gem of gems) {
        writeGeneratedFile(gemOutputDir, gem.name.toDefName() + '.generated.xml', gemTemplate({gem}));
    }

    writeGeneratedFile(defOfOutputDir, 'ThingDefOf.Item.generated.cs', thingDefOfTemplate({gems}));
}


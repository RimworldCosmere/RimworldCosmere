import {resolve} from 'node:path';

import {compileTemplate, writeGeneratedFile} from '../../../Helpers';
import {GemRegistry} from '../../../Gems/GemRegistry';
import {RESOURCES_MOD_DIR} from '../../../constants';

const gemDefTemplate = compileTemplate(__dirname, 'GemDef.xml.template');
const gemDefOutputDir = resolve(RESOURCES_MOD_DIR, 'Defs', 'Gem');

const gemDefOfTemplate = compileTemplate(__dirname, 'GemDefOf.cs.template');
const thingDefOfMineableTemplate = compileTemplate(__dirname, 'ThingDefOf.Gems.Mineable.cs.template');
const thingDefOfItemTemplate = compileTemplate(__dirname, 'ThingDefOf.Gems.Item.cs.template');
const CosmereResources = resolve(RESOURCES_MOD_DIR, 'CosmereResources');

const mineableTemplate = compileTemplate(__dirname, 'MineableGemDef.xml.template');
const mineableOutputDir = resolve(RESOURCES_MOD_DIR, 'Defs', 'Thing', 'Gem', 'Mineable');

const itemTemplate = compileTemplate(__dirname, 'ItemGemDef.xml.template');
const itemOutputDir = resolve(RESOURCES_MOD_DIR, 'Defs', 'Thing', 'Gem', 'Item');

export default function generate() {
    const gems = Object.values(GemRegistry.Gems);
    for (const gem of gems) {
        writeGeneratedFile(gemDefOutputDir, gem.name.toDefName() + '.generated.xml', gemDefTemplate({gem}));
    }

    writeGeneratedFile(CosmereResources, 'GemDefOf.generated.cs', gemDefOfTemplate({gems}));


    const mineable = Object.values(GemRegistry.Gems).filter((x) => !!x.mining);
    for (const gem of mineable) {
        writeGeneratedFile(mineableOutputDir, gem.name.toDefName() + '.generated.xml', mineableTemplate({gem}));
    }
    writeGeneratedFile(CosmereResources, 'ThingDefOf.Gems.Mineable.generated.cs', thingDefOfMineableTemplate({gems: mineable}));

    const items = Object.values(GemRegistry.Gems);
    for (const gem of items) {
        writeGeneratedFile(itemOutputDir, gem.name.toDefName() + '.generated.xml', itemTemplate({gem}));
    }
    writeGeneratedFile(CosmereResources, 'ThingDefOf.Gems.Items.generated.cs', thingDefOfItemTemplate({gems: items}));
}


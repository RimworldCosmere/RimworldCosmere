import {resolve} from 'node:path';

import {compileTemplate, writeGeneratedFile} from '../../Helpers';
import {MetalRegistry} from '../../Metals/MetalRegistry';
import {METALS_MOD_DIR} from '../../constants';

const metalDefTemplate = compileTemplate(__dirname, 'MetalDef.xml.template');
const metalDefOutputDir = resolve(METALS_MOD_DIR, 'Defs', 'Metal');

const metalDefOfTemplate = compileTemplate(__dirname, 'MetalDefOf.cs.template');
const thingDefOfMineableTemplate = compileTemplate(__dirname, 'ThingDefOf.Mineable.cs.template');
const thingDefOfItemTemplate = compileTemplate(__dirname, 'ThingDefOf.Item.cs.template');
const CosmereResources = resolve(METALS_MOD_DIR, 'CosmereResources');

const mineableTemplate = compileTemplate(__dirname, 'MineableMetalDef.xml.template');
const mineableOutputDir = resolve(METALS_MOD_DIR, 'Defs', 'Mineable');

const itemTemplate = compileTemplate(__dirname, 'ItemMetalDef.xml.template');
const itemOutputDir = resolve(METALS_MOD_DIR, 'Defs', 'Item');

export default function () {
    const metals = Object.values(MetalRegistry.Metals);
    for (const metal of metals) {
        writeGeneratedFile(metalDefOutputDir, metal.name.toDefName() + '.generated.xml', metalDefTemplate({metal}));
    }

    writeGeneratedFile(CosmereResources, 'MetalDefOf.generated.cs', metalDefOfTemplate({metals}));


    const mineable = Object.values(MetalRegistry.Metals).filter((x) => !!x.mining);
    for (const metal of mineable) {
        writeGeneratedFile(mineableOutputDir, metal.name.toDefName() + '.generated.xml', mineableTemplate({metal}));
    }
    writeGeneratedFile(CosmereResources, 'ThingDefOf.Mineable.generated.cs', thingDefOfMineableTemplate({metals: mineable}));

    const items = Object.values(MetalRegistry.Metals);
    for (const metal of items) {
        writeGeneratedFile(itemOutputDir, metal.name.toDefName() + '.generated.xml', itemTemplate({metal}));
    }
    writeGeneratedFile(CosmereResources, 'ThingDefOf.Items.generated.cs', thingDefOfItemTemplate({metals: items}));
}


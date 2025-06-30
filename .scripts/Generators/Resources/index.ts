import {resolve} from 'node:path';
import {upperFirst} from "lodash";

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

const alloyRecipeTemplate = compileTemplate(__dirname, 'AlloyRecipeDef.xml.template');
const alloyRecipeOutputDir = resolve(METALS_MOD_DIR, 'Defs', 'Recipe');

export default function () {
    console.log("Generating MetalDefs");
    const metals = Object.values(MetalRegistry.Metals);
    for (const metal of metals) {
        writeGeneratedFile(metalDefOutputDir, upperFirst(metal.Name) + '.generated.xml', metalDefTemplate({metal}));
    }

    console.log("Generating MetalDefOf");
    writeGeneratedFile(CosmereResources, 'MetalDefOf.generated.cs', metalDefOfTemplate({metals}));


    console.log("Generating Mineable Metal ThingDefs");
    const mineable = Object.values(MetalRegistry.Metals).filter((x) => x.Mining !== undefined);
    for (const metal of mineable) {
        writeGeneratedFile(mineableOutputDir, upperFirst(metal.Name) + '.generated.xml', mineableTemplate({metal}));
    }
    console.log("Generating ThingDefOf.Mineable");
    writeGeneratedFile(CosmereResources, 'ThingDefOf.Mineable.generated.cs', thingDefOfMineableTemplate({metals: mineable}));

    console.log("Generating Metal Item ThingDefs");
    const items = Object.values(MetalRegistry.Metals);
    for (const metal of items) {
        writeGeneratedFile(itemOutputDir, upperFirst(metal.Name) + '.generated.xml', itemTemplate({metal}));
    }
    console.log("Generating ThingDefOf.Items");
    writeGeneratedFile(CosmereResources, 'ThingDefOf.Items.generated.cs', thingDefOfItemTemplate({metals: items}));

    // @todo Implement the god metals
    console.log("Generating Alloy Item RecipeDef");
    const alloys = Object.values(MetalRegistry.Metals).filter((x) => x.Alloy !== undefined && !x.GodMetal);
    for (const metal of alloys) {
        //writeGeneratedFile(alloyRecipeOutputDir, upperFirst(metal.Name) + '.generated.xml', alloyRecipeTemplate({metal}));
    }
}


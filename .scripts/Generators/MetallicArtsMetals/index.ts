import {resolve} from 'node:path';

import {compileTemplate, writeGeneratedFile} from '../../Helpers';
import {MetalRegistry} from '../../Metals/MetalRegistry';
import {SCADRIAL_MOD_DIR} from '../../constants';
import {toDefName} from "../../Helpers/Handlebars";

const metalTemplate = compileTemplate(__dirname, 'MetallicArtsMetalDef.xml.template');
const metalOutputDir = resolve(SCADRIAL_MOD_DIR, 'Defs', 'Things', 'Metals');
const patchMetalDefOfTemplate = compileTemplate(__dirname, 'PatchScadrialMetalDef.xml.template');
const patchMetalDefOfOutputDir = resolve(SCADRIAL_MOD_DIR, 'Patches', 'Metals');

const metalDefOfTemplate = compileTemplate(__dirname, 'MetallicArtsMetalDefOf.cs.template');
const metalDefOfOutputDir = resolve(SCADRIAL_MOD_DIR, 'CosmereScadrial');

export default function () {
    const metals = Object.values(MetalRegistry.Metals)
        .filter(x => !!x.Allomancy || !!x.Feruchemy);
    for (const metal of metals) {
        writeGeneratedFile(metalOutputDir, toDefName(metal.Name) + '.generated.xml', metalTemplate({metal}));
        writeGeneratedFile(patchMetalDefOfOutputDir, toDefName(metal.Name) + '.generated.xml', patchMetalDefOfTemplate({metal}));
    }

    writeGeneratedFile(metalDefOfOutputDir, 'MetallicArtsMetalDefOf.generated.cs', metalDefOfTemplate({metals}));
}


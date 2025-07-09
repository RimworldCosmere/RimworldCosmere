import {resolve} from 'node:path';

import {compileTemplate, writeGeneratedFile} from '../../Helpers';
import {MetalRegistry} from '../../Metals/MetalRegistry';
import {SCADRIAL_MOD_DIR} from '../../constants';

const metalTemplate = compileTemplate(__dirname, 'MetallicArtsMetalDef.xml.template');
const metalOutputDir = resolve(SCADRIAL_MOD_DIR, 'Defs', 'Things', 'Metals');
const patchMetalDefTemplate = compileTemplate(__dirname, 'PatchScadrialMetalDef.xml.template');
const patchMetalDefOutputDir = resolve(SCADRIAL_MOD_DIR, 'Patches', 'Metals');

const metalDefOfTemplate = compileTemplate(__dirname, 'MetallicArtsMetalDefOf.cs.template');
const defOfOutputDir = resolve(SCADRIAL_MOD_DIR, 'CosmereScadrial');

const recordsTemplate = compileTemplate(__dirname, 'Records.xml.template');
const recordDefOfTemplate = compileTemplate(__dirname, 'RecordDefOf.cs.template');
const recordsOutputDir = resolve(SCADRIAL_MOD_DIR, 'Defs');

export default function () {
    const metals = Object.values(MetalRegistry.Metals)
        .filter(x => !!x.allomancy || !!x.feruchemy);
    for (const metal of metals) {
        writeGeneratedFile(metalOutputDir, metal.name.toDefName() + '.generated.xml', metalTemplate({metal}));
        writeGeneratedFile(patchMetalDefOutputDir, metal.name.toDefName() + '.generated.xml', patchMetalDefTemplate({metal}));
    }

    writeGeneratedFile(recordsOutputDir, 'Records.generated.xml', recordsTemplate({metals}));
    writeGeneratedFile(defOfOutputDir, 'MetallicArtsMetalDefOf.generated.cs', metalDefOfTemplate({metals}));
    writeGeneratedFile(defOfOutputDir, 'RecordDefOf.generated.cs', recordDefOfTemplate({metals}));
}


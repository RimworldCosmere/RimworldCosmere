import {resolve} from 'node:path';

import {compileTemplate, writeGeneratedFile} from '../../Helpers';
import {MetalRegistry} from '../../Metals/MetalRegistry';
import {SCADRIAL_MOD_DIR} from '../../constants';
import {toDefName} from "../../Helpers/Handlebars";

const defOfTemplate = compileTemplate(__dirname, 'HediffDefOf.cs.template');
const defOfOutputDir = resolve(SCADRIAL_MOD_DIR, 'CosmereScadrial');

const defTemplate = compileTemplate(__dirname, 'HediffDef.xml.template');


export default function () {
    const metals = Object.values(MetalRegistry.Metals).filter(x => !!x.feruchemy?.userName);
    for (const metal of metals) {
        const outputDir = resolve(SCADRIAL_MOD_DIR, 'Defs', 'Feruchemy', toDefName(metal.name));
        writeGeneratedFile(outputDir, 'Hediff.generated.xml', defTemplate({metal}));
    }

    writeGeneratedFile(defOfOutputDir, 'HediffDefOf.Feruchemy.generated.cs', defOfTemplate({metals}));
}
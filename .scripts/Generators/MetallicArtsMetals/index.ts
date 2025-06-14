import {resolve} from 'node:path';
import {upperFirst} from "lodash";

import {compileTemplate, writeGeneratedFile} from '../../Helpers';
import {MetalRegistry} from '../../Metals/MetalRegistry';
import {SCADRIAL_MOD_DIR} from '../../constants';

const metalTemplate= compileTemplate(__dirname, 'MetallicArtsMetalDef.xml.template');
const metalOutputDir = resolve(SCADRIAL_MOD_DIR, 'Defs', 'Metals');
const patchMetalDefOfTemplate = compileTemplate(__dirname, 'PatchScadrialMetalDef.xml.template');
const patchMetalDefOfOutputDir = resolve(SCADRIAL_MOD_DIR, 'Patches', 'Metals');

const metalDefOfTemplate = compileTemplate(__dirname, 'MetallicArtsMetalDefOf.cs.template');
const metalDefOfOutputDir = resolve(SCADRIAL_MOD_DIR, 'CosmereScadrial');

export default function() {
  console.log("Generating MetallicArtMetalDefs");
  const metals = Object.values(MetalRegistry.Metals)
    .filter(x => x.Allomancy !== null || x.Feruchemy !== null);
  for (const metal of metals) {
    writeGeneratedFile(metalOutputDir, upperFirst(metal.Name) + '.generated.xml', metalTemplate({metal}));
    writeGeneratedFile(patchMetalDefOfOutputDir, upperFirst(metal.Name) + '.generated.xml', patchMetalDefOfTemplate({metal}));
  }

  console.log("Generating MetallicArtsMetalDefOf");
  writeGeneratedFile(metalDefOfOutputDir, 'MetallicArtsMetalDefOf.generated.cs', metalDefOfTemplate({metals}));
}


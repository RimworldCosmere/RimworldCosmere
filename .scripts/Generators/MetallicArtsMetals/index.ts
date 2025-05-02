import Handlebars from 'handlebars';
import {readFileSync, writeFileSync} from 'node:fs';
import {resolve} from 'node:path';
import {rimrafSync} from 'rimraf';

import {MetalRegistry} from '../../Metals/MetalRegistry';
import {SCADRIAL_MOD_DIR} from '../../constants';
import {mkdirSync} from "fs";
import {upperFirst} from "lodash";

const metalTemplate= Handlebars.compile(readFileSync(resolve(__dirname, 'MetallicArtsMetalDef.xml.template'), 'utf8'));
const metalOutputDir = resolve(SCADRIAL_MOD_DIR, 'Defs', 'Metals');

const metalDefOfTemplate = Handlebars.compile(readFileSync(resolve(__dirname, 'MetallicArtsMetalDefOf.cs.template'), 'utf8'));
const metalDefOfOutputDir = resolve(SCADRIAL_MOD_DIR, 'CosmereScadrial');


export default function() {
  console.log("Generating MetallicArtMetalDefs");
  const metals = Object.values(MetalRegistry.Metals)
    .filter(x => x.Allomancy !== null && x.Feruchemy !== null);
  for (const metal of metals) {
    writeFileSync(resolve(metalOutputDir, upperFirst(metal.Name) + '.generated.xml'), metalTemplate({metal}), 'utf8');
  }

  console.log("Generating MetallicArtsMetalDefOf");
  writeFileSync(resolve(metalDefOfOutputDir, 'MetallicArtsMetalDefOf.generated.cs'), metalDefOfTemplate({metals}), 'utf8');
}


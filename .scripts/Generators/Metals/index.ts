import Handlebars from 'handlebars';
import {readFileSync, writeFileSync} from 'node:fs';
import {resolve} from 'node:path';
import {rimrafSync} from 'rimraf';

import {MetalRegistry} from '../../Metals/MetalRegistry';
import {METALS_MOD_DIR} from '../../constants';
import {mkdirSync} from "fs";
import {upperFirst} from "lodash";

const metalDefTemplate= Handlebars.compile(readFileSync(resolve(__dirname, 'MetalDef.xml.template'), 'utf8'));
const metalDefOutputDir = resolve(METALS_MOD_DIR, 'Defs', 'Metal');

const metalDefOfTemplate = Handlebars.compile(readFileSync(resolve(__dirname, 'MetalDefOf.cs.template'), 'utf8'));
const metalDefOfOutputDir = resolve(METALS_MOD_DIR, 'CosmereMetals');

const mineableTemplate = Handlebars.compile(readFileSync(resolve(__dirname, 'MineableMetalDef.xml.template'), 'utf8'));
const mineableOutputDir = resolve(METALS_MOD_DIR, 'Defs', 'Mineable');

const itemTemplate = Handlebars.compile(readFileSync(resolve(__dirname, 'ItemMetalDef.xml.template'), 'utf8'));
const itemOutputDir = resolve(METALS_MOD_DIR, 'Defs', 'Item');

const alloyRecipeTemplate = Handlebars.compile(readFileSync(resolve(__dirname, 'AlloyRecipeDef.xml.template'), 'utf8'));
const alloyRecipeOutputDir = resolve(METALS_MOD_DIR, 'Defs', 'Recipe');

export default function() {
  console.log("Generating MetalDefs");
  const metals = Object.values(MetalRegistry.Metals);
  for (const metal of metals) {
    writeFileSync(resolve(metalDefOutputDir, upperFirst(metal.Name) + '.generated.xml'), metalDefTemplate({metal}), 'utf8');
  }

  console.log("Generating MetalDefOf");
  writeFileSync(resolve(metalDefOfOutputDir, 'MetalDefOf.generated.cs'), metalDefOfTemplate({metals}), 'utf8');
  
  
  console.log("Generating Mineable Metal ThingDefs");
  const mineable = Object.values(MetalRegistry.Metals).filter((x) => x.Mining !== undefined);
  for (const metal of mineable) {
    writeFileSync(resolve(mineableOutputDir, upperFirst(metal.Name) + '.generated.xml'), mineableTemplate({metal}), 'utf8');
  }
  console.log("Generating Metal Item ThingDefs");
  const items = Object.values(MetalRegistry.Metals).filter((x) => !x.GodMetal);
  for (const metal of items) {
    writeFileSync(resolve(itemOutputDir, upperFirst(metal.Name) + '.generated.xml'), itemTemplate({metal}), 'utf8');
  }
  
  // @todo Implement the god metals
  console.log("Generating Alloy Item ThingDefs");
  const alloys = Object.values(MetalRegistry.Metals).filter((x) => x.Alloy !== undefined && !x.GodMetal);
  for (const metal of alloys) {
    writeFileSync(resolve(alloyRecipeOutputDir, upperFirst(metal.Name) + '.generated.xml'), alloyRecipeTemplate({metal}), 'utf8');
  }
}


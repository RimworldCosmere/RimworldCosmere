import Handlebars from 'handlebars';
import {readFileSync, writeFileSync} from 'node:fs';
import {resolve} from 'node:path';
import {rimrafSync} from 'rimraf';

import {MetalRegistry} from '../../Metals/MetalRegistry';
import {CORE_MOD_DIR} from '../../constants';
import {mkdirSync} from "fs";
import {upperFirst} from "lodash";

const metalDefTemplate= Handlebars.compile(readFileSync(resolve(__dirname, 'MetalDef.xml.template'), 'utf8'));
const metalDefOutputDir = resolve(CORE_MOD_DIR, 'Defs', 'Metals', 'Generated', 'Metal');

const metalDefOfTemplate = Handlebars.compile(readFileSync(resolve(__dirname, 'MetalDefOf.cs.template'), 'utf8'));
const metalDefOfOutputDir = resolve(CORE_MOD_DIR, 'CosmereCore');

const mineableTemplate = Handlebars.compile(readFileSync(resolve(__dirname, 'MineableMetalDef.xml.template'), 'utf8'));
const mineableOutputDir = resolve(CORE_MOD_DIR, 'Defs', 'Metals', 'Generated', 'Mineable');

const itemTemplate = Handlebars.compile(readFileSync(resolve(__dirname, 'ItemMetalDef.xml.template'), 'utf8'));
const itemOutputDir = resolve(CORE_MOD_DIR, 'Defs', 'Metals', 'Generated', 'Item');

const alloyRecipeTemplate = Handlebars.compile(readFileSync(resolve(__dirname, 'AlloyRecipeDef.xml.template'), 'utf8'));
const alloyRecipeOutputDir = resolve(CORE_MOD_DIR, 'Defs', 'Metals', 'Generated', 'Recipe');

export default function() {
  [metalDefOutputDir, mineableOutputDir, itemOutputDir, alloyRecipeOutputDir].forEach(x => {
    rimrafSync(x);
    mkdirSync(x, {recursive: true});
  });

  console.log("Generating MetalDefs");
  const metals = Object.values(MetalRegistry.Metals);
  for (const metal of metals) {
    writeFileSync(resolve(metalDefOutputDir, upperFirst(metal.Name) + '.xml'), metalDefTemplate({metal}), 'utf8');
  }

  console.log("Generating MetalDefOf");
  writeFileSync(resolve(metalDefOfOutputDir, 'MetalDefOf.cs'), metalDefOfTemplate({metals}), 'utf8');
  
  
  console.log("Generating Mineable Metal ThingDefs");
  const mineable = Object.values(MetalRegistry.Metals).filter((x) => x.Mining !== undefined);
  for (const metal of mineable) {
    writeFileSync(resolve(mineableOutputDir, upperFirst(metal.Name) + '.xml'), mineableTemplate({metal}), 'utf8');
  }
  console.log("Generating Metal Item ThingDefs");
  const items = Object.values(MetalRegistry.Metals).filter((x) => !x.GodMetal);
  for (const metal of items) {
    writeFileSync(resolve(itemOutputDir, upperFirst(metal.Name) + '.xml'), itemTemplate({metal}), 'utf8');
  }
  
  // @todo Implement the god metals
  console.log("Generating Alloy Item ThingDefs");
  const alloys = Object.values(MetalRegistry.Metals).filter((x) => x.Alloy !== undefined && !x.GodMetal);
  for (const metal of alloys) {
    writeFileSync(resolve(alloyRecipeOutputDir, upperFirst(metal.Name) + '.xml'), alloyRecipeTemplate({metal}), 'utf8');
  }
}


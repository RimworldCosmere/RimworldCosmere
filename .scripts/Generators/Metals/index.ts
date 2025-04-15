import Handlebars from 'handlebars';
import {readFileSync, writeFileSync} from 'node:fs';
import {resolve} from 'node:path';
import {rimrafSync} from 'rimraf';

import {MetalRegistry} from '../../Metals/MetalRegistry';
import {CORE_MOD_DIR} from '../../constants';
import {mkdirSync} from "fs";
import {upperFirst} from "lodash";

const mineableMetalDefTemplate = readFileSync(resolve(__dirname, 'MineableMetalDef.xml.template'), 'utf8');
const mineableTemplate = Handlebars.compile(mineableMetalDefTemplate);
const mineableOutputDir = resolve(CORE_MOD_DIR, 'Defs', 'Metals', 'Generated', 'Mineable');
const metalDefTemplate = readFileSync(resolve(__dirname, 'MetalDef.xml.template'), 'utf8');
const metalTemplate = Handlebars.compile(metalDefTemplate);
const metalOutputDir = resolve(CORE_MOD_DIR, 'Defs', 'Metals', 'Generated', 'Item');
const alloyRecipeDefTemplate = readFileSync(resolve(__dirname, 'AlloyRecipeDef.xml.template'), 'utf8');
const alloyRecipeTemplate = Handlebars.compile(alloyRecipeDefTemplate);
const alloyRecipeOutputDir = resolve(CORE_MOD_DIR, 'Defs', 'Metals', 'Generated', 'Recipe');

export default function() {
  rimrafSync(mineableOutputDir);
  mkdirSync(mineableOutputDir, {recursive: true});
  rimrafSync(metalOutputDir);
  mkdirSync(metalOutputDir, {recursive: true});
  rimrafSync(alloyRecipeOutputDir);
  mkdirSync(alloyRecipeOutputDir, {recursive: true});
  
  console.log("Generating Mineable Metals");
  const mineable = Object.values(MetalRegistry.Metals).filter((x) => x.Mining !== undefined);
  for (const metalInfo of mineable) {
    writeFileSync(resolve(mineableOutputDir, upperFirst(metalInfo.Name) + '.xml'), mineableTemplate({metal: metalInfo}), 'utf8');
  }
  console.log("Generating All Metals");
  const metals = Object.values(MetalRegistry.Metals).filter((x) => !x.GodMetal);
  for (const metalInfo of metals) {
    writeFileSync(resolve(metalOutputDir, upperFirst(metalInfo.Name) + '.xml'), metalTemplate({metal: metalInfo}), 'utf8');
  }
  
  // @todo Implement the god metals
  const alloys = Object.values(MetalRegistry.Metals).filter((x) => x.Alloy !== undefined && !x.GodMetal);
  for (const metalInfo of alloys) {
    writeFileSync(resolve(alloyRecipeOutputDir, upperFirst(metalInfo.Name) + '.xml'), alloyRecipeTemplate({metal: metalInfo}), 'utf8');
  }
}


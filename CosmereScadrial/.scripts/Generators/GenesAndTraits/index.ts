import Handlebars from 'handlebars';
import {readFileSync, writeFileSync} from 'node:fs';
import {resolve} from 'node:path';
import {rimrafSync} from 'rimraf';

import {MetalRegistry} from '../../Metals/MetalRegistry';
import {bootstrap, MOD_DIR} from '../../bootstrap';
import {mkdirSync} from "fs";
import {upperFirst} from "lodash";

bootstrap()

const geneAndTraitDefTemplate = readFileSync(resolve(__dirname, 'GeneAndTraitDef.xml.template'), 'utf8');
const template = Handlebars.compile(geneAndTraitDefTemplate);
const outputDir = resolve(MOD_DIR, 'Defs', 'Investiture', 'Generated');

rimrafSync(outputDir);
mkdirSync(outputDir, {recursive: true});

let order = 2
for (const metal of Object.keys(MetalRegistry.Metals)) {
  const metalInfo = MetalRegistry.Metals[metal];
  if (metalInfo.GodMetal) continue;

  writeFileSync(resolve(outputDir, upperFirst(metalInfo.Name) + '.xml'), template({
    metal: metalInfo,
    defName: metalInfo.DefName ?? upperFirst(metalInfo.Name),
    order: order++,
  }), 'utf8');
}

['Mistborn', 'FullFeruchemist'].forEach(type => {
  const templateString = readFileSync(resolve(__dirname, type + '.xml.template'), 'utf8');
  const template = Handlebars.compile(templateString);
  const metals = Object.values(MetalRegistry.Metals).filter(x => !x.GodMetal).map((metal) => metal.Name);

  writeFileSync(resolve(outputDir, type + '.xml'), template({metals}), 'utf8');
})
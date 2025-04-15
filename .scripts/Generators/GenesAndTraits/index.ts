import Handlebars from 'handlebars';
import {readFileSync, writeFileSync} from 'node:fs';
import {resolve} from 'node:path';
import {rimrafSync} from 'rimraf';

import {MetalRegistry} from '../../Metals/MetalRegistry';
import {SCADRIAL_MOD_DIR} from '../../constants';
import {mkdirSync} from "fs";
import {upperFirst} from "lodash";


const geneAndTraitDefTemplate = readFileSync(resolve(__dirname, 'GeneAndTraitDef.xml.template'), 'utf8');
const template = Handlebars.compile(geneAndTraitDefTemplate);
const outputDir = resolve(SCADRIAL_MOD_DIR, 'Defs', 'Investiture', 'Generated');

export default function () {
  rimrafSync(outputDir);
  mkdirSync(outputDir, {recursive: true});

  let order = 2
  for (const metal of Object.keys(MetalRegistry.Metals)) {
    const metalInfo = MetalRegistry.Metals[metal];
    if (metalInfo.GodMetal || !metalInfo.Allomancy) continue;

    writeFileSync(resolve(outputDir, upperFirst(metalInfo.Name) + '.xml'), template({
      metal: metalInfo,
      defName: metalInfo.DefName ?? upperFirst(metalInfo.Name),
      order: order++,
    }), 'utf8');
  }

  ['Mistborn', 'FullFeruchemist'].forEach(type => {
    const templateString = readFileSync(resolve(__dirname, type + '.xml.template'), 'utf8');
    const template = Handlebars.compile(templateString);
    const metals = Object.values(MetalRegistry.Metals).filter(x => !x.GodMetal && !!x.Allomancy).map((metal) => metal.Name);
    const abilities = Object.values(MetalRegistry.Metals).reduce((acc, metal) => {
      const typeData = metal[type === 'Mistborn' ? 'Allomancy' : 'Feruchemy'];
      if (typeData?.Abilities) {
        acc.push(...typeData.Abilities);
      }

      return acc;
    }, [] as string[]);

    writeFileSync(resolve(outputDir, type + '.xml'), template({metals, abilities}), 'utf8');
  })
}
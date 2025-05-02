import Handlebars from 'handlebars';
import {readFileSync, writeFileSync} from 'node:fs';
import {resolve} from 'node:path';
import {rimrafSync} from 'rimraf';

import {MetalRegistry} from '../../Metals/MetalRegistry';
import {SCADRIAL_MOD_DIR} from '../../constants';
import {mkdirSync} from "fs";
import {upperFirst} from "lodash";

const template = Handlebars.compile(readFileSync(resolve(__dirname, 'GeneAndTraitDef.xml.template'), 'utf8'));
const outputDir = resolve(SCADRIAL_MOD_DIR, 'Defs', 'Genes');


const defOfTemplate = Handlebars.compile(readFileSync(resolve(__dirname, 'DefOf.cs.template'), 'utf8'));
const defOfOutputDir = resolve(SCADRIAL_MOD_DIR, 'CosmereScadrial');

export default function () {
  let order = 2
  const metals = Object.values(MetalRegistry.Metals).filter(x => !x.GodMetal && (!!x.Allomancy || !!x.Feruchemy));
  for (const metalInfo of metals) {
    writeFileSync(resolve(outputDir, upperFirst(metalInfo.Name) + '.generated.xml'), template({
      metal: metalInfo,
      defName: metalInfo.DefName ?? upperFirst(metalInfo.Name),
      order: order++,
    }), 'utf8');
  }

  ['Gene', 'Trait'].forEach(type => {
    writeFileSync(resolve(defOfOutputDir, type + 'DefOf.generated.cs'), defOfTemplate({type, metals}), 'utf8');
  });

  ['Mistborn', 'FullFeruchemist'].forEach(type => {
    const templateString = readFileSync(resolve(__dirname, type + '.xml.template'), 'utf8');
    const template = Handlebars.compile(templateString);
    const {abilities, rightClickAbilities} = Object.values(MetalRegistry.Metals).reduce((acc, metal) => {
      const typeData = metal[type === 'Mistborn' ? 'Allomancy' : 'Feruchemy'];
      if (typeData?.Abilities) {
        acc.abilities.push(...typeData.Abilities);
      }


      return acc;
    }, {abilities: [] as string[], rightClickAbilities: [] as string[]});

    writeFileSync(resolve(outputDir, type + '.xml'), template({metals, abilities, rightClickAbilities}), 'utf8');
  })
}
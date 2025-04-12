import Handlebars from 'handlebars';
import {readFileSync, writeFileSync} from 'node:fs';
import {resolve} from 'node:path';
import {rimrafSync} from 'rimraf';

import {MetalRegistry} from '../../Metals/MetalRegistry';
import {bootstrap, MOD_DIR, SCADRIAL_MOD_DIR} from '../../bootstrap';
import {mkdirSync} from "fs";
import {upperFirst} from "lodash";

bootstrap()

const defTemplate = readFileSync(resolve(__dirname, 'VialDef.xml.template'), 'utf8');
const template = Handlebars.compile(defTemplate);
const outputDir = resolve(SCADRIAL_MOD_DIR, 'Defs', 'Vial', 'Generated');

rimrafSync(outputDir);
mkdirSync(outputDir, {recursive: true});

for (const metal of Object.keys(MetalRegistry.Metals)) {
  const metalInfo = MetalRegistry.Metals[metal];
  if (metalInfo.GodMetal || !metalInfo.Allomancy) continue;

  writeFileSync(resolve(outputDir, upperFirst(metalInfo.Name) + '.xml'), template({
    metal: metalInfo,
    defName: metalInfo.DefName ?? upperFirst(metalInfo.Name)
  }), 'utf8');
}

// Multi-Metal vials
const multiVialDefTemplate = readFileSync(resolve(__dirname, 'multiVialDef.xml.template'), 'utf8');
const multiVialTemplate = Handlebars.compile(multiVialDefTemplate);
const multiVialGroups = [
  {
    defName: "Physical",
    name: "Physical",
    metals: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Group === 'physical').map(x => upperFirst(x.Name)),
    defNames: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Group === 'physical').map(x => x.DefName ?? upperFirst(x.Name)),
  },
  {
    defName: "Mental",
    name: "Mental",
    metals: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Group === 'mental').map(x => upperFirst(x.Name)),
    defNames: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Group === 'mental').map(x => x.DefName ?? upperFirst(x.Name)),
  },
  {
    defName: "Enhancement",
    name: "Enhancement",
    metals: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Group === 'enhancement').map(x => upperFirst(x.Name)),
    defNames: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Group === 'enhancement').map(x => x.DefName ?? upperFirst(x.Name)),
  },
  {
    defName: "Temporal",
    name: "Temporal",
    metals: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Group === 'temporal').map(x => upperFirst(x.Name)),
    defNames: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Group === 'temporal').map(x => x.DefName ?? upperFirst(x.Name)),
  },
  {
    defName: "External",
    name: "External",
    metals: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Axis === 'external').map(x => upperFirst(x.Name)),
    defNames: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Axis === 'external').map(x => x.DefName ?? upperFirst(x.Name)),
  },
  {
    defName: "Internal",
    name: "Internal",
    metals: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Axis === 'internal').map(x => upperFirst(x.Name)),
    defNames: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Axis === 'internal').map(x => x.DefName ?? upperFirst(x.Name)),
  },
  {
    defName: "Pushing",
    name: "Pushing",
    metals: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Polarity === 'pushing').map(x => upperFirst(x.Name)),
    defNames: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Polarity === 'pushing').map(x => x.DefName ?? upperFirst(x.Name)),
  },
  {
    defName: "Pulling",
    name: "Pulling",
    metals: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Polarity === 'pulling').map(x => upperFirst(x.Name)),
    defNames: Object.values(MetalRegistry.Metals).filter(x => x.Allomancy?.Polarity === 'pulling').map(x => x.DefName ?? upperFirst(x.Name)),
  },
  {
    defName: "PhysicalMental",
    name: "Physical + Mental",
    metals: Object.values(MetalRegistry.Metals).filter(x => ['physical', 'mental'].includes(x.Allomancy?.Group ?? '')).map(x => upperFirst(x.Name)),
    defNames: Object.values(MetalRegistry.Metals).filter(x => ['physical', 'mental'].includes(x.Allomancy?.Group ?? '')).map(x => x.DefName ?? upperFirst(x.Name)),
  },
  {
    defName: "EnhancementTemporal",
    name: "Enhancement + Temporal",
    metals: Object.values(MetalRegistry.Metals).filter(x => ['enhancement', 'temporal'].includes(x.Allomancy?.Group ?? '')).map(x => upperFirst(x.Name)),
    defNames: Object.values(MetalRegistry.Metals).filter(x => ['enhancement', 'temporal'].includes(x.Allomancy?.Group ?? '')).map(x => x.DefName ?? upperFirst(x.Name)),
  },
];
for (const group of multiVialGroups) {
  writeFileSync(resolve(outputDir, group.defName + '.xml'), multiVialTemplate(group))
}
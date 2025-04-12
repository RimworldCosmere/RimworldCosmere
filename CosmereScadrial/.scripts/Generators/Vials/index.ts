import Handlebars from 'handlebars';
import {readFileSync, writeFileSync} from 'node:fs';
import {resolve} from 'node:path';
import {rimrafSync} from 'rimraf';

import {MetalRegistry} from '../../Metals/MetalRegistry';
import {bootstrap, MOD_DIR} from '../../bootstrap';
import {mkdirSync} from "fs";
import {upperFirst} from "lodash";

bootstrap()

const defTemplate = readFileSync(resolve(__dirname, 'VialDef.xml.template'), 'utf8');
const template = Handlebars.compile(defTemplate);
const outputDir = resolve(MOD_DIR, 'Defs', 'Vial', 'Generated');

rimrafSync(outputDir);
mkdirSync(outputDir, {recursive: true});

for (const metal of Object.keys(MetalRegistry.Metals)) {
  const metalInfo = MetalRegistry.Metals[metal];
  if (metalInfo.GodMetal) continue;

  writeFileSync(resolve(outputDir, upperFirst(metalInfo.Name) + '.xml'), template({
    metal: metalInfo,
    defName: metalInfo.DefName ?? upperFirst(metalInfo.Name)
  }), 'utf8');
}


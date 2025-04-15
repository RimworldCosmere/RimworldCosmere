import Handlebars from 'handlebars';
import {readFileSync, writeFileSync} from 'node:fs';
import {resolve} from 'node:path';
import {rimrafSync} from 'rimraf';
import * as metals from '../../Resources/MetalRegistry.json';

import {CORE_MOD_DIR, SCADRIAL_MOD_DIR} from '../../constants';
import {mkdirSync} from "fs";

const mods = [
  {dir: CORE_MOD_DIR, templateName: 'CoreMetalRegistry'},
  {dir: SCADRIAL_MOD_DIR, templateName: 'ScadrialMetalRegistry'}
]

export default function () {
  mods.forEach(({dir, templateName}) => {
    const defTemplate = readFileSync(resolve(__dirname, `${templateName}.xml.template`), 'utf8');
    const template = Handlebars.compile(defTemplate);
    const outputDir = resolve(dir, 'Resources', 'Generated');

    rimrafSync(resolve(outputDir, 'MetalRegistry.xml'));
    mkdirSync(outputDir, {recursive: true});
    writeFileSync(resolve(outputDir, 'MetalRegistry.xml'), template({metals}))
  })
}
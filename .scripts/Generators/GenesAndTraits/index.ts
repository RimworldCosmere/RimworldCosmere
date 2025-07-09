import {resolve} from 'node:path';

import {compileTemplate, writeGeneratedFile} from '../../Helpers';
import {MetalRegistry} from '../../Metals/MetalRegistry';
import {SCADRIAL_MOD_DIR} from '../../constants';
import {toDefName} from "../../Helpers/Handlebars";
import {upperFirst} from "lodash";

const defOfTemplate = compileTemplate(__dirname, 'DefOf.cs.template');
const defOfOutputDir = resolve(SCADRIAL_MOD_DIR, 'CosmereScadrial');

export default function () {
    for (const type of ['allomancy', 'feruchemy'] as const) {
        const geneTemplate = compileTemplate(__dirname, upperFirst(type) + 'GeneDef.xml.template');
        const traitTemplate = compileTemplate(__dirname, upperFirst(type) + 'TraitDef.xml.template');
        const outputDir = resolve(SCADRIAL_MOD_DIR, 'Defs', upperFirst(type));

        let order = 2
        const metals = Object.values(MetalRegistry.Metals).filter(x => !x.godMetal && (!!x[type])).concat(MetalRegistry.Metals.Atium);
        for (const metalInfo of metals) {
            writeGeneratedFile(
                resolve(outputDir, toDefName(metalInfo.name)),
                'Gene.generated.xml',
                geneTemplate({
                    metal: metalInfo,
                    defName: metalInfo.defName ?? toDefName(metalInfo.name),
                    order: order++,
                }),
            );
            writeGeneratedFile(
                resolve(outputDir, toDefName(metalInfo.name)),
                'Trait.generated.xml',
                traitTemplate({
                    metal: metalInfo,
                    defName: metalInfo.defName ?? toDefName(metalInfo.name),
                    order: order++,
                }),
            );
        }

        ['Gene', 'Trait'].forEach(kind => {
            writeGeneratedFile(defOfOutputDir, kind + 'DefOf.' + upperFirst(type) + '.generated.cs', defOfTemplate({
                type: type === 'allomancy' ? 'Misting' : 'Ferring',
                kind,
                metals
            }));
        });
    }


    ['Mistborn', 'FullFeruchemist'].forEach(type => {
        const dir = type === 'Mistborn' ? 'allomancy' : 'feruchemy';
        const metals = Object.values(MetalRegistry.Metals).filter(x => !x.godMetal && (!!x[dir])).concat(MetalRegistry.Metals.Atium);
        const template = compileTemplate(__dirname, type + '.xml.template');
        const {abilities, rightClickAbilities} = Object.values(MetalRegistry.Metals).reduce((acc, metal) => {
            const typeData = metal[dir];
            if (typeData?.abilities) {
                acc.abilities.push(...typeData.abilities);
            }


            return acc;
        }, {abilities: [] as string[], rightClickAbilities: [] as string[]});

        writeGeneratedFile(resolve(SCADRIAL_MOD_DIR, 'Defs', upperFirst(dir), type), 'Trait.generated.xml', template({
            metals,
            abilities,
            rightClickAbilities
        }));
    })
}
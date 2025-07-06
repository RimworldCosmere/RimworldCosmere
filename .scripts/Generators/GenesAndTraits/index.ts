import {resolve} from 'node:path';

import {compileTemplate, writeGeneratedFile} from '../../Helpers';
import {MetalRegistry} from '../../Metals/MetalRegistry';
import {SCADRIAL_MOD_DIR} from '../../constants';
import {toDefName} from "../../Helpers/Handlebars";

const defOfTemplate = compileTemplate(__dirname, 'DefOf.cs.template');
const defOfOutputDir = resolve(SCADRIAL_MOD_DIR, 'CosmereScadrial');

export default function () {
    for (const type of ['Allomancy', 'Feruchemy'] as const) {
        const geneTemplate = compileTemplate(__dirname, type + 'GeneDef.xml.template');
        const traitTemplate = compileTemplate(__dirname, type + 'TraitDef.xml.template');
        const outputDir = resolve(SCADRIAL_MOD_DIR, 'Defs', type);

        let order = 2
        const metals = Object.values(MetalRegistry.Metals).filter(x => !x.GodMetal && (!!x[type])).concat(MetalRegistry.Metals.Atium);
        for (const metalInfo of metals) {
            writeGeneratedFile(
                resolve(outputDir, toDefName(metalInfo.Name)),
                'Gene.generated.xml',
                geneTemplate({
                    metal: metalInfo,
                    defName: metalInfo.DefName ?? toDefName(metalInfo.Name),
                    order: order++,
                }),
            );
            writeGeneratedFile(
                resolve(outputDir, toDefName(metalInfo.Name)),
                'Trait.generated.xml',
                traitTemplate({
                    metal: metalInfo,
                    defName: metalInfo.DefName ?? toDefName(metalInfo.Name),
                    order: order++,
                }),
            );
        }

        ['Gene', 'Trait'].forEach(kind => {
            writeGeneratedFile(defOfOutputDir, kind + 'DefOf.' + type + '.generated.cs', defOfTemplate({
                type: type === 'Allomancy' ? 'Misting' : 'Ferring',
                kind,
                metals
            }));
        });
    }


    ['Mistborn', 'FullFeruchemist'].forEach(type => {
        const dir = type === 'Mistborn' ? 'Allomancy' : 'Feruchemy';
        const metals = Object.values(MetalRegistry.Metals).filter(x => !x.GodMetal && (!!x[dir])).concat(MetalRegistry.Metals.Atium);
        const template = compileTemplate(__dirname, type + '.xml.template');
        const {abilities, rightClickAbilities} = Object.values(MetalRegistry.Metals).reduce((acc, metal) => {
            const typeData = metal[dir];
            if (typeData?.Abilities) {
                acc.abilities.push(...typeData.Abilities);
            }


            return acc;
        }, {abilities: [] as string[], rightClickAbilities: [] as string[]});

        writeGeneratedFile(resolve(SCADRIAL_MOD_DIR, 'Defs', dir, type), 'Trait.generated.xml', template({
            metals,
            abilities,
            rightClickAbilities
        }));
    })
}
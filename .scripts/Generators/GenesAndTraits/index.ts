import {resolve} from 'node:path';
import {upperFirst} from "lodash";

import {compileTemplate, writeGeneratedFile} from '../../Helpers';
import {MetalRegistry} from '../../Metals/MetalRegistry';
import {SCADRIAL_MOD_DIR} from '../../constants';

const template = compileTemplate(__dirname, 'GeneAndTraitDef.xml.template');
const outputDir = resolve(SCADRIAL_MOD_DIR, 'Defs', 'Genes');

const defOfTemplate = compileTemplate(__dirname, 'DefOf.cs.template');
const defOfOutputDir = resolve(SCADRIAL_MOD_DIR, 'CosmereScadrial');

export default function () {
    let order = 2
    const metals = Object.values(MetalRegistry.Metals).filter(x => !x.GodMetal && (!!x.Allomancy || !!x.Feruchemy));
    for (const metalInfo of metals) {
        writeGeneratedFile(
            outputDir,
            upperFirst(metalInfo.Name) + '.generated.xml',
            template({
                metal: metalInfo,
                defName: metalInfo.DefName ?? upperFirst(metalInfo.Name),
                order: order++,
            }),
        );
    }

    ['Gene', 'Trait'].forEach(type => {
        writeGeneratedFile(defOfOutputDir, type + 'DefOf.generated.cs', defOfTemplate({type, metals}));
    });

    ['Mistborn', 'FullFeruchemist'].forEach(type => {
        const template = compileTemplate(__dirname, type + '.xml.template');
        const {abilities, rightClickAbilities} = Object.values(MetalRegistry.Metals).reduce((acc, metal) => {
            const typeData = metal[type === 'Mistborn' ? 'Allomancy' : 'Feruchemy'];
            if (typeData?.Abilities) {
                acc.abilities.push(...typeData.Abilities);
            }


            return acc;
        }, {abilities: [] as string[], rightClickAbilities: [] as string[]});

        writeGeneratedFile(outputDir, type + '.generated.xml', template({metals, abilities, rightClickAbilities}));
    })
}
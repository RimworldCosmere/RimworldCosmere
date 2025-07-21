import {resolve} from 'node:path';
import {compileTemplate, writeGeneratedFile} from '../../Helpers';
import {ROSHAR_MOD_DIR} from '../../constants';
import { SurgeRegistry } from '../../Models/Surges/SurgeRegistry';
import { RadiantOrderRegistry } from '../../Models/RadiantOrders/RadiantOrderRegistry';

const surgeDefTemplate = compileTemplate(__dirname, 'SurgeDef.xml.template')
const surgeDefOutputDir = resolve(ROSHAR_MOD_DIR, 'Defs', 'Surges');

const surgeDefOfTemplate = compileTemplate(__dirname, 'SurgeDefOf.cs.template')
const CosmereRoshar = resolve(ROSHAR_MOD_DIR, 'CosmereRoshar');

const radiantOrderDefTemplate = compileTemplate(__dirname, 'RadiantOrderDef.xml.template')
const radiantOrderDefOutputDir = resolve(ROSHAR_MOD_DIR, 'Defs', 'RadiantOrders');

const radiantOrderDefOfTemplate = compileTemplate(__dirname, 'RadiantOrderDefOf.cs.template')

const geneDefTemplate = compileTemplate(__dirname, 'GeneDef.xml.template')
const geneDefOutputDir = resolve(ROSHAR_MOD_DIR, 'Defs', 'Genes');

const geneDefOfTemplate = compileTemplate(__dirname, 'GeneDefOf.cs.template')
const traitDefOfTemplate = compileTemplate(__dirname, 'TraitDefOf.cs.template')

export default function () {
    const surges = Object.values(SurgeRegistry.Surges);
    for (const surge of surges) {
        writeGeneratedFile(surgeDefOutputDir, surge.name.toDefName() + '.generated.xml', surgeDefTemplate({surge}));
    }

    writeGeneratedFile(CosmereRoshar, 'SurgeDefOf.generated.cs', surgeDefOfTemplate({surges}));

    const orders = Object.values(RadiantOrderRegistry.RadiantOrders);
    for (const order of orders) {
        writeGeneratedFile(radiantOrderDefOutputDir, order.name.toDefName() + '.generated.xml', radiantOrderDefTemplate({order}));
        writeGeneratedFile(geneDefOutputDir, order.name.toDefName() + '.generated.xml', geneDefTemplate({order}));
    }

    writeGeneratedFile(CosmereRoshar, 'RadiantOrderDefOf.generated.cs', radiantOrderDefOfTemplate({orders}));
    writeGeneratedFile(CosmereRoshar, 'GeneDefOf.RadiantOrders.generated.cs', geneDefOfTemplate({orders}));
    writeGeneratedFile(CosmereRoshar, 'TraitDefOf.RadiantOrders.generated.cs', traitDefOfTemplate({orders}));
}
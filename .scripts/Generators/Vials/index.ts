import {resolve} from 'node:path';
import {upperFirst} from "lodash";

import {compileTemplate, writeGeneratedFile} from '../../Helpers';
import {MetalRegistry} from '../../Metals/MetalRegistry';
import {SCADRIAL_MOD_DIR} from '../../constants';

const template = compileTemplate(__dirname, 'VialDef.xml.template');
const outputDir = resolve(SCADRIAL_MOD_DIR, 'Defs', 'Things', 'Vials');

const thingDefOfVialTemplate = compileTemplate(__dirname, 'ThingDefOf.Vial.cs.template');
const CosmereScadrial = resolve(SCADRIAL_MOD_DIR, 'CosmereScadrial');

// Multi-Metal vials
const multiVialTemplate = compileTemplate(__dirname, 'multiVialDef.xml.template');
const Metals = Object.values(MetalRegistry.Metals).filter((x) => x.Name !== "aluminum" && x.Allomancy !== undefined && !x.GodMetal);
const multiVialGroups = [
    {
        defName: "Physical",
        name: "Physical",
        metals: Metals.filter(x => x.Allomancy!.Group === 'physical').map(x => upperFirst(x.Name)),
    },
    {
        defName: "Mental",
        name: "Mental",
        metals: Metals.filter(x => x.Allomancy!.Group === 'mental').map(x => upperFirst(x.Name)),
    },
    {
        defName: "Enhancement",
        name: "Enhancement",
        metals: Metals.filter(x => x.Allomancy!.Group === 'enhancement').map(x => upperFirst(x.Name)),
    },
    {
        defName: "Temporal",
        name: "Temporal",
        metals: Metals.filter(x => x.Allomancy!.Group === 'temporal').map(x => upperFirst(x.Name)),
    },
    {
        defName: "External",
        name: "External",
        metals: Metals.filter(x => x.Allomancy!.Axis === 'external').map(x => upperFirst(x.Name)),
    },
    {
        defName: "Internal",
        name: "Internal",
        metals: Metals.filter(x => x.Allomancy!.Axis === 'internal').map(x => upperFirst(x.Name)),
    },
    {
        defName: "Pushing",
        name: "Pushing",
        metals: Metals.filter(x => x.Allomancy!.Polarity === 'pushing').map(x => upperFirst(x.Name)),
    },
    {
        defName: "Pulling",
        name: "Pulling",
        metals: Metals.filter(x => x.Allomancy!.Polarity === 'pulling').map(x => upperFirst(x.Name)),
    },
    {
        defName: "PhysicalMental",
        name: "Physical + Mental",
        metals: Metals.filter(x => ['physical', 'mental'].includes(x.Allomancy!.Group)).map(x => upperFirst(x.Name)),
    },
    {
        defName: "EnhancementTemporal",
        name: "Enhancement + Temporal",
        metals: Metals.filter(x => ['enhancement', 'temporal'].includes(x.Allomancy!.Group)).map(x => upperFirst(x.Name)),
    },
    {
        defName: "All",
        name: "All",
        metals: Metals.filter(x => x.Allomancy?.Group !== null).map(x => upperFirst(x.Name)),
    },
];

export default function () {
    const metals = Object.values(MetalRegistry.Metals).filter(x => x.Allomancy);
    for (const metal of metals) {
        writeGeneratedFile(outputDir, upperFirst(metal.Name) + '.generated.xml', template({
            metal,
            defName: metal.DefName ?? upperFirst(metal.Name)
        }));
    }

    console.log("Generating ThingDefOf.Vial");
    writeGeneratedFile(CosmereScadrial, 'ThingDefOf.Vial.generated.cs', thingDefOfVialTemplate({metals}));

    /*for (const group of multiVialGroups) {
        writeGeneratedFile(outputDir, group.defName + '.generated.xml', multiVialTemplate(group));
    }*/
}


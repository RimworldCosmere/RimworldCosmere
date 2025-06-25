import {
    MetalAllomancyInfo,
    MetalAlloyInfo,
    MetalBuildableInfo,
    MetalFeruchemyInfo,
    MetalInfo,
    MetalMiningInfo
} from "./MetalInfo";
import {upperFirst} from 'lodash';
import {validate, ValidationError} from "jsonschema";
import metals from '../Resources/MetalRegistry.json';
import schema from '../Resources/MetalRegistry.schema.json';

export class MetalRegistry {
    public static Metals: Record<string, MetalInfo> = {};

    public static LoadMetalRegistry() {
        var result = validate(metals, schema);
        if (!result.valid) {
            console.error(result.errors);
            throw new ValidationError("Failed to validate");
        }

        MetalRegistry.Metals = metals.reduce((curr: any, metal: Record<string, any>) => {
            if (metal.disabled) return curr;

            const metalInfo = new MetalInfo({
                Name: metal.name,
                Description: metal.description,
                DefName: metal.defName,
                GodMetal: metal.godMetal ?? false,
                Stackable: metal.stackable ?? true,
                DrawSize: metal.drawSize ?? .25,
                MaxAmount: metal.maxAmount ?? 100,
                MarketValue: metal.marketValue ?? (metal.alloy ? 2 : 0.5),
                Color: metal.color,
                ColorTwo: metal.colorTwo,
                Allomancy: metal.allomancy ? new MetalAllomancyInfo({
                    Axis: metal.allomancy.axis ?? 'None',
                    Description: metal.allomancy.description,
                    Group: metal.allomancy.group ?? 'None',
                    Polarity: metal.allomancy.polarity ?? 'None',
                    UserName: metal.allomancy.userName,
                    Abilities: metal.allomancy.abilities ?? [],
                }) : undefined,
                Feruchemy: metal.feruchemy ? new MetalFeruchemyInfo({
                    Description: metal.feruchemy.description,
                    Group: metal.feruchemy.group ?? 'None',
                    UserName: metal.feruchemy.userName,
                    Abilities: metal.feruchemy.abilities ?? [],
                }) : undefined,
                Buildable: metal.buildable ? new MetalBuildableInfo({
                    Buildings: metal.buildable.buildings,
                    Items: metal.buildable.items,
                    Commonality: metal.buildable.commonality,
                }) : undefined,
                Alloy: metal.alloy ? new MetalAlloyInfo(metal.alloy.ingredients, metal.alloy.products) : undefined,
                Mining: metal.mining ? new MetalMiningInfo({
                    Description: metal.mining.description,
                    HitPoints: metal.mining.hitPoints,
                    Yield: metal.mining.yield,
                    Commonality: metal.mining.commonality,
                    SizeRange: metal.mining.sizeRange,
                }) : undefined,
            });

            curr[upperFirst(metal.name)] = metalInfo;

            return curr;
        }, MetalRegistry.Metals);
    }
}
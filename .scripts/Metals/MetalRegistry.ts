import {MetalAllomancyInfo, MetalAlloyInfo, MetalBuildableInfo, MetalFeruchemyInfo, MetalInfo, MetalMiningInfo} from "./MetalInfo";
import {upperFirst} from 'lodash';
import * as metals from '../Resources/MetalRegistry.json';

export class MetalRegistry {
  public static Metals: Record<string, MetalInfo> = {};

  public static LoadMetalRegistry() {
    MetalRegistry.Metals = metals.reduce((curr: any, metal: Record<string, any>) => {
      const metalInfo = new MetalInfo({
        Name: metal.name,
        Description: metal.description,
        DefName: metal.defName,
        GodMetal: metal.godMetal ?? false,
        MaxAmount: metal.maxAmount ?? 100,
        Color: metal.color,
        ColorTwo: metal.colorTwo,
        Allomancy: metal.allomancy ? new MetalAllomancyInfo({
          Axis: metal.allomancy.axis,
          Description: metal.allomancy.description,
          Group: metal.allomancy.group,
          Polarity: metal.allomancy.polarity,
          UserName: metal.allomancy.userName,
          Abilities: metal.allomancy.abilities ?? [],
        }) : undefined,
        Feruchemy: metal.feruchemy ? new MetalFeruchemyInfo({
          Description: metal.feruchemy.description,
          Group: metal.feruchemy.group,
          UserName: metal.feruchemy.userName,
          Abilities: metal.feruchemy.abilities ?? [],
        }) : undefined,
        Buildable: metal.buildable ? new MetalBuildableInfo({
          Buildings: metal.buildable.buildings,
          Items: metal.buildable.items,
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
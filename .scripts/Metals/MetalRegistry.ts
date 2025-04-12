import {MetalAllomancyInfo, MetalFeruchemyInfo, MetalInfo} from "./MetalInfo";
import {upperFirst} from 'lodash';
import * as metals from '../Resources/MetalRegistry.json';

export class MetalRegistry {
  static Metals: Record<string, MetalInfo> = {};

  public static LoadMetalRegistry() {
    MetalRegistry.Metals = metals.reduce((curr: any, metal: Record<string, any>) => {
      const metalInfo = new MetalInfo({
        Name: metal.name,
        DefName: metal.defName,
        GodMetal: metal.godMetal ?? false,
        MaxAmount: metal.maxAmount ?? 100,
        Color: metal.color,
        Allomancy: metal.allomancy ? new MetalAllomancyInfo({
          Axis: metal.allomancy.axis,
          Description: metal.allomancy.description,
          Group: metal.allomancy.group,
          Polarity: metal.allomancy.polarity,
          UserName: metal.allomancy.userName,
        }) : undefined,
        Feruchemy: metal.feruchemy ? new MetalFeruchemyInfo({
          Description: metal.feruchemy.description,
          Group: metal.feruchemy.group,
          UserName: metal.feruchemy.userName,
        }) : undefined,
      });

      curr[upperFirst(metal.name)] = metalInfo;

      return curr;
    }, MetalRegistry.Metals);
  }
}
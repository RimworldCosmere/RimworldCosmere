import {resolve} from 'node:path';
import {MetalAllomancyInfo, MetalFeruchemyInfo, MetalInfo} from "./MetalInfo";
import {readFileSync} from "fs";
import {XMLParser} from "fast-xml-parser";
import {upperFirst} from 'lodash';

export class MetalRegistry {
  static Metals: Record<string, MetalInfo> = {};

  public static LoadMetalRegistry() {
    const filename = resolve('..', 'Resources', 'MetalRegistry.xml');
    const xml = readFileSync(filename, 'utf-8');
    const parser = new XMLParser();
    const result = parser.parse(xml, false);

    MetalRegistry.Metals = result.MetalRegistry.metals.li.reduce((curr: any, metal: Record<string, any>) => {
      const metalInfo = new MetalInfo({
        Name: metal.name,
        DefName: metal.defName,
        GodMetal: metal.godMetal ?? false,
        MaxAmount: metal.maxAmount ?? 100,
        Color: metal.color.replace('(', '').replace(')', '').replaceAll(' ', '').split(','),
        Allomancy: new MetalAllomancyInfo({
          Axis: metal.allomancy.axis,
          Description: metal.allomancy.description,
          Group: metal.allomancy.group,
          Polarity: metal.allomancy.polarity,
          UserName: metal.allomancy.userName,
        }),
        Feruchemy: new MetalFeruchemyInfo({
          Description: metal.feruchemy.description,
          Group: metal.feruchemy.group,
          UserName: metal.feruchemy.userName,
        }),
      });

      curr[upperFirst(metal.name)] = metalInfo;
      
      return curr;
    }, MetalRegistry.Metals);
  }
}
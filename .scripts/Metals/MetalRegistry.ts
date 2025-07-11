import {
    MetalAllomancyInfo,
    MetalAlloyInfo,
    MetalBuildableInfo,
    MetalFeruchemyInfo,
    MetalInfo,
    MetalMiningInfo
} from "./MetalInfo";
import {upperFirst} from 'lodash';
import {loadAllJsonSync} from "../Helpers";
import {resolve} from "node:path";

export class MetalRegistry {
    public static Metals: Record<string, MetalInfo> = {};

    public static LoadMetalRegistry() {
        const metals = loadAllJsonSync(resolve(__dirname, '..', 'Resources', 'Metals'));
        MetalRegistry.Metals = metals.reduce((curr: any, metal: Record<string, any>) => {
            if (metal.disabled) return curr;

            curr[upperFirst(metal.name)] = new MetalInfo({
                ...metal,
                allomancy: metal.allomancy ? new MetalAllomancyInfo(metal.allomancy) : undefined,
                feruchemy: metal.feruchemy ? new MetalFeruchemyInfo(metal.feruchemy) : undefined,
                buildable: metal.buildable ? new MetalBuildableInfo(metal.buildable) : undefined,
                alloy: metal.alloy ? new MetalAlloyInfo(metal.alloy) : undefined,
                mining: metal.mining ? new MetalMiningInfo(metal.mining) : undefined,
            });

            return curr;
        }, MetalRegistry.Metals);
    }
}
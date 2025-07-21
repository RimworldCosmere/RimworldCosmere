import {AllomancyInfo, AlloyInfo, BuildableInfo, FeruchemyInfo, MetalInfo, MiningInfo} from "./MetalInfo";
import {upperFirst} from 'lodash';
import {loadAllData} from "../../Helpers";

export class MetalRegistry {
    public static Metals: Record<string, MetalInfo> = {};

    public static LoadRegistry() {
        const metals = loadAllData('Metals');
        MetalRegistry.Metals = metals.reduce((curr: any, metal: Record<string, any>) => {
            if (metal.disabled) return curr;

            curr[upperFirst(metal.name)] = new MetalInfo({
                ...metal,
                allomancy: metal.allomancy ? new AllomancyInfo(metal.allomancy) : undefined,
                feruchemy: metal.feruchemy ? new FeruchemyInfo(metal.feruchemy) : undefined,
                buildable: metal.buildable ? new BuildableInfo(metal.buildable) : undefined,
                alloy: metal.alloy ? new AlloyInfo(metal.alloy) : undefined,
                mining: metal.mining ? new MiningInfo(metal.mining) : undefined,
            });

            return curr;
        }, MetalRegistry.Metals);
    }
}
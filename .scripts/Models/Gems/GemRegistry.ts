import {GemInfo, MiningInfo} from "./GemInfo";
import {upperFirst} from 'lodash';
import {loadAllData} from "../../Helpers";

export class GemRegistry {
    public static Gems: Record<string, GemInfo> = {};

    public static LoadRegistry() {
        const gems = loadAllData('Gems');
        GemRegistry.Gems = gems.reduce((curr: any, gem: Record<string, any>) => {
            if (gem.disabled) return curr;

            curr[upperFirst(gem.name)] = new GemInfo({
                ...gem,
                mining: gem.mining ? new MiningInfo(gem.mining) : undefined,
            });

            return curr;
        }, GemRegistry.Gems);
    }
}
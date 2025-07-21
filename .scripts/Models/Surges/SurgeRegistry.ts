import {SurgeInfo} from "./SurgeInfo";
import {upperFirst} from 'lodash';
import {loadAllData} from "../../Helpers";

export class SurgeRegistry {
    public static Surges: Record<string, SurgeInfo> = {};

    public static LoadRegistry() {
        const surges = loadAllData('Surges');
        SurgeRegistry.Surges = surges.reduce((curr: any, surge: Record<string, any>) => {
            if (surge.disabled) return curr;

            curr[upperFirst(surge.name)] = new SurgeInfo(surge);

            return curr;
        }, SurgeRegistry.Surges);
    }
}
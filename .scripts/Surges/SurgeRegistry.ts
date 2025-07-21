import {SurgeInfo} from "./SurgeInfo";
import {upperFirst} from 'lodash';
import {loadAllJsonSync} from "../Helpers";
import {resolve} from "node:path";

export class SurgeRegistry {
    public static Surges: Record<string, SurgeInfo> = {};

    public static LoadRegistry() {
        const surges = loadAllJsonSync(resolve(__dirname, '..', 'Resources', 'Surges'));
        SurgeRegistry.Surges = surges.reduce((curr: any, surge: Record<string, any>) => {
            if (surge.disabled) return curr;

            curr[upperFirst(surge.name)] = new SurgeInfo(surge);

            return curr;
        }, SurgeRegistry.Surges);
    }
}
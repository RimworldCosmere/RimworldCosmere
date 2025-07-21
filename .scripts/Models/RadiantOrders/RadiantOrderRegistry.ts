import {RadiantOrderIdeal, RadiantOrderInfo} from "./RadiantOrderInfo";
import {upperFirst} from 'lodash';
import {loadAllData} from "../../Helpers";

export class RadiantOrderRegistry {
    public static RadiantOrders: Record<string, RadiantOrderInfo> = {};

    public static LoadRegistry() {
        const radiantOrders = loadAllData('RadiantOrders');
        RadiantOrderRegistry.RadiantOrders = radiantOrders.reduce((curr: any, radiantOrder: Record<string, any>) => {
            if (radiantOrder.disabled) return curr;

            curr[upperFirst(radiantOrder.name)] = new RadiantOrderInfo({
                ...radiantOrder,
                ideals: radiantOrder.ideals.map((i: any) => new RadiantOrderIdeal(i))
            });

            return curr;
        }, RadiantOrderRegistry.RadiantOrders);
    }
}
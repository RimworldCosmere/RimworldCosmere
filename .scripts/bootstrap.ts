import {shouldSkipGeneration} from './cache';
import './Helpers/Handlebars';
import {MetalRegistry} from './Metals/MetalRegistry';
import {GemRegistry} from "./Gems/GemRegistry";
import {SurgeRegistry} from "./Surges/SurgeRegistry";
import {RadiantOrderRegistry} from "./RadiantOrders/RadiantOrderRegistry";

export async function bootstrap(name: string) {
    MetalRegistry.LoadRegistry();
    GemRegistry.LoadRegistry();
    SurgeRegistry.LoadRegistry();
    RadiantOrderRegistry.LoadRegistry();

    const generator = await import((`./Generators/${name}`)).then((x) => x.default);

    return {generator, shouldSkip: shouldSkipGeneration(name)}
}
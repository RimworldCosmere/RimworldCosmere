import {shouldSkipGeneration} from './cache';
import './Helpers/Handlebars';
import { GemRegistry } from './Models/Gems/GemRegistry';
import { MetalRegistry } from './Models/Metals/MetalRegistry';
import { RadiantOrderRegistry } from './Models/RadiantOrders/RadiantOrderRegistry';
import { SurgeRegistry } from './Models/Surges/SurgeRegistry';

export async function bootstrap(name: string) {
    MetalRegistry.LoadRegistry();
    GemRegistry.LoadRegistry();
    SurgeRegistry.LoadRegistry();
    RadiantOrderRegistry.LoadRegistry();

    const generator = await import((`./Generators/${name}`)).then((x) => x.default);

    return {generator, shouldSkip: shouldSkipGeneration(name)}
}
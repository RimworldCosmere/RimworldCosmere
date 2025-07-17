import {shouldSkipGeneration} from './cache';
import './Helpers/Handlebars';
import {MetalRegistry} from './Metals/MetalRegistry';
import {GemRegistry} from "./Gems/GemRegistry";

export async function bootstrap(name: string) {
    MetalRegistry.LoadRegistry();
    GemRegistry.LoadRegistry();

    const generator = await import((`./Generators/${name}`)).then((x) => x.default);

    return {generator, shouldSkip: shouldSkipGeneration(name)}
}
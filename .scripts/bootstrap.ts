import {shouldSkipGeneration} from './cache';
import './Helpers/Handlebars';
import {MetalRegistry} from './Metals/MetalRegistry';

export async function bootstrap(name: string) {
    MetalRegistry.LoadMetalRegistry();

    const generator = await import((`./Generators/${name}`)).then((x) => x.default);

    return {generator, shouldSkip: shouldSkipGeneration(name)}
}
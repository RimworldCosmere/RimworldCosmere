import { shouldSkipGeneration } from './cache';
import './Helpers/Handlebars';
import { MetalRegistry } from './Metals/MetalRegistry';

export async function bootstrap() {
  MetalRegistry.LoadMetalRegistry();
  
  const genName = process.argv[2];
  const generator = await import((`./Generators/${genName}`)).then((x) => x.default);
  
  return {generator, genName, shouldSkip: shouldSkipGeneration(genName)}
}
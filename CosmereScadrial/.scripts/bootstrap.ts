import './Helpers/Handlebars';
import { MetalRegistry } from './Metals/MetalRegistry';
import {resolve} from "node:path";

export const MOD_DIR = resolve(__dirname, '..');

export function bootstrap() {
  MetalRegistry.LoadMetalRegistry();
}
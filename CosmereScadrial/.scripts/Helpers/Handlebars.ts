import Handlebars from 'handlebars';
import {toLower, upperFirst} from "lodash";
import {MetalRegistry} from "../Metals/MetalRegistry";

Handlebars.registerHelper('lower', (string: string) => {
  return toLower(string);
})

Handlebars.registerHelper('capitalize', (string: string) => {
  return upperFirst(string);
});
import Handlebars, { HelperOptions } from 'handlebars';
import {toLower, upperFirst} from "lodash";

Handlebars.registerHelper('lower', (string: string) => {
  return toLower(string);
});
Handlebars.registerHelper('capitalize', (string: string) => {
  return upperFirst(string);
});
Handlebars.registerHelper('join', (strings: (number|string)[], character: string) => {
  return strings?.join(character);
});
Handlebars.registerHelper('rgb', (color: [number, number, number]) => {
  return `(${color.join(', ')})`;
});
Handlebars.registerHelper('rgba', (color: [number, number, number], alpha: number) => {
  return `(${color.join(', ')}, ${alpha})`;
});
Handlebars.registerHelper('range', (range: [number, number]) => {
  return `${range.join("~")}`;
});
Handlebars.registerHelper('isdefined', function (value) {
  return value !== undefined;
});
Handlebars.registerHelper('count', function (value: Array<any>) {
  return value.length;
});
Handlebars.registerHelper('log', function (value: any) {
  return console.log(value);
});
Handlebars.registerHelper('fallback', function (value: any, fallback: any) {
  console.log('Fallback: ', {value, fallback});
  return value === undefined ? fallback : value;
});
Handlebars.registerHelper('ifEquals', function(arg1: any, arg2: any, options: HelperOptions) {
  return (arg1 == arg2) ? options.fn(this) : options.inverse(this);
});
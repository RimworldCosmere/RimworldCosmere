import Handlebars from 'handlebars';
import {toLower, upperFirst} from "lodash";

require('handlebars-helpers')();

export const toTitleCase = (string: string) => string
    .toLowerCase()
    .split(' ')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
export const toDefName = (string: string) => toTitleCase(string).replace(/\s/g, '');

Handlebars.registerHelper('lower', (string: string) => {
    return toLower(string);
});
Handlebars.registerHelper('capitalize', (string: string) => {
    return upperFirst(string);
});
Handlebars.registerHelper('title', (string: string) => {
    return toTitleCase(string);
});
Handlebars.registerHelper('defName', (string: string) => {
    return toDefName(string);
});
Handlebars.registerHelper('join', (strings: (number | string)[], character: string) => {
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
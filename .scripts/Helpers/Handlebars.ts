import Handlebars from 'handlebars';
import {toLower, upperFirst} from "lodash";

require('handlebars-helpers')();

Handlebars.registerHelper('log', (...args: any[]) => console.log(...args));

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
Handlebars.registerHelper('gt', function (valueOne: number, valueTwo: number, block) {
    return valueOne > valueTwo ? block.fn() : null;
});
Handlebars.registerHelper('count', function (value: Array<any>) {
    return value.length;
});
Handlebars.registerHelper('log', function (value: any) {
    return console.log(value);
});
Handlebars.registerHelper('multiply', function (valueOne: number, valueTwo: number) {
    return Number((valueOne * valueTwo).toFixed(6));
});
Handlebars.registerHelper('add', function (valueOne: number, valueTwo: number) {
    return valueOne + valueTwo;
});
Handlebars.registerHelper('getStatForStage', function (stage: number, step: number) {
    return Number((1 + step * (stage + 1)).toFixed(8));
});
Handlebars.registerHelper('fallback', function (value: any, fallback: any) {
    console.log('Fallback: ', {value, fallback});
    return value === undefined ? fallback : value;
});
Handlebars.registerHelper('times', function (n, block) {
    let accum = '';
    for (let i = 0; i < n; ++i) {
        block.data.time = i;
        accum += block.fn(i);
    }
    return accum;
});
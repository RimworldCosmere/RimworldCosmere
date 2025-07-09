import Handlebars from 'handlebars';

require('handlebars-helpers')();

declare global {
    interface String {
        toTitleCase(): string;

        toDefName(): string;

        capitalize(): string;
    }
}

String.prototype.toTitleCase = function (this: string): string {
    return this
        .toLowerCase()
        .split(' ')
        .map(word => word.charAt(0).toUpperCase() + word.slice(1))
        .join(' ');
}
String.prototype.toDefName = function (this: string): string {
    return this.toTitleCase().replace(/\s/g, '');
}
String.prototype.capitalize = function (this: string): string {
    return this.charAt(0).toUpperCase() + this.slice(1);
}

Handlebars.registerHelper('log', (...args: any[]) => console.log(...args));
Handlebars.registerHelper('lower', (string: string) => {
    return string.toLowerCase();
});
Handlebars.registerHelper('capitalize', (string: string) => {
    return string.capitalize();
});
Handlebars.registerHelper('title', (string: string) => {
    return string.toTitleCase();
});
Handlebars.registerHelper('defName', (string: string) => {
    return string.toDefName();
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
Handlebars.registerHelper({
    eq: (v1, v2) => v1 === v2,
    ne: (v1, v2) => v1 !== v2,
    lt: (v1, v2) => v1 < v2,
    gt: (v1, v2) => v1 > v2,
    lte: (v1, v2) => v1 <= v2,
    gte: (v1, v2) => v1 >= v2,
    and() {
        return Array.prototype.every.call(arguments, Boolean);
    },
    or() {
        return Array.prototype.slice.call(arguments, 0, -1).some(Boolean);
    }
});
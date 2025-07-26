export class RadiantOrderInfo {
    public name: string;
    public descriptions: { raw: string; cut: string; sphere: string; mining?: string; };
    public color: [number, number, number];
    public surges: string[];
    public gemstone: string;
    public ideals: RadiantOrderIdeal[];

    constructor(self: Partial<RadiantOrderInfo>) {
        Object.assign(this, self);
    }
}

export class RadiantOrderIdeal {
    public label: string;
    public description: string;
    public quotes: string[];
    public abilities: string[];

    constructor(self: Partial<RadiantOrderIdeal>) {
        Object.assign(this, self);
    }
}
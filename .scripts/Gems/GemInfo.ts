export type AllomancyGroup = 'physical' | 'mental' | 'enhancement' | 'temporal';
export type FeruchemyGroup = 'physical' | 'cognitive' | 'spiritual' | 'hybrid';
export type Axis = 'internal' | 'external';
export type Polarity = 'pushing' | 'pulling';

export class GemInfo {
    public name: string;
    public descriptions: { raw: string; cut: string; sphere: string; mining?: string; };
    public color: [number, number, number];
    public colorTwo?: [number, number, number];
    public glowColor: [number, number, number];
    public stackable: boolean = false;
    public drawSize: number = 1;
    public baseBeauty: number;
    public baseMarketValue: number;
    public baseStormlight: number;
    public mining?: MiningInfo;

    constructor(self: Partial<GemInfo>) {
        Object.assign(this, self);
    }
}

export class MiningInfo {
    public hitPoints: number;
    public yield: number;
    public commonality: number;
    public sizeRange: [number, number];

    constructor(self: Partial<MiningInfo>) {
        Object.assign(this, self);
    }
}
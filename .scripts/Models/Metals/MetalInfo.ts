export type AllomancyGroup = 'physical' | 'mental' | 'enhancement' | 'temporal';
export type FeruchemyGroup = 'physical' | 'cognitive' | 'spiritual' | 'hybrid';
export type Axis = 'internal' | 'external';
export type Polarity = 'pushing' | 'pulling';

export class MetalInfo {
    public name: string;
    public description: string;
    public defName?: string;
    public color: [number, number, number];
    public colorTwo?: [number, number, number];
    public godMetal: boolean = false;
    public stackable: boolean = true;
    public drawSize: number = 1;
    public beauty: number;
    public marketValue: number;
    public maxAmount: number = 100;
    public genesToGrant: Record<string, string[]> = {};
    public allomancy?: AllomancyInfo
    public feruchemy?: FeruchemyInfo;
    public buildable?: BuildableInfo;
    public mining?: MiningInfo;
    public alloy?: AlloyInfo;

    constructor(self: Partial<MetalInfo>) {
        Object.assign(this, self);
    }
}

export class AllomancyInfo {
    public userName?: string;
    public description: string;
    public group: AllomancyGroup | 'None' = 'None';
    public axis: Axis | 'None' = 'None';
    public polarity: Polarity | 'None' = 'None';
    public abilities: string[] = [];

    constructor(self: Partial<AllomancyInfo>) {
        Object.assign(this, self);
    }
}

export class FeruchemyInfo {
    public userName?: string;
    public description: string;
    public group: FeruchemyGroup | 'None' = 'None';
    public abilities: string[] = [];
    public attribute?: string;
    public customHediffClass: boolean = false;
    public store?: FeruchemyAbilityInfo;
    public tap?: FeruchemyAbilityInfo;

    constructor(self: Partial<FeruchemyInfo>) {
        Object.assign(this, self);
        this.store = self.store ? new FeruchemyAbilityInfo(self.store) : undefined;
        this.tap = self.tap ? new FeruchemyAbilityInfo(self.tap) : undefined;
    }
}

export class FeruchemyAbilityInfo {
    public description: string;
    public stages: number = 20;
    public multiplyBySeverity: boolean = false;
    public capacityMods: Record<string, { factor?: number; offset?: number }>;
    public statOffsets?: Record<string, number>;
    public statFactors?: Record<string, number>;
    public hungerRateFactor?: number;

    constructor(self: Partial<FeruchemyAbilityInfo>) {
        Object.assign(this, self);
    }
}

export class BuildableInfo {
    public commonality: number;
    public defense?: {
        sharp?: number;
        blunt?: number;
        heat?: number;
        coldInsulation?: number;
        heatInsulation?: number;
    };
    public offense?: {
        sharp?: number;
        blunt?: number;
        armorPenetration?: number;
        cooldown?: number;
    }
    public stuffStatFactors?: Record<string, number>;

    constructor(self: Partial<BuildableInfo>) {
        Object.assign(this, self);
    }
}

export class MiningInfo {
    public description?: string;
    public hitPoints: number;
    public yield: number;
    public commonality: number;
    public sizeRange: [number, number];

    constructor(self: Partial<MiningInfo>) {
        Object.assign(this, self);
    }
}

interface AlloyInput {
    type: 'simple' | 'complex' | 'god';
    ingredients: {
        item: string | string[];
        count: number;
    }[];
    stuff?: string | string[];
    stuffCount?: number;
    product: {
        item?: string;
        count: number;
    };
}

export class AlloyInfo {
    public ingredients: AlloyIngredient[];
    public stuff?: string[];
    public stuffCount?: number;
    public product: AlloyProduct;
    public type: 'simple' | 'complex' | 'god';

    public constructor({type, ingredients, stuff, stuffCount, product = {count: 10}}: AlloyInput) {
        this.type = type;
        this.ingredients = ingredients.map(i => new AlloyIngredient({
            items: i.item ? (Array.isArray(i.item) ? i.item : [i.item]) : undefined,
            count: i.count,
        }));
        this.product = new AlloyProduct({
            item: product.item,
            count: product.count,
        });
        this.stuff = stuff ? (Array.isArray(stuff) ? stuff : [stuff]) : undefined;
        this.stuffCount = stuffCount;
    }
}

export class AlloyIngredient {
    public items?: string[];
    public stuffs?: string[];
    public count: number;

    constructor(self: Partial<AlloyIngredient>) {
        Object.assign(this, self);
    }
}

export class AlloyProduct {
    public item: string;
    public count: number;

    constructor(self: Partial<AlloyProduct>) {
        Object.assign(this, self);
    }
}
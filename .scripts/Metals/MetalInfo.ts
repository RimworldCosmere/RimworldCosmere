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
    public allomancy?: MetalAllomancyInfo
    public feruchemy?: MetalFeruchemyInfo;
    public buildable?: MetalBuildableInfo;
    public mining?: MetalMiningInfo;
    public alloy?: MetalAlloyInfo;

    constructor(self: Partial<MetalInfo>) {
        Object.assign(this, self);
    }
}

export class MetalAllomancyInfo {
    public userName?: string;
    public description: string;
    public group: AllomancyGroup | 'None' = 'None';
    public axis: Axis | 'None' = 'None';
    public polarity: Polarity | 'None' = 'None';
    public abilities: string[] = [];

    constructor(self: Partial<MetalAllomancyInfo>) {
        Object.assign(this, self);
    }
}

export class MetalFeruchemyInfo {
    public userName?: string;
    public description: string;
    public group: FeruchemyGroup | 'None' = 'None';
    public abilities: string[] = [];
    public attribute?: string;
    public customHediffClass: boolean = false;
    public store?: FeruchemyAbilityInfo;
    public tap?: FeruchemyAbilityInfo;

    constructor(self: Partial<MetalFeruchemyInfo>) {
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

export class MetalBuildableInfo {
    public buildings: boolean;
    public items: boolean;
    public commonality: number;

    constructor(self: Partial<MetalBuildableInfo>) {
        Object.assign(this, self);
    }
}

export class MetalMiningInfo {
    public description?: string;
    public hitPoints: number;
    public yield: number;
    public commonality: number;
    public sizeRange: [number, number];

    constructor(self: Partial<MetalMiningInfo>) {
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

export class MetalAlloyInfo {
    public ingredients: MetalAlloyIngredient[];
    public stuff?: string[];
    public stuffCount?: number;
    public product: MetalAlloyProduct;
    public type: 'simple' | 'complex' | 'god';

    public constructor({type, ingredients, stuff, stuffCount, product = {count: 10}}: AlloyInput) {
        this.type = type;
        this.ingredients = ingredients.map(i => new MetalAlloyIngredient({
            items: i.item ? (Array.isArray(i.item) ? i.item : [i.item]) : undefined,
            count: i.count,
        }));
        this.product = new MetalAlloyProduct({
            item: product.item,
            count: product.count,
        });
        this.stuff = stuff ? (Array.isArray(stuff) ? stuff : [stuff]) : undefined;
        this.stuffCount = stuffCount;
    }
}

export class MetalAlloyIngredient {
    public items?: string[];
    public stuffs?: string[];
    public count: number;

    constructor(self: Partial<MetalAlloyIngredient>) {
        Object.assign(this, self);
    }
}

export class MetalAlloyProduct {
    public item: string;
    public count: number;

    constructor(self: Partial<MetalAlloyProduct>) {
        Object.assign(this, self);
    }
}
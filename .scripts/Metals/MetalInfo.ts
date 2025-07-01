export type AllomancyGroup = 'physical' | 'mental' | 'enhancement' | 'temporal';
export type FeruchemyGroup = 'physical' | 'cognitive' | 'spiritual' | 'hybrid';
export type Axis = 'internal' | 'external';
export type Polarity = 'pushing' | 'pulling';

export class MetalInfo {
    public Name: string;
    public Description: string;
    public DefName?: string;
    public Color: [number, number, number];
    public ColorTwo?: [number, number, number];
    public GodMetal: boolean;
    public Stackable: boolean = true;
    public DrawSize: number = 1;
    public MarketValue: number;
    public MaxAmount: number = 100;
    public Allomancy?: MetalAllomancyInfo
    public Feruchemy?: MetalFeruchemyInfo;
    public Buildable?: MetalBuildableInfo;
    public Mining?: MetalMiningInfo;
    public Alloy?: MetalAlloyInfo;

    constructor(self: Partial<MetalInfo>) {
        Object.assign(this, self);
    }
}

export class MetalAllomancyInfo {
    public UserName: string;
    public Description: string;
    public Group: AllomancyGroup;
    public Axis: Axis;
    public Polarity: Polarity;
    public Abilities: string[];

    constructor(self: Partial<MetalAllomancyInfo>) {
        Object.assign(this, self);
    }
}

export class MetalFeruchemyInfo {
    public UserName: string;
    public Description: string;
    public Group: FeruchemyGroup;
    public Abilities: string[];

    constructor(self: Partial<MetalFeruchemyInfo>) {
        Object.assign(this, self);
    }
}

export class MetalBuildableInfo {
    public Buildings: boolean;
    public Items: boolean;
    public Commonality: number;

    constructor(self: Partial<MetalBuildableInfo>) {
        Object.assign(this, self);
    }
}

export class MetalMiningInfo {
    public Description?: string;
    public HitPoints: number;
    public Yield: number;
    public Commonality: number;
    public SizeRange: [number, number];

    constructor(self: Partial<MetalMiningInfo>) {
        Object.assign(this, self);
    }
}

interface AlloyInput {
    type: 'simple' | 'complex';
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
    public Ingredients: MetalAlloyIngredient[];
    public Stuff?: string[];
    public StuffCount?: number;
    public Product: MetalAlloyProduct;
    public Type: 'simple' | 'complex';

    public constructor({type, ingredients, stuff, stuffCount, product = {count: 10}}: AlloyInput) {
        this.Type = type;
        this.Ingredients = ingredients.map(i => new MetalAlloyIngredient({
            Items: i.item ? (Array.isArray(i.item) ? i.item : [i.item]) : undefined,
            Count: i.count,
        }));
        this.Product = new MetalAlloyProduct({
            Item: product.item,
            Count: product.count,
        });
        this.Stuff = stuff ? (Array.isArray(stuff) ? stuff : [stuff]) : undefined;
        this.StuffCount = stuffCount;
    }
}

export class MetalAlloyIngredient {
    public Items?: string[];
    public Stuffs?: string[];
    public Count: number;

    constructor(self: Partial<MetalAlloyIngredient>) {
        Object.assign(this, self);
    }
}

export class MetalAlloyProduct {
    public Item: string;
    public Count: number;

    constructor(self: Partial<MetalAlloyProduct>) {
        Object.assign(this, self);
    }
}
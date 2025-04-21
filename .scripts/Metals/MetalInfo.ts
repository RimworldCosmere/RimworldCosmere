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

export class MetalAlloyInfo {
  public Ingredients: MetalAlloyIngredient[];
  public Products: MetalAlloyProduct[];
  public constructor(ingredients: Array<{item: string | string[], count: number}>, products: {item?: string; count: number}[] = [{count: 10}]) {
    this.Ingredients = ingredients.map(i => new MetalAlloyIngredient({
      Items: Array.isArray(i.item) ? i.item : [i.item],
      Count: i.count,
    }));
    this.Products = products.map(p => new MetalAlloyProduct({
      Item: p.item,
      Count: p.count,
    }))
  }
}

export class MetalAlloyIngredient {
  public Items: string[];
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
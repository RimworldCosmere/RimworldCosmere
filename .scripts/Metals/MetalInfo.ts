export type AllomancyGroup = 'physical' | 'mental' | 'enhancement' | 'temporal';
export type FeruchemyGroup = 'physical' | 'cognitive' | 'spiritual' | 'hybrid';
export type Axis = 'internal' | 'external';
export type Polarity = 'pushing' | 'pulling';

export class MetalInfo {
  public Allomancy: MetalAllomancyInfo
  public Color: [number, number, number];
  public DefName?: string;
  public Feruchemy: MetalFeruchemyInfo;
  public GodMetal: boolean;
  public MaxAmount: number = 100;
  public Name: string;
  constructor(self: Partial<MetalInfo>) {
    Object.assign(this, self);
  }
}

export class MetalAllomancyInfo {
  public Axis: Axis;
  public Description: string;
  public Group: AllomancyGroup;
  public Polarity: Polarity;
  public UserName: string;
  constructor(self: Partial<MetalAllomancyInfo>) {
    Object.assign(this, self);
  }
}

export class MetalFeruchemyInfo {
  public Description: string;
  public Group: FeruchemyGroup;
  public UserName: string;
  constructor(self: Partial<MetalFeruchemyInfo>) {
    Object.assign(this, self);
  }
}
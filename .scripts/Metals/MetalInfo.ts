export type AllomancyGroup = 'physical' | 'mental' | 'enhancement' | 'temporal';
export type FeruchemyGroup = 'physical' | 'cognitive' | 'spiritual' | 'hybrid';
export type Axis = 'internal' | 'external';
export type Polarity = 'pushing' | 'pulling';

export class MetalInfo {
  public Name: string;
  public DefName?: string;
  public Color: [number, number, number];
  public GodMetal: boolean;
  public MaxAmount: number = 100;
  public Allomancy?: MetalAllomancyInfo
  public Feruchemy?: MetalFeruchemyInfo;
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
  constructor(self: Partial<MetalAllomancyInfo>) {
    Object.assign(this, self);
  }
}

export class MetalFeruchemyInfo {
  public UserName: string;
  public Description: string;
  public Group: FeruchemyGroup;
  constructor(self: Partial<MetalFeruchemyInfo>) {
    Object.assign(this, self);
  }
}
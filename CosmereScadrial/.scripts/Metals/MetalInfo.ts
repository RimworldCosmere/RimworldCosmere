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
  public Axis: string;
  public Description: string;
  public Group: string;
  public Polarity: string;
  public UserName: string;
  constructor(self: Partial<MetalAllomancyInfo>) {
    Object.assign(this, self);
  }
}

export class MetalFeruchemyInfo {
  public Description: string;
  public Group: string;
  public UserName: string;
  constructor(self: Partial<MetalFeruchemyInfo>) {
    Object.assign(this, self);
  }
}
export class SprenInfo {
    public name: string;
    public description: string;
    public radiantOrder: string;
    public color?: Color;
    public colorTwo?: Color;
    public glowColor: Color;

    constructor(self: Partial<SprenInfo>) {
        Object.assign(this, self);
    }
}
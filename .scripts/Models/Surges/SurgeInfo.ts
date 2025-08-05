export class SurgeInfo {
    public name: string;
    public description: string;
    public abilityDescriptions: string[];
    public abilities: string[];

    constructor(self: Partial<SurgeInfo>) {
        Object.assign(this, self);
    }
}
export class SurgeInfo {
    public name: string;
    public descriptions: { raw: string; cut: string; sphere: string; mining?: string; };
    public abilities: string[];

    constructor(self: Partial<SurgeInfo>) {
        Object.assign(this, self);
    }
}
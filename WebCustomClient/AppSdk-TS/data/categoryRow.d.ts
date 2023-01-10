export declare class CategoryRow {
    Id: number;
    Name: string;
    Description: string;
    BadgeDesign: string;
    AccessProfileId?: number;
    IsVisitor: boolean;
    DefaultValidityPeriod: string;
    Parameters: string;
    constructor(p?: Partial<CategoryRow>);
    /**
     * Return validity period from now on
     * @returns start and end date from now on
     */
    getValidityPeriod(): {
        start: Date;
        end: Date;
    };
}

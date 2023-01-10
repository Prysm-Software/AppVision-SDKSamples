export declare class ProfileRow {
    constructor(user: Partial<ProfileRow>);
    Id: number;
    Name: string;
    Description: string;
    IsAdmin: boolean;
    DbOptions: string;
    DbRoles: string;
    CommandExceptions: string;
    Parameters: string;
    Filters: string;
    FiltersFile: string;
    FiltersWrite: string;
    PriorityLevel: number;
    Url: string;
    options: any;
    get filters(): string[];
    get rights(): string[];
    checkRight(right: string): boolean;
    checkCommandException(command: string): boolean;
    addRight(rightName: string): void;
    removeRight(rightName: string): void;
    addFilter(filter: string): void;
    removeFilter(filter: string): void;
}

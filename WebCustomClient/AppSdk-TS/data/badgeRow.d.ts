import { PersonRow } from "./personRow";
export declare class BadgeRow {
    Code: string;
    Format: string;
    Info: string;
    Status: number;
    BadgeTypeId: number;
    PersonId?: number;
    Person: PersonRow;
    constructor(b: Partial<BadgeRow>);
    /**
     * Set the status of the BadgeRow instance
     * @param status New status
     */
    setStatus(status: number): void;
}

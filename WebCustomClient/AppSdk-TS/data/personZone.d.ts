import { PersonRow } from "./personRow";
import { Zone } from "./zone";
export declare class PersonZone {
    ZoneId: number;
    PersonId: number;
    DateInput: string;
    Zone: Zone;
    Person: PersonRow;
    get dateInput(): Date;
    constructor(obj: Partial<PersonZone>);
}

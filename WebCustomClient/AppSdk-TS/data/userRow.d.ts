import { DateTimeOffset } from "./dateTimeOffset";
import { ProfileRow } from "./profileRow";
import { UserState } from "./userState";
export declare class UserRow {
    constructor(user: Partial<UserRow>);
    Name: string;
    FirstName: string;
    LastName: string;
    DbOptions: string;
    Language: string;
    ProfileId: number;
    Parameters: string;
    CreateDate: DateTimeOffset;
    ValidityStart: DateTimeOffset;
    ValidityEnd: DateTimeOffset;
    PasswordDate: string;
    Id: number;
    Picture: string;
    State: UserState;
    Email: string;
    options: any;
    profile: ProfileRow;
    get fullName(): string;
    set passwordDate(d: Date);
    get passwordDate(): Date;
    checkRight(right: string, commandException?: string): boolean;
}

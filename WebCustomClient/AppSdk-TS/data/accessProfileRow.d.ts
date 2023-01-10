import { AccessProfileRightRow } from "./accessProfileRightRow";
import { CategoryRow } from "./categoryRow";
import { DepartmentRow } from "./departmentRow";
export declare class AccessProfileRow {
    Id: number;
    Name: string;
    Description: string;
    Parameters: string;
    AccessProfileRights: AccessProfileRightRow[];
    Departments: DepartmentRow[];
    Categories: CategoryRow[];
    constructor(p: Partial<AccessProfileRow>);
}

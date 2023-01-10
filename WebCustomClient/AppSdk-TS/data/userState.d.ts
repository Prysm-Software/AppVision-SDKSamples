import { ChatMessage } from "./chat";
export declare class UserState {
    Id: number;
    Name: string;
    Parameters: string;
    UnreadTalks: ChatMessage[];
    constructor(obj: Partial<UserState>);
}

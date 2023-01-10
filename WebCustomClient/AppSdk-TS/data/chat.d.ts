import { DateTimeOffset } from "./dateTimeOffset";
export declare type ChatMessageType = "File" | "Alarm" | "Variable" | "Syno" | "Info" | "Text";
export declare class ChatMessage {
    Id: number;
    TalkId: number;
    Message: string;
    MessageType: ChatMessageType;
    Date: DateTimeOffset;
    AuthorId: number;
    get date(): Date;
    constructor(msg: Partial<ChatMessage>);
}
export declare class Talk {
    Id: number;
    Members: number[];
    CreatedDate: DateTimeOffset;
    Title: string;
    LastMessage: ChatMessage;
    LastMessageRead?: number;
    CreatorId: number;
    get createdDate(): Date;
    constructor(talk: Partial<Talk>);
}

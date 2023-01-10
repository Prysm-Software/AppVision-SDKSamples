export declare class XMLElement {
    name: string;
    attributes: {
        [name: string]: string;
    };
    value: string;
    children: XMLElement[];
    getElementsByTagName(tagName: string): XMLElement[];
}
export declare class XmlParser {
    parseFromString(xmlText: string): XMLElement;
    toString(xml: any): string;
    private _parseFromString;
    private _encodeCDATAValues;
    private _parseTag;
    private _parseValue;
    private _convertTagsArrayToTree;
    private _toString;
    private _convertTagToText;
}

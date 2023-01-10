export declare abstract class StringHelper {
    static getParentName(name: string, separator?: string): string;
    static truncate(str: string, lenght: number): string;
    static isNullOrEmpty(str: string): boolean;
    static toAlphaNum(str: string, replacement: '_'): string;
    static trim(str: string, char: string): string;
    static format(str: string, ...params: string[]): string;
}

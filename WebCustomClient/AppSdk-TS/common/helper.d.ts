export declare abstract class Helper {
    /**
     * Wait x milliseconds
     * @param ms Milliseconds to wait
     * @returns Promise to wait
     */
    static delay(ms: number): Promise<void>;
    /**
     * Get parameter from string lines (\r\n)
     * @param parameterName Parameter name (case insensitive)
     * @param lines Lines to search in
     * @param defaultValue Default value if nothing found
     * @returns Parameter value
     */
    static getParameter(parameterName: string, lines: string, defaultValue?: string): string;
    /**
     * Return a property of a JS object.
     * @param object Object
     * @param propertyName Propertyname. Recursive if contains '.'
     * @returns Property value
     * @example getObjectPropValue(myObj, 'State.Value')
     */
    static getObjectPropValue(object: any, propertyName: string): any;
    /**
     * Clone only number, boolean, string and date field type of an object
     * @param obj Object to clone
     * @returns Cloned object
     */
    static lightCloneObject<T>(obj: T): T;
    /**
     * Parse string int number
     * @param str String to parse
     * @param fallbackValue Fallback value if parse failed
     * @returns Number
     */
    static parseInt(str: string, fallbackValue?: number): number;
    /**
     * Parse string into float number
     * @param str String to parse
     * @param fallbackValue Fallback value if parse failed
     * @returns Float number
     */
    static parseFloat(str: string, fallbackValue?: number): number;
    /**
     * Parse string into bool
     * @param str String to parse. Accepted : true, 1, on, yes and their opposite. (case insensitive)
     * @param fallbackValue Fallback value if nothing found
     * @returns Boolean value
     */
    static parseBool(str: string, fallbackValue?: boolean): boolean;
    static convertRegExpPattern(pattern: string): string;
    /**
     * Generate a new Guid string
     * @returns Guid string
     */
    static guid(): string;
    /**
     * Increments the last charracter of a string: B1 -> B2, A -> B, ...
     * @returns  string
     */
    static incrementName(str: string): string;
    static isCharNumber(c: any): boolean;
}

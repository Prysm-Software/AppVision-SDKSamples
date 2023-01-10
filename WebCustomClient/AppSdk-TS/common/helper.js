"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Helper = void 0;
class Helper {
    /**
     * Wait x milliseconds
     * @param ms Milliseconds to wait
     * @returns Promise to wait
     */
    static delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    /**
     * Get parameter from string lines (\r\n)
     * @param parameterName Parameter name (case insensitive)
     * @param lines Lines to search in
     * @param defaultValue Default value if nothing found
     * @returns Parameter value
     */
    static getParameter(parameterName, lines, defaultValue = null) {
        if (lines) {
            const array = lines.split('\r\n');
            for (let i = 0; i < array.length; i++) {
                if (array[i].toLowerCase().startsWith(parameterName.toLowerCase() + '=')) {
                    return array[i].substring(parameterName.length + 1);
                }
            }
        }
        return defaultValue;
    }
    /**
     * Return a property of a JS object.
     * @param object Object
     * @param propertyName Propertyname. Recursive if contains '.'
     * @returns Property value
     * @example getObjectPropValue(myObj, 'State.Value')
     */
    static getObjectPropValue(object, propertyName) {
        if (propertyName.indexOf('.') === -1) {
            return object[propertyName];
        }
        else {
            const arr = propertyName.split('.');
            let value = object[arr[0]];
            for (let i = 1; i < arr.length; i++) {
                if (value) {
                    value = value[arr[i]];
                }
                else {
                    break;
                }
            }
            return value;
        }
    }
    /**
     * Clone only number, boolean, string and date field type of an object
     * @param obj Object to clone
     * @returns Cloned object
     */
    static lightCloneObject(obj) {
        const clone = {};
        for (const prop in obj) {
            if (typeof obj[prop] === "string" || typeof obj[prop] === "number" || typeof obj[prop] === "boolean" || obj[prop] instanceof Date) {
                clone[prop] = obj[prop];
            }
        }
        return clone;
    }
    /**
     * Parse string int number
     * @param str String to parse
     * @param fallbackValue Fallback value if parse failed
     * @returns Number
     */
    static parseInt(str, fallbackValue = 0) {
        let value = parseInt(str);
        if (isNaN(value))
            value = fallbackValue;
        return value;
    }
    /**
     * Parse string into float number
     * @param str String to parse
     * @param fallbackValue Fallback value if parse failed
     * @returns Float number
     */
    static parseFloat(str, fallbackValue = 0) {
        let value = parseFloat(str);
        if (isNaN(value))
            value = fallbackValue;
        return value;
    }
    /**
     * Parse string into bool
     * @param str String to parse. Accepted : true, 1, on, yes and their opposite. (case insensitive)
     * @param fallbackValue Fallback value if nothing found
     * @returns Boolean value
     */
    static parseBool(str, fallbackValue = false) {
        switch (str === null || str === void 0 ? void 0 : str.toLowerCase()) {
            case "true":
            case "1":
            case "on":
            case "yes":
                return true;
            default:
                return fallbackValue;
        }
    }
    // todo: Copier coller de la fonction d'appcommon.dll. A tester en JS
    static convertRegExpPattern(pattern) {
        if (!pattern)
            return null;
        return "^" + pattern
            .replace("$", "\\$")
            .replace(".", "\\.")
            .replace("*", "(.*)") + "$";
    }
    /**
     * Generate a new Guid string
     * @returns Guid string
     */
    static guid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            const r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
    /**
     * Increments the last charracter of a string: B1 -> B2, A -> B, ...
     * @returns  string
     */
    static incrementName(str) {
        var indexDigit = -1;
        for (let i = 0; i < str.length; i++) {
            if (this.isCharNumber(str[i])) {
                if (indexDigit == -1)
                    indexDigit = i;
            }
            else
                indexDigit = -1;
        }
        if (indexDigit == -1) // pas de numéro?
         {
            //				return str+"1";
            // -> incrémente le dernier caractère
            if (str.length > 0) {
                if ((str[str.length - 1] >= 'A' && str[str.length - 1] < 'Z') || (str[str.length - 1] >= 'a' && str[str.length - 1] < 'z'))
                    return str.substring(0, str.length - 1) + String.fromCharCode(str[str.length - 1].charCodeAt(0) + 1);
                if (str[str.length - 1] == 'Z')
                    return str.substring(0, str.length - 1) + "AA";
                if (str[str.length - 1] == 'z')
                    return str.substring(0, str.length - 1) + "aa";
                return str + "1";
            }
            str = "0";
            indexDigit = 0;
        }
        var nDigit = str.length - indexDigit;
        var prefixe = str.substring(0, indexDigit);
        var num = this.parseInt(str.substring(indexDigit)) + 1;
        var len = num.toString().length;
        for (let i = 0; i < nDigit - len; i++)
            prefixe += '0';
        return prefixe + num.toString();
    }
    static isCharNumber(c) {
        return c >= '0' && c <= '9';
    }
}
exports.Helper = Helper;

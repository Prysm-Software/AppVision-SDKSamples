"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.StringHelper = void 0;
class StringHelper {
    static getParentName(name, separator = '.') {
        const i = name.lastIndexOf(separator);
        return (i > -1) ? name.substring(0, i) : '';
    }
    static truncate(str, lenght) {
        return lenght > str.length ? str.substring(0, lenght) : str.substring(0, lenght) + '...';
    }
    static isNullOrEmpty(str) {
        return str == null || str === '';
    }
    static toAlphaNum(str, replacement) {
        if (str) {
            return str.replace(/[\W_]+/g, replacement !== null && replacement !== void 0 ? replacement : "_");
        }
        return str;
    }
    static trim(str, char) {
        let i = 0;
        let j = str.length - 1;
        while (str[i] === char)
            i++;
        while (str[j] === char)
            j--;
        return str.slice(i, j + 1);
    }
    static format(str, ...params) {
        return str.replace(/{(\d+)}/g, (match, number) => {
            return typeof params[number] != 'undefined'
                ? params[number]
                : match;
        });
    }
}
exports.StringHelper = StringHelper;

"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ArrayHelper = void 0;
class ArrayHelper {
    static groupBy(arr, key) {
        return arr.reduce((rv, x) => {
            let v = x[key];
            let el = rv.find((r) => r && r.key === v);
            if (el) {
                el.values.push(x);
            }
            else {
                rv.push({ key: v, values: [x] });
            }
            return rv;
        }, []);
    }
    ;
    /**
     * Recusive foreach. Apply callback to array and children
     * @param arr Base array
     * @param childKey Children property name
     * @param callback Callback to apply
     */
    static recursiveForEach(arr, childrenKey, callback) {
        var _a;
        for (const item of arr) {
            callback(item);
            if (((_a = item[childrenKey]) === null || _a === void 0 ? void 0 : _a.lenght) > 0) {
                this.recursiveForEach(item[childrenKey], childrenKey, callback);
            }
        }
    }
    ;
}
exports.ArrayHelper = ArrayHelper;

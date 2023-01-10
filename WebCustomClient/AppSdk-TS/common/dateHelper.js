"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.DateHelper = void 0;
class DateHelper {
    static toJSDate(date) {
        return new Date(parseInt(date.DateTime.replace(/[^0-9 +]/g, '')));
    }
    static strToJSDate(date) {
        if (!date)
            return null;
        const str = date.split("+").shift();
        return new Date(parseInt(str.replace(/[^0-9 +]/g, '')));
    }
    static stringToDate(dateStr) {
        return new Date(Date.parse(dateStr));
    }
    static toDateTimeString(date) {
        if (date == null)
            return "";
        let dd = date.getDate();
        let mm = date.getMonth() + 1; //January is 0!
        let yyyy = date.getFullYear();
        let h = date.getHours();
        let m = date.getMinutes();
        let s = date.getSeconds();
        if (dd < 10)
            dd = '0' + dd;
        if (mm < 10)
            mm = '0' + mm;
        if (h < 10)
            h = '0' + h;
        if (m < 10)
            m = '0' + m;
        if (s < 10)
            s = '0' + s;
        return yyyy + '-' + mm + '-' + dd + 'T' + h + ':' + m + ':' + s;
    }
    static toDateString(date) {
        if (date == null)
            return "";
        let dd = date.getDate();
        let mm = date.getMonth() + 1; //January is 0!
        let yyyy = date.getFullYear();
        if (dd < 10)
            dd = '0' + dd;
        if (mm < 10)
            mm = '0' + mm;
        return yyyy + '-' + mm + '-' + dd;
    }
    static today() {
        const now = new Date();
        return new Date(now.getFullYear(), now.getMonth(), now.getDate());
    }
}
exports.DateHelper = DateHelper;

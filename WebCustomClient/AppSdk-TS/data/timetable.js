"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.TimetableWithPeriod = exports.TimetableItem = exports.Timetable = exports.TimetableType = void 0;
const dateHelper_1 = require("../common/dateHelper");
var TimetableType;
(function (TimetableType) {
    TimetableType[TimetableType["Daily"] = 0] = "Daily";
    TimetableType[TimetableType["Weekly"] = 1] = "Weekly";
    TimetableType[TimetableType["WeeklyWithHolidays"] = 2] = "WeeklyWithHolidays";
})(TimetableType = exports.TimetableType || (exports.TimetableType = {}));
class Timetable {
    constructor(tt) {
        Object.assign(this, tt);
    }
}
exports.Timetable = Timetable;
class TimetableItem {
    constructor(tt = null) {
        Object.assign(this, tt);
    }
    get getDay() {
        return (this.TotalMinutes / (24 * 60));
    }
    get getHour() {
        return (this.TotalMinutes / 60) % 24;
    }
    get getMinute() {
        return (this.TotalMinutes % 60);
    }
    isValid(timeTableType) {
        if (this.Day < 0)
            return false;
        switch (timeTableType) {
            case TimetableType.Daily: return this.Day == 0;
            case TimetableType.Weekly: return this.Day < 7;
            case TimetableType.WeeklyWithHolidays: return this.Day < (7 + 3); // 3 listes de jour férié maximum
            default: return false;
        }
    }
    equals(item) {
        if (item == null)
            return false;
        return item.TotalMinutes == this.TotalMinutes && item.Value == this.Value;
    }
    get isHoliday() {
        return (this.Day >= 7);
    }
}
exports.TimetableItem = TimetableItem;
class TimetableWithPeriod {
    constructor(tt = null) {
        Object.assign(this, tt);
    }
    get start() {
        return dateHelper_1.DateHelper.stringToDate(this.Start);
    }
    get end() {
        return dateHelper_1.DateHelper.stringToDate(this.End);
    }
    match(date) {
        return this.start.getTime() <= date && date < this.end.getTime();
    }
}
exports.TimetableWithPeriod = TimetableWithPeriod;

"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.TimetableManager = void 0;
const timetable_1 = require("../data/timetable");
class TimetableManager {
    constructor(http, variableManager) {
        this.http = http;
        this.variableManager = variableManager;
    }
    fromXML(xml) {
        if ((xml === null || xml === void 0 ? void 0 : xml.length) > 0) {
            try {
                const parser = new DOMParser();
                const xmlDoc = parser.parseFromString(xml, "text/xml");
                const xmlTimetableNode = xmlDoc.getElementsByTagName("Timetable")[0];
                const xmlType = xmlDoc.getElementsByTagName("Type")[0].innerHTML;
                const xmlItems = xmlTimetableNode.getElementsByTagName("Items");
                var r = [];
                for (let xmlItem of xmlItems) {
                    let item = new timetable_1.TimetableItem();
                    item.TotalMinutes = parseInt(xmlItem.getElementsByTagName("TotalMinutes")[0].innerHTML, 10);
                    item.Value = parseInt(xmlItem.getElementsByTagName("Value")[0].innerHTML, 0);
                    r.push(item);
                }
                var type;
                switch (xmlType) {
                    case 'Daily':
                        type = timetable_1.TimetableType.Daily;
                        break;
                    case 'Weekly':
                        type = timetable_1.TimetableType.Weekly;
                        break;
                    case 'WeeklyWithHolidays':
                        type = timetable_1.TimetableType.WeeklyWithHolidays;
                        break;
                }
                return new timetable_1.Timetable({
                    Type: type,
                    Items: r,
                    DefaultItems: new timetable_1.TimetableItem[0],
                });
            }
            catch (e) {
            }
        }
        return new timetable_1.Timetable({
            Type: timetable_1.TimetableType.Daily,
            Items: new timetable_1.TimetableItem[0],
            DefaultItems: new timetable_1.TimetableItem[0],
        });
    }
}
exports.TimetableManager = TimetableManager;

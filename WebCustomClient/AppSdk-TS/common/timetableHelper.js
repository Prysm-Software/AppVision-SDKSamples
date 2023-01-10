"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.TimetableHelper = void 0;
const timetable_1 = require("../data/timetable");
const dateHelper_1 = require("../common/dateHelper");
class TimetableHelper {
    static fromXML(xml) {
        var _a;
        if ((xml === null || xml === void 0 ? void 0 : xml.length) > 0) {
            try {
                const parser = new DOMParser();
                const xmlDoc = parser.parseFromString(xml, "text/xml");
                const xmlTimetableNode = xmlDoc.getElementsByTagName("Timetable")[0];
                const xmlType = (_a = xmlDoc.getElementsByTagName("Type")[0]) === null || _a === void 0 ? void 0 : _a.innerHTML;
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
                const xmlItems = xmlTimetableNode.getElementsByTagName("Items")[0];
                ;
                const xmlTimetableItems = xmlItems.getElementsByTagName("TimetableItem");
                const xmlDefaultItems = xmlTimetableNode.getElementsByTagName("DefaultItems")[0];
                var xmlTimetableDefaultItems = undefined;
                if (xmlDefaultItems != undefined)
                    xmlTimetableDefaultItems = xmlDefaultItems.getElementsByTagName("TimetableItem");
                const xmlItemsWithPeriod = xmlTimetableNode.getElementsByTagName("ItemsWithPeriod")[0];
                var xmlTimetableWithPeriod = undefined;
                if (xmlItemsWithPeriod != undefined)
                    xmlTimetableWithPeriod = xmlItemsWithPeriod.getElementsByTagName("TimetableWithPeriod");
                var r = [];
                if (xmlTimetableItems != undefined)
                    for (let i = 0; i < xmlTimetableItems.length; i++) {
                        var xmlItem = xmlTimetableItems[i];
                        let item = new timetable_1.TimetableItem();
                        item.TotalMinutes = parseInt(xmlItem.getElementsByTagName("TotalMinutes")[0].innerHTML, 10);
                        item.Value = parseInt(xmlItem.getElementsByTagName("Value")[0].innerHTML, 0);
                        r.push(item);
                    }
                var d = [];
                if (xmlTimetableDefaultItems != undefined)
                    for (let xmlItem of xmlTimetableDefaultItems) {
                        let item = new timetable_1.TimetableItem();
                        item.TotalMinutes = parseInt(xmlItem.getElementsByTagName("TotalMinutes")[0].innerHTML, 10);
                        item.Value = parseInt(xmlItem.getElementsByTagName("Value")[0].innerHTML, 0);
                        d.push(item);
                    }
                var p = [];
                if (xmlTimetableWithPeriod != undefined)
                    for (let xmlTimetableWithP of xmlTimetableWithPeriod) {
                        let tt = new timetable_1.TimetableWithPeriod();
                        tt.Start = xmlTimetableWithP.getElementsByTagName("Start")[0].innerHTML;
                        tt.End = xmlTimetableWithP.getElementsByTagName("End")[0].innerHTML;
                        const xmlItems = xmlTimetableWithP.getElementsByTagName("Items")[0];
                        const xmlTimetableItems = xmlItems.getElementsByTagName("TimetableItem");
                        var r = [];
                        if (xmlTimetableItems != undefined)
                            for (let xmlItem of xmlTimetableItems) {
                                let item = new timetable_1.TimetableItem();
                                item.TotalMinutes = parseInt(xmlItem.getElementsByTagName("TotalMinutes")[0].innerHTML, 10);
                                item.Value = parseInt(xmlItem.getElementsByTagName("Value")[0].innerHTML, 0);
                                r.push(item);
                            }
                        tt.Items = r;
                        p.push(tt);
                    }
                return new timetable_1.Timetable({
                    Type: type,
                    Items: r,
                    DefaultItems: d,
                    ItemsWithPeriod: p
                });
            }
            catch (e) {
                console.error(e.toString());
            }
        }
        return new timetable_1.Timetable({
            Type: timetable_1.TimetableType.Daily,
            Items: [],
            DefaultItems: [],
        });
    }
    static toXML(timetable) {
        let xml = '\<?xml version=\x221.0\x22 encoding=\x22utf-16\x22?><Timetable xmlns:xsd=\x22http://www.w3.org/2001/XMLSchema\x22 xmlns:xsi=\x22http://www.w3.org/2001/XMLSchema-instance\x22>\x0D\x0A  ';
        xml += '<Type>' + timetable_1.TimetableType[timetable.Type] + '</Type>\x0D\x0A  ';
        xml += '<Items>\x0D\x0A  ';
        timetable.Items.forEach(t => {
            xml += '<TimetableItem>\x0D\x0A  ';
            xml += '<TotalMinutes>' + t.TotalMinutes + '</TotalMinutes>\x0D\x0A  ';
            xml += '<Value>' + t.Value + '</Value>\x0D\x0A  ';
            xml += '</TimetableItem>\x0D\x0A  ';
        });
        xml += '</Items>\x0D\x0A  ';
        if (timetable.DefaultItems != undefined) {
            xml += '<DefaultItems>\x0D\x0A  ';
            timetable.DefaultItems.forEach(t => {
                xml += '<TimetableItem>\x0D\x0A  ';
                xml += '<TotalMinutes>' + t.TotalMinutes + '</TotalMinutes>\x0D\x0A  ';
                xml += '<Value>' + t.Value + '</Value>\x0D\x0A  ';
                xml += '</TimetableItem>\x0D\x0A  ';
            });
            xml += '</DefaultItems>\x0D\x0A  ';
        }
        else
            xml += '<DefaultItems />\x0D\x0A  ';
        if (timetable.ItemsWithPeriod != undefined) {
            xml += '<ItemsWithPeriod>\x0D\x0A  ';
            timetable.ItemsWithPeriod.forEach(tp => {
                xml += '<TimetableWithPeriod>\x0D\x0A  ';
                xml += '<Start>' + dateHelper_1.DateHelper.toDateTimeString(tp.start) + '</Start>\x0D\x0A  ';
                xml += '<End>' + dateHelper_1.DateHelper.toDateTimeString(tp.end) + '</End>\x0D\x0A  ';
                xml += '<Items>';
                tp.Items.forEach(t => {
                    xml += '<TimetableItem>\x0D\x0A  ';
                    xml += '<TotalMinutes>' + t.TotalMinutes + '</TotalMinutes>\x0D\x0A  ';
                    xml += '<Value>' + t.Value + '</Value>\x0D\x0A  ';
                    xml += '</TimetableItem>\x0D\x0A  ';
                });
                xml += '</Items>\x0D\x0A  ';
                xml += '</TimetableWithPeriod>\x0D\x0A  ';
            });
            xml += '</ItemsWithPeriod>\x0D\x0A  ';
        }
        xml += '</Timetable>';
        return xml;
    }
    static compareItems(items1, items2) {
        if (items1 == null && items2 == null)
            return true;
        if (items1 == null || items2 == null)
            return false;
        if (items1.length != items2.length)
            return false;
        for (let i = 0; i < items1.length; i++)
            if (!items1[i].equals(items2[i]))
                return false;
        return true;
    }
}
exports.TimetableHelper = TimetableHelper;

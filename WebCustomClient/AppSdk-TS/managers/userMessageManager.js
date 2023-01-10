"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.UserMessageManager = void 0;
const xmlParser_1 = require("../common/xmlParser");
const userMessageRow_1 = require("../data/userMessageRow");
class UserMessageManager {
    constructor(http, optionManager) {
        var _a, _b, _c, _d, _e, _f;
        this.categories = [];
        this.http = http;
        const xml = optionManager.get('App.Client.UserMessage.MessageObjects');
        if (xml) {
            const parser = new xmlParser_1.XmlParser();
            const doc = parser.parseFromString(xml);
            const root = doc.getElementsByTagName("ArrayOfUserMessageCategory")[0];
            const cats = [];
            if (root) {
                const arr = root.getElementsByTagName("UserMessageCategory");
                if (arr) {
                    for (const cat of arr) {
                        const properties = [];
                        const propsRoot = cat.getElementsByTagName("Properties")[0];
                        if (propsRoot) {
                            const xmlProps = propsRoot.getElementsByTagName('UserMessageCategoryProperty');
                            if (xmlProps) {
                                for (const prop of xmlProps) {
                                    properties.push({
                                        Name: prop.getElementsByTagName('Name')[0].value,
                                        Description: (_a = prop.getElementsByTagName('Description')[0]) === null || _a === void 0 ? void 0 : _a.value,
                                        Required: ((_c = (_b = prop.getElementsByTagName('IsRequired')[0]) === null || _b === void 0 ? void 0 : _b.value) === null || _c === void 0 ? void 0 : _c.toLowerCase()) === 'true',
                                        Values: (_d = prop.getElementsByTagName('Value')[0]) === null || _d === void 0 ? void 0 : _d.value,
                                        Type: (_e = prop.getElementsByTagName('Type')[0]) === null || _e === void 0 ? void 0 : _e.value
                                    });
                                }
                            }
                        }
                        cats.push({
                            Description: (_f = cat.getElementsByTagName("Description")[0]) === null || _f === void 0 ? void 0 : _f.value,
                            Properties: properties
                        });
                    }
                }
            }
            this.categories = cats;
        }
    }
    /**
     * Get user messages categories
     * @returns
     */
    getCategories() {
        return [...this.categories];
    }
    /**
     * Get user messages from the server
     * @param dateStart Date start filter
     * @param dateEnd Date end filter
     * @param filters Filter operand
     * @param maxCount Max count result. Default is 2500
     * @returns Array of UserMessageRow
     */
    getAll(dateStart, dateEnd, filters, maxCount = 2500) {
        return __awaiter(this, void 0, void 0, function* () {
            let uri = `AppVisionService.svc/GetUserMessages?minId=0&maxCount=${maxCount}&dateStart=${dateStart.toISOString()}`;
            if (dateEnd) {
                uri += `&dateEnd=${dateEnd.toISOString()}`;
            }
            if (filters) {
                uri += `&filters=${filters}`;
            }
            const resp = yield this.http.get(uri);
            const json = yield resp.json();
            return json.map(e => new userMessageRow_1.UserMessageRow(e));
        });
    }
}
exports.UserMessageManager = UserMessageManager;

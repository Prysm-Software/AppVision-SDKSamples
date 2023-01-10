"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.CategoryRow = void 0;
const helper_1 = require("../common/helper");
const stringHelper_1 = require("../common/stringHelper");
class CategoryRow {
    constructor(p = null) {
        this.Id = 0;
        this.Name = '';
        this.Description = '';
        this.BadgeDesign = '';
        this.AccessProfileId = null;
        this.IsVisitor = false;
        this.Parameters = '';
        this.DefaultValidityPeriod = '';
        Object.assign(this, p);
    }
    /**
     * Return validity period from now on
     * @returns start and end date from now on
     */
    getValidityPeriod() {
        const now = new Date();
        if (!stringHelper_1.StringHelper.isNullOrEmpty(this.DefaultValidityPeriod)) {
            const parts = this.DefaultValidityPeriod.split('');
            const type = parts.pop().toLowerCase();
            const l = helper_1.Helper.parseInt(parts.join(), 0);
            if (l > 0) {
                switch (type) {
                    case 'h':
                        now.setHours(now.getHours() + l);
                        break;
                    case 'd':
                        now.setDate(now.getDate() + l);
                        break;
                    case 'm':
                        now.setMonth(now.getMonth() + l);
                        break;
                    case 'y':
                        now.setFullYear(now.getFullYear() + l);
                        break;
                    default:
                        now.setFullYear(now.getFullYear() + 10);
                        break;
                }
            }
        }
        else {
            now.setFullYear(now.getFullYear() + 10);
        }
        return { start: new Date(), end: now };
    }
}
exports.CategoryRow = CategoryRow;

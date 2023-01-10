"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.RowCustomColorManager = void 0;
const colorHelper_1 = require("../common/colorHelper");
const customColor_1 = require("../data/customColor");
class RowCustomColorManager {
    constructor(optionManager, areaManager, alarmManager) {
        this.customColors = [];
        this.optionManager = optionManager;
        this.areaManager = areaManager;
        this.alarmManager = alarmManager;
        this.loadInternal();
    }
    getCustomColors() {
        return this.customColors;
    }
    /**
     * Get the custom color of an object
     * @param obj AlarmRow, AreaState, GroupState, VariableState or EventRow
     * @returns Css color or undefined
     */
    getCustomColor(obj) {
        let cc;
        if (this.customColors) {
            this.customColors.forEach((value) => {
                if (value.isMatch(obj)) {
                    return cc = value;
                }
            });
        }
        return cc;
    }
    loadInternal() {
        let cc = [];
        for (let n = 1;; n++) {
            let optionName = "App.CustomColor." + n;
            let filter = this.optionManager.get(optionName + ".Filter");
            let sbackground = this.getColor(optionName + ".Background");
            let sforeground = this.getColor(optionName + ".Foreground");
            if (filter == null || sbackground == null || sforeground == null)
                break;
            cc.push(new customColor_1.CustomColor(filter, sbackground, sforeground, this.alarmManager, this.areaManager));
        }
        if (cc && cc.length > 0)
            this.customColors = cc;
    }
    getColor(key) {
        return colorHelper_1.ColorHelper.xamlToCssColor(this.optionManager.get(key));
    }
}
exports.RowCustomColorManager = RowCustomColorManager;

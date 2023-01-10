"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.VariableState = void 0;
const dateHelper_1 = require("../common/dateHelper");
class VariableState {
    constructor(varState) {
        Object.assign(this, varState);
    }
    get photo() {
        if (this.Parameters && this.Parameters.findIndex(n => n.Name.toLowerCase() === 'photo') !== -1)
            return this.Parameters.find(n => n.Name.toLowerCase() === 'photo').Value;
        return null;
    }
    get valueDesc() {
        if (this.Value == null)
            return "";
        let prefix = '';
        if (this.IsLocked)
            prefix += '(L) ';
        if (this.IsAlarmMasked)
            prefix += '(M) ';
        return prefix + this.formatValueDesc();
    }
    get date() {
        return dateHelper_1.DateHelper.toJSDate(this.Date);
    }
    formatValueDesc() {
        var _a;
        const intValue = +this.Value;
        if (((_a = this.Row) === null || _a === void 0 ? void 0 : _a.StateDescs) && this.Row.StateDescs.indexOf(':') !== -1 && !isNaN(intValue)) {
            const separator = this.Row.StateDescs.indexOf('\r\n') > -1 ? '\r\n' : '\n';
            const parts = this.Row.StateDescs.split(separator).map(s => ({ value: s.split(':')[0], label: s.split(':')[1] }));
            if (parts.findIndex(v => v.value.startsWith('b')) !== -1 && intValue !== 0) {
                // Bit enum
                let strValue = "";
                parts.filter(v => v.value.startsWith('b')).forEach(bit => {
                    const bitStr = bit.value.substring(1);
                    const bitNb = +bitStr;
                    if (!isNaN(bitNb)) {
                        if ((intValue & (1 << bitNb)) !== 0) {
                            strValue += strValue.length === 0 ? bit.label : ", " + bit.label;
                        }
                    }
                });
                return strValue;
            }
            else {
                // Val enum
                const label = parts.find(p => p.value === this.Value.toString());
                if (label)
                    return label.label;
            }
        }
        return this.Value;
    }
}
exports.VariableState = VariableState;

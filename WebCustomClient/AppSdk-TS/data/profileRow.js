"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ProfileRow = void 0;
const helper_1 = require("../common/helper");
class ProfileRow {
    constructor(user) {
        Object.assign(this, user);
        this.options = {};
        if (this.DbOptions) {
            this.DbOptions
                .split(/[\r\n]/)
                .filter(a => a)
                .map(l => l.split('='))
                .forEach(i => this.options[i[0]] = i[1]);
        }
    }
    get filters() { return this.Filters ? this.Filters.split(',') : []; }
    get rights() { return this.DbRoles ? this.DbRoles.split(',') : []; }
    checkRight(right) {
        return this.IsAdmin || this.rights.indexOf(right) > -1;
    }
    checkCommandException(command) {
        if (!command || !this.CommandExceptions)
            return false;
        const ss = command.split('=');
        const operand = ss[0];
        const commands = this.CommandExceptions.replace(" ", "").replace("=*", "").split(/\n|\r/).filter(s => s);
        // test sur la correspondance exact, quand l'autorisation a été faite sur un état
        if (commands.indexOf(command) > -1)
            return true;
        // test sur la variable
        if (commands.indexOf(operand) > -1)
            return true;
        // cas des exceptions avec caractères génériques
        for (const c of commands) {
            if (c.indexOf('*') > -1) {
                const reg = new RegExp(helper_1.Helper.convertRegExpPattern(c));
                // test sur la variable et l'état
                if (reg.test(command))
                    return true;
                // test sur la variable
                if (reg.test(operand))
                    return true;
            }
        }
        return false;
    }
    addRight(rightName) {
        if (this.rights.indexOf(rightName) < 0) {
            const copy = [...this.rights, rightName];
            this.DbRoles = copy.join(',');
        }
    }
    removeRight(rightName) {
        const index = this.rights.indexOf(rightName);
        if (index >= 0) {
            const copy = [...this.rights];
            copy.splice(index, 1);
            this.DbRoles = copy.join(',');
        }
    }
    addFilter(filter) {
        if (this.filters.indexOf(filter) < 0) {
            const copy = [...this.filters, filter];
            this.Filters = copy.join(',');
        }
    }
    removeFilter(filter) {
        const index = this.filters.indexOf(filter);
        if (index >= 0) {
            const copy = [...this.filters];
            copy.splice(index, 1);
            this.Filters = copy.join(',');
        }
    }
}
exports.ProfileRow = ProfileRow;

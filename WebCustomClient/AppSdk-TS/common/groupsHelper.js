"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.GroupsHelper = void 0;
class GroupsHelper {
    /**
     * Check the following properties : groups, FullGroupNames, GroupNames to tell if a specific group is present
     * @param target Object to check
     * @param groupName Group name
     * @returns True or False
     */
    static isFromGroup(target, groupName) {
        if (groupName.startsWith('$G.')) {
            groupName = groupName.substring(3);
        }
        if (target.groups && Array.isArray(target.groups)) {
            if (target.groups.indexOf(groupName) >= 0) {
                return true;
            }
            for (const group of target.groups) {
                if (group.startsWith(groupName + '.')) {
                    return true;
                }
            }
        }
        if (target.FullGroupNames && Array.isArray(target.FullGroupNames)) {
            if (target.FullGroupNames.indexOf(groupName) >= 0) {
                return true;
            }
            for (const group of target.FullGroupNames) {
                if (group.startsWith(groupName + '.')) {
                    return true;
                }
            }
        }
        if (target.GroupNames && Array.isArray(target.GroupNames)) {
            if (target.GroupNames.indexOf(groupName) >= 0) {
                return true;
            }
            for (const group of target.GroupNames) {
                if (group.startsWith(groupName + '.')) {
                    return true;
                }
            }
        }
        return false;
    }
}
exports.GroupsHelper = GroupsHelper;

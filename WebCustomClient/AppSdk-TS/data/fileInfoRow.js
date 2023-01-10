"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.FileInfoRow = exports.FileInfoType = void 0;
const treeRow_1 = require("./treeRow");
var FileInfoType;
(function (FileInfoType) {
    FileInfoType[FileInfoType["Undefined"] = 0] = "Undefined";
    FileInfoType[FileInfoType["Cs"] = 1] = "Cs";
    FileInfoType[FileInfoType["Reference"] = 2] = "Reference";
    FileInfoType[FileInfoType["Synoptic"] = 3] = "Synoptic";
    FileInfoType[FileInfoType["Symbol"] = 4] = "Symbol";
    FileInfoType[FileInfoType["Folder"] = 5] = "Folder";
    FileInfoType[FileInfoType["Header"] = 6] = "Header";
    FileInfoType[FileInfoType["Footer"] = 7] = "Footer";
    FileInfoType[FileInfoType["Pcs"] = 8] = "Pcs";
    FileInfoType[FileInfoType["Layout"] = 9] = "Layout";
    FileInfoType[FileInfoType["Workflow"] = 10] = "Workflow";
    FileInfoType[FileInfoType["Js"] = 11] = "Js";
})(FileInfoType = exports.FileInfoType || (exports.FileInfoType = {}));
class FileInfoRow extends treeRow_1.TreeRow {
    get type() {
        return FileInfoType[this.TypeInt];
    }
    ;
}
exports.FileInfoRow = FileInfoRow;

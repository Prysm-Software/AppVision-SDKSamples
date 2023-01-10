"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Args = exports.DocumentModel = exports.ContainerModel = exports.LayoutPanelProperties = exports.LayoutModel = exports.EnvironmentModel = void 0;
class EnvironmentModel {
    constructor() {
        this.isSideBarVisible = true;
        this.isHeaderVisible = true;
    }
}
exports.EnvironmentModel = EnvironmentModel;
class LayoutModel {
    constructor(mode = "videox1", containers = []) {
        this.containers = [];
        this.mode = mode;
        this.containers = containers;
        this.properties = new Map();
    }
    static createDefault(documents) {
        return new LayoutModel('x' + documents.length, documents.map((d, index) => new ContainerModel("c" + (index + 1), { name: d })));
    }
}
exports.LayoutModel = LayoutModel;
class LayoutPanelProperties {
}
exports.LayoutPanelProperties = LayoutPanelProperties;
class ContainerModel {
    constructor(containerName, document = null) {
        this.name = containerName;
        this.document = document;
    }
}
exports.ContainerModel = ContainerModel;
class DocumentModel {
    constructor() {
        this.args = [];
    }
}
exports.DocumentModel = DocumentModel;
class Args {
    static fromString(args, separator = ',') {
        if (!args)
            return [];
        return args.split(separator).map(line => {
            const arg = new Args();
            arg.name = line;
            const idx = line.indexOf('=');
            if (idx > 0) {
                // OLD : cas particulier pour Severity>=n
                // NEW : Gérer avec MinSeverity MaxSeverity
                // if (line[idx - 1] === '>' || line[idx - 1] === '<')
                //     return arg;
                arg.name = line.substring(0, idx).trim();
                arg.value = line.substring(idx + 1);
            }
            return arg;
        });
    }
    static getValue(args, name) {
        var _a;
        name = name === null || name === void 0 ? void 0 : name.toLowerCase();
        return (_a = args === null || args === void 0 ? void 0 : args.find(a => a.name.toLowerCase() === name)) === null || _a === void 0 ? void 0 : _a.value;
    }
}
exports.Args = Args;

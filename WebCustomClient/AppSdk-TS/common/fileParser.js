"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.FileParser = void 0;
const environmentModels_1 = require("../data/environmentModels");
class FileParser {
    static parseWebEnvironment(file) {
        var _a, _b;
        const json = JSON.parse(file);
        const layout = new environmentModels_1.LayoutModel();
        layout.mode = json.layout.mode;
        if (json.layout.containers) {
            json.layout.containers.forEach(c => {
                const container = new environmentModels_1.ContainerModel(c.name);
                container.isSelectable = c.isSelectable;
                container.hasMenu = c.hasMenu;
                container.allowDragAndDrop = c.allowDragAndDrop;
                //if (c.args) {
                //    c.args.forEach(a => {
                //        const arg = new Args();
                //        Object.assign(arg, a);
                //        container.args.push(arg);
                //    });
                //}
                if (c.document) {
                    container.document = new environmentModels_1.DocumentModel();
                    container.document.name = c.document.name;
                    if (c.document.args) {
                        c.document.args.forEach(a => {
                            const arg = new environmentModels_1.Args();
                            Object.assign(arg, a);
                            container.document.args.push(arg);
                        });
                    }
                }
                layout.containers.push(container);
            });
        }
        if (json.layout.properties) {
            Object.keys(json.layout.properties).forEach(key => {
                const propValue = new environmentModels_1.LayoutPanelProperties();
                Object.assign(propValue, json.layout.properties[key]);
                layout.properties.set(key, propValue);
            });
        }
        const env = new environmentModels_1.EnvironmentModel();
        env.isSideBarVisible = (_a = json.isSideBarVisible) !== null && _a !== void 0 ? _a : true;
        env.isHeaderVisible = (_b = json.isHeaderVisible) !== null && _b !== void 0 ? _b : true;
        env.layout = layout;
        return env;
    }
    static parseClassicContext(file) {
        const parser = new DOMParser();
        const xmlDoc = parser.parseFromString(file, "text/xml");
        const layoutXml = xmlDoc.getElementsByTagName("layout")[0];
        if (layoutXml) {
            const layoutName = layoutXml.getAttribute("type");
            let layout;
            if (layoutName.split('.').pop().toLowerCase().startsWith("video"))
                layout = this.parseVideoLayout(layoutXml);
            else
                layout = this.parseClientLayout(layoutXml);
            const env = new environmentModels_1.EnvironmentModel();
            env.layout = layout;
            return env;
        }
        return null;
    }
    static parseVideoLayout(layoutElement) {
        const layoutName = layoutElement.getAttribute("type");
        let childrenExpected = 1;
        //env.mode = mode;
        const layout = new environmentModels_1.LayoutModel();
        layout.mode = layoutName.split(".").pop().toLowerCase();
        switch (layout.mode) {
            case "videox1":
                childrenExpected = 1;
                break;
            case "videox2":
                childrenExpected = 2;
                break;
            case "videox3":
                childrenExpected = 3;
                break;
            case "x4":
            case "videox4":
                childrenExpected = 4;
                break;
            case "videox6":
                childrenExpected = 6;
                break;
            case "videox6b":
                childrenExpected = 6;
                break;
            case "videox8":
                childrenExpected = 8;
                break;
            case "videox9":
                childrenExpected = 9;
                break;
            case "videox10":
                childrenExpected = 10;
                break;
            case "videox12":
                childrenExpected = 12;
                break;
            case "videox13":
                childrenExpected = 13;
                break;
            case "videox16":
                childrenExpected = 16;
                break;
            default:
                break;
        }
        for (let i = 0; i < childrenExpected; i++) {
            const containerName = "c" + (i + 1);
            const container = this.parseContainerXml(layoutElement, containerName);
            if (container != null)
                layout.containers.push(container);
        }
        return layout;
    }
    static parseClientLayout(layoutElement) {
        const layout = new environmentModels_1.LayoutModel();
        layout.mode = layoutElement.getAttribute("type").split(".").pop().toLowerCase();
        let containerNames = [];
        switch (layout.mode) {
            case "x1":
                containerNames = ["main"];
                break;
            case "x4":
                containerNames = ["c1", "c2", "c3", "c4"];
                break;
            case "layoutbottom":
                containerNames = ["main", "bottom"];
                break;
            case "layouttop":
                containerNames = ["top", "main"];
                break;
            case "layoutbottomleftright":
                containerNames = ["bottom", "left", "right", "main"];
                break;
            case "layouttopbottom":
                containerNames = ["bottom", "top", "main"];
                break;
            case "layoutleft":
                containerNames = ["left", "main"];
                break;
            case "layouttopleft":
                containerNames = ["left", "top", "main"];
                break;
            case "layoutleftright":
                containerNames = ["left", "right", "main"];
                break;
            case "layouttopleftright":
                containerNames = ["top", "left", "right", "main"];
                break;
            default:
                break;
        }
        for (let i = 0; i < containerNames.length; i++) {
            const containerName = containerNames[i];
            const container = this.parseContainerXml(layoutElement, containerName);
            if (container != null)
                layout.containers.push(container);
        }
        return layout;
    }
    static parseContainerXml(layout, containerName) {
        var _a, _b, _c;
        const containerXml = layout.getElementsByTagName(containerName)[0];
        if (containerXml != null) {
            const container = new environmentModels_1.ContainerModel(containerName);
            container.allowDragAndDrop = ((_a = containerXml.getAttribute("allowDragAndDrop")) === null || _a === void 0 ? void 0 : _a.toLowerCase()) !== 'false';
            container.hasMenu = ((_b = containerXml.getAttribute("hasMenu")) === null || _b === void 0 ? void 0 : _b.toLowerCase()) !== 'false';
            container.isSelectable = ((_c = containerXml.getAttribute("isSelectable")) === null || _c === void 0 ? void 0 : _c.toLowerCase()) !== 'false';
            const documentXml = containerXml.getElementsByTagName("document")[0];
            if (documentXml != null) {
                const argsXml = documentXml.getElementsByTagName("arg");
                const args = [];
                for (let j = 0; j < argsXml.length; j++) {
                    const parts = argsXml[j].innerHTML.split('=');
                    if (parts.length > 1) {
                        const name = parts[0];
                        const value = parts[1];
                        if (name != null && value != null) {
                            args.push({ name, value });
                        }
                    }
                    else {
                        args.push({ name: '', value: argsXml[j].innerHTML });
                    }
                }
                container.document = {
                    name: documentXml.getAttribute("name"),
                    args
                };
            }
            return container;
        }
        return null;
    }
}
exports.FileParser = FileParser;

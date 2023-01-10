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
exports.OptionManager = void 0;
const rxjs_1 = require("rxjs");
class OptionManager {
    constructor(http) {
        this.http = http;
        this.rowsUpdated = new rxjs_1.Subject();
    }
    /**
     * Get option value
     * @param optionName Option name
     * @returns Option value as string
     */
    get(optionName) {
        return this.options[optionName];
    }
    /**
     * Set option value
     * @param optionName Option name
     * @param value Option value
     */
    set(optionName, value) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppVisionService.svc/SetOption?name=' + optionName, JSON.stringify(value));
            this.options[optionName] = value;
        });
    }
    /** @internal */
    updateRowsInternal(rows) {
        for (const o of rows) {
            this.options[o.Name] = o.Value;
        }
        this.rowsUpdated.next(rows);
    }
    /** @internal */
    loadInternal(userManager) {
        return __awaiter(this, void 0, void 0, function* () {
            this.options = {};
            this.loadDefaultOptions(); // load default options
            const resp = yield this.http.get('AppVisionService.svc/GetOptions');
            const json = yield resp.json();
            for (const i of json)
                this.options[i.Name] = i.Value;
            // override with profile option
            for (const key of Object.keys(userManager.currentUser.profile.options))
                this.options[key] = userManager.currentUser.profile.options[key];
            // override with user option
            for (const key of Object.keys(userManager.currentUser.options))
                this.options[key] = userManager.currentUser.options[key];
        });
    }
    loadDefaultOptions() {
        this.options["App.Color.Alarm.Foreground"] = "red";
        this.options["App.Color.Alarm.Background"] = "transparent";
        this.options["App.Color.Alarm.WaitAck.Foreground"] = "white";
        this.options["App.Color.Alarm.WaitAck.Background"] = "red";
        this.options["App.Color.Alarm.WaitAckEnded.Foreground"] = "white";
        this.options["App.Color.Alarm.WaitAckEnded.Background"] = "magenta";
        this.options["App.Color.Alarm.Processing.Foreground"] = "black";
        this.options["App.Color.Alarm.Processing.Background"] = "orange";
        this.options["App.Color.Alarm.Waiting.Foreground"] = "white";
        this.options["App.Color.Alarm.Waiting.Background"] = "gray";
        this.options["App.Color.Event.User.Foreground"] = "black";
        this.options["App.Color.Event.User.Background"] = "cyan";
        this.options["App.Color.Event.Server.Foreground"] = "gray";
        this.options["App.Color.Event.Protocol.Foreground"] = "gray";
        this.options["App.Color.Event.Reader.Denied.Background"] = "yellow";
        this.options["App.Color.Event.Reader.Granted.Background"] = "yellow";
        this.options["App.Color.Event.Reader.Denied.Foreground"] = "red";
        this.options["App.Color.Event.Reader.Granted.Foreground"] = "black";
        this.options["App.Color.Event.Groups.Foreground"] = "gray";
        this.options["App.Color.State.Numeric.Foreground"] = "green";
        // Vidéo
        this.options["App.Client.Video.StretchVideo"] = "False";
        //Timetable
        this.options["App.Color.Timetable.0"] = '#FF6464';
        this.options["App.Color.Timetable.1"] = '#64FF64';
        this.options["App.Color.Timetable.2"] = '#6464FF';
        this.options["App.Color.Timetable.3"] = '#FFFF64';
        this.options["App.Color.Timetable.4"] = '#FF64FF';
        this.options["App.Color.Timetable.5"] = '#64FFFF';
        this.options["App.Color.Timetable.6"] = '#C8C8C8';
        this.options["App.Color.Timetable.7"] = '#FFC8C8';
        this.options["App.Color.Timetable.8"] = '#C8FFC8';
        this.options["App.Color.Timetable.9"] = '#C8C8FF';
        this.options["App.Color.Timetable.10"] = '#FFFFFF';
    }
}
exports.OptionManager = OptionManager;

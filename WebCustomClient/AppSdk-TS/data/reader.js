"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ReaderStatus = void 0;
var ReaderStatus;
(function (ReaderStatus) {
    /// <summary>
    /// In this case, it's AppVision server that checks access control.
    /// </summary>
    ReaderStatus[ReaderStatus["BadgeForServerChecking"] = 0] = "BadgeForServerChecking";
    /// <summary>
    /// Badge authorized.
    /// </summary>
    ReaderStatus[ReaderStatus["BadgeOk"] = 10] = "BadgeOk";
    ReaderStatus[ReaderStatus["BadgeOkByUser"] = 11] = "BadgeOkByUser";
    ReaderStatus[ReaderStatus["BadgeOkWithoutPassing"] = 12] = "BadgeOkWithoutPassing";
    ReaderStatus[ReaderStatus["BadgeOkOut"] = 13] = "BadgeOkOut";
    ReaderStatus[ReaderStatus["BadgeOkUnknown"] = 40] = "BadgeOkUnknown";
    ReaderStatus[ReaderStatus["BadgeOkAlarm"] = 48] = "BadgeOkAlarm";
    ReaderStatus[ReaderStatus["BadgeOkDuress"] = 49] = "BadgeOkDuress";
    /// <summary>
    /// Badge not authorized.
    /// </summary>
    ReaderStatus[ReaderStatus["BadgeNok"] = 50] = "BadgeNok";
    ReaderStatus[ReaderStatus["BadgeNokSchedule"] = 51] = "BadgeNokSchedule";
    ReaderStatus[ReaderStatus["BadgeNokSite"] = 52] = "BadgeNokSite";
    ReaderStatus[ReaderStatus["BadgeNokGroup"] = 53] = "BadgeNokGroup";
    ReaderStatus[ReaderStatus["BadgeNokCode"] = 54] = "BadgeNokCode";
    ReaderStatus[ReaderStatus["BadgeNokLock"] = 55] = "BadgeNokLock";
    ReaderStatus[ReaderStatus["BadgeNokInactive"] = 56] = "BadgeNokInactive";
    ReaderStatus[ReaderStatus["BadgeNokBlacklist"] = 57] = "BadgeNokBlacklist";
    ReaderStatus[ReaderStatus["BadgeNokPeriod"] = 58] = "BadgeNokPeriod";
    ReaderStatus[ReaderStatus["BadgeNokPincode"] = 59] = "BadgeNokPincode";
    ReaderStatus[ReaderStatus["BadgeNokUnknown"] = 60] = "BadgeNokUnknown";
    ReaderStatus[ReaderStatus["BadgeNokMax"] = 99] = "BadgeNokMax";
})(ReaderStatus = exports.ReaderStatus || (exports.ReaderStatus = {}));

"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.FileHelper = void 0;
const fileType_1 = require("../data/fileType");
class FileHelper {
    static getFileType(str) {
        const parts = str.split('.');
        if (parts.length > 1) {
            const ext = parts[parts.length - 1];
            switch (ext.toLowerCase()) {
                case 'png':
                case 'jpg':
                case 'jpeg':
                case 'bmp':
                case 'gif':
                case 'tiff':
                    return fileType_1.FileType.Image;
                case 'mp4':
                case 'avi':
                case 'mkv':
                case 'webm':
                case 'flv':
                case 'wmv':
                case 'mov':
                case 'mpg':
                case 'mpeg':
                case 'm4v':
                    return fileType_1.FileType.Video;
                case '3gp':
                case 'mp3':
                case 'aac':
                case 'm4a':
                case 'wma':
                case 'wav':
                    return fileType_1.FileType.Audio;
                default:
                    return fileType_1.FileType.Other;
            }
        }
        return fileType_1.FileType.Other;
    }
}
exports.FileHelper = FileHelper;

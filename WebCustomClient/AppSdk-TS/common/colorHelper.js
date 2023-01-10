"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ColorHelper = void 0;
class ColorHelper {
    /**
     * Convert XAML color into a CSS color
     * @param xamlColor Xaml format color
     * @returns CSS color
     */
    static xamlToCssColor(xamlColor) {
        if (xamlColor === null || xamlColor === void 0 ? void 0 : xamlColor.startsWith('#'))
            return '#' + xamlColor.substring(3) + xamlColor.substring(1, 3);
        return xamlColor;
    }
    /**
     * Add opacitity to a color
     * @param rgbaColor
     * @param opacity opacity value : 0 to 1
     * @returns New color
     */
    static addOpacity(rgbaColor, opacity) {
        if (rgbaColor === null || rgbaColor === void 0 ? void 0 : rgbaColor.startsWith('#')) {
            const color = rgbaColor.substring(0, 7);
            return color + Math.round(opacity * 255).toString(16);
        }
        return rgbaColor;
    }
}
exports.ColorHelper = ColorHelper;

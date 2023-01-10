export declare abstract class ColorHelper {
    /**
     * Convert XAML color into a CSS color
     * @param xamlColor Xaml format color
     * @returns CSS color
     */
    static xamlToCssColor(xamlColor: string): string;
    /**
     * Add opacitity to a color
     * @param rgbaColor
     * @param opacity opacity value : 0 to 1
     * @returns New color
     */
    static addOpacity(rgbaColor: string, opacity: number): string;
}

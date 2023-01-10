export declare abstract class ArrayHelper {
    static groupBy(arr: any[], key: string): any[];
    /**
     * Recusive foreach. Apply callback to array and children
     * @param arr Base array
     * @param childKey Children property name
     * @param callback Callback to apply
     */
    static recursiveForEach(arr: any[], childrenKey: string, callback: (obj: any) => void): void;
}

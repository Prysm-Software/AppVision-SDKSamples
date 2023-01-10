export declare abstract class Base64Helper {
    private static keyStr;
    /**
     * Encode into base64
     * @param input Input string to encode
     * @returns Encoded string in Base64
     */
    static encode(input: string): string;
    /**
     * Decode base64 string
     * @param input input to decode
     * @returns decoded string
     */
    static decode(input: string): string;
    static blobToBase64(blob: Blob): Promise<string | ArrayBuffer>;
    static base64ToBlob(b64: string, type?: string): Blob;
    private static utf8Encode;
    private static utf8Decode;
}

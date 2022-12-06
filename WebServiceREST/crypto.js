
async function pbkdf2(password, salt = 'aazsderfgth', iterations = 1000) {
    const txtEnc = new TextEncoder();
    const baseKey = await window.crypto.subtle.importKey('raw', txtEnc.encode(password), 'PBKDF2', false, ["deriveKey"]);
    const key = await window.crypto.subtle.deriveKey({ "name": "PBKDF2", "hash": "SHA-1", "iterations": iterations, "salt": txtEnc.encode(salt) }, baseKey, { "name": "HMAC", "hash": "SHA-1" }, true, ["sign"]);
    const expKey = await window.crypto.subtle.exportKey("raw", key);
    return btoa(String.fromCharCode(...new Uint8Array(expKey)));
}
// PasskeyAuthentication: web part of the plugin, for the AppVision 4.10.2 web client.
//
// The web client loads this ES module at run time for the method Passkey (Ui = Plugin): the URL is the WebModule= parameter published
// by PasskeyAuth.Describe(), /authplugins/Passkey.js, so the file goes to the AppVision Bin\Web\authplugins folder of the server.
// No build step, no dependency: the browser does the WebAuthn ceremony itself (navigator.credentials), the web client gives the
// two Login calls of the ceremony (ctx.login, ctx.continueLogin) and keeps the rest (sign-in warning, second factor, refusals).
//
// Contract (apiVersion 1): export default { apiVersion, unavailable(ctx), signIn: form, actions: [form] }, a form being
// { label, icon, help, fields, run(ctx) }; texts are { en, fr, ... }. See the README, "Web client".

const texts = {
    signIn: { en: 'Sign in with a passkey', fr: 'Me connecter avec une passkey' },
    register: { en: 'Register this device', fr: 'Enregistrer cet appareil' },
    registerHelp: {
        en: 'Creates a passkey for your account on this device (Windows Hello, security key or phone). Your password is asked once.',
        fr: 'Crée une passkey pour votre compte sur cet appareil (Windows Hello, clé de sécurité ou téléphone). Votre mot de passe est demandé une fois.',
    },
    device: { en: 'Name of this device', fr: 'Nom de cet appareil' },
    unsupported: {
        en: 'Passkeys need a secure connection (HTTPS) and a recent browser',
        fr: 'Les passkeys demandent une connexion sécurisée (HTTPS) et un navigateur récent',
    },
    cancelled: { en: 'The passkey prompt was cancelled or timed out', fr: "L'invite de la passkey a été annulée ou a expiré" },
    credentials: { en: 'User name and password required', fr: 'Identifiant et mot de passe requis' },
    noOptions: { en: 'Passkey: options expected from the server', fr: 'Passkey : options attendues du serveur' },
};

const text = (ctx, key) => texts[key][ctx.language] ?? texts[key].en;

// sign-in: Step=Begin answers 401 "Passkey:Get:{json}", the authenticator signs the challenge, Step=Finish opens the session.
// No user name: Windows (or the browser) lists the passkeys registered for this server and the user picks one if there are several
// (discoverable credentials); the server opens the session of the account the chosen passkey belongs to.
async function signIn(ctx) {
    const json = parse(ctx, await ctx.login({ Step: 'Begin' }, 'Passkey:Get:'));

    const credential = await ceremony(ctx, () => navigator.credentials.get({
        publicKey: {
            rpId: json.RpId,
            challenge: fromBase64Url(json.Challenge),
            allowCredentials: (json.AllowCredentials ?? []).map(id => ({ type: 'public-key', id: fromBase64Url(id) })),
            userVerification: 'required',   // the server requires the UV flag: the passkey is two factors
            timeout: 120000,
        },
    }));
    const response = credential.response;

    await ctx.continueLogin({
        Step: 'Finish',
        CredentialId: credential.id,        // already base64url
        ClientData: toBase64Url(response.clientDataJSON),
        AuthenticatorData: toBase64Url(response.authenticatorData),
        Signature: toBase64Url(response.signature),
        UserHandle: response.userHandle ? toBase64Url(response.userHandle) : '',
    });
}

// registration: the user name and password prove the identity (ProofMethod of the plugin), Step=RegisterBegin answers
// 401 "Passkey:Create:{json}", the authenticator creates the key pair, Step=RegisterFinish stores the public key and opens the session
async function register(ctx) {
    if (!ctx.values.username || !ctx.values.password)
        throw new Error(text(ctx, 'credentials'));
    const json = parse(ctx, await ctx.login({ Step: 'RegisterBegin' }, 'Passkey:Create:'));

    const credential = await ceremony(ctx, () => navigator.credentials.create({
        publicKey: {
            rp: { id: json.RpId, name: json.RpName },
            user: { id: fromBase64Url(json.UserId), name: json.UserName, displayName: json.DisplayName },
            challenge: fromBase64Url(json.Challenge),
            pubKeyCredParams: [{ type: 'public-key', alg: -7 }, { type: 'public-key', alg: -257 }],    // ES256, RS256: those of the server
            excludeCredentials: (json.ExcludeCredentials ?? []).map(id => ({ type: 'public-key', id: fromBase64Url(id) })),
            authenticatorSelection: { residentKey: 'required', userVerification: 'required' },        // sign-in without a user name
            attestation: 'none',
            timeout: 120000,
        },
    }));

    await ctx.continueLogin({
        Step: 'RegisterFinish',
        Username: json.UserName,
        Device: ctx.values.device || defaultDeviceName(),
        ClientData: toBase64Url(credential.response.clientDataJSON),
        Attestation: toBase64Url(credential.response.attestationObject),
    });
}

// name proposed for the device: system and browser ("Windows – Google Chrome"), the user may type a more telling one ("Office PC")
function defaultDeviceName() {
    const ua = navigator.userAgent;
    const brand = navigator.userAgentData?.brands?.map(b => b.brand).find(b => !/not.?a.?brand|chromium/i.test(b));
    const browser = brand ?? (/Edg\//.test(ua) ? 'Microsoft Edge' : /Firefox\//.test(ua) ? 'Firefox' : /Chrome\//.test(ua) ? 'Chrome'
        : /Safari\//.test(ua) ? 'Safari' : '');
    const os = navigator.userAgentData?.platform || (/Windows/.test(ua) ? 'Windows' : /Android/.test(ua) ? 'Android'
        : /iPhone|iPad/.test(ua) ? 'iOS' : /Mac OS/.test(ua) ? 'macOS' : /Linux/.test(ua) ? 'Linux' : '');
    return [os, browser].filter(s => s).join(' – ') || 'Browser';
}

function parse(ctx, options) {
    if (!options)
        throw new Error(text(ctx, 'noOptions'));
    return JSON.parse(options);
}

// browser prompt: cancelled or timed out (NotAllowedError) told in the user's language
async function ceremony(ctx, prompt) {
    try {
        const credential = await prompt();
        if (credential)
            return credential;
    } catch (e) {
        if (e?.name !== 'NotAllowedError')
            throw e;
    }
    throw new Error(text(ctx, 'cancelled'));
}

const toBase64Url = buffer =>
    btoa(String.fromCharCode(...new Uint8Array(buffer))).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');

const fromBase64Url = value => {
    const base64 = (value ?? '').replace(/-/g, '+').replace(/_/g, '/');
    return Uint8Array.from(atob(base64 + '='.repeat((4 - base64.length % 4) % 4)), c => c.charCodeAt(0));
};

export default {
    apiVersion: 1,
    unavailable: ctx => window.isSecureContext && typeof window.PublicKeyCredential === 'function' ? null : text(ctx, 'unsupported'),
    signIn: { label: texts.signIn, icon: 'fas fa-fingerprint', fields: [], run: signIn },
    actions: [{
        label: texts.register,
        icon: 'fas fa-plus-circle',
        help: texts.registerHelp,
        fields: ['username', 'password', { name: 'device', label: texts.device, optional: true, value: defaultDeviceName() }],
        run: register,
    }],
};

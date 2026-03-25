# TwoFactorAuthentication — Custom 2FA Login Plugin

## Overview

A **server-side authentication plugin** that adds two-factor authentication
to AppVision login. The class library is loaded by the AppVision server at
startup.

### Authentication flow

```
1. Client sends credentials with  Authentication=TwoFactor
2. Server validates username + password   →  base.Login()
3. Server generates a 4-digit code, e-mails it, returns HTTP 401
   with body  "Continuation needed"
4. Client prompts operator for the code and re-submits with  Code=XXXX
5. Server validates the code (10-minute expiry) and returns UserRow
```

> Use this as a starting point for any custom authentication scheme:
> hardware token, SSO, LDAP, smart card, biometric, etc.

**References:** `appCommon.dll`, `appData.dll`, `appSDK.dll`,
`appServer.dll`, `appServer.Auth.dll`  
**Output:** `TwoFactorAuthentication.dll` → `AppVision 4\Bin\`

---

## Project Structure

| File | Role |
|---|---|
| `TwoFactorAuth.cs` | Single-class plugin: login flow, code generation, validation |

---

## Key Concepts

### 1. `AppUserLogin` base class

```csharp
public class TwoFactor : AppUserLogin
{
    public override string Name => "TwoFactor";

    public override void Initialize(IServer server) { /* one-time init */ }

    public override UserRow Login(NameValue[] args)
    {
        UserRow user = base.Login(args); // validates name + password
        // … second-factor logic …
        return user;
    }
}
```

### 2. `NameValue[]` args

Extract values with `Helper.GetParameter`:

```csharp
string auth = Helper.GetParameter("Authentication", args); // "TwoFactor"
string code = Helper.GetParameter("Code",           args); // "1234" or ""
string name = Helper.GetParameter("Name",           args); // username
string pwd  = Helper.GetParameter("Password",       args); // password
```

### 3. Continuation protocol — HTTP 401

When the first factor succeeds but the code has not yet been provided:

```csharp
throw new WebFaultException<string>(
    "Continuation needed",
    HttpStatusCode.Unauthorized);
```

The AppVision client SDK recognises this and re-submits with the operator's
code appended to `args`.

### 4. `LoginException`

Reject a login attempt with a typed reason:

```csharp
throw new LoginException(LoginResult.IncorrectUserPassword, "Invalid code");
```

| `LoginResult` value | Meaning |
|---|---|
| `IncorrectUserPassword` | Wrong code or password |
| `InvalidUser` | User not found |
| `AccountDisabled` | Account is locked |
| `AccountExpired` | Account has expired |

### 5. Code expiry and housekeeping

```csharp
// Stored in a ConcurrentDictionary<string, Candidate>
// Housekeeping at start of every Login() call:
void Housekeeping()
{
    var expired = _pending.Where(kv => kv.Value.Age > TimeSpan.FromMinutes(10));
    foreach (var kv in expired) _pending.TryRemove(kv.Key, out _);
}
```

### 6. `Helper.SendMail`

```csharp
Helper.SendMail(user.Email, "AppVision login code", $"Your code: {code}");
```

Requires valid SMTP settings in AppVision (**Options › Mail**).

---

## AppVision SDK Reference

### `AppUserLogin` (`appServer.dll`)

| Member | Description |
|---|---|
| `.Name` | Provider identifier — matched against `Authentication=` arg |
| `.Initialize(IServer)` | One-time initialisation at server startup |
| `.Login(NameValue[] args)` | Authentication entry point |
| `base.Login(args)` | Default username / password check |

### `LoginException` (`appServer.Auth.dll`)

```csharp
new LoginException(LoginResult.IncorrectUserPassword, "message")
```

### `Helper` (`appCommon.dll`)

| Member | Description |
|---|---|
| `.GetParameter(key, args)` | Extract a named value from `NameValue[]` |
| `.SendMail(to, subject, body)` | Send an SMTP e-mail via server settings |

---

## Configuration in AppVision

```
AppVision 4\Bin\   ← copy TwoFactorAuthentication.dll here

Configurator › Options › Authentication › Login provider
    TwoFactorAuthentication.dll#TwoFactorAuthentication.TwoFactor

Configurator › Options › Mail › SMTP settings
    Configure a valid SMTP server

Security › Users › [user] › Email
    Ensure each user has a valid e-mail address
```

---

## How to Build and Deploy

1. Build in **Release** configuration.  
2. Copy `TwoFactorAuthentication.dll` → `AppVision 4\Bin\`.  
3. **Restart the AppVision server** to load the plugin.

---

## Debugging

Attach Visual Studio to the AppVision server process.  
Use a local mail catcher (e.g. **smtp4dev**) during development to intercept e-mails.

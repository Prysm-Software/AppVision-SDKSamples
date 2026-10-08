# ExternalAuthentication — Sign in with the accounts of an external directory

**Used for:** letting the users of a site sign in to AppVision with the account they already have in another system
(the user base of a customer application, an HR or access control database, the REST API of an identity provider)
when it is not one of the methods built into AppVision (Windows / Active Directory, LDAP, SAML, client certificate).
The plugin checks the user name and password against that system, and gives the user the AppVision **profile** that
matches their **groups** in it. No AppVision account has to be created by hand.

**Audience:** C# developers who integrate AppVision with a customer information system. Server-side code: the plugin
runs inside `appServer.exe`.

**Available in:** AppVision 4.10 and later.

---

## How it works

```
appClient / appConfig / SDK / web client          appServer.exe
  user name + password  ─────────────────────▶   ExternalDirectoryAuth.Login(args)
                                                    │ IUserDirectory.Authenticate(user, password)   ◀── your system
                                                    │ groups ──▶ profile whose parameter ExternalGroup= matches
                                                    ▼
                                                 UserRow { Name, FirstName, LastName, Email, ProfileId }
                                                    │ 1st sign-in: external account created (no local password)
                                                    ▼ next ones: account updated (names, e-mail, profile)
                                                 session opened with the rights of the profile
```

| File | Role |
|---|---|
| `ExternalDirectoryAuth.cs` | The server plugin (`IAuthPlugin`): checks the account, maps the groups to a profile, returns the user. |
| `UserDirectory.cs` | `IUserDirectory`, the external system, and `JsonUserDirectory`, the one of this sample: a JSON file with salted password hashes. **Replace it** by a call to your system. |
| `ExternalSignIn.cs` | The sign-in screen of the clients: the standard user name / password form. |
| `ExternalUsers.json` | The demo directory: 3 users, groups `Operators`, `Supervisors`, `Contractors`. |
| `New-PasswordHash.ps1` | Computes the hash of a password for `ExternalUsers.json`. |

**References** (*Copy Local = False*, the DLL is loaded from the AppVision `Bin` folder): `appSDK.dll`
(`IAuthPlugin`), `appData.dll` (`UserRow`, `ProfileRow`), `appCommon.dll`, `appScript.dll`
(`ScriptServerBase.Server`: the server API in the server process), `appUI.dll` (standard sign-in form).
**Output:** `ExternalAuthentication.dll`.

---

## The plugin

```csharp
public class ExternalDirectoryAuth : IAuthPlugin
{
    public UserRow Login(NameValue[] args)
    {
        var userName = Helper.GetParameter("Username", args);
        var password = Helper.GetParameter("Password", args);

        var server = ScriptServerBase.Server;                       // server API: options, profiles, folders...
        IUserDirectory directory = new JsonUserDirectory(Path.Combine(server.GetPathParam(), "Server", "ExternalUsers.json"));
        var account = directory.Authenticate(userName, password)
            ?? throw new UnauthorizedAccessException("unknown user, account disabled or wrong password");

        var profile = /* first group of the account claimed by a profile parameter ExternalGroup=<group> */;

        return new UserRow { Name = account.Name, FirstName = ..., LastName = ..., Email = ..., ProfileId = profile.Id };
    }
}
```

- **Refuse by throwing an exception**, never return `null`. The refusal is recorded in the events (*Login failed*).
- **A `UserRow` without `Id`** is an **external account** for the server: created at the first sign-in (type
  *external*, no AppVision password), updated at the next ones. Its history (acknowledgements, reports, events) stays
  attached to it. An external account cannot sign in with an AppVision password. If an AppVision account with the
  same name already exists, AppVision 4.10 turns it into the external account (option
  `App.Main.Authentication.OverwriteLocalUser`, true by default); from 4.11 a local account is never taken over.
- **Local fallback.** When the plugin refuses, the server tries the AppVision accounts with the same user name and
  password: `ADM` and the other local accounts keep working, which is the way back in if the directory is down or
  the plugin misconfigured. A refused directory user therefore gets the message of that second attempt (*Invalid user
  or password*, or *Incorrect user type* for an existing external account).
- **Initialize is not called by every version**: read what you need through `ScriptServerBase.Server` when a user
  signs in.

### Replace the directory

`JsonUserDirectory` stands for your system. Implement `IUserDirectory.Authenticate` with its API, for example a REST
identity service:

```csharp
public ExternalUser Authenticate(string userName, string password)
{
    var response = _http.PostAsync(_url + "/authenticate",
        new StringContent(JsonConvert.SerializeObject(new { userName, password }), Encoding.UTF8, "application/json")).Result;
    if (response.StatusCode == HttpStatusCode.Unauthorized)
        return null;
    response.EnsureSuccessStatusCode();
    return JsonConvert.DeserializeObject<ExternalUser>(response.Content.ReadAsStringAsync().Result);   // name, groups...
}
```

Keep it fast (every sign-in waits for it, set a timeout) and over HTTPS. Read the address of your system from an
AppVision option (`server.GetOption("App.Main.Authentication.External.Url")`, created in the configurator *Options*)
rather than hard-coding it.

---

## Install

1. Build the project (`bin\Debug\` or `bin\Release\`).
2. Copy `ExternalAuthentication.dll` into the AppVision `Bin` folder (`C:\Program Files\AppVision 4\Bin`) of the
   **server**, and of the **client and configurator workstations**: AppVision 4.10 clients load their sign-in screen
   from the same DLL. Restart the clients.
3. Copy `ExternalUsers.json` into the `Param\Server` folder of the configuration on the server
   (`Configurations\<config>\Param\Server`): this folder is not sent to the clients.
4. Map the groups to profiles: Configurator › **Users › Profiles**, *Parameters* of the profile:
   `ExternalGroup=Operators` on the operators' profile, `ExternalGroup=Supervisors` on the supervisors' one.
5. Configurator › **Options**, `App.Main.Authentication` = `ExternalAuthentication.dll`. The next sign-ins go through
   the plugin.

Demo accounts of `ExternalUsers.json` (test only):

| User | Password | Groups | Result |
|---|---|---|---|
| `jdoe` | `Operator-2026` | Operators | signed in, profile of `ExternalGroup=Operators` |
| `asmith` | `Supervisor-2026` | Supervisors, Operators | signed in, profile of `ExternalGroup=Supervisors` (first group claimed) |
| `bmartin` | `Contractor-2026` | Contractors | refused: account disabled (`"disabled": true`) |

Result on a test server (users `Users` profile with `ExternalGroup=Operators`, `Administrators` with
`ExternalGroup=Supervisors`), sign-ins made with the .NET SDK:

```
jdoe     OK      -> John Doe, profile id 1, type 1 (external)
asmith   OK      -> Alice Smith, profile id 2, type 1 (external)
jdoe     REFUSED -> Incorrect user type JDOE              (wrong password, then local fallback)
bmartin  REFUSED -> Login:Invalid user or password        (disabled in the directory)
ADM      OK      -> Administrator, profile id 2, type 0   (local account, fallback)
```

To add a user, add an entry to the JSON file with the hash given by `.\New-PasswordHash.ps1 "the password"`. The file
is read at each sign-in.

---

## Troubleshooting

| Symptom | Cause |
|---|---|
| Every directory user is refused, `ADM` works | The DLL is not in the server `Bin`, the option value is wrong, or `ExternalUsers.json` is not in `Param\Server`: see the server log (*ExternalDirectory*, *FileNotFoundException*). |
| `no AppVision profile for the groups ...` in the log | No profile has `ExternalGroup=` with one of the user's groups. |
| *Incorrect user type* | Wrong password for a directory user who already signed in once (message of the local fallback). |
| *Local user exists* (4.11) | An AppVision account with the same name exists: a local account is never taken over. Rename or delete it. |
| The sign-in screen of a client is empty or wrong | AppVision 4.10 client without `ExternalAuthentication.dll` in its `Bin`. |
| Locked out | Sign in with a local administrator account (fallback), or set `App.Main.Authentication` back to `0` (standard). |

---

## Learning path

```
ClientSimple  ->  ExternalAuthentication
SDK client        server plugin: who may sign in, with which profile
```

TwoFactorAuthentication - Custom Login Provider Sample
======================================================

OVERVIEW
--------
A server-side authentication plugin that adds two-factor authentication (2FA)
to AppVision login. It is a class library loaded by the AppVision server at
startup.

Flow:
  1. Client sends credentials with  Authentication=TwoFactor.
  2. Server validates username + password against the built-in AppVision
     user repository (base.Login).
  3. Server generates a 4-digit code, e-mails it to the user, and returns
     HTTP 401 Unauthorized with the body "Continuation needed".
  4. Client prompts the operator for the code and re-submits with Code=XXXX.
  5. Server validates the code (10-minute expiry) and returns the UserRow.

Use this as a starting point for any custom authentication scheme:
  hardware token, SSO, LDAP, smart card, biometric, etc.

This C# project references:
  appCommon.dll       (Prysm.AppVision.Common)     - Helper.GetParameter, Helper.SendMail
  appData.dll         (Prysm.AppVision.Data)        - UserRow, NameValue
  appSDK.dll          (Prysm.AppVision.SDK)         - IServer
  appServer.dll       (Prysm.AppVision.Server)      - AppUserLogin base class
  appServer.Auth.dll  (Prysm.AppVision.Server.Auth) - LoginException, LoginResult

The output 'TwoFactorAuthentication.dll' is copied into 'AppVision 4\Bin\' folder.


PROJECT STRUCTURE
-----------------
  TwoFactorAuth.cs   Single-class plugin: login flow, code generation and validation.


KEY CONCEPTS
------------
1. AppUserLogin base class
   All custom login providers inherit AppUserLogin (appServer.dll).
   Override:
       Name                 Unique provider name (matched against the
                            "Authentication" login parameter).
       Initialize(server)   Called once at server startup.
       Login(args)          Called on every login attempt.
   Chain to the default implementation:
       base.Login(args)     Validates name + password against AppVision users.
                            Returns UserRow on success, throws LoginException on failure.

2. NameValue[] args
   The Login method receives an array of key=value pairs from the client.
   Use Helper.GetParameter to extract values by key:
       Helper.GetParameter("Authentication", args)   -> "TwoFactor"
       Helper.GetParameter("Code", args)             -> "1234" (may be empty)
       Helper.GetParameter("Name", args)             -> username
       Helper.GetParameter("Password", args)         -> password (clear or hashed)

3. Continuation protocol
   When the first factor succeeds but the second factor is not yet provided,
   throw WebFaultException<string> with HTTP 401:
       throw new WebFaultException<string>("Continuation needed",
                                           HttpStatusCode.Unauthorized);
   The AppVision client SDK recognises this response and re-submits with the
   operator-provided code appended to the args.

4. LoginException
   Use LoginException to reject a login attempt with a typed result:
       throw new LoginException(LoginResult.IncorrectUserPassword, "Invalid code");
   Available results: IncorrectUserPassword, InvalidUser, AccountDisabled,
                      AccountExpired, NoLicense, ...

5. Code expiry and housekeeping
   Issued codes are stored in a static ConcurrentDictionary<string, Candidate>.
   Housekeeping() iterates the dictionary and removes entries older than 10 minutes.
   Call it at the start of Login() to avoid unbounded growth.

6. Helper.SendMail
   Sends an SMTP e-mail using the server's configured SMTP settings:
       Helper.SendMail(toAddress, subject, htmlBody);
   Ensure AppVision server has valid SMTP settings (Configurator > Options > Mail).


APPVISION SDK REFERENCE
-----------------------
AppUserLogin  (appServer.dll)
  .Name                            Provider identifier string.
  .Initialize(IServer server)      One-time initialisation.
  .Login(NameValue[] args)         Authentication entry point.
  base.Login(args)                 Default username/password check.

LoginException  (appServer.Auth.dll)
  new LoginException(LoginResult, message)   Reject a login attempt.

LoginResult  (appServer.Auth.dll)
  .IncorrectUserPassword / .InvalidUser / .AccountDisabled / ...

Helper  (appCommon.dll)
  .GetParameter(key, args)         Extract a named value from NameValue[].
  .SendMail(to, subject, body)     Send an SMTP e-mail via server settings.

WebFaultException<T>  (System.ServiceModel.Web)
  Used to return HTTP 401 with a string body to signal continuation.


CONFIGURATION IN APPVISION
---------------------------
1. Copy TwoFactorAuthentication.dll to:  AppVision 4\Bin\

2. In the Configurator:
       Options > Authentication > Login provider
           Set to: TwoFactorAuthentication.dll#TwoFactorAuthentication.TwoFactor

3. Configure SMTP:
       Options > Mail > SMTP settings

4. Ensure users have a valid e-mail address in:
       Security > Users > [user] > Email


HOW TO BUILD AND DEPLOY
-----------------------
1. Build in Release configuration.
2. Copy TwoFactorAuthentication.dll to:  AppVision 4\Bin\
3. Restart the AppVision server to load the plugin.


DEBUGGING
---------
- Attach Visual Studio debugger to the AppVision server process.
- Set a breakpoint in Login() and use a test client to trigger a login.
- Monitor SMTP traffic with a local mail catcher (e.g. smtp4dev) during development.

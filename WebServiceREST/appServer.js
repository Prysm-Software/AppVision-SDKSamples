
/**
 * Thin JavaScript wrapper around the AppVision REST service
 * (<code>AppVisionService.svc</code>).
 *
 * Typical usage:
 * <pre>
 *   const srv = new AppServer();
 *   await srv.login('http://localhost', 'ADM', '123');
 *   // server events are now streamed via the keepAlive polling loop
 *   srv.logout();
 * </pre>
 *
 * After a successful {@link login} the instance exposes:
 * - {@link user}  – the logged-in user object returned by <code>GetCurrentUser</code>.
 * - {@link desc}  – the project description returned by <code>GetOption</code>.
 */
class AppServer {

    /**
     * Initialises the default Fetch option objects shared by all HTTP requests.
     *
     * - {@link get}  – options for GET requests expecting a JSON response.
     * - {@link post} – options for POST requests sending and expecting JSON.
     *
     * The session cookie set by the server during {@link login} is forwarded
     * automatically thanks to <code>credentials: 'include'</code>.
     */
    constructor() {
        this.get = {
            // ask server to reply with JSON formated data
            headers: { 'Accept': 'application/json' },
            // send session id in cookie
            credentials: 'include'
        };
        this.post = {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        }
    }

    /**
     * Opens a session on the AppVision server and starts the notification
     * polling loop.
     *
     * Steps performed in order:
     *  1. <code>/Open</code> – establishes the HTTP session (the server sets a
     *     session cookie).
     *  2. <code>/Login</code> – authenticates with username and password.
     *  3. <code>/GetCurrentUser</code> and <code>/GetOption</code> – fetched in
     *     the background to populate {@link user} and {@link desc}.
     *  4. <code>/AddFilterNotifications</code> + <code>/StartNotifications</code>
     *     – subscribes to all server events.
     *  5. Schedules the first {@link keepAlive} call after 500 ms.
     *
     * @async
     * @param {string} host - Base URL of the AppVision server,
     *   e.g. <code>http://localhost</code>.
     * @param {string} user - AppVision login name.
     * @param {string} pwd  - Plain-text password sent in the credentials array.
     * @throws {string} A human-readable error string when any mandatory step
     *   returns a non-200 HTTP status.
     */
    async login(host, user, pwd) {
        this.host = host;

        // connect to server
        let resp = await fetch(host + '/AppVisionService.svc/Open?clientProductDescription=WebService%20demo', this.get);
        if (resp.status != 200)
            throw 'Failed to connect to ' + host;

        // compute password hash
        const sessionId = await resp.json();
        const credentials = [
            { Name: "username", Value: user },
            { Name: "password", Value: pwd }
        ];

        // Authorization header required when AD/LDAP authentication is enabled on the server
        //this.post.headers.Authorization = 'Basic ' + btoa(user + ':' + pwd);

        // log in AppVision server
        resp = await fetch(`${host}/AppVisionService.svc/Login`, { ...this.post, body: JSON.stringify(credentials) });
        if (resp.status != 200)
            throw `User ${user} unauthorized`;

        // retrieve user information
        fetch(host + '/AppVisionService.svc/GetCurrentUser', this.get)
            .then(r => r.json())
            .then(u => this.user = u);

        // retrieve configuration information
        fetch(host + '/AppVisionService.svc/GetOption?name=App.Main.ProjectDescription', this.get)
            .then(r => r.json())
            .then(opt => this.desc = opt);

        // register for all server events (filter = All)
        resp = await fetch(host + '/AppVisionService.svc/AddFilterNotifications', { ...this.post, body: '"All"' });
        if (resp.status != 200)
            throw 'AddFilterNotifications failed';
        resp = await fetch(host + '/AppVisionService.svc/StartNotifications?send=false', this.get);
        if (resp.status != 200)
            throw 'StartNotifications failed';

        // poll for server event
        setTimeout(() => this.keepAlive(), 500);
    }


    // terminate server connection
    /**
     * Closes the current AppVision session.
     *
     * Fire-and-forget: the server invalidates the session regardless of the
     * client outcome, so no response handling is required.
     */
    logout() {
        fetch(this.host + '/AppVisionService.svc/Logout', this.get);
    }


    /**
     * Polls the server for pending notifications every 500 ms.
     *
     * <code>/KeepAlive</code> returns the count of buffered notifications.
     * When the count is positive, <code>/GetNotifications</code> retrieves up
     * to 100 items at a time; each payload is dispatched to
     * {@link gotEvent}.  The next poll is always scheduled after a successful
     * response, even if no notifications were pending.
     *
     * @async
     * @private
     */
    async keepAlive() {
        try {
            let resp = await fetch(this.host + '/AppVisionService.svc/KeepAlive', this.get);
            let notifCount = await resp.json();
            if (notifCount > 0) {

                await fetch(this.host + '/AppVisionService.svc/GetNotifications?count=100', this.get)
                    .then(res => res.json())
                    .then(notifs => {
                        for (let n of notifs) {
                            // Data is either a single object or an array depending on the notification type
                            if (!Array.isArray(n.Data))
                                this.gotEvent(n.Data)
                            else
                                for (let it of n.Data)
                                    this.gotEvent(it);
                        }
                    });
            }
            // schedule the next poll
            setTimeout(() => this.keepAlive(), 500);
        } catch (err) {
            document.writeln(err);
        }
    }

    /**
     * Dispatches a single deserialized notification to the appropriate UI
     * handler based on its WCF <code>__type</code> discriminator.
     *
     * | `__type` value | Action |
     * |---|---|
     * | `ServerState:#Prysm.AppVision.Data`    | Stores the latest server-state snapshot in {@link serverState}. |
     * | `AlarmInfo:#Prysm.AppVision.Data`      | Appends an alarm line to `#log`. |
     * | `EventRow:#Prysm.AppVision.Data`       | Appends an event line to `#log`. |
     * | `VariableState:#Prysm.AppVision.Data`  | Appends a variable=value line to `#log`. |
     *
     * @param {Object} notif          - Deserialized notification payload.
     * @param {string} notif.__type   - WCF polymorphic type discriminator.
     */
    gotEvent(notif) {
        const log = document.getElementById('log');
        if (notif.hasOwnProperty('__type')) {
            switch (notif.__type) {
                case 'ServerState:#Prysm.AppVision.Data':
                    this.serverState = notif;
                    break;
                case 'AlarmInfo:#Prysm.AppVision.Data':
                    log.innerHTML += `Alarm:${notif.Event}: ${notif.Alarm.Description}<br/>`;
                    break;
                case 'EventRow:#Prysm.AppVision.Data':
                    log.innerHTML += notif.StateDesc + ': ' + notif.Description + '<br>';
                    break;
                case 'VariableState:#Prysm.AppVision.Data':
                    log.innerHTML += notif.Name + '=' + notif.Value + '<br>';
                    break;
            }
        }
    }
}
/**
 * Minimal client for the AppVision REST service (AppVisionService.svc), without any SDK.
 *
 * Plain JavaScript and the Fetch API: no DOM access in this file, so the same class also runs in
 * Node.js 18+ (global fetch). The page (index.html) only provides the callbacks.
 *
 *   const srv = new AppServer('http://localhost');
 *   srv.onNotification = n => console.log(n);   // VariableState, AlarmInfo, EventRow...
 *   await srv.login('ADM', '123');               // opens the session and starts polling
 *   const state = await srv.get('GetVariableStateByName?name=' + encodeURIComponent('Building.Temp'));
 *   await srv.logout();
 *
 * Session: the id returned by Open is sent in the "SessionID" header of every request. No cookie
 * is needed, which is also what a non-browser client (Python, C#, PowerShell...) has to do.
 */
class AppServer {

    /**
     * @param {string} host Base URL of the AppVision server, e.g. "http://localhost" or "https://server".
     */
    constructor(host) {
        this.host = host.replace(/\/+$/, '');
        this.sessionId = null;
        this.user = null;           // UserRow of the logged-in user (GetCurrentUser)
        this.projectName = null;    // option App.Main.ProjectDescription
        this.serverState = null;    // last ServerState notification
        this.pollPeriod = 500;      // ms between two KeepAlive calls

        /** Called with each notification payload (object with a __type field). */
        this.onNotification = () => { };
        /** Called once when the session is lost or polling fails (Error). */
        this.onDisconnected = () => { };

        this._timer = null;
    }

    /** Full URL of a REST operation, e.g. url('KeepAlive'). */
    url(operation) {
        return `${this.host}/AppVisionService.svc/${operation}`;
    }

    /** GET a REST operation and return the parsed JSON (undefined for an empty response). */
    async get(operation) {
        return this._send(operation, { method: 'GET' });
    }

    /** POST a JSON body to a REST operation and return the parsed JSON (undefined if empty). */
    async post(operation, body, extraHeaders) {
        return this._send(operation, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', ...extraHeaders },
            body: JSON.stringify(body)
        });
    }

    async _send(operation, init) {
        const headers = { 'Accept': 'application/json', ...init.headers };
        if (this.sessionId)
            headers['SessionID'] = this.sessionId;

        const resp = await fetch(this.url(operation), { ...init, headers });
        const text = await resp.text();
        if (!resp.ok)
            throw new Error(`${operation}: HTTP ${resp.status} ${text || resp.statusText}`);
        return text ? JSON.parse(text) : undefined;
    }

    /**
     * Opens a session, logs in, subscribes to all notifications and starts the polling loop.
     * @param {string} username AppVision user (or "$P.<protocol>" for a protocol account)
     * @param {string} password Password, sent once to Login. Use https outside a test bench.
     */
    async login(username, password) {
        // 1. session (noCookie=true: the session id travels in the SessionID header)
        this.sessionId = await this.get('Open?clientProductName=WebServiceREST&clientProductDescription='
            + encodeURIComponent('AppVision REST sample') + '&noCookie=true');
        if (!this.sessionId)
            throw new Error('Open: no session id returned');

        // 2. authentication. The Basic header is only read when the server delegates authentication
        //    (Active Directory, LDAP...); it is sent on this single request, as the TypeScript SDK does.
        try {
            await this.post('Login',
                [{ Name: 'username', Value: username }, { Name: 'password', Value: password }],
                { 'Authorization': 'Basic ' + btoa(unescape(encodeURIComponent(username + ':' + password))) });
        } catch (err) {
            await this._close();
            throw err;
        }

        // 3. context of the session
        [this.user, this.projectName] = await Promise.all([
            this.get('GetCurrentUser'),
            this.get('GetOption?name=App.Main.ProjectDescription')
        ]);

        // 4. notifications: filter "All" (alternatives: "$V.Building.*", "$A.Area1", "$G.Group1"...)
        await this.post('AddFilterNotifications', 'All');
        await this.get('StartNotifications?send=false');

        // 5. polling loop
        this._timer = setTimeout(() => this._poll(), this.pollPeriod);
    }

    /** Stops polling and closes the session on the server. */
    async logout() {
        clearTimeout(this._timer);
        this._timer = null;
        await this._close();
    }

    async _close() {
        if (!this.sessionId)
            return;
        try {
            await this.get('Close');
        } catch {
            // the session may already be gone on the server side
        }
        this.sessionId = null;
    }

    /**
     * KeepAlive keeps the session open and returns the number of waiting notifications
     * (-1 when the server no longer knows the session). GetNotifications dequeues them.
     */
    async _poll() {
        try {
            let count = await this.get('KeepAlive');
            if (count < 0)
                throw new Error('Session closed by the server (timeout, restart or logout)');

            // dequeue by blocks of 100
            for (; count > 0; count -= 100) {
                const notifications = await this.get('GetNotifications?count=100');
                if (!notifications?.length)
                    break;
                // each Notification has a Data field: one object or an array of objects
                for (const n of notifications)
                    for (const data of [].concat(n.Data ?? []))
                        this._dispatch(data);
            }
        } catch (err) {
            this._timer = null;
            this.sessionId = null;
            this.onDisconnected(err);
            return;
        }
        if (this.sessionId)
            this._timer = setTimeout(() => this._poll(), this.pollPeriod);
    }

    _dispatch(data) {
        if (data?.__type === 'ServerState:#Prysm.AppVision.Data')
            this.serverState = data;
        this.onNotification(data);
    }
}

/** Values of AlarmInfo.Event (enum AlarmEvent, serialized as a number). */
AppServer.AlarmEvents = ['Start', 'End', 'Update', 'Acknowledge', 'Report', 'Resume', 'Delete', 'Transfer', 'None', 'Complete'];

if (typeof module !== 'undefined')
    module.exports = { AppServer };

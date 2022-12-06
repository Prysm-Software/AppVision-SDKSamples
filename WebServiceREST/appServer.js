
class AppServer {

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
            { Name: "password", Value: await pbkdf2(sessionId + await pbkdf2(user + pwd)) }
        ];

        // Authorization header required if AD/LDAP auth is enabled
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
    logout() {
        fetch(this.host + '/AppVisionService.svc/Logout', this.get);
    }


    // the KeepAlive request returns the count of pending notifications,
    // call GetNotifications to retrieve them
    async keepAlive() {
        try {
            let resp = await fetch(host + '/AppVisionService.svc/KeepAlive', this.get);
            let notifCount = await resp.json();
            if (notifCount > 0) {

                await fetch(host + '/AppVisionService.svc/GetNotifications?count=100', this.get)
                    .then(res => res.json())
                    .then(notifs => {
                        for (let n of notifs) {
                            if (!Array.isArray(n.Data))
                                this.gotEvent(n.Data)
                            else
                                for (let it of n.Data)
                                    this.gotEvent(it);
                        }
                    });
            }
            setTimeout(() => this.keepAlive(), 500);
        } catch (err) {
            document.writeln(err);
        }
    }

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


    // compute SHA-512 hash
    async sha512(data){
        const hash = await crypto.subtle.digest({name: "SHA-512"}, Uint8Array.from(data));
        return Array.prototype.map.call(new Uint8Array(hash), x => x.toString(16).padStart(2, '0')).join(''); // hex string
    }
}
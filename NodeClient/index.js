// NodeClient - AppVision JavaScript SDK (@prysm/appsdk) from Node.js, without a browser.
//
//   node index.js                     connect, print a summary, then follow variables and alarms live (Ctrl+C to quit)
//   node index.js set <var> <value>   write a value into a variable, e.g. node index.js set DrvSampleVar.Output1 1
//   node index.js ack <alarmId>       acknowledge an alarm
//   node index.js history [hours]     print the events of the last hours (default 24)
//
// Connection settings come from environment variables (defaults: http://localhost, ADM / 123):
//   APPVISION_HOST, APPVISION_USER, APPVISION_PASSWORD

// The SDK targets the browser: its base64 helper calls window.btoa / window.atob. Node (16+) has btoa and atob
// as globals, so expose them under 'window' before loading the SDK.
globalThis.window ??= globalThis;

const { AppServer, DateHelper, AlarmEvent } = require('@prysm/appsdk');

const options = {
	host: process.env.APPVISION_HOST || 'http://localhost',
	username: process.env.APPVISION_USER || 'ADM',
	password: process.env.APPVISION_PASSWORD || '123',
	productName: 'NodeClient',
	productDescription: 'AppVision SDK sample (Node.js)',
	productCompany: 'Partner',
	// Required outside a browser: without it, login() reads navigator.userAgent, which Node < 21 does not have
	productProcess: 'node',
	clientHostname: require('os').hostname(),
};

// One AppServer instance = one session. login() loads the configuration (areas, groups, variables, alarms...)
// into memory: create it once for the whole application.
const server = new AppServer();

const time = (dto) => (dto ? DateHelper.toJSDate(dto).toLocaleString() : '');
const quality = (state) => (state.Quality === 255 ? '' : ' (stale)'); // 255 = good, 0 = unknown

async function main() {
	const [command, ...args] = process.argv.slice(2);

	console.log(`Connecting to ${options.host} as ${options.username}...`);
	await server.login(options);
	console.log('Connected.');

	try {
		switch (command) {
			case 'set':
				return await setValue(args[0], args[1]);
			case 'ack':
				return await acknowledge(Number(args[0]));
			case 'history':
				return await history(Number(args[0] || 24));
			case undefined:
				summary();
				return follow(); // runs until Ctrl+C
			default:
				console.log('Usage: node index.js [set <var> <value> | ack <alarmId> | history [hours]]');
		}
	} finally {
		if (command !== undefined)
			await server.logout();
	}
}

// Reads from the cache that login() filled: synchronous, no request to the server.
function summary() {
	const variables = server.variableManager.getRows();
	console.log(`\n${variables.length} variables, ${server.areaManager.getRows().length} areas, ${server.groupManager.getRows().length} groups`);

	console.log('\nVariables with a value:');
	for (const state of server.variableManager.getStates().filter((s) => s.Value !== '' && s.Value != null).slice(0, 20))
		console.log(`  ${state.Name.padEnd(40)} ${String(state.Value).padEnd(10)}${quality(state)}`);

	const alarms = server.alarmManager.getAlarms();
	console.log(`\n${alarms.length} current alarm(s):`);
	for (const a of alarms)
		console.log(`  #${a.Id} [${a.Severity}] ${a.Description} - ${a.StateDesc}${a.WaitAcknowledge ? ' (to acknowledge)' : ''}`);
}

// Live feed. Nothing is received until startKeepAlive() runs: it polls the server's notification channel and
// feeds every rxjs Subject of the SDK.
function follow() {
	const subscriptions = [
		server.variableManager.statesChanged.subscribe((states) => {
			for (const s of states)
				console.log(`${new Date().toLocaleTimeString()}  ${s.Name} = ${s.Value}${quality(s)}`);
		}),
		server.alarmManager.alarmsChanged.subscribe((infos) => {
			for (const info of infos)
				console.log(`${new Date().toLocaleTimeString()}  alarm #${info.Alarm.Id} ${AlarmEvent[info.Event]}: ${info.Alarm.Description}`);
		}),
		server.connectionLost.subscribe(() => {
			console.error('Connection to the server lost.');
			process.exit(2);
		}),
	];
	server.startKeepAlive();
	console.log('\nFollowing variables and alarms, Ctrl+C to quit...');

	process.on('SIGINT', async () => {
		subscriptions.forEach((s) => s.unsubscribe());
		await server.logout(); // frees the session (and the client licence) on the server
		console.log('Disconnected.');
		process.exit(0);
	});
}

// Values are always written as strings: Bool '1'/'0', Enum = state index, Double with '.' as decimal separator.
async function setValue(name, value) {
	if (!server.variableManager.getRowByName(name))
		throw new Error(`Unknown variable ${name}`);
	await server.variableManager.set(name, value);
	console.log(`${name} <- ${value}`);
}

// Acknowledging takes the alarm in charge; it ends only when its condition is over (and its report written,
// when the alarm requires one).
async function acknowledge(id) {
	const alarm = server.alarmManager.getById(id);
	if (!alarm)
		throw new Error(`No current alarm #${id}`);
	await server.alarmManager.acknowledgeById(id);
	console.log(`Alarm #${id} acknowledged: ${alarm.Description}`);
}

// History is read from the server (async, paged by maxCount): never call it on each notification.
async function history(hours) {
	const end = new Date();
	const start = new Date(end.getTime() - hours * 3600 * 1000);
	const events = await server.eventManager.getHistoEvents(start, end, '', [], [], 50);
	console.log(`\nLast ${events.length} event(s) of the last ${hours} h:`);
	for (const e of events)
		console.log(`  ${time(e.Date).padEnd(22)} ${(e.OriginDesc || '').padEnd(14)} ${e.Description} ${e.StateDesc || ''}`);
}

main().catch(async (err) => {
	console.error('Error:', err?.message ?? err);
	process.exitCode = 1;
	try { await server.logout(); } catch { /* not connected */ }
});

import React from 'react';
import './App.css';
// Types only: at run time the SDK objects come from the AppVision web client (window.parent.sdk),
// so the SDK code is not bundled with this page.
import type { AppServer, AlarmRow } from '@prysm/appsdk';

/**
 * Custom page for the AppVision web client.
 *
 * The web client loads this page in an <iframe> and exposes its connected TypeScript SDK as
 * `window.sdk.server` (an AppServer). The page reuses that session: no login, no extra connection.
 * This only works when the page has the same origin as the web client, i.e. when it is served by the
 * AppVision server itself (Bin\Web\CustomControl).
 */

/** AppServer of the hosting web client, or undefined when the page is opened on its own. */
function getHostServer(): AppServer | undefined {
  try {
    return (window.parent as any).sdk?.server;
  } catch {
    return undefined;   // parent page of another origin: access refused by the browser
  }
}

function App() {
  const [server] = React.useState(getHostServer);
  const [alarms, setAlarms] = React.useState<AlarmRow[]>([]);
  const [lastChange, setLastChange] = React.useState('');

  React.useEffect(() => {
    if (!server?.isConnected)
      return;

    // current alarms (SDK cache, kept up to date by the web client), then on each alarm change;
    // most severe first, then oldest first
    const refreshAlarms = () => setAlarms([...server.alarmManager.getAlarms(20)]
      .sort((a, b) => b.Severity - a.Severity || (a.dateStart?.getTime() ?? 0) - (b.dateStart?.getTime() ?? 0)));
    refreshAlarms();
    const subAlarms = server.alarmManager.alarmsChanged.subscribe(refreshAlarms);

    // variable changes
    const subStates = server.variableManager.statesChanged.subscribe(states => {
      const s = states[states.length - 1];
      setLastChange(`${s.Name} = ${s.Value}`);
    });

    // unsubscribe when the page is unloaded: the subjects belong to the web client, they outlive this iframe
    return () => {
      subAlarms.unsubscribe();
      subStates.unsubscribe();
    };
  }, [server]);

  if (!server)
    return <div className="App">
      <h3>Custom page for the AppVision web client</h3>
      <p>No AppVision session: open this page inside the AppVision web client (see README.md).</p>
    </div>;

  return <div className="App">
    <h3>Custom page for the AppVision web client</h3>
    <p>{server.userManager.currentUser.fullName} - {server.http.host}</p>
    <p>{server.variableManager.getStates().length} variables. Last change: {lastChange || '-'}</p>

    <table>
      <thead>
        <tr><th>Start</th><th>Severity</th><th>Description</th><th>State</th></tr>
      </thead>
      <tbody>
        {alarms.map(a => <tr key={a.Id}>
          <td className="nowrap">{a.dateStart?.toLocaleString([], { dateStyle: 'short', timeStyle: 'short' })}</td>
          <td>{a.Severity}</td>
          <td>{a.Description}</td>
          <td>{a.StateDesc}</td>
        </tr>)}
        {alarms.length === 0 && <tr><td colSpan={4}>No current alarm</td></tr>}
      </tbody>
    </table>
  </div>;
}

export default App;

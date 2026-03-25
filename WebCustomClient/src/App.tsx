import React from 'react';
import './App.css';
import { AppServer } from "@prysm/appsdk/appServer"
import { VariableState } from '@prysm/appsdk/data/variableState';
import { AlarmInfo } from '@prysm/appsdk/data/alarmInfo';

/**
 * Root component of the AppVision custom web-client panel.
 *
 * The AppVision Web Client embeds this app inside an `<iframe>`.
 * The TypeScript SDK (`AppServer`) is retrieved from the parent window's
 * `sdk.server` object, so this panel shares the live session of the hosting
 * Web Client — no separate login is needed.
 *
 * Patterns demonstrated:
 * - Retrieving the shared {@link AppServer} via `window.parent.window.sdk`.
 * - Fetching all variable states with `variableManager.getStates()`.
 * - Subscribing to real-time alarm changes via `alarmManager.alarmsChanged`.
 * - Displaying the current user name and server URL from `AppServer`.
 */
function App() {


  const [appServer, setAppServer] = React.useState<AppServer>();

  /**
   * SDK initialisation — runs once on mount.
   *
   * Reads `window.parent.window.sdk.server` to obtain the `AppServer`
   * instance injected by the AppVision Web Client into the `<iframe>` host.
   * The try/catch handles the case where the component is opened outside
   * AppVision (e.g. during local `npm start` development).
   */
  React.useEffect(() => {

    /**
     * Retrieves the `AppServer` reference from the parent window and stores
     * it in component state, triggering the second effect.
     */
    const initSdk = () => {
      try {
        const appServer: AppServer = (window.parent.window as any).sdk?.server;


        if (appServer) {

          console.log("[CustomWebClient] Init Sdk Success - ", appServer.http.host);

          setAppServer(appServer);

        }
      }
      catch (e) {
        console.log("[CustomWebClient] Init Sdk Failed - ", e)
      }
    }

    initSdk();

  }, []);

  /**
   * Main data effect — re-runs whenever `appServer` changes.
   *
   * 1. Guards against an undefined or disconnected server.
   * 2. Fetches a one-shot snapshot of all variable states.
   * 3. Subscribes to `alarmManager.alarmsChanged` for real-time alarm updates.
   *
   * The cleanup function unsubscribes from the alarm stream when the
   * component unmounts or `appServer` changes, preventing memory leaks.
   */
  React.useEffect(() => {

    //work with AppServer

    if (appServer == undefined || !appServer.isConnected)
      return;

    /**
     * Fetches a snapshot of every variable's current value.
     * Extend this to filter by name or store the results in component state.
     */
    const getStates = async () => {
      const states: VariableState[] = await appServer.variableManager.getStates();
    }

    getStates();

    // Subscribe to real-time alarm changes; the cleanup return unsubscribes
    const subAlarms = appServer.alarmManager.alarmsChanged.subscribe((alarms: AlarmInfo[]) => {
      alarms.forEach(alarm => {
        console.log("[CustomWebClient] Alarm - ", alarm)
      });
    })


    return () => { subAlarms?.unsubscribe();}  // cleanup: avoid memory leaks
  }, [appServer]);





  // Render: show a connection banner only when AppServer is available
  return <div className='App'>
    <text>Custom Control for Web Client</text>

    {/* Display the current user and server URL once connected */}
    {appServer && <div className='Custom'>
      <label>{appServer.userManager.currentUser.fullName} - </label>
      <label>{appServer.http.host}</label>
    </div>}


  </div>
}

export default App;
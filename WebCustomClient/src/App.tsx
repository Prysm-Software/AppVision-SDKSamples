import React from 'react';
import './App.css';
import { AppServer } from "@prysm/appsdk/appServer"
import { VariableState } from '@prysm/appsdk/data/variableState';
import { AlarmInfo } from '@prysm/appsdk/data/alarmInfo';

function App() {


  const [appServer, setAppServer] = React.useState<AppServer>();

  React.useEffect(() => {


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

  React.useEffect(() => {

    //work with AppServer

    if (appServer == undefined || !appServer.isConnected)
      return;

    const getStates = async () => {
      const states: VariableState[] = await appServer.variableManager.getStates();
    }

    getStates();

    const subAlarms = appServer.alarmManager.alarmsChanged.subscribe((alarms: AlarmInfo[]) => {
      alarms.forEach(alarm => {
        console.log("[CustomWebClient] Alarm - ", alarm)
      });
    })


    return () => { subAlarms?.unsubscribe();}
  }, [appServer]);





  return <div className='App'>
    <text>Custom Control for Web Client</text>

    {appServer && <div className='Custom'>
      <label>{appServer.userManager.currentUser.fullName} - </label>
      <label>{appServer.http.host}</label>
    </div>}


  </div>
}

export default App;
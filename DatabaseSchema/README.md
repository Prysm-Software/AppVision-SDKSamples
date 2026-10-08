# DatabaseSchema — AppVision SQL Server databases

**Used for:** understanding where AppVision stores its configuration, history and current state, for reporting or
for checking data during an integration.

**Available in:** AppVision 4.x. The scripts in `SQL/` and the diagrams in `Diagrams/` describe the databases of
AppVision 4.10; columns are added from one version to the next.

> **Prefer the APIs.** The supported ways to read and write AppVision data are the SDK, the REST service and the OData
> services (see [WebServiceREST](../WebServiceREST/README.md), [WebServiceOData](../WebServiceOData/README.md)). They
> apply user rights, notify the clients and survive schema changes. Use SQL **read-only**, for reports: a row written
> directly in a table is not seen by the running server, and can be overwritten or break it.

---

## The databases

| Database | File (in the configuration folder) | Content | Script | OData service |
|---|---|---|---|---|
| `Param` | `Param\Param.mdf` | Configuration: `Variables`, `Areas`, `Groups`, `Users`, `ProfileUsers`, `Protocols`, `Options`, `Holidays`, `Scenarios`, `Instructions`, `Mailings`... | `SQL/Param.sql` | `AppVisionDataService.svc` |
| `Histo` | `Histo\Histo.mdf` | History: `Events`, `Alarms`, `Values` (archived variable values), `Modifs` (configuration changes), `UserMessages`, chat | `SQL/Histo.sql` | `AppVisionHistoDataService.svc` |
| `State` | `Histo\State.mdf` | Current state, reloaded at server start: `CurrentStates` (variable values), `Alarms` (current alarms), `Overlays` (overlays placed by program), `ScenarioStates`, `UserStates` | `SQL/State.sql` | — |
| `AppControl` | `Param\AppControl.mdf` | Access control: `Apc_People`, `Apc_Badges`, `Apc_BadgeTypes`, `Apc_Categories`, `Apc_Departments`, `Apc_AccessProfiles`, `Apc_Zones`, `Apc_Visitors`... | `SQL/AppControl.sql` | `AppControlDataService.svc` |

The connection strings (`ParamContext`, `HistoContext`, `StateContext`, `AppControlContext`) are in
`AppVision 4\Bin\appServer.exe.config`. By default the databases are attached to SQL Server LocalDB
(`(localdb)\MSSQLLocalDB`); a site can use a SQL Server instance instead.

Conventions: column names are prefixed with the table (`EventDate`, `EventStateDesc`...); dates of events and alarms
are `datetimeoffset`; `*Extension`, `*CustomExtension` and `*Parameters` columns hold free data (often XML or JSON) of
drivers and scripts.

> **The scripts drop and recreate the databases** (`DROP DATABASE` at the top). They are documentation: never run
> them on an AppVision server.

### Diagrams

`Diagrams/` contains one entity-relationship diagram per database: every table with its columns (type, `PK`, `FK`,
`null` when the column accepts NULL) and the foreign keys between tables. The `.mmd` files are their
[Mermaid](https://mermaid.js.org/) source (rendered by GitHub and by most Markdown editors), the `.png` files the
rendered images.

| Database | Diagram |
|---|---|
| `Param` | [Diagrams/Param.png](Diagrams/Param.png) |
| `Histo` | [Diagrams/Histo.png](Diagrams/Histo.png) |
| `State` | [Diagrams/State.png](Diagrams/State.png) |
| `AppControl` | [Diagrams/AppControl.png](Diagrams/AppControl.png) |

The diagrams and the scripts are generated from the same AppVision 4.10 databases, after the server has upgraded
them: they include the columns that the server adds at startup. Most identifiers are not foreign keys: the
`Histo` and `State` tables refer to the `Param` tables by id (`AreaId`, `SourceId`, `UserIdAck`...) and to the
variables by name, across databases.

---

## Example: last events with PowerShell

`Query events history.ps1` prints the last 25 events of the `Histo` database, newest first.

1. Install the SQL Server module once: `Install-Module SqlServer -Scope CurrentUser`.
2. With LocalDB, run the script as the Windows account that runs the AppVision server (a LocalDB instance belongs to
   one account). With a SQL Server instance, pass it: `& '.\Query events history.ps1' -ServerInstance '.\SQLEXPRESS'`.
3. Expected output: a table `EventDate | EventStateDesc | EventDescription | EventAreaDesc | EventOriginDesc` with
   server starts, logins, variable changes...

![Output of the script](Query%20result.png)

The same data without SQL access: `GetHistoEvents` of the REST service, or the `Events` set of
`http://<server>/AppVisionHistoDataService.svc/` (OData, `$filter`, `$orderby=EventDate desc`, `$top=25`).

---

## Troubleshooting

| Symptom | Cause |
|---|---|
| `Invoke-Sqlcmd` is not recognized | The `SqlServer` module is not installed (step 1). |
| `Cannot open database "Histo"` | LocalDB of another Windows account, or the server is not started (LocalDB attaches the files when the server opens them). Check the account and the connection string. |
| `A network-related or instance-specific error` | Wrong `-ServerInstance`: copy the `Data Source` of `HistoContext` from `appServer.exe.config`. |
| Empty result | No event recorded yet, or the history was purged (retention options). |

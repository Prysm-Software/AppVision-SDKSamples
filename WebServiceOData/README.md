# WebServiceOData — Read and write the AppVision configuration over OData

**Used for:** reading or changing the AppVision configuration (options, variables, areas, groups, users,
timetables...) from another application, without the AppVision SDK: synchronisation with a CMMS or a BIM tool,
bulk creation of variables, configuration reports.

**Available in:** AppVision 4.x (OData V3 service `AppVisionDataService.svc`).

**Audience:** .NET developers for the sample itself; the [HTTP section](#without-net) shows the same requests for any
language.

---

## How it works

The AppVision server publishes its configuration database (`Param`) as an OData V3 service:
`http(s)://<server>/AppVisionDataService.svc/`. Each table is an *entity set* that supports query (`GET` with
`$filter`, `$orderby`, `$top`, `$skip`, `$expand`), creation (`POST`), update (`MERGE`/`PUT`) and deletion
(`DELETE`), within the rights of the user.

| File | Role |
|---|---|
| `Program.cs` | Menu, `CreateContext()` (URL, credentials) and the four operations |
| `Service References/ParamServiceReference/` | Client classes generated from the service metadata (`ParamContext`, `OptionRow`, `VariableRow`...) |

**References:** NuGet `Microsoft.Data.Services.Client` 5.8.4 (WCF Data Services client) and its dependencies. No
AppVision DLL. **Output:** `WebServiceOData.exe` in `bin\Debug\`, with the WCF Data Services client DLLs (copied from the
AppVision `Bin` folder, which ships them).

```csharp
var ctx = new ParamContext(new Uri("http://localhost/AppVisionDataService.svc/"))
{
	Credentials = new NetworkCredential("ADM", "123"),     // HTTP Basic authentication
	IgnoreMissingProperties = true,                         // tolerate columns added by newer servers
};

foreach (var opt in ctx.Options)                            // GET Options
	Console.WriteLine($"{opt.Name} = {opt.Value}");

var row = ctx.Options.Where(o => o.Name == "TestOData").FirstOrDefault();   // GET Options?$filter=Name eq 'TestOData'
row.Value = "new value";
ctx.UpdateObject(row);                                      // marks the entity as modified
ctx.SaveChanges();                                          // MERGE Options('TestOData')
```

**Authentication.** The service answers `401` with `WWW-Authenticate: Basic`; the client then sends the user and
password of `Credentials`. Use `https` outside a test bench, and an account whose profile has only the rights needed.

**`IgnoreMissingProperties`.** The generated classes match the server they were generated from. A newer server
returns more properties (for example `Guid` on variables since 4.8); without this setting, reading them throws
*The property 'Guid' does not exist on type 'VariableRow'*.

**Timetables** are variables of type 6 (`VariableType.Timetable`) whose `Expression` holds the schedule in XML:

```xml
<Timetable><Type>Daily</Type><Items>
  <TimetableItem><TotalMinutes>0</TotalMinutes><Value>0</Value></TimetableItem>     <!-- 00:00 -> 0 -->
  <TimetableItem><TotalMinutes>465</TotalMinutes><Value>1</Value></TimetableItem>   <!-- 07:45 -> 1 -->
  <TimetableItem><TotalMinutes>690</TotalMinutes><Value>0</Value></TimetableItem>   <!-- 11:30 -> 0 -->
</Items></Timetable>
```

| `Type` | `TotalMinutes` counted from |
|---|---|
| `Daily` | 00:00 of each day (0 to 1439) |
| `Weekly` | Sunday 00:00 (Monday 07:45 = 1440 + 465 = 1905) |
| `WeeklyWithHolidays` | Sunday 00:00, then one extra day per holiday list after Saturday |

---

## Run the sample

**Prerequisites:** an AppVision 4.x server with its web service on HTTP (port 80) or HTTPS (port 443) (options
`App.Server.WebService.Http` / `Https`), a user with configuration rights (test bench: `ADM` / `123`), Visual Studio
with .NET Framework 4.8. Change `ServiceUrl`, `UserName` and `Password` at the top of `Program.cs` for another server.

1. Build the project, run `WebServiceOData.exe` (F5 in Visual Studio).
2. **1** lists the options: `name: App.Main.ProjectDescription value: ...`, one line per option.
3. **2** adds the option `TestOData = 1234`, **3** sets it to the current date and time, **1** shows the change.
4. **4** adds the timetable variable `TimetableTest1`: it appears in the configurator and its value
   follows the schedule above (1 from 07:45 to 11:30 and from 14:30 to 17:30).
5. **0** quits.

The executable is built in the AppVision `Bin` folder, next to the `Microsoft.Data.*.dll` and `System.Spatial.dll`
files it needs (referenced with *Copy Local = False*). To run it elsewhere, copy these four DLLs next to it.

---

## Without .NET

The same service from any HTTP client. Add `Accept: application/json` to get JSON (OData V3 "JSON light").

```bash
curl -u ADM:123 -H "Accept: application/json" "http://localhost/AppVisionDataService.svc/Options?\$top=5"
curl -u ADM:123 -H "Accept: application/json" "http://localhost/AppVisionDataService.svc/Variables?\$filter=TypeInt%20eq%206&\$select=Id,Name,Description"
curl -u ADM:123 -H "Accept: application/json" -H "Content-Type: application/json" -X POST \
     -d '{"Name":"TestOData","Value":"1234"}' "http://localhost/AppVisionDataService.svc/Options"
```

```python
import requests
r = requests.get("http://localhost/AppVisionDataService.svc/Options", auth=("ADM", "123"),
                 headers={"Accept": "application/json"}, params={"$top": "5"})
for opt in r.json()["value"]:
    print(opt["Name"], "=", opt["Value"])
```

The metadata of all entity sets and properties: `GET http://localhost/AppVisionDataService.svc/$metadata`
(authenticated).

| Entity set | Content |
|---|---|
| `Options` | Server and client options (`Name`, `Value`) |
| `Variables` | Variables, nodes, timetables (`Id`, `Name`, `Description`, `TypeInt`, `AreaId`, `Address`, `Expression`, alarm settings...) |
| `Areas`, `Groups` | Areas (physical location) and groups (trades) |
| `Users`, `Profiles` | Users and profiles (rights, filters) |
| `Protocols` | Protocols (drivers) |
| `Holidays`, `Instructions`, `Mailings`, `Links`, `TextReports`, `FileInfos`, `Scenarios` | Other configuration tables |

History is published the same way by `AppVisionHistoDataService.svc` (`Events`, `Alarms`, `Values`, `Modifs`...),
and access control data by `AppControlDataService.svc` (see [WebServiceODataAppControl](../WebServiceODataAppControl/README.md)).

---

## Update the generated classes

The classes of `Service References` were generated from an older server: properties added since then are missing
(ignored at run time thanks to `IgnoreMissingProperties`), and the entity sets `Actions` and `PlannedActions` no
longer exist on the server. To regenerate them for your server:

1. Download the metadata: `curl -u ADM:123 -o service.edmx "http://localhost/AppVisionDataService.svc/$metadata"`.
2. Generate the classes with `DataSvcUtil.exe` (WCF Data Services 5.6 tools):
   `DataSvcUtil /in:service.edmx /out:Reference.cs /language:CSharp /version:3.0 /dataServiceCollection`, and set
   the namespace of the generated classes to `WebServiceOData.ParamServiceReference`.

---

## Troubleshooting

| Symptom | Cause |
|---|---|
| `Unable to connect to the remote server` | Server not started, or web service not enabled on this port / scheme. Open `http://localhost/AppVisionDataService.svc/` in a browser. |
| `DataServiceClientException: 401 Unauthorized` | Wrong user or password, or account locked. |
| `... does not exist on type ...` | `IgnoreMissingProperties` removed: see above. |
| Error on **2** (add option) | The option `TestOData` already exists: use **3**. |
| Error on **4** (add timetable) | The variable `TimetableTest1` already exists, or the user has no configuration right. |
| `Could not load file or assembly 'Microsoft.Data.Services.Client'` | The executable is not next to the OData DLLs (see *Run the sample*). |

The message returned by the server is in the inner exception, printed under the first line. Fiddler or the
`Network` tab of a browser show the HTTP requests.

---

## See also

- [WebServiceODataAppControl](../WebServiceODataAppControl/README.md) — the same for AppControl people and badges
- [AppControlSynchro](../AppControlSynchro/README.md) — CSV import of people over OData
- [DatabaseSchema](../DatabaseSchema/README.md) — the tables behind the entity sets
- [WebServiceREST](../WebServiceREST/README.md) — live values, alarms and notifications (REST)

# WebServiceODataAppControl — People and badges of AppControl over OData

**Used for:** reading and creating access-control data (people, badges, categories, departments, access
profiles...) from another application: HR system, visitor management, badge printing. AppControl is the
access-control module of AppVision.

**Available in:** AppVision 4.x with AppControl (OData V3 service `AppControlDataService.svc`). Automatic person ids
(`Id = 0` on creation) need AppVision 4.9 or later.

**Audience:** .NET developers; the HTTP requests are the same from any language (see
[WebServiceOData](../WebServiceOData/README.md#without-net)).

---

## How it works

`http(s)://<server>/AppControlDataService.svc/` publishes the AppControl database: `People`, `Badges`,
`BadgeTypes`, `Categories`, `Departments`, `AccessProfiles`, `AccessProfileRights`, `PersonProfiles`,
`PersonRights`, `Zones`, `Visitors`, `Locations`... Authentication and conventions are those of
[WebServiceOData](../WebServiceOData/README.md): HTTP Basic, generated client classes, `IgnoreMissingProperties`.

| File | Role |
|---|---|
| `Program.cs` | Menu, `CreateContext()` (URL, credentials) and the five operations |
| `Service References/AppControlDataServiceReference/` | Generated client classes (`AppControlContext`, `Person`, `Badge`...) |

**References:** NuGet `Microsoft.Data.Services.Client` 5.8.4. **Output:** `WebServiceODataAppControl.exe`, in
`bin\Debug\` (or `bin\Release\`) of the project, with the OData DLLs.

The generated `AppControlContext` is the OData client of the service. It is not the server-side
`Prysm.AppControl.Data.AppControlContext` (Entity Framework) described in the API help, despite the same name.

**Documentation:** *Access control*, `http://<server>/doccsharp/html/04bcc881-e1bf-4e10-bb05-ea4adf6f91ea.htm`, and
*Web services and JavaScript SDK*, `http://<server>/doccsharp/html/11442ac1-c309-4c8f-8837-050c0cf5fe13.htm`, in the
API help served by your server (URLs relative to your server, see [Documentation](../README.md#documentation)).

```csharp
var ctx = CreateContext();                                   // URL + NetworkCredential + IgnoreMissingProperties

foreach (var p in ctx.People.Where(p => p.CategoryId == 1))  // GET People?$filter=CategoryId eq 1
	Console.WriteLine($"{p.FirstName} {p.LastName}");

foreach (var p in ctx.People.Expand("Badges"))               // GET People?$expand=Badges
	foreach (var b in p.Badges)
		Console.WriteLine(b.Code);
```

Without `Expand("Badges")`, `p.Badges` stays empty: navigation properties are not loaded by default.

**Creating a person and its badge.** Leave `person.Id` to 0: the server gives the next free id (4.9 and later) and
`SaveChanges` copies it into the object. Then create the badge with this id:

```csharp
ctx.AddToPeople(person);           // person.Id = 0
ctx.SaveChanges();                 // POST People -> person.Id is now the id given by the server

badge.PersonId = person.Id;        // key of a badge: Code + Format + BadgeTypeId
ctx.AddToBadges(badge);
ctx.SaveChanges();                 // POST Badges
```

| Field | Rule |
|---|---|
| `Person.CategoryId` | Category of the person; must exist (`Categories`). The sample uses 1. |
| `Person.ValidityStart` / `ValidityEnd` | Validity of the person (the sample: now, for 8 hours). |
| `Person.CompanyId` | Free identifier (employee number). Unique when the option `AppControl.People.CompanyIdUnique` is `true`. |
| `Badge.Code`, `Badge.Format`, `Badge.BadgeTypeId` | Key of the badge; `BadgeTypeId` must exist (`BadgeTypes`, 1 by default). |

The user needs the AppControl rights: display people, add/delete people, update badges.

---

## Run the sample

**Prerequisites:** an AppVision 4.x server with AppControl, its web service on HTTP (port 80) or HTTPS (port 443), a
category and a badge type with id 1 (create them in AppControl if the database is empty), a user with the AppControl
rights (test bench: `ADM` / `123`). Change `ServiceUrl`, `UserName` and `Password` at the top of `Program.cs` for
another server.

1. Build the project, run `WebServiceODataAppControl.exe`.
2. **4** asks a first name and a last name and creates the person: `person 12 added`.
3. **5** does the same with a badge code: `person 13 and badge 123456 added`.
4. **1** lists the people (`13: John Smith`), **2** those of category 1, **3** the people with their badges:
   ```
   13: John Smith
     badge: 123456
   ```
5. **0** quits. The people appear in AppControl (web client or AppControl client).

---

## Troubleshooting

| Symptom | Cause |
|---|---|
| `DataServiceClientException: 401 Unauthorized` on read | Wrong credentials. |
| `401 Unauthorized` on creation | The user lacks the AppControl right to add people or update badges. |
| Error on creation mentioning a foreign key | Category 1 or badge type 1 does not exist. |
| `Company ID ... is already used` | `AppControl.People.CompanyIdUnique` is enabled and another person has this `CompanyId`. |
| Error on badge creation | A badge with the same code, format and type already exists. |
| A list is empty | No data yet, or the user's profile filters categories / departments. |

See also the troubleshooting of [WebServiceOData](../WebServiceOData/README.md#troubleshooting) (connection,
regeneration of the client classes).

---

## See also

- [AppControlSynchro](../AppControlSynchro/README.md) — import people from a CSV file with the same service
- [DriverAppControl](../DriverAppControl/README.md) — driver for access-control hardware
- [DatabaseSchema](../DatabaseSchema/README.md) — the `Apc_*` tables

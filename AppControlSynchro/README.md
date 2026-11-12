# AppControlSynchro — Import people from a CSV file into AppControl

**Used for:** keeping the AppControl cardholders in line with an external list (HR export, contractor list): new
people are added, changed people are updated, unchanged people are left as they are. Typically run by a scheduled
task.

**Available in:** AppVision 4.9 and later with AppControl (service `AppControlDataService.svc`; automatic person ids).

**Audience:** .NET developers or integrators who adapt the CSV columns to their source.

---

## How it works

For each line of the CSV file:

1. the department is looked up by its description (`Departments`);
2. the person is looked up by `CompanyId` (the `PersonID` column);
3. unknown person: **added** (category 1, active); known person with different values: **updated**
   (`UpdateObject`); identical: skipped.

The additions and updates are queued in the OData context and sent by one `SaveChanges()` at the end (one HTTP
request per person). Each step is printed and appended to `SynchroAppControl.log` in the current folder.

| File | Role |
|---|---|
| `Program.cs` | CSV reading, comparison, upsert |
| `Service References/AppControlServiceReference/` | Generated client classes (`AppControlContext`, `Person`...) |

**References:** NuGet `Microsoft.Data.Services.Client` 5.8.4. **Output:** `SynchroAppControl.exe`, in `bin\Debug\`
(or `bin\Release\`) of the project, with the OData DLLs: it needs no AppVision DLL and runs from any folder.
Authentication and client classes: see [WebServiceOData](../WebServiceOData/README.md).

**Documentation:** *Access control*, `http://<server>/doccsharp/html/04bcc881-e1bf-4e10-bb05-ea4adf6f91ea.htm`, and
*Web services and JavaScript SDK*, `http://<server>/doccsharp/html/11442ac1-c309-4c8f-8837-050c0cf5fe13.htm`, in the
API help served by your server (URLs relative to your server, see [Documentation](../README.md#documentation)).

---

## CSV format

Semicolon-separated, **no header line**, 7 columns:

```
FirstName;LastName;PersonID;Department;ValidDateFrom;ValidDateTo;DateOfBirth
John;Smith;EMP001;Maintenance;2026-01-01;2026-12-31;1990-06-15
Alice;Martin;EMP002;;2026-01-01;;1985-02-20
```

| Column | Stored in | Rule |
|---|---|---|
| `FirstName`, `LastName` | `FirstName`, `LastName` | |
| `PersonID` | `CompanyId` | Key used to find the person again: must be unique in the file. |
| `Department` | `DepartmentId` | Matched on the department description; empty or unknown: no department. |
| `ValidDateFrom` | `ValidityStart` | Date (ISO `yyyy-MM-dd` recommended); empty: today. |
| `ValidDateTo` | `ValidityEnd` | Date; empty: `ValidDateFrom` + 1 year. |
| `DateOfBirth` | `Info1` | Free text. |

A line without exactly 7 columns, or with an invalid date, is reported and skipped.

---

## Run the sample

**Prerequisites:** AppVision 4.9+ with AppControl, web service on HTTP (port 80) or HTTPS (port 443), a category with
id 1 (`DEFAULT_CATEGORY_ID`), the departments of the file created in AppControl (optional), a user with the rights to
add and update people (test bench: `ADM` / `123`; change `ServiceUrl`, `UserName`, `Password` in `Program.cs`).

```
SynchroAppControl.exe people.csv
```

Expected output, first run then second run with John's end date changed:

```
Start SynchroAppControl
 add EMP001 John Smith
 add EMP002 Alice Martin
2 people added or updated.
End SynchroAppControl
...
 update EMP001 John Smith
 no change EMP002 Alice Martin
1 people added or updated.
```

The people appear in AppControl, with their department and validity dates.

---

## Adapting it

- **Other columns:** map them to the other `Person` fields (`Email`, `Phone`, `Company`, `Info2`... see the
  `Apc_People` table in [DatabaseSchema](../DatabaseSchema/README.md)) in `ImportLine`, and add
  them to the comparison.
- **People removed from the file:** the sample never deletes. To deactivate them, list the people of the category
  and set `IsActive = false` (or `ValidityEnd`) for those absent from the file.
- **Badges:** create them after `SaveChanges()`, once the people have their id (see
  [WebServiceODataAppControl](../WebServiceODataAppControl/README.md)).
- **Errors:** `SaveChanges()` stops at the first rejected person; the ones sent before are kept. Call `SaveChanges()`
  after each line to isolate errors, at the cost of speed.

---

## Troubleshooting

| Symptom | Cause |
|---|---|
| `Exception ... 401 Unauthorized` | Wrong credentials, or no right to add / update people. |
| `Company ID ... is already used` | Option `AppControl.People.CompanyIdUnique` enabled and the id is used by another person (or twice in the file). |
| `Incorrect line` | Not 7 columns separated by `;` (check the separator of your export). |
| `Validity start invalid` | Date not readable in the culture of the machine: use `yyyy-MM-dd`. |
| Everything is updated at each run | A value differs in format between the file and AppControl (spaces, time part of a date). |

---

## See also

- [WebServiceODataAppControl](../WebServiceODataAppControl/README.md) — read and create people and badges
- [WebServiceOData](../WebServiceOData/README.md) — OData basics, HTTP requests without .NET

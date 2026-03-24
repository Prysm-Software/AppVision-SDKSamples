# AppControlSynchro â€” AppControl People Import Tool

## Overview

A console utility that imports **cardholders from a CSV file** into the
AppControl access control module via its OData web service.

For each CSV row the tool:

1. Looks up the department by name.
2. Checks whether a person with the same `CompanyId` already exists.
3. **Inserts** new people; **updates** changed records; **skips** unchanged records.
4. Batches all changes into a single `SaveChanges()` call.

**References:** `System.Data.Services.Client`, `AppControlServiceReference` (generated proxy)  
**Output:** `SynchroAppControl.exe` â€” standalone console application

---

## Project Structure

| File | Role |
|---|---|
| `Program.cs` | Main loop, CSV parsing, upsert logic |
| `AppControlServiceReference/` | Auto-generated OData proxy classes |

---

## CSV Format

Semicolon-separated, one person per line, **no header row**:

```
FirstName;LastName;PersonID;Department;ValidDateFrom;ValidDateTo;DateOfBirth
```

| Column | Required | Description |
|---|---|---|
| `FirstName` | âœ“ | Person first name |
| `LastName` | âœ“ | Person last name |
| `PersonID` | âœ“ | Unique company identifier (badge correlation key) |
| `Department` | | Department name (matched by description in AppControl) |
| `ValidDateFrom` | | ISO date, e.g. `2024-01-01`. Defaults to today |
| `ValidDateTo` | | ISO date. Defaults to `ValidDateFrom` + 1 year |
| `DateOfBirth` | | Stored in `Info1` field |

**Example:**

```
Alice;Dupont;EMP001;Security;2024-01-01;2025-01-01;1990-06-15
```

---

## Key Concepts

### 1. `AppControlContext` (OData proxy)

```csharp
var ctx = new AppControlContext(
    new Uri("http://localhost/AppControlDataService.svc/"));
ctx.Credentials = new NetworkCredential("ADM", "123");

// LINQ â†’ OData $filter
Person existing = ctx.People
    .Where(p => p.CompanyId == id)
    .FirstOrDefault();
```

### 2. Upsert pattern

```csharp
if (existing == null)
    ctx.AddToPeople(newPerson);       // INSERT
else if (HasChanged(existing, row))
    UpdateFields(existing, row);      // UPDATE (mark via ctx.UpdateObject)
// else: skip â€” no changes
```

### 3. Batched `SaveChanges()`

All insert/update operations are queued and committed in **one HTTP batch
request** at the end â€” significantly faster than per-row commits:

```csharp
ctx.SaveChanges();
```

### 4. Error handling

Per-row errors are logged and the row is skipped; the import continues.

---

## AppControl OData Reference

`AppControlDataService.svc` â€” `http://localhost/AppControlDataService.svc/`

| Entity set | Key fields |
|---|---|
| `People` | `Id`, `FirstName`, `LastName`, `CompanyId`, `Email`, `DepartmentId`, `CategoryId`, `IsActive`, `ValidityStart`, `ValidityEnd`, `Info1`â€“`Info5` |
| `Badges` | `Id`, `Code`, `PersonId`, `BadgeTypeId`, `IsActive` |
| `Departments` | `Id`, `Description` |
| `Categories` | `Id`, `Description` |

---

## How to Build and Run

1. Build in **Release** configuration.  
2. Run:

```
SynchroAppControl.exe path\to\people.csv
```

Redirect output to a log file:

```
SynchroAppControl.exe people.csv > import.log
```

---

## See Also

- [WebServiceODataAppControl](../WebServiceODataAppControl/README.md) â€” interactive AppControl OData CRUD
- [WebServiceOData](../WebServiceOData/README.md) â€” OData access to AppVision config data

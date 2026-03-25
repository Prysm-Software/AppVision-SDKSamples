# WebServiceODataAppControl — AppControl OData Web Service Sample

## Overview

An interactive console application that demonstrates **CRUD operations** against
the AppControl OData web service (`AppControlDataService.svc`).

Operations demonstrated:

1. List all people.
2. List people filtered by category.
3. List people **with their badges** (OData `$expand`).
4. Add a new person.
5. Add a person together with a new badge.

> Use this as a reference for any HR or visitor-management system that needs
> to create or query badge-holder records in AppControl.

**References:** `System.Data.Services.Client`, `AppControlDataServiceReference` (generated proxy)  
**Output:** `WebServiceODataAppControl.exe` — standalone console application

---

## Project Structure

| File | Role |
|---|---|
| `Program.cs` | Interactive menu and all CRUD operations |
| `AppControlDataServiceReference/` | Auto-generated OData proxy classes |

---

## Key Concepts

### 1. `AppControlContext` (OData proxy)

```csharp
var ctx = new AppControlContext(
    new Uri("http://localhost/AppControlDataService.svc/"));
ctx.Credentials = new NetworkCredential("ADM", "123");
```

### 2. Querying people (GET)

```csharp
// All people
foreach (var p in ctx.People)
    Console.WriteLine($"{p.FirstName} {p.LastName}");

// Filtered by category
foreach (var p in ctx.People.Where(p => p.CategoryId == categoryId))
    Console.WriteLine(p.FirstName);
```

### 3. OData `$expand` — load navigation properties

```csharp
foreach (var p in ctx.People.Expand("Badges"))
{
    Console.WriteLine(p.FirstName);
    foreach (var b in p.Badges)
        Console.WriteLine($"  badge: {b.Code}");
}
```

Without `.Expand("Badges")`, `p.Badges` is an empty collection.

### 4. Adding a person (POST)

```csharp
var person = new Person
{
    FirstName      = "Alice",
    LastName       = "Dupont",
    IsActive       = true,
    ValidityStart  = DateTime.Now,
    ValidityEnd    = DateTime.Now.AddHours(8),
    CategoryId     = 1   // must exist
};
ctx.AddToPeople(person);
ctx.SaveChanges();
```

### 5. Adding a person with a badge (two linked POSTs)

AppControl requires `badge.PersonId` and `person.Id` to be set before
`SaveChanges()`. Since the server generates `Id`, read the highest existing
Id and increment:

```csharp
person.Id = ctx.People
    .OrderByDescending(p => p.Id)
    .ToArray()
    .Select(p => p.Id)
    .FirstOrDefault() + 1;

badge.PersonId = person.Id;
ctx.AddToPeople(person);
ctx.AddToBadges(badge);
ctx.SaveChanges();
```

> **Note:** In production prefer a server-side stored procedure or the
> AppControl REST API to avoid race conditions on Id generation.

### 6. Required foreign keys

| Field | Default exists | Description |
|---|---|---|
| `CategoryId` | ✓ (Id 1) | Person category |
| `BadgeTypeId` | ✓ (Id 1) | Badge type |
| `DepartmentId` | optional | May be `null` |

---

## AppControl OData Reference

`AppControlDataService.svc` — `http://localhost/AppControlDataService.svc/`

| Entity set | Key fields |
|---|---|
| `People` | `Id`, `FirstName`, `LastName`, `CompanyId`, `Email`, `CategoryId`, `DepartmentId`, `IsActive`, `ValidityStart`, `ValidityEnd`, `Info1`–`Info5` |
| `Badges` | `Id`, `Code`, `PersonId`, `BadgeTypeId`, `IsActive`, `ValidityStart`, `ValidityEnd` |
| `Departments` | `Id`, `Description` |
| `Categories` | `Id`, `Description` |
| `BadgeTypes` | `Id`, `Description` |

**Navigation properties:**

| Property | Description |
|---|---|
| `Person.Badges` | Collection of badges belonging to this person |
| `Badge.Person` | Parent person |

---

## How to Build and Run

1. Ensure AppVision with AppControl is running on `localhost`.  
2. Build in **Release** configuration.  
3. Run `WebServiceODataAppControl.exe` and select an operation.

---

## Debugging

Use **Fiddler** to inspect OData HTTP requests.  
The service returns XML error details on constraint violations.

---

## See Also

- [WebServiceOData](../WebServiceOData/README.md) — OData for AppVision config (options, variables)
- [AppControlSynchro](../AppControlSynchro/README.md) — CSV bulk import using the same context
- [DriverAppControl](../DriverAppControl/README.md) — driver that talks to AppControl hardware

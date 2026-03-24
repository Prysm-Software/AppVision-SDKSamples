# WebServiceOData â€” AppVision OData Web Service Sample

## Overview

An interactive console application that demonstrates **CRUD operations** against
the AppVision configuration OData web service (`AppVisionDataService.svc`).

Operations demonstrated:

1. List all configuration options.
2. Add a new option.
3. Update an existing option value.
4. Add a timetable variable with an XML schedule expression.

> Use this as a reference for any integration that needs to read or modify
> the AppVision configuration database programmatically â€” without the SDK.

**References:** `System.Data.Services.Client`, `ParamServiceReference` (generated proxy)  
**Output:** `WebServiceOData.exe` â€” standalone console application

---

## Project Structure

| File | Role |
|---|---|
| `Program.cs` | Interactive menu and all CRUD operations |
| `ParamServiceReference/` | Auto-generated OData proxy classes |

---

## Key Concepts

### 1. `ParamContext` (OData proxy)

```csharp
var ctx = new ParamContext(
    new Uri("http://localhost/AppVisionDataService.svc/"));
ctx.Credentials = new NetworkCredential("ADM", "123");
```

### 2. Reading options (GET)

```csharp
foreach (var opt in ctx.Options)
    Console.WriteLine($"{opt.Name} = {opt.Value}");
```

### 3. Creating an option (POST)

```csharp
var opt = new OptionRow { Name = "MyKey", Value = "MyValue" };
ctx.AddToOptions(opt);
ctx.SaveChanges();
```

### 4. Updating an option (PATCH / MERGE)

```csharp
var opt = ctx.Options.Where(o => o.Name == "MyKey").FirstOrDefault();
opt.Value = "NewValue";
ctx.UpdateObject(opt);   // required â€” marks entity as modified
ctx.SaveChanges();
```

### 5. Timetable variable

Timetables are stored as `VariableRow` with `TypeInt = 6`.  
The schedule is XML assigned to `Expression`.

```csharp
var tt = new VariableRow
{
    Name        = "MyTimetable",
    Description = "Office hours",
    TypeInt     = 6,
    Expression  =
        "<?xml version=\"1.0\"?>" +
        "<Timetable><Type>Daily</Type><Items>" +
        // TotalMinutes = minutes from Sunday 00:00; Value = state (0/1)
        "<TimetableItem><TotalMinutes>465</TotalMinutes><Value>1</Value></TimetableItem>" +
        "<TimetableItem><TotalMinutes>690</TotalMinutes><Value>0</Value></TimetableItem>" +
        "</Items></Timetable>"
};
ctx.AddToVariables(tt);
ctx.SaveChanges();
```

| `TotalMinutes` | Day / time |
|---|---|
| 0 | Sunday 00:00 |
| 1440 | Monday 00:00 |
| 465 | Monday 07:45 |

---

## AppVision OData Reference

`AppVisionDataService.svc` â€” `http://localhost/AppVisionDataService.svc/`

| Entity set | Key fields |
|---|---|
| `Options` | `Name`, `Value` |
| `Variables` | `Name`, `Description`, `TypeInt`, `Expression`, `Address` |
| `Protocols` | `Name`, `Manager`, `Description` |
| `Users` | `Name`, `FullName`, `Email` |
| `Areas` | `Name`, `Description`, `ParentId` |

---

## How to Build and Run

1. Ensure AppVision is running on `localhost`.  
2. Build in **Release** configuration.  
3. Run `WebServiceOData.exe` and select an operation from the menu.

---

## Debugging

Use **Fiddler** or a network trace to inspect OData HTTP requests.  
The service returns human-readable XML error messages on failure.

---

## See Also

- [WebServiceODataAppControl](../WebServiceODataAppControl/README.md) â€” same pattern for AppControl data
- [AppControlSynchro](../AppControlSynchro/README.md) â€” CSV bulk import using the same OData approach

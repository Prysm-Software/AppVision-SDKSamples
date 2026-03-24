AppControlSynchro - AppControl People Import Sample
====================================================

OVERVIEW
--------
A console utility that imports people (cardholders) from a CSV file into the
AppControl access control module via its OData web service.

For each CSV row the tool:
  - Looks up the department by name.
  - Checks whether a person with the same CompanyId already exists.
  - Adds new people and updates changed people.
  - Skips people with no changes (no unnecessary writes).
  - Batches all changes into a single SaveChanges() call.

Use this as a reference for any CSV / spreadsheet synchronisation tool that
needs to push badge-holder data into AppControl.

This C# project references:
  System.Data.Services.Client  - OData WCF client (generated proxy)
  AppControlServiceReference   - Generated proxy for AppControlDataService.svc

The output 'SynchroAppControl.exe' is a standalone console application.


PROJECT STRUCTURE
-----------------
  Program.cs                          Main loop, CSV parsing, upsert logic.
  AppControlServiceReference/         Auto-generated OData proxy classes.


CSV FORMAT
----------
Semicolon-separated, one person per line, no header row:

  FirstName;LastName;PersonID;Department;ValidDateFrom;ValidDateTo;DateOfBirth

  Fields:
    FirstName      Required. Person first name.
    LastName       Required. Person last name.
    PersonID       Required. Unique company identifier (badge correlation key).
    Department     Optional. Department description (matched by name in AppControl).
    ValidDateFrom  Optional. ISO date (e.g. 2024-01-01). Defaults to today.
    ValidDateTo    Optional. ISO date. Defaults to ValidDateFrom + 1 year.
    DateOfBirth    Optional. Stored in Info1 field.

  Example line:
    Alice;Dupont;EMP001;Security;2024-01-01;2025-01-01;1990-06-15


KEY CONCEPTS
------------
1. OData context (AppControlContext)
   AppControlContext is auto-generated from AppControlDataService.svc.
   Instantiate with the service URL and credentials:
       var ctx = new AppControlContext(new Uri("http://localhost/AppControlDataService.svc/"));
       ctx.Credentials = new NetworkCredential("ADM", "123");
   LINQ queries are translated to OData $filter requests:
       ctx.People.Where(p => p.CompanyId == id).FirstOrDefault()
       ctx.Departments.Where(d => d.Description == name).FirstOrDefault()

2. Upsert pattern
   For each CSV row:
       Person existing = ctx.People.Where(p => p.CompanyId == ...).FirstOrDefault();
       if (existing == null)   ctx.AddToPeople(newPerson);    // INSERT
       else if (changed)       /* update fields on existing */ // UPDATE
       else                    /* skip, no changes */          // NO-OP

3. Batched SaveChanges
   All insert/update operations are queued in the context and committed in one
   HTTP batch request at the end:
       ctx.SaveChanges();
   This is significantly faster than calling SaveChanges after every row.

4. Department lookup
   Department is matched by its Description string. If no matching department is
   found, DepartmentId is left null (AppControl allows nullable department).

5. Error handling
   Per-row errors are logged to the console and the row is skipped.
   The import continues for remaining rows.


APPVISION / APPCONTROL OData REFERENCE
---------------------------------------
AppControlDataService.svc  (http://localhost/AppControlDataService.svc/)
  Entity sets:
    People         Person rows (cardholders).
    Badges         Badge rows linked to people.
    Departments    Department lookup table.
    Categories     Category lookup table.

  Key fields of Person:
    Id, FirstName, LastName, CompanyId, Email
    DepartmentId, CategoryId, IsActive
    ValidityStart, ValidityEnd, Info1..Info5
    DateCreation, DateModification

  Person.Badges navigation property:
    ctx.People.Expand("Badges")   Eager-load badges in a single request.


HOW TO BUILD AND RUN
--------------------
1. Build in Release configuration.
2. Run:
       SynchroAppControl.exe path\to\people.csv


DEBUGGING
---------
- Redirect console output to a log file:
       SynchroAppControl.exe people.csv > import.log
- Use Fiddler or a network trace to inspect the OData HTTP requests.
- Check AppControl administration UI to verify imported people.


SEE ALSO
--------
WebServiceODataAppControl   - Interactive console tool for AppControl OData CRUD.
WebServiceOData             - OData access to AppVision configuration data.

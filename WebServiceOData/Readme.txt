WebServiceOData - AppVision OData Web Service Sample
====================================================

OVERVIEW
--------
An interactive console application that demonstrates CRUD operations against
the AppVision configuration OData web service (AppVisionDataService.svc).

Operations demonstrated:
  1. List all configuration options.
  2. Add a new option.
  3. Update an existing option value.
  4. Add a timetable variable with an XML schedule expression.

Use this as a reference for any external tool or integration that needs to
read or modify the AppVision configuration database programmatically.

This C# project references:
  System.Data.Services.Client   - OData WCF client runtime
  ParamServiceReference         - Auto-generated proxy for AppVisionDataService.svc

The output 'WebServiceOData.exe' is a standalone console application.


PROJECT STRUCTURE
-----------------
  Program.cs               Interactive menu and all CRUD operations.
  ParamServiceReference/   Auto-generated OData proxy classes.


KEY CONCEPTS
------------
1. ParamContext (OData proxy)
   ParamContext is auto-generated from AppVisionDataService.svc.
   Instantiate with the service URL and credentials:
       var ctx = new ParamContext(new Uri("http://localhost/AppVisionDataService.svc/"));
       ctx.Credentials = new NetworkCredential("ADM", "123");

2. Reading options  (GET)
   ctx.Options returns an IQueryable<OptionRow>.
   Standard LINQ is translated to OData $filter / $orderby:
       foreach (var opt in ctx.Options)
           Console.WriteLine(opt.Name + " = " + opt.Value);

3. Creating an option  (POST)
       var opt = new OptionRow { Name = "MyKey", Value = "MyValue" };
       ctx.AddToOptions(opt);
       ctx.SaveChanges();

4. Updating an option  (PATCH / MERGE)
       var opt = ctx.Options.Where(o => o.Name == "MyKey").FirstOrDefault();
       opt.Value = "NewValue";
       ctx.UpdateObject(opt);
       ctx.SaveChanges();
   Note: always call UpdateObject() to mark the entity as modified before
   SaveChanges(), otherwise no HTTP request is sent.

5. Timetable variable
   Timetables are stored as VariableRow with TypeInt = 6.
   The schedule is an XML string assigned to the Expression field.
   TimetableItem.TotalMinutes is the number of minutes from Sunday 00:00.
   TimetableItem.Value is the variable state (0 = off, 1 = on):
       <TimetableItem><TotalMinutes>465</TotalMinutes><Value>1</Value></TimetableItem>
       // 465 min = Monday 07:45 -> state changes to 1 (active)

6. Authentication
   The service uses Windows/Basic authentication.
   Credentials are passed via context.Credentials.
   Default AppVision credentials: user "ADM", password "123".


APPVISION OData SERVICE REFERENCE
----------------------------------
AppVisionDataService.svc  (http://localhost/AppVisionDataService.svc/)
  Entity sets:
    Options      OptionRow:   Name, Value
    Variables    VariableRow: Name, Description, TypeInt, Expression, Address, ...
    Protocols    ProtocolRow: Name, Manager, Description
    Users        UserRow:     Name, FullName, Email, ...
    Areas        AreaRow:     Name, Description, ParentId, ...

  OptionRow key fields:
    Name     Option identifier (e.g. "App.Main.ProjectDescription").
    Value    String value.

  VariableRow key fields for a timetable:
    Name        Variable name.
    Description Human-readable label.
    TypeInt     6 for timetable type.
    Expression  XML schedule string.


HOW TO BUILD AND RUN
--------------------
1. Ensure AppVision is running on localhost (or update the service URL).
2. Build in Release configuration.
3. Run WebServiceOData.exe and select an operation from the menu.


DEBUGGING
---------
- Use Fiddler or a network trace to inspect OData HTTP requests and responses.
- The OData service returns human-readable XML error messages on failure.


SEE ALSO
--------
WebServiceODataAppControl   - Same pattern for AppControl people/badges data.
AppControlSynchro           - CSV bulk import of people into AppControl.

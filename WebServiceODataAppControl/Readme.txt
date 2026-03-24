WebServiceODataAppControl - AppControl OData Web Service Sample
===============================================================

OVERVIEW
--------
An interactive console application that demonstrates CRUD operations against
the AppControl access control OData web service (AppControlDataService.svc).

Operations demonstrated:
  1. List all people.
  2. List people filtered by category.
  3. List people with their badges (OData Expand).
  4. Add a new person.
  5. Add a person together with a new badge.

Use this as a reference for any external system (HR, visitor management, etc.)
that needs to create or query badge-holder records in AppControl.

This C# project references:
  System.Data.Services.Client          - OData WCF client runtime
  AppControlDataServiceReference       - Auto-generated proxy for AppControlDataService.svc

The output 'WebServiceODataAppControl.exe' is a standalone console application.


PROJECT STRUCTURE
-----------------
  Program.cs                          Interactive menu and all CRUD operations.
  AppControlDataServiceReference/     Auto-generated OData proxy classes.


KEY CONCEPTS
------------
1. AppControlContext (OData proxy)
   AppControlContext is auto-generated from AppControlDataService.svc.
       var ctx = new AppControlContext(new Uri("http://localhost/AppControlDataService.svc/"));
       ctx.Credentials = new NetworkCredential("ADM", "123");

2. Querying people  (GET)
       foreach (var p in ctx.People)   // all people
       foreach (var p in ctx.People.Where(p => p.CategoryId == id))  // by category

3. Expanding navigation properties  (OData $expand)
   To load related entities in one request, use .Expand():
       foreach (var p in ctx.People.Expand("Badges"))
       {
           Console.WriteLine(p.FirstName);
           foreach (var b in p.Badges)
               Console.WriteLine("  badge: " + b.Code);
       }
   Without Expand, p.Badges is an empty collection.

4. Adding a person  (POST)
       var person = new Person
       {
           FirstName = "Alice", LastName = "Dupont",
           IsActive = true,
           ValidityStart = DateTime.Now,
           ValidityEnd = DateTime.Now.AddHours(8),
           CategoryId = 1  // category must exist
       };
       ctx.AddToPeople(person);
       ctx.SaveChanges();

5. Adding a person with a badge  (two linked POSTs)
   AppControl requires the badge PersonId and the person Id to be set before
   SaveChanges. Since the server generates the Id, read the highest existing
   Id and increment by 1 to synthesise the new Id:
       person.Id = ctx.People.OrderByDescending(p => p.Id)
                             .ToArray().Select(p => p.Id).FirstOrDefault() + 1;
       badge.PersonId = person.Id;
       ctx.AddToPeople(person);
       ctx.AddToBadges(badge);
       ctx.SaveChanges();
   In a production implementation, prefer a server-side stored procedure or
   the AppControl REST API to avoid race conditions on Id generation.

6. Required foreign keys
   CategoryId  - must reference an existing category (Id 1 exists by default).
   BadgeTypeId - must reference an existing badge type (Id 1 exists by default).
   DepartmentId - optional; null is accepted.


APPCONTROL OData SERVICE REFERENCE
------------------------------------
AppControlDataService.svc  (http://localhost/AppControlDataService.svc/)
  Entity sets:
    People         Person rows (cardholders).
    Badges         Badge rows.
    Departments    Department lookup.
    Categories     Category lookup.
    BadgeTypes     Badge type lookup.

  Person key fields:
    Id, FirstName, LastName, CompanyId, Email
    CategoryId, DepartmentId, IsActive
    ValidityStart, ValidityEnd
    Info1 .. Info5  (custom text fields)

  Badge key fields:
    Id, Code, PersonId, BadgeTypeId, IsActive
    ValidityStart, ValidityEnd

  Navigation properties:
    Person.Badges         Collection of Badge.
    Badge.Person          Parent Person.


HOW TO BUILD AND RUN
--------------------
1. Ensure AppVision with AppControl is running on localhost.
2. Build in Release configuration.
3. Run WebServiceODataAppControl.exe and select an operation from the menu.


DEBUGGING
---------
- Use Fiddler to inspect OData HTTP requests.
- OData service returns XML error details on constraint violations.
- Verify AppControl categories and badge types exist before adding records.


SEE ALSO
--------
WebServiceOData       - OData access to AppVision configuration (options, variables).
AppControlSynchro     - CSV bulk import tool using the same AppControlContext.
DriverAppControl      - Driver that communicates with AppControl controllers.

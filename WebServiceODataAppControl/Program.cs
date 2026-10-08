using System;
using System.Linq;
using System.Net;
using WebServiceODataAppControl.AppControlDataServiceReference;

namespace WebServiceODataAppControl
{
	class Program
	{
		// AppVision server and account (test bench defaults). Use https and a dedicated account on a real site.
		const string ServiceUrl = "http://localhost/AppControlDataService.svc/";
		const string UserName = "ADM";
		const string Password = "123";

		static void Main()
		{
			for (bool exit = false; !exit;)
			{
				Console.WriteLine();
				Console.WriteLine("1: get people list");
				Console.WriteLine("2: get people list by category");
				Console.WriteLine("3: get people and badges list");
				Console.WriteLine("4: add person");
				Console.WriteLine("5: add person with badge");
				Console.WriteLine("0: quit");
				Console.WriteLine();
				var key = Console.ReadKey(true);
				switch (key.KeyChar)
				{
					case '1': GetPeople(); break;
					case '2': GetPeopleByCategory(1); break;
					case '3': GetPeopleAndBadges(); break;
					case '4': AddPerson(); break;
					case '5': AddPersonWithBadge(); break;
					case '0': exit = true; break;
				}
			}
		}

		/// <summary>
		/// OData context of the AppControl database (people, badges, access profiles...), with Basic authentication.
		/// </summary>
		static AppControlContext CreateContext()
		{
			return new AppControlContext(new Uri(ServiceUrl))
			{
				Credentials = new NetworkCredential(UserName, Password),
				// The proxy classes were generated from an older server: ignore the properties added since then
				// instead of failing with "The property 'X' does not exist on type ...".
				IgnoreMissingProperties = true,
			};
		}

		private static void GetPeople()
		{
			try
			{
				var ctx = CreateContext();
				foreach (var p in ctx.People)
					Console.WriteLine($"{p.Id}: {p.FirstName} {p.LastName}");
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private static void GetPeopleByCategory(int categoryId)
		{
			try
			{
				var ctx = CreateContext();
				// LINQ Where -> OData $filter=CategoryId eq 1, evaluated by the server
				foreach (var p in ctx.People.Where(p => p.CategoryId == categoryId))
					Console.WriteLine($"{p.Id}: {p.FirstName} {p.LastName}");
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private static void GetPeopleAndBadges()
		{
			try
			{
				var ctx = CreateContext();
				// $expand=Badges: the badges come in the same response
				foreach (var p in ctx.People.Expand("Badges"))
				{
					Console.WriteLine($"{p.Id}: {p.FirstName} {p.LastName}");
					foreach (var b in p.Badges)
						Console.WriteLine("  badge: " + b.Code);
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		static Person ReadPerson()
		{
			var person = new Person();
			Console.Write("firstname: ");
			person.FirstName = Console.ReadLine();
			Console.Write("lastname: ");
			person.LastName = Console.ReadLine();
			person.IsActive = true;
			person.ValidityStart = DateTime.Now;
			person.ValidityEnd = DateTime.Now.AddHours(8);
			person.CategoryId = 1; // category 1 must exist
			// person.Id is left to 0: the server gives the next free id and returns it
			return person;
		}

		private static void AddPerson()
		{
			try
			{
				var person = ReadPerson();
				var ctx = CreateContext();
				ctx.AddToPeople(person);
				ctx.SaveChanges();
				Console.WriteLine($"person {person.Id} added");
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private static void AddPersonWithBadge()
		{
			try
			{
				var person = ReadPerson();
				var badge = new Badge();
				Console.Write("badge code: ");
				badge.Code = Console.ReadLine();
				badge.Format = "";
				badge.BadgeTypeId = 1; // badge type 1 must exist

				var ctx = CreateContext();

				// 1. create the person: SaveChanges updates person.Id with the id given by the server
				ctx.AddToPeople(person);
				ctx.SaveChanges();

				// 2. create the badge of this person
				badge.PersonId = person.Id;
				ctx.AddToBadges(badge);
				ctx.SaveChanges();
				Console.WriteLine($"person {person.Id} and badge {badge.Code} added");
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		/// <summary>
		/// The useful message of an OData error (e.g. 401, duplicate key) is in the inner exception.
		/// </summary>
		static void ShowError(Exception ex)
		{
			Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
			for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
				Console.WriteLine($"  {inner.GetType().Name}: {inner.Message}");
		}
	}
}

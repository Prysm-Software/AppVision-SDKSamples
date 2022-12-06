using WebServiceODataAppControl.AppControlDataServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceODataAppControl
{
	class Program
	{
		static void Main(string[] args)
		{
			for (bool exit=false;!exit;)
			{
				Console.WriteLine();
				Console.WriteLine("1: get people list");
				Console.WriteLine("2: get people list by category");
				Console.WriteLine("3: get people and badges list");
				Console.WriteLine("4: add person");
				Console.WriteLine("5: add person with badge");
				Console.WriteLine("0: quit");
				Console.WriteLine();
				var key = Console.ReadKey();
				switch (key.KeyChar)
				{
					case '1': GetPeople(); break;
					case '2': GetPeopleByCategory(1); break;
					case '3': GetPeopleAndBadges(); break;
					case '4': AddPerson(); break;
					case '5': AddPersonWithBadge(); break;
					case '0': exit = true;break;
				}
			}
		}

		private static void GetPeople()
		{
			try
			{
				var context = new AppControlDataServiceReference.AppControlContext(new Uri("http://localhost/AppControlDataService.svc/"));
				context.Credentials = new NetworkCredential("ADM", "123");
				foreach (var p in context.People)
					Console.WriteLine("name: " + p.FirstName + " " + p.LastName);
			}
			catch (Exception ex)
			{ Console.WriteLine(ex.ToString()); }
		}
		private static void GetPeopleByCategory(int categoryId)
		{
			try
			{
				var context = new AppControlDataServiceReference.AppControlContext(new Uri("http://localhost/AppControlDataService.svc/"));
				context.Credentials = new NetworkCredential("ADM", "123");
				foreach (var p in context.People.Where(p => p.CategoryId == categoryId))
					Console.WriteLine("name: " + p.FirstName + " " + p.LastName);
			}
			catch (Exception ex)
			{ Console.WriteLine(ex.ToString()); }
		}
		private static void GetPeopleAndBadges()
		{
			try
			{
				var context = new AppControlDataServiceReference.AppControlContext(
				new Uri("http://localhost/AppControlDataService.svc/"));
				context.Credentials = new NetworkCredential("ADM", "123");
				foreach (var p in context.People.Expand("Badges"))
				{
					Console.WriteLine("name: " + p.FirstName + " " + p.LastName);
					foreach (var b in p.Badges)
						Console.WriteLine(" badge: " + b.Code);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		private static void AddPerson()
		{
			try
			{
				Person person = new Person();
				Console.Write("firstname: ");
				person.FirstName = Console.ReadLine();
				Console.Write("lastname: ");
				person.LastName = Console.ReadLine();
				person.IsActive = true;
				person.ValidityStart = DateTime.Now;
				person.ValidityEnd = DateTime.Now.AddHours(8);
				person.CategoryId = 1; // category 1 must exist
				var ctx = new AppControlDataServiceReference.AppControlContext(
				new Uri("http://localhost/AppControlDataService.svc/"));
				ctx.Credentials = new NetworkCredential("ADM", "123");
				ctx.AddToPeople(person);
				ctx.SaveChanges();
				Console.WriteLine("person added");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		private static void AddPersonWithBadge()
		{
			try
			{
				Person person = new Person();
				Console.Write("firstname: ");
				person.FirstName = Console.ReadLine();
				Console.Write("lastname: ");
				person.LastName = Console.ReadLine();
				person.IsActive = true;
				person.ValidityStart = DateTime.Now;
				person.ValidityEnd = DateTime.Now.AddHours(8);
				person.CategoryId = 1; // category 1 must exist

				Badge badge = new Badge();
				Console.Write("badge code: ");
				badge.Code = Console.ReadLine();
				badge.BadgeTypeId = 1; // badgeType 1 must exist
				person.Badges.Add(badge);

				var ctx = new AppControlDataServiceReference.AppControlContext(
				new Uri("http://localhost/AppControlDataService.svc/"));
				ctx.Credentials = new NetworkCredential("ADM", "123");
				person.Id = ctx.People.OrderByDescending(p => p.Id).ToArray().Select(p => p.Id).FirstOrDefault() + 1;
				badge.PersonId = person.Id;
				ctx.AddToPeople(person);
				ctx.AddToBadges(badge);
				ctx.SaveChanges();
				Console.WriteLine("person and badge added");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}

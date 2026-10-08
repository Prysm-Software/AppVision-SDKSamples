using System;
using System.Linq;
using System.Net;
using WebServiceOData.ParamServiceReference;

namespace WebServiceOData
{
	class Program
	{
		// AppVision server and account (test bench defaults). Use https and a dedicated account on a real site.
		const string ServiceUrl = "http://localhost/AppVisionDataService.svc/";
		const string UserName = "ADM";
		const string Password = "123";

		static void Main()
		{
			for (bool exit = false; !exit;)
			{
				Console.WriteLine();
				Console.WriteLine("1: get options list");
				Console.WriteLine("2: add an option");
				Console.WriteLine("3: update an option");
				Console.WriteLine("4: add a timetable");
				Console.WriteLine("0: quit");
				Console.WriteLine();
				var key = Console.ReadKey(true);
				switch (key.KeyChar)
				{
					case '1': GetOptions(); break;
					case '2': AddOption(); break;
					case '3': UpdateOption(); break;
					case '4': AddTimetable(); break;
					case '0': exit = true; break;
				}
			}
		}

		/// <summary>
		/// OData context of the AppVision configuration database (Param), with Basic authentication.
		/// </summary>
		static ParamContext CreateContext()
		{
			return new ParamContext(new Uri(ServiceUrl))
			{
				Credentials = new NetworkCredential(UserName, Password),
				// The proxy classes were generated from an older server: ignore the properties added since then
				// instead of failing with "The property 'X' does not exist on type ...".
				IgnoreMissingProperties = true,
			};
		}

		private static void GetOptions()
		{
			try
			{
				var ctx = CreateContext();
				foreach (var p in ctx.Options)
					Console.WriteLine("name: " + p.Name + " value: " + p.Value);
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private static void AddOption()
		{
			try
			{
				var opt = new OptionRow { Name = "TestOData", Value = "1234" };
				var ctx = CreateContext();
				ctx.AddToOptions(opt);
				ctx.SaveChanges();
				Console.WriteLine("add option name: " + opt.Name + " value: " + opt.Value);
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private static void UpdateOption()
		{
			try
			{
				var ctx = CreateContext();
				var opt = ctx.Options.Where(o => o.Name == "TestOData").FirstOrDefault();
				if (opt == null)
				{
					Console.WriteLine("Option TestOData unknown (add it first)");
					return;
				}
				opt.Value = DateTime.Now.ToString();
				ctx.UpdateObject(opt);
				ctx.SaveChanges();
				Console.WriteLine("update option name: " + opt.Name + " value: " + opt.Value);
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private static void AddTimetable()
		{
			try
			{
				var tt = new VariableRow { Name = "TimetableTest1", Description = "Timetable test 1", TypeInt = 6 }; // 6: VariableType.Timetable
				// Daily timetable: TotalMinutes = minutes since 00:00 of the day (a Weekly timetable counts from Sunday 00:00).
				// Value = state of the timetable variable from that time on: here 1 from 07:45 to 11:30 and from 14:30 to 17:30.
				tt.Expression = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
								"<Timetable xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
								"<Type>Daily</Type>" +
								"<Items>" +
								"<TimetableItem><TotalMinutes>0</TotalMinutes><Value>0</Value></TimetableItem>" +
								"<TimetableItem><TotalMinutes>465</TotalMinutes><Value>1</Value></TimetableItem>" +
								"<TimetableItem><TotalMinutes>690</TotalMinutes><Value>0</Value></TimetableItem>" +
								"<TimetableItem><TotalMinutes>870</TotalMinutes><Value>1</Value></TimetableItem>" +
								"<TimetableItem><TotalMinutes>1050</TotalMinutes><Value>0</Value></TimetableItem>" +
								"</Items>" +
								"</Timetable>";

				var ctx = CreateContext();
				ctx.AddToVariables(tt);
				ctx.SaveChanges();
				Console.WriteLine("add variable name: " + tt.Name + " id: " + tt.Id);
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

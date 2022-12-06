using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceOData
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("1: get options list");
			Console.WriteLine("2: add an option");
			Console.WriteLine("3: update an option");
			Console.WriteLine("4: add a timetable");
			Console.WriteLine();

			var key = Console.ReadKey();
			switch (key.KeyChar)
			{
				case '1': GetOptions();break;
				case '2': AddOption(); break;
				case '3': UpdateOption(); break;
				case '4': AddTimetable(); break;
			}

			Console.ReadLine();
		}

		private static void GetOptions()
		{
			try
			{
				var ctx = new ParamServiceReference.ParamContext(new Uri("http://localhost/AppVisionDataService.svc/"));
				ctx.Credentials = new NetworkCredential("ADM", "123");
				foreach (var p in ctx.Options)
					Console.WriteLine("name: " + p.Name + " value: " + p.Value);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		private static void AddOption()
		{
			try
			{
				ParamServiceReference.OptionRow opt = new ParamServiceReference.OptionRow { Name = "TestOData", Value = "1234" };
				var ctx = new ParamServiceReference.ParamContext(new Uri("http://localhost/AppVisionDataService.svc/"));
				ctx.Credentials = new NetworkCredential("ADM", "123");
				ctx.AddToOptions(opt);
				ctx.SaveChanges();
				Console.WriteLine("add option name: " + opt.Name + " value: " + opt.Value);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		private static void UpdateOption()
		{
			try
			{
				var ctx = new ParamServiceReference.ParamContext(new Uri("http://localhost/AppVisionDataService.svc/"));
				ctx.Credentials = new NetworkCredential("ADM", "123");
				var opt = ctx.Options.Where(o => o.Name == "TestOData").FirstOrDefault();
				if( opt==null)
				{
					Console.WriteLine("Option TestOData unknown");
					return;
				}
				opt.Value = DateTime.Now.ToString();
				ctx.UpdateObject(opt);
				ctx.SaveChanges();
				Console.WriteLine("update option name: " + opt.Name + " value: " + opt.Value);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		private static void AddTimetable()
		{
			try
			{
				ParamServiceReference.VariableRow tt = new ParamServiceReference.VariableRow { Name = "TimetableTest1", Description= "Timetable test 1", TypeInt=6 }; // 6: variable type for timetable
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
				// TimetableItem.TotalMinutes = minutes from Sunday 00h00
				// TimetableItem.Value = variable state

				var ctx = new ParamServiceReference.ParamContext(new Uri("http://localhost/AppVisionDataService.svc/"));
				ctx.Credentials = new NetworkCredential("ADM", "123");
				ctx.AddToVariables(tt);
				ctx.SaveChanges();
				Console.WriteLine("add variable name: " + tt.Name );
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

	}
}

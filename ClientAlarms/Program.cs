using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientAlarms
{
	/// <summary>
	/// Console client that follows the alarms of AppVision and acts on them like an operator:
	/// list, acknowledge, report, close, raise an alarm from an external system, and read the alarm and event history.
	/// </summary>
	/// <remarks>
	/// Command line: <c>ClientAlarms.exe [host] [user] [password]</c> (defaults: localhost, ADM, 123).
	/// Typical uses: bridge to a ticketing or maintenance system, PSIM integration, alarm statistics.
	/// </remarks>
	static class Program
	{
		/// <summary>Client connection to the AppVision server.</summary>
		static readonly AppServer _server = new AppServer();
		static readonly object _consoleLock = new object();

		static async Task<int> Main(string[] args)
		{
			var host = args.Length > 0 ? args[0] : "localhost";
			var user = args.Length > 1 ? args[1] : "ADM";
			var password = args.Length > 2 ? args[2] : "123";

			try
			{
				Print($"Connecting to {host} as {user}...");

				// Subscribe BEFORE starting the notifications, so that no change is missed.
				// AlarmChanged is raised on a background thread.
				_server.AlarmManager.AlarmChanged += OnAlarmChanged;
				_server.ControllerManager.ConnectionLost += (hostname, reconnecting) => Print($"Connection lost with {hostname}");

				_server.Open(host);                 // "localhost", "server:8080", "https://server"...
				_server.Login(user, password);

				// Fills the local cache (areas, groups, users, variables...) used by GetCurrentAlarms and the
				// descriptions. Required: the alarm lists are read from this cache.
				await _server.LoadDatabase(true);

				// Only the alarm notifications: no variable states, no events.
				_server.AddFilterNotifications("NotificationType=AlarmRow");
				// true: receive the current alarms now, which fills the alarm cache.
				// false: notifications by polling (keep-alive), no local web service to open (no admin rights needed).
				_server.StartNotifications(true, false);
				await Task.Delay(1000);             // let the initial alarms arrive

				Print("Connected. Type 'help' for the commands.");
				ListAlarms();

				await CommandLoop();
				return 0;
			}
			catch (Exception ex)
			{
				// Typical causes: server not reachable, wrong user or password, licence (no SDK client left).
				Print("Error: " + ex.Message);
				return 1;
			}
			finally
			{
				// Frees the session and the SDK client licence on the server at once.
				if (_server.IsConnected)
					_server.Close();
			}
		}

		static async Task CommandLoop()
		{
			string line;
			while ((line = Console.ReadLine()) != null)
			{
				var words = line.Trim().Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
				if (words.Length == 0)
					continue;
				try
				{
					switch (words[0].ToLowerInvariant())
					{
						case "list": ListAlarms(words.Length > 1 ? words[1] : null); break;
						case "ack": await Acknowledge(words.ElementAtOrDefault(1)); break;
						case "report": await Report(ParseId(words), words.ElementAtOrDefault(2) ?? "", alarmWaiting: false); break;
						case "pause": await Report(ParseId(words), words.ElementAtOrDefault(2) ?? "", alarmWaiting: true); break;
						case "close": await Close(ParseId(words)); break;
						case "raise": await Raise(words); break;
						case "history": await AlarmHistory(ParseHours(words)); break;
						case "events": await EventHistory(ParseHours(words)); break;
						case "quit": case "exit": return;
						default: Help(); break;
					}
				}
				catch (Exception ex)
				{
					Print("Error: " + ex.Message);
				}
			}
		}

		static void Help() => Print(@"Commands:
  list [severity]           current alarms (optionally with a minimum severity)
  ack <id> | ack all        acknowledge an alarm, or all of them
  report <id> <text>        write the report of an alarm (ends its treatment)
  pause <id> <text>         write a report and keep the alarm waiting (treatment paused)
  close <id>                close a completed alarm whose condition is over
  raise <severity> <text>   raise an alarm from this program (e.g. an external system fault)
  history [hours]           alarms of the last hours (default 24)
  events [hours]            events of the last hours (default 24)
  quit");

		#region Live alarms

		/// <summary>
		/// Called for each change of an alarm: Start (new), End (condition over), Acknowledge, Report,
		/// Delete (left the list), Update...
		/// </summary>
		static void OnAlarmChanged(AlarmInfo info)
		{
			var a = info.Alarm;
			Print($"  >> {info.Event,-11} #{a.Id} [{a.Severity}] {a.Description} - {a.StateDesc}");
		}

		static void ListAlarms(string minSeverity = null)
		{
			// Filters: "Severity >= 50", "$A.Area1|$A.Area2", "$G.Fire", "$V.Building1.*", "Status=10"...
			var filters = minSeverity == null ? new string[0] : new[] { "Severity >= " + int.Parse(minSeverity) };
			var alarms = _server.AlarmManager.GetCurrentAlarms(filters);

			Print($"{alarms.Length} current alarm(s):");
			foreach (var a in alarms.OrderByDescending(a => a.Severity).ThenBy(a => a.DateStart))
				Print($"  #{a.Id,-5} [{a.Severity,3}] {a.DateStart.LocalDateTime:yyyy-MM-dd HH:mm}  {a.Description} - {a.StateDesc}  ({Waiting(a)})");
		}

		/// <summary>
		/// An alarm has two independent axes: its condition (WaitEnd: still in alarm) and its treatment by an
		/// operator (acknowledge, report, pause). It leaves the list only when both are over.
		/// </summary>
		static string Waiting(AlarmRow a)
		{
			var waiting = new List<string>();
			if (a.WaitAcknowledge) waiting.Add("to acknowledge");
			if (a.WaitProcess == true) waiting.Add("paused");
			else if (a.WaitReport) waiting.Add("report expected");
			if (a.WaitEnd) waiting.Add("condition active");
			if (CanClose(a)) waiting.Add("ready to close");
			return waiting.Count == 0 ? a.Status.ToString() : string.Join(", ", waiting);
		}

		/// <summary>
		/// Treatment completed (acknowledged, nothing pending: Status = Completed) and condition over (!WaitEnd).
		/// Note: IsReadyToClose only means "workflow or report completed", not that the condition is over.
		/// </summary>
		static bool CanClose(AlarmRow a) => a.Status == AlarmStatus.Completed && !a.WaitEnd;

		#endregion

		#region Operator actions

		static async Task Acknowledge(string id)
		{
			if (id == "all")
			{
				_server.AlarmManager.AcknowledgeAllAlarms();
				return;
			}
			// Acknowledging takes the alarm in charge; it does not end it while its condition is active.
			var ok = await _server.AlarmManager.AcknowledgeAlarm(long.Parse(id));
			Print(ok ? $"Alarm #{id} acknowledged" : $"Alarm #{id} not acknowledged (unknown, or already acknowledged)");
		}

		/// <param name="alarmWaiting">true: the treatment is paused (status "waiting"), it resumes later.</param>
		static async Task Report(long id, string text, bool alarmWaiting)
		{
			// A report written before the acknowledgement is kept: with nothing else pending,
			// the alarm can then be completed (and closed) as soon as it is acknowledged.
			var ok = await _server.AlarmManager.ReportAlarm(id, text, alarmWaiting);
			Print(ok ? $"Report written on alarm #{id}" : $"Report refused on alarm #{id}");
		}

		static async Task Close(long id)
		{
			// Possible only when the treatment is completed and the condition is over: the server ignores the
			// request otherwise. Without the option "alarms closed manually", it closes such alarms by itself.
			var alarm = await _server.AlarmManager.GetCurrentAlarmById(id) ?? throw new Exception($"No current alarm #{id}");
			if (!CanClose(alarm))
				throw new Exception($"Alarm #{id} cannot be closed yet ({Waiting(alarm)})");
			await _server.AlarmManager.CloseCompletedAlarm(id);
			Print($"Alarm #{id} closed");
		}

		/// <summary>
		/// Raises an alarm that does not come from a variable, e.g. a fault reported by an external system.
		/// It has no condition to clear: it ends when it is acknowledged (and reported, if required).
		/// </summary>
		static async Task Raise(string[] words)
		{
			if (words.Length < 3)
				throw new Exception("Usage: raise <severity 1-100> <text>");
			var id = await _server.AlarmManager.AddAlarmExt(int.Parse(words[1]), "External", words[2],
				info: "Raised by the ClientAlarms sample");
			Print($"Alarm #{id} raised");
		}

		#endregion

		#region History

		/// <summary>
		/// History is read by pages, oldest first from dateStart: ask for pages until one is incomplete.
		/// Keep the time window and the page count reasonable, the history database can be large.
		/// </summary>
		static async Task AlarmHistory(int hours)
		{
			var since = DateTime.Now.AddHours(-hours);
			const int pageSize = 100;
			var count = 0;
			for (int page = 0; page < 10; page++)
			{
				var rows = await _server.AlarmManager.GetHistoAlarmsByPage(page, pageSize, false, since,
					filters: null, types: null, users: null, filterInstruction: null, filterReport: null);
				foreach (var a in rows)
					Print($"  {a.DateStart.LocalDateTime:yyyy-MM-dd HH:mm}  #{a.Id,-5} [{a.Severity,3}] {a.Description} - {a.StateDesc}"
						+ (a.UserNameAck != null ? $"  ack by {a.UserNameAck} {a.DateAck.LocalDateTime:HH:mm}" : "")
						+ (a.DateClose != null ? $"  closed {a.DateClose.Value.LocalDateTime:HH:mm}" : ""));
				count += rows.Length;
				if (rows.Length < pageSize)
					break;
			}
			Print($"{count} alarm(s) since {since:yyyy-MM-dd HH:mm}");
		}

		static async Task EventHistory(int hours)
		{
			var since = DateTime.Now.AddHours(-hours);
			const int pageSize = 100;
			var count = 0;
			for (int page = 0; page < 10; page++)
			{
				// Filters as for the alarms ($A., $G., $V.); types/subtypes narrow the kind of events,
				// e.g. new[] { EventType.User } for the operator actions only.
				var rows = await _server.EventManager.GetHistoEventsByPage(page, pageSize, false, since,
					filters: null, types: null, subtypes: null);
				foreach (var e in rows)
					Print($"  {e.Date.LocalDateTime:yyyy-MM-dd HH:mm}  {e.Type,-9} {e.OriginDesc,-20} {e.Description} {e.StateDesc}");
				count += rows.Length;
				if (rows.Length < pageSize)
					break;
			}
			Print($"{count} event(s) since {since:yyyy-MM-dd HH:mm}");
		}

		#endregion

		static long ParseId(string[] words) =>
			words.Length > 1 && long.TryParse(words[1], out var id) ? id : throw new Exception("Alarm id expected");

		static int ParseHours(string[] words) =>
			words.Length > 1 && int.TryParse(words[1], out var h) ? h : 24;

		static void Print(string text)
		{
			lock (_consoleLock)
				Console.WriteLine(text);
		}
	}
}

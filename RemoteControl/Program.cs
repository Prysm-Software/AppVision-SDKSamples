using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteControl
{
	/// <summary>
	/// Console application that drives the screens of the AppVision operator clients: it lists the connected
	/// clients, loads a synoptic, a camera or a list on a workstation, reads back what is displayed and, in
	/// "follow" mode, brings up each new alarm on a chosen workstation (PSIM-like behaviour).
	/// </summary>
	/// <remarks>Command line: <c>RemoteControl.exe [host] [user] [password]</c> (defaults: localhost, ADM, 123).</remarks>
	static class Program
	{
		static readonly AppServer _server = new AppServer();
		static readonly object _consoleLock = new object();
		/// <summary>Target of the "follow" mode, null when off.</summary>
		static string _followTarget;

		static async Task<int> Main(string[] args)
		{
			var host = args.Length > 0 ? args[0] : "localhost";
			var user = args.Length > 1 ? args[1] : "ADM";
			var password = args.Length > 2 ? args[2] : "123";
			try
			{
				Print($"Connecting to {host} as {user}...");
				_server.AlarmManager.AlarmChanged += OnAlarmChanged;
				_server.Open(host);
				_server.Login(user, password);
				// Local cache of the configuration: used to find the synoptic of the source of an alarm.
				await _server.LoadDatabase(true);

				// Alarm notifications only, used by the "follow" mode. Commands to clients need no subscription.
				_server.AddFilterNotifications("NotificationType=AlarmRow");
				_server.StartNotifications(false, false);

				Print("Connected. Type 'help' for the commands.");
				await ListClients();
				await CommandLoop();
				return 0;
			}
			catch (Exception ex)
			{
				Print("Error: " + ex.Message);
				return 1;
			}
			finally
			{
				if (_server.IsConnected)
					_server.Close();
			}
		}

		static async Task CommandLoop()
		{
			string line;
			while ((line = Console.ReadLine()) != null)
			{
				// <command> <target> <document> [arg1] [arg2]...   arguments separated by spaces
				var w = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (w.Length == 0)
					continue;
				try
				{
					var target = w.ElementAtOrDefault(1);
					switch (w[0].ToLowerInvariant())
					{
						case "clients": await ListClients(); break;
						case "load": Load(target, w.ElementAtOrDefault(2) ?? "", w.Skip(3).ToArray()); break;
						case "open": Open(target, w.ElementAtOrDefault(2) ?? "", w.Skip(3).ToArray()); break;
						case "clear": _server.ClientManager.Clear(Required(target), 0, ""); break;
						case "show": await Show(Required(target)); break;
						case "follow": Follow(target); break;
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

		static void Help() => Print(@"Commands (<target> = host name, @User.<name>, @Profile.<name>, @All, or a ';' list):
  clients                          connected clients
  load <target> <document> [args]  load a document in the main window of the target
  open <target> <document> [args]  open a document in a new window
  clear <target>                   empty the main window
  show <host>                      what the main window of a workstation displays
  follow <target> | follow off     bring up each new alarm on the target
  quit
Documents: a synoptic (Folder/Plan.xaml), a variable or camera ($V.Node.Cam1), an area ($A.Hall),
  a built-in view (@AlarmsList, @EventsHistory, @Cameras...), a layout (VideoX4), an environment (Night.env)
Args: Key=Value, e.g. Zoom=Auto, DateStart=-10m, ColumnSort=Severity,Descending, WindowTitle=Alarm");

		static async Task ListClients()
		{
			var infos = await _server.ClientManager.GetClientInfos();
			var clients = infos?.ClientsConnected?.Where(c => !c.IsProtocol).ToArray() ?? new ClientInfo[0];
			Print($"{clients.Length} client(s) connected to {infos?.ServerName}:");
			foreach (var c in clients.OrderBy(c => c.HostName))
				Print($"  {c.HostName,-28} user {c.Name,-10} {c.ProductName,-16} since {c.DateStart:HH:mm}");
		}

		/// <summary>
		/// Loads a document in a container of a window of the target client(s).
		/// windowNum 0 = active window (1 = main window, -1 = last opened); container "" = the main container.
		/// </summary>
		static void Load(string target, string document, string[] args)
		{
			_server.ClientManager.Load(Required(target), 0, "", document, args);
			Print($"Sent: load '{document}' on {target}");
		}

		/// <summary>Opens a new window; args can also hold window options: Size=w,h, Topmost, Unique=replace...</summary>
		static void Open(string target, string document, string[] args)
		{
			_server.ClientManager.Open(Required(target), document, args);
			Print($"Sent: open '{document}' on {target}");
		}

		/// <summary>
		/// Commands are fire-and-forget: a wrong target or a refused document is not reported. Read back the
		/// layout of the window to know what is really displayed. Read-back needs one workstation (host name).
		/// </summary>
		static async Task Show(string host)
		{
			var layout = await _server.ClientManager.GetLayout(host, 1);
			Print(string.IsNullOrEmpty(layout) ? $"No layout received from {host} (not connected, or not an operator client)" : layout);
		}

		static void Follow(string target)
		{
			_followTarget = target == null || target.Equals("off", StringComparison.OrdinalIgnoreCase) ? null : target;
			Print(_followTarget == null ? "Follow mode off" : $"Each new alarm will be shown on {_followTarget}");
		}

		/// <summary>Called on a background thread for each alarm change: brings up each new alarm on the target.</summary>
		static void OnAlarmChanged(AlarmInfo info)
		{
			var target = _followTarget;
			if (target == null || info.Event != AlarmEvent.Start)
				return;
			var document = AlarmDocument(info.Alarm);
			_server.ClientManager.Load(target, 0, "", document);
			Print($"  >> alarm #{info.Alarm.Id} {info.Alarm.Description}: {document} loaded on {target}");
		}

		/// <summary>
		/// "@alarm_&lt;id&gt;" makes the client show the video of the alarm (playback around its start) or the
		/// synoptic of its source variable, centred on it. An alarm with neither (raised by a program, variable
		/// without synoptic) would change nothing on screen: show the alarm list instead.
		/// </summary>
		static string AlarmDocument(AlarmRow alarm)
		{
			if (!string.IsNullOrEmpty(alarm.Video))
				return "@alarm_" + alarm.Id;
			var source = alarm.Type == AlarmType.Variable ? _server.VariableManager.GetRowById(alarm.SourceId) : null;
			if (!string.IsNullOrEmpty(source?.FullSynoptic))
				return "@alarm_" + alarm.Id;
			return "@AlarmsList";
		}

		static string Required(string target) => target ?? throw new Exception("Target expected (host name, @User.<name>, @All...)");

		static void Print(string text)
		{
			lock (_consoleLock)
				Console.WriteLine(text);
		}
	}
}

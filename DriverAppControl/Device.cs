using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using Prysm.AppControl.SDK;
using Prysm.AppControl.SDK.AppControlDataServiceReference;
using System.Collections.Concurrent;

#pragma warning disable CS0067 // x is never used

namespace DriverAppControl
{
	/// <summary>
	/// Class for an access controler device.
	/// </summary>
	class Device
	{
		#region public events & properties
		/// <summary>
		/// Event to notify a connection/disconnection
		/// </summary>
		internal event ConnectChangedEventHandler ConnectChanged;
		/// <summary>
		/// Event to notify an input update
		/// </summary>
		internal event InputChangedEventHandler InputChanged;
		/// <summary>
		/// Access control event
		/// </summary>
		internal event AccessControlEventHandler AccessControlEvent;
		/// <summary>
		/// Trace event
		/// </summary>
		internal event TraceEventHandler TraceEvent;

		/// <summary>Gets whether the communication channel to the controller is open.</summary>
		internal bool IsOpened
		{
			get { return _channel.IsOpened; }
		}
		/// <summary>AppVision variable row ID.</summary>
		internal int Id
		{
			get;
			private set;
		}
		/// <summary>AppVision variable name (e.g. <c>Access.C001</c>).</summary>
		internal string Name
		{
			get;
			private set;
		}
		/// <summary>Network address of the physical controller.</summary>
		internal string Address
		{
			get { return _address; }
		}
		/// <summary>
		/// Delegates for trace and error
		/// </summary>
		public static Action<string, bool> TraceDelegate = null;
		public static Action<string, bool> ErrorDelegate = null;
		#endregion

		// access controller hardware capacities
		internal const int MAX_READER = 4;		// readers per controller
		internal const int MAX_HOLIDAY = 24;	// public holiday slots
		internal const int MAX_TIMETABLE = 48;	// timetable programme slots
		internal const int MAX_USER = 10000;	// cardholder entries
		internal const int MAX_PROFILE = 256;	// access profile slots

		/// <summary>
		/// Initialises the device with its AppVision identity and communication settings.
		/// </summary>
		/// <param name="id">AppVision variable row ID.</param>
		/// <param name="name">AppVision variable name used to build sub-variable names.</param>
		/// <param name="address">Network address of the physical controller.</param>
		/// <param name="parameters">Key=value parameter string from the AppVision variable configuration.</param>
		/// <param name="timeout">Communication timeout in milliseconds.</param>
		/// <param name="maxRetry">Maximum number of retries before raising a connection error.</param>
		internal Device(int id, string name, string address, string parameters, int timeout, int maxRetry)
		{
			Id = id;
			Name = name;
			_address = address;
			_badgeType = Helper.GetParameter<int>("BADGETYPE", parameters, 1);		// 1: default badge type

			#region initialize _readerNames
			_readerNames = new string[MAX_READER];
			for (int i = 0; i < _readerNames.Length; i++)
				_readerNames[i] = name + ".Reader" + (i + 1).ToString("00");
			#endregion

			_channel = new ChannelService(address, parameters, timeout, maxRetry);
			_channel.TraceDelegate = Trace;
			_channel.ErrorDelegate = Error;
		}

		#region Start(),Stop()
		/// <summary>
		/// Start thread
		/// </summary>
		internal void Start()
		{
			_thread = new Thread(new ThreadStart(Main));
			_thread.Start();
		}
		/// <summary>
		/// Stop thread
		/// </summary>
		internal void Stop()
		{
			if (_thread != null)
				if (_thread.IsAlive)
				{
					Trace("Stop thread ...");
					_stopThread = true;
					if (!_thread.Join(5000))
						_thread.Abort();
				}
		}
		#endregion
		#region SendCommand()
		/// <summary>
		/// Send a command to the server
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public void SendCommand(object command)
		{
			_commands.Enqueue(command);
		}
		#endregion

		#region Main()
		/// <summary>
		/// Thread main
		/// </summary>
		void Main()
		{
			Trace("Start thread ...");
			_stopThread = false;
			while (!_stopThread)
			{
				try
				{
					// check connection
					if (!_channel.IsOpened)
					{
						_channel.Open();
					}
					OnConnect(ConnectType.Connected);

					// process waiting commands
					ProcessCommands();
					// read events
					ReadEvents();
					Thread.Sleep(50);
				}
				catch (Exception ex)
				{
					Error("Main exception:" + ex.Message);
					OnConnect(ConnectType.Default,ex.Message);
					_channel.Close();
					Thread.Sleep(5000);
				}
			}
			try
			{
				OnConnect(ConnectType.Disconnected);
				_channel.Close();
			}
			catch (Exception ex)
			{
				Trace("Exception: " + ex.Message);
			}
			Trace("End thread.");
		}
		#endregion

		#region ReadEvents()
		/// <summary>
		/// Reads incoming hardware events from the controller and raises the appropriate
		/// <see cref="InputChanged"/> or <see cref="AccessControlEvent"/> events.
		/// </summary>
		private void ReadEvents()
		{
			// todo:
			//
			// read events
			//	...
			//
			// then send notifications to driver level:
			//
			// InputChanged?.Invoke(this, new InputChangedEventArgs(Name, type, inputId, inputValue, inputDate));
			// or (readerId = 1..MAX_READER, userId = AppControl person id, 0 if unknown)
			// AccessControlEvent?.Invoke(this, new AccessControlEventArgs(Name, readerId, rStatus, userId, badgeCode, rDate));
		}
		#endregion
		#region OnConnect()
		/// <summary>
		/// Updates the connection state and raises <see cref="ConnectChanged"/> if the state changed.
		/// </summary>
		void OnConnect(ConnectType connected, string info = "")
		{
			if (_connected != connected)
			{
				_connected = connected;
				Trace("OnConnect " + connected);
				ConnectChangedEventArgs e = new ConnectChangedEventArgs(Name, connected,info);
				if (ConnectChanged != null)
					ConnectChanged(this, e);
			}
		}
		ConnectType _connected = ConnectType.Unknown;
		#endregion
		#region ProcessCommands()
		/// <summary>
		/// Process the commands waiting
		/// </summary>
		void ProcessCommands()
		{
			object command;
			while( _commands.TryDequeue(out command))
			{
				if (command is CommandAction)
					ProcessCommandAction(command as CommandAction);
				else if (command is CommandDownloadList)
					DownloadList((command as CommandDownloadList).Users, (command as CommandDownloadList).Timetables, (command as CommandDownloadList).Holidays, (command as CommandDownloadList).Parameters);
				else if (command is CommandDownloadInfo)
					Download((command as CommandDownloadInfo).Parameter);
			}
		}
		/// <summary>Processes a direct action command (e.g. door open/close) sent from AppVision.</summary>
		void ProcessCommandAction(CommandAction cmd)
		{
			// todo:
		}
		ConcurrentQueue<object> _commands = new ConcurrentQueue<object>();		// commands queue
		#endregion

		#region Methods for downloading
		#region Download()
		/// <summary>
		/// Download data to the device
		/// </summary>
		/// <param name="info">Holidays,Timetables,Rights with ; separator</param>
		void Download(string info)
		{
			try
			{
				OnConnect(ConnectType.Downloading);

				string[] infos = info.Split(',');
				bool downloadHolidays = infos.Contains("Holidays");
				bool downloadTimetables = infos.Contains("Timetables");
				bool downloadRights = infos.Contains("Rights") || infos.Contains("People");
				bool downloadParameters = infos.Contains("Parameters");

				if (downloadHolidays)
					DownloadHolidays();
				Datalayer dl = new Datalayer(Driver.Server);
				PersonMapRight[] mapRights = null;
				List<DeviceIndex> timetableIndexes=null;
				if (downloadTimetables || downloadRights)
				{
					// Access rights of every person on the readers of this controller (one TimetableMap entry per reader).
					// Called on the device thread (no synchronization context): waiting for the Task is safe here.
					mapRights = dl.GetAllPeopleMapByReaderNames(_readerNames).Result;
					timetableIndexes = BuildTimetableIndexes(mapRights);
					SaveTimetableIndexes(timetableIndexes.ToArray());
					DownloadTimetables(timetableIndexes.ToArray());
				}
				if (downloadRights)
				{
					var profiles = BuildProfiles(mapRights);
					SaveProfiles(profiles.ToArray());
					DownloadProfiles(timetableIndexes,profiles);

					var userIndexes = BuildUserIndexes(mapRights);
					SaveUserIndexes(userIndexes.ToArray());
					DownloadUsers(userIndexes,mapRights);
				}
				if (downloadParameters)
				{
					var v = dl.GetVariable(Name);
					if (v != null)
					{
						_channel.DownloadParameters(v.Parameters);
					}
				}
			}
			catch (Exception ex)
			{
				Error("Download: " + ex.ToString());
			}
		}
		#endregion
		#region DownloadList()
		/// <summary>
		/// Download a list of modifications
		/// </summary>
		void DownloadList(List<int> downloadUsers, List<int> downloadTimetables, bool downloadHolidays, bool downloadParameters)
		{
			try
			{
				OnConnect(ConnectType.Downloading);

				if (downloadHolidays)
					DownloadHolidays();
				DeviceIndex[] timetableIndexes=null;
				if (downloadTimetables.Count > 0 || downloadUsers.Count > 0)
					timetableIndexes = LoadTimetableIndexes();
				if (downloadTimetables.Count > 0)
				{
					foreach (int timetableId in downloadTimetables)
						DownloadTimetable(ref timetableIndexes,timetableId);
				}
				if (downloadUsers.Count > 0)
				{
					var profiles = LoadProfiles().ToList();
					foreach (int personId in downloadUsers)
						DownloadUser(LoadUserIndexes(), timetableIndexes, profiles, personId);	// reload: each call may update the index file
				}
				if (downloadParameters)
				{
					Datalayer dl = new Datalayer(Driver.Server);
					var v = dl.GetVariable(Name);
					if (v != null)
					{
						_channel.DownloadParameters(v.Parameters);
					}
				}
			}
			catch (Exception ex)
			{
				Error("DownloadList: " + ex.ToString());
			}
		}
		#endregion
		#region AddDeviceIndex()
		/// <summary>
		/// Appends a new <see cref="DeviceIndex"/> entry mapping <paramref name="appId"/> to the
		/// next available device-side sequential ID.
		/// </summary>
		/// <returns>The newly assigned device ID.</returns>
		private int AddDeviceIndex(ref DeviceIndex[] ids, int appId)
		{
			int deviceId = GetNextDeviceId(ids);
			List<DeviceIndex> list = new List<DeviceIndex>(ids);
			list.Add(new DeviceIndex { AppId = appId, DeviceId = deviceId });
			ids = list.ToArray();
			return deviceId;
		}
		/// <summary>Returns the next sequential device ID (max existing + 1, starting at 1).</summary>
		private int GetNextDeviceId(DeviceIndex[] ids)
		{
			if( ids.Length==0)
				return 1;
			return ids.Select(id => id.DeviceId).Max() + 1;
		}
		#endregion

		#region DownloadHolidays()
		void DownloadHolidays()
		{
			Trace("DownloadHolidays ...",true);
			Datalayer dl = new Datalayer(Driver.Server);
			var holidays = dl.GetHolidays();
			if (holidays.Length > MAX_HOLIDAY)
				Error("Too holidays");

			_channel.Holiday_Reset();
			int count = 0;
			foreach (var holiday in holidays.Take(MAX_HOLIDAY))
				if (_channel.Holiday_Update(count, holiday.Date))
					count++;		// next device slot
				else
					Error("Holiday_Update: " + holiday.Date);
			_channel.Holiday_Complete();
			Trace(" " + count + " day(s).", true);
		}
		#endregion
		#region DownloadTimetables()
		void DownloadTimetables(DeviceIndex[] timetableIndexes)
		{
			Trace("DownloadTimetables ...", true);
			if (timetableIndexes.Length > MAX_TIMETABLE)
				Error("Too timetables " + timetableIndexes.Length);

			_channel.Timetable_Reset();
			int count = 0;
			foreach (var id in timetableIndexes)
			{
				if (DownloadTimetable(ref timetableIndexes, id.AppId))
					count++;
				if (count == MAX_TIMETABLE)
					break;
			}
			_channel.Timetable_Complete();
			Trace(" " + count + " timetable(s).", true);
		}
		bool DownloadTimetable( ref DeviceIndex[] timetableIndexes,int appId)
		{
			Datalayer dl = new Datalayer(Driver.Server);
			var timetable = dl.GetTimetableById(appId);
			if (timetable == null)
			{
				Error("DownloadTimetable: unknown id: " + appId);
				return false;
			}
			int deviceId = GetDeviceId(timetableIndexes, appId);
			if (deviceId == 0)	// new timetable?
			{
				if (timetableIndexes.Length >= MAX_TIMETABLE)
				{
					Error("Too timetables");
					return false;
				}
				deviceId = AddDeviceIndex(ref timetableIndexes, appId);
				SaveTimetableIndexes(timetableIndexes);
			}
			if (!_channel.Timetable_Update(new DeviceTimetable(deviceId, timetable)))
			{
				Error("Timetable_Update: " + timetable);
				return false;
			}
			return true;
		}
		#endregion
		#region DownloadProfiles()
		void DownloadProfiles( IEnumerable<DeviceIndex> timetableIndexes, List<DeviceProfile> profiles)
		{
			Trace("DownloadProfiles ...", true);
			if (profiles.Count > MAX_PROFILE)
				Error("Too profiles");

			_channel.Profile_Reset();
			int count = 0;
			foreach (var p in profiles)
			{
				var p2 = BuildProfileWithDeviceIds(timetableIndexes, p);	// replace timetable appId by timetable deviceId
				if (_channel.Profile_Update(p2))
					count++;
				else
					Error("Profile_Update: " + p);
				if (count == MAX_PROFILE)
					break;
			}
			_channel.Profile_Complete();
			Trace(string.Format(" {0} profile(s).", profiles.Count), true);
		}
		#endregion
		#region DownloadUsers()
		void DownloadUsers(IEnumerable<DeviceIndex> userIndexes, PersonMapRight[] mapRights)
		{
			Trace("DownloadUsers ...", true);

			_channel.User_Reset();
			int count = 0;
			foreach (var right in mapRights)
			{
				if( count>=MAX_USER)
				{
					Error("Too users");
					break;
				}
				if (!CheckUser(right))		// inactive, expired, no badge nor PIN code: not downloaded (see BuildUserIndexes)
					continue;
				int profileId = (int)right.Tag;
				count += DownloadUser(userIndexes, right.Person,profileId);
			}
			_channel.User_Complete();
			Trace(string.Format(" {0} user(s)", count), true);
		}
		#endregion
		#region DownloadUser()
		/// <summary>
		/// Download user
		/// </summary>
		/// <param name="did">Device user id</param>
		/// <param name="pm">Person map right</param>
		/// <returns>The number of badge for this person</returns>
		int DownloadUser(IEnumerable<DeviceIndex> userIndexes, Person person, int profileId)
		{
			Trace("DownloadUser " + person.FirstName + " " + person.LastName, true);
			var dids = GetDeviceIds(userIndexes, person.Id);
			int deviceIdCount = Math.Max(1, FilterBadges(person.Badges).Count());	// one device entry per badge, one for a PIN-only person
			if (dids.Count() != deviceIdCount)		// new badge or badge removed?
			{
				DownloadDeleteUser(userIndexes, person.Id);
				userIndexes = userIndexes.Where(id => id.AppId != person.Id);
				if (userIndexes.Count() + deviceIdCount >= MAX_USER)
				{
					Error("Too users");
					return 0;
				}
				var list = userIndexes.ToArray();
				for (int i = 0; i < deviceIdCount; i++)
					AddDeviceIndex(ref list, person.Id);
				SaveUserIndexes(list);
				userIndexes = list;
			}
			dids = GetDeviceIds(userIndexes, person.Id);

			// check if person has only a pincode
			if (FilterBadges(person.Badges).Count() == 0)
			{
				var user = new DeviceUser
				{
					Id = dids.First(),
					DateStart = person.ValidityStart,
					DateEnd = person.ValidityEnd,
					PinCode = person.PinCode,
					FullName = person.FirstName + " " + person.LastName,
					BadgeCode = "",
					ProfileId = profileId
				};
				if (_channel.User_Update(user))
					return 1;
				Error("User_Update: " + user);
				return 0;
			}
			else
			{
				int count = 0;
				foreach (var badge in FilterBadges(person.Badges))
				{
					var user = new DeviceUser
					{
						Id = dids.ToArray()[count],
						DateStart = person.ValidityStart,
						DateEnd = person.ValidityEnd,
						PinCode = person.PinCode,
						FullName = person.FirstName + " " + person.LastName,
						BadgeCode = badge.Code,
						ProfileId = profileId
					};
					if (_channel.User_Update(user))
						count++;
					else
						Error("User_Update: " + user);
				}
				return count;
			}
		}
		/// <summary>
		/// Downloads a single user modification: looks up the person's access rights, refreshes
		/// (or removes) the entry on the device and updates the profile table if needed.
		/// </summary>
		void DownloadUser(IEnumerable<DeviceIndex> userIndexes, IEnumerable<DeviceIndex> timetableIndexes, List<DeviceProfile> profiles, int personId)
		{
			Datalayer dl = new Datalayer(Driver.Server);
			// Access rights of one person on the readers of this controller, or null if the person does not exist.
			var right = dl.GetPersonMapByReaderIds2(dl.GetVariableIds(_readerNames), personId).Result;
			if (right == null)
			{
				Trace("DownloadUser unknown " + personId);
				DownloadDeleteUser(userIndexes,personId);
				return;
			}
			if (!CheckUser(right))
			{
				Trace("DownloadUser invalid "+personId);
				DownloadDeleteUser(userIndexes,personId);
				return;
			}
			bool newProfil;
			var profile = GetProfile(right.TimetableMap,out newProfil,profiles);
			if (profile == null)	// too profiles?
				return;
			if( newProfil )
			{
				var profile2 = BuildProfileWithDeviceIds(timetableIndexes, profile);	// replace timetable appId by timetable deviceId
				if (!_channel.Profile_Update(profile2))
					Error("Profile_Update: " + profile2);
				SaveProfiles(profiles.ToArray());
			}
			DownloadUser(userIndexes, right.Person, profile.Id);
		}

		/// <summary>
		/// Returns a copy of <paramref name="profile"/> where each timetable ID has been
		/// translated from the AppVision ID to the device-side sequential ID.
		/// </summary>
		private DeviceProfile BuildProfileWithDeviceIds(IEnumerable<DeviceIndex> timetableIndexes,DeviceProfile profile)
		{
			var profile2 = new DeviceProfile { Id = profile.Id, TimetableMap = new TimetableAndValidity[profile.TimetableMap.Length] };
			for (int i = 0; i < profile.TimetableMap.Length; i++)
				profile2.TimetableMap[i] = new TimetableAndValidity(
					GetDeviceId(timetableIndexes, profile.TimetableMap[i].TimetableId),
					profile.TimetableMap[i].ValidityStart,
					profile.TimetableMap[i].ValidityEnd);
			return profile2;
		}
		#endregion
		#region DownloadDeleteUser()
		/// <summary>
		/// Removes a person from the device and cleans up their entries in the user index file.
		/// </summary>
		private void DownloadDeleteUser(IEnumerable<DeviceIndex> userIndexes, int personId)
		{
			Trace("DownloadDeleteUser "+personId, true);
			// delete in the user DeviceIndex list
			var list = new List<DeviceIndex>();
			list.AddRange(userIndexes.Where(id => id.AppId != personId));
			SaveUserIndexes(list.ToArray());

			// delete in the device
			var dids = GetDeviceIds(userIndexes, personId);
			foreach (var did in dids)
				_channel.User_Delete(did);
		}
		#endregion
		#region CheckUser()
		/// <summary>
		/// Check user validity
		/// </summary>
		/// <param name="pm"></param>
		/// <returns></returns>
		bool CheckUser(PersonMapRight pm)
		{
			if (pm?.Person == null)		// person not found (can happen, see Datalayer documentation)
				return false;
			if (!pm.Person.IsActive)
				return false;
			if (pm.Person.ValidityEnd.Date < DateTime.Now)
				return false;
			if (string.IsNullOrEmpty(pm.Person.PinCode) && FilterBadges(pm.Person.Badges).Count() == 0)
				return false;
			return true;
		}
		#endregion
		#region FilterBadges()
		/// <summary>
		/// Return badges which have the BadgeType.
		/// </summary>
		/// <param name="badges"></param>
		/// <returns></returns>
		IEnumerable<Badge> FilterBadges(IEnumerable<Badge> badges)
		{
			return badges.Where(b => b.BadgeTypeId == _badgeType);		// filter badge type
		}
		#endregion

		#region BuildTimetableIndexes(),BuildUserIndexes()
		/// <summary>
		/// Build the timetable indexes which are used in the mapRights.
		/// </summary>
		/// <param name="mapRights"></param>
		/// <returns></returns>
		List<DeviceIndex> BuildTimetableIndexes(PersonMapRight[] mapRights)
		{
			Trace("Build timetable indexes ...");
			// build id list
			List<int> timetableIds = new List<int>();
			foreach (var p in mapRights)
				foreach (var tv in p.TimetableMap)
					if ( !timetableIds.Contains(tv.TimetableId))
						timetableIds.Add(tv.TimetableId);

			// build device id list
			List<DeviceIndex> timetableIndexes = new List<DeviceIndex>();
			int did = 1;	// start device index = 1
			foreach (int id in timetableIds)
				timetableIndexes.Add(new DeviceIndex { AppId = id, DeviceId = did++ });
			return timetableIndexes;
		}
		/// <summary>
		/// Build the user indexes which are used in the mapRights.
		/// </summary>
		/// <param name="mapRights"></param>
		/// <returns></returns>
		List<DeviceIndex> BuildUserIndexes(PersonMapRight[] mapRights)
		{
			Trace("Build user indexes ...");
			List<int> personIds = new List<int>();
			foreach (var right in mapRights)
				if (CheckUser(right))
				{
					personIds.Add(right.Person.Id);
					// if a person has more than 1 badge, create several DeviceIndex
					for (int i = 0; i < (FilterBadges(right.Person.Badges).Count() - 1); i++)
						personIds.Add(right.Person.Id);
				}
			List<DeviceIndex> userIndexes = new List<DeviceIndex>();
			int did = 1;
			foreach (int id in personIds)
				userIndexes.Add(new DeviceIndex { AppId = id, DeviceId = did++ });
			return userIndexes;
		}
		#endregion
		#region BuildProfiles()
		/// <summary>
		/// Build profile list
		/// and initialize PersonMapRight.Tag with profile n°
		/// </summary>
		/// <param name="rights"></param>
		List<DeviceProfile> BuildProfiles(PersonMapRight[] rights)
		{
			Trace("Build profiles ...");
			bool b;
			List<DeviceProfile> profiles = new List<DeviceProfile>();
			foreach (var right in rights)
			{
				if (right.HasRight)
				{
					var profile = GetProfile(right.TimetableMap, out b,profiles);
					if (profile == null)					// too access profiles?
						right.Tag = 0;
					else
						right.Tag = profile.Id;				// save profile id in right
				}
				else
					right.Tag = 0;
			}
			Trace(string.Format(" {0} profile(s)", profiles.Count));
			return profiles;
		}
		#endregion

		#region GetProfile()
		/// <summary>
		/// Search a profile with the same map 
		/// or create the profile if it doesn't exist.
		/// </summary>
		/// <param name="timetableMap">timetable map</param>
		/// <param name="newProfil">True to indicate that a new profile was created</param>
		/// <param name="profiles">Profiles list</param>
		/// <returns>null if the profile can't be created</returns>
		DeviceProfile GetProfile(TimetableAndValidity[] timetableMap, out bool newProfil, List<DeviceProfile> profiles)
		{
			newProfil = false;
			var pr = FindProfile(timetableMap,profiles);
			if (pr == null)
			{
				if (profiles.Count == MAX_PROFILE)
				{
					Error("Too access profiles");
					return null;
				}
				pr = new DeviceProfile(profiles.Count + 1, timetableMap);
				profiles.Add(pr);
				newProfil = true;
			}
			return pr;
		}
		#endregion
		#region FindProfile()
		/// <summary>
		/// Search a profile with the same map.
		/// </summary>
		/// <param name="timetableMap"></param>
		/// <returns>null if not found</returns>
		DeviceProfile FindProfile(TimetableAndValidity[] timetableMap, IEnumerable<DeviceProfile> profiles)
		{
			if (_profileEmpty.Compare(timetableMap))	// empty profile?
				return _profileEmpty;
			foreach (var p in profiles)
				if (p.Compare(timetableMap))
					return p;
			return null;
		}
		#endregion
		#region LoadProfiles(),SaveProfiles()
		/// <summary>Deserialises the access-profile cache from <c>{name}_P.xml</c>.</summary>
		DeviceProfile[] LoadProfiles()
		{
			try
			{
				string filename = Name + "_P.xml";
				if (File.Exists(filename))
				{
					Trace("Load file " + filename);
					XmlSerializer ser = new XmlSerializer(typeof(DeviceProfile[]));
					using (FileStream file = new FileStream(filename, FileMode.Open))
						return (DeviceProfile[])ser.Deserialize(file);
				}
			}
			catch (Exception ex)
			{
				Error("LoadProfiles(): " + ex.Message);
			}
			return new DeviceProfile[] { };		// no file yet: no full download done
		}
		/// <summary>Serialises the access-profile cache to <c>{name}_P.xml</c>.</summary>
		void SaveProfiles(DeviceProfile[] profiles)
		{
			try
			{
				string filename = Name + "_P.xml";
				Trace("Save file " + filename);
				XmlSerializer ser = new XmlSerializer(typeof(DeviceProfile[]));
				using (StreamWriter writer = new StreamWriter(filename))
					ser.Serialize(writer, profiles);
			}
			catch (Exception ex)
			{
				Error("SaveProfiles(): " + ex.Message);
			}
		}
		#endregion
		#region LoadTimetableIndexes(),SaveTimetableIndexes(),LoadUserIndexes(),SaveUserIndexes()
		/// <summary>Deserialises the timetable index map from <c>{name}_TID.xml</c>.</summary>
		DeviceIndex[] LoadTimetableIndexes()
		{
			try
			{
				string filename = Name + "_TID.xml";
				if (File.Exists(filename))
				{
					Trace("Load file " + filename);
					XmlSerializer ser = new XmlSerializer(typeof(DeviceIndex[]));
					using (FileStream file = new FileStream(filename, FileMode.Open))
						return (DeviceIndex[])ser.Deserialize(file);
				}
			}
			catch (Exception ex)
			{
				Error("LoadTimetableIndexes(): " + ex.Message);
			}
			return new DeviceIndex[]{};
		}
		/// <summary>Deserialises the user index map from <c>{name}_UID.xml</c>.</summary>
		DeviceIndex[] LoadUserIndexes()
		{
			try
			{
				string filename = Name + "_UID.xml";
				if (File.Exists(filename))
				{
					Trace("Load file " + filename);
					XmlSerializer ser = new XmlSerializer(typeof(DeviceIndex[]));
					using (FileStream file = new FileStream(filename, FileMode.Open))
						return (DeviceIndex[])ser.Deserialize(file);
				}
			}
			catch (Exception ex)
			{
				Error("LoadUserIndexes(): " + ex.Message);
			}
			return new DeviceIndex[] { };
		}
		/// <summary>Serialises the timetable index map to <c>{name}_TID.xml</c>.</summary>
		void SaveTimetableIndexes(DeviceIndex[] timetableIds)
		{
			try
			{
				string filename = Name + "_TID.xml";
				Trace("Save file " + filename);
				XmlSerializer ser = new XmlSerializer(typeof(DeviceIndex[]));
				using (StreamWriter writer = new StreamWriter(filename))
					ser.Serialize(writer, timetableIds);
			}
			catch (Exception ex)
			{
				Error("SaveTimetableIndexes(): " + ex.Message);
			}
		}
		/// <summary>Serialises the user index map to <c>{name}_UID.xml</c>.</summary>
		void SaveUserIndexes(DeviceIndex[] userIds)
		{
			try
			{
				string filename = Name + "_UID.xml";
				Trace("Save file " + filename);
				XmlSerializer ser = new XmlSerializer(typeof(DeviceIndex[]));
				using (StreamWriter writer = new StreamWriter(filename))
					ser.Serialize(writer, userIds);
			}
			catch (Exception ex)
			{
				Error("SaveUserIndexes(): " + ex.Message);
			}
		}
		#endregion
		#region GetDeviceId(),GetDeviceIds()
		/// <summary>Returns the first device ID mapped from <paramref name="appId"/>, or 0 if not found.</summary>
		private int GetDeviceId(IEnumerable<DeviceIndex> dids, int appId)
		{
			return (from did in dids where did.AppId == appId select did.DeviceId).FirstOrDefault();
		}
		/// <summary>Returns all device IDs mapped from <paramref name="appId"/> (one per badge).</summary>
		private IEnumerable<int> GetDeviceIds(IEnumerable<DeviceIndex> dids, int appId)
		{
			return (from did in dids where did.AppId == appId select did.DeviceId);
		}
		#endregion
		#endregion

		#region Trace(),Error()
		/// <summary>Logs a diagnostic message; optionally raises a <see cref="TraceEvent"/>.</summary>
		void Trace(string text, bool sendToServer = false)
		{
			TraceDelegate("[" + Name + "]: " + text, false);
			if (sendToServer)
				if (TraceEvent != null)
					TraceEvent(this, new TraceEventArgs(Name, text, -1));
		}
		/// <summary>Logs an error message; optionally raises a <see cref="TraceEvent"/> with severity 1.</summary>
		void Error(string text, bool sendToServer = true)
		{
			ErrorDelegate("[" + Name + "]: " + text, false);
			if (sendToServer)
				if (TraceEvent != null)
					TraceEvent(this, new TraceEventArgs(Name, text, 1));
		}
		#endregion

		string _address;
		volatile bool _stopThread;
		Thread _thread;
		ChannelService _channel;

		/// <summary>Lazily loaded timetable index map (AppVision ID → device ID).</summary>
		DeviceIndex[] TimetableIndexes
		{
			get
			{
				if (_timetableIndexes == null)
					_timetableIndexes = LoadTimetableIndexes();
				return _timetableIndexes;
			}
		}
		DeviceIndex[] _timetableIndexes = null;

		string[] _readerNames;		// list of reader names for this device
		int _badgeType;
		DeviceProfile _profileEmpty = new DeviceProfile();
	}

	// Data classes for downloading
	#region class DeviceIndex
	[Serializable]
	public class DeviceIndex
	{
		/// <summary>
		/// App id.
		/// </summary>
		public int AppId
		{ get; set; }
		/// <summary>
		/// Device id.
		/// </summary>
		public int DeviceId
		{ get; set; }
	}
	#endregion
	#region class DeviceTimetable
	[Serializable]
	public class DeviceTimetable
	{
		/// <summary>
		/// Device id.
		/// </summary>
		public int Id
		{ get; set; }

		/// <summary>Raw timetable data packed in the device-specific format.</summary>
		public int[] Data
		{ get; set; }

		/// <summary>Converts an AppVision <see cref="Timetable"/> to the device representation.</summary>
		public DeviceTimetable(int deviceId, Timetable t)
		{
			Id = deviceId;
		}
	}
	#endregion
	#region class DeviceProfile
	/// <summary>
	/// Device access profile: contains access rights for readers
	/// </summary>
	[Serializable]
	public class DeviceProfile
	{
		/// <summary>
		/// Device id.
		/// </summary>
		public int Id
		{ get; set; }
		/// <summary>
		/// Map of timetable id foreach reader
		/// with device timetable id
		/// </summary>
		public TimetableAndValidity[] TimetableMap
		{ get; set; }

		/// <summary>
		/// Create an empty profile
		/// </summary>
		public DeviceProfile()
		{
			Id = 0;
			TimetableMap = new TimetableAndValidity[Device.MAX_READER];
			for (int i = 0; i < TimetableMap.Length; i++)
				TimetableMap[i] = new TimetableAndValidity();
		}
		public DeviceProfile(int id, TimetableAndValidity[] timetableMap)
		{
			Id = id;
			TimetableMap = timetableMap;
		}
		public bool Compare(TimetableAndValidity[] timetableMap)
		{
			for (int i = 0; i < TimetableMap.Length && i < timetableMap.Length; i++)
				if (TimetableMap[i] == null && timetableMap[i] != null)
					return false;
				else if (TimetableMap[i] != null && timetableMap[i] == null)
					return false;
				else if (TimetableMap[i].TimetableId != timetableMap[i].TimetableId)
					return false;
				else if (TimetableMap[i].ValidityStart != timetableMap[i].ValidityStart)
					return false;
				else if (TimetableMap[i].ValidityEnd != timetableMap[i].ValidityEnd)
					return false;
			return true;
		}
	}
	#endregion
	#region class DeviceUser
	/// <summary>Person entry transferred to an access-control device.</summary>
	public class DeviceUser
	{
		/// <summary>
		/// Device id.
		/// </summary>
		public int Id
		{ get; set; }

		/// <summary>Personal identification number.</summary>
		public string PinCode
		{ get; set; }
		/// <summary>Physical badge code (may be empty when entry is PIN-only).</summary>
		public string BadgeCode
		{ get; set; }
		/// <summary>Display name sent to the device.</summary>
		public string FullName
		{ get; set; }
		/// <summary>Validity start date.</summary>
		public DateTime DateStart
		{ get; set; }
		/// <summary>Validity end date.</summary>
		public DateTime DateEnd
		{ get; set; }

		/// <summary>Index of the access profile assigned to this user.</summary>
		public int ProfileId
		{ get; set; }
	}
	#endregion

	#region enum DeviceEventType
	/// <summary>Identifies the type of hardware event reported by a device input.</summary>
	public enum DeviceEventType
	{
		None,
		AccessControlDenied,
		AccessControlGranted,
		DoorMode,
		DoorState,
	}
	#endregion

	#region TraceEventHandler & TraceEventArgs
	/// <summary>Delegate for device trace and error notifications.</summary>
	public delegate void TraceEventHandler(object sender, TraceEventArgs e);
	/// <summary>Data for a trace or error message raised by a device.</summary>
	public class TraceEventArgs : EventArgs
	{
		public TraceEventArgs(string deviceName, string text, int severity)
		{
			DeviceName = deviceName;
			Text = text;
			Severity = severity;
		}
		public string DeviceName;
		public string Text;
		public int Severity;
	}
	#endregion
	#region InputChangedEventHandler & InputChangedEventArgs
	/// <summary>Delegate for door/zone state or mode change notifications.</summary>
	public delegate void InputChangedEventHandler(object sender, InputChangedEventArgs e);
	/// <summary>Data for an input-state change event (door state or door mode).</summary>
	public class InputChangedEventArgs : EventArgs
	{
		public InputChangedEventArgs(string deviceName, DeviceEventType type, int inputId, int inputValue, DateTimeOffset dateUtc)
		{
			DeviceName = deviceName;
			Type = type;
			InputId = inputId;
			InputValue = inputValue;
			DateUtc = dateUtc;
		}
		public string DeviceName;
		public DeviceEventType Type;
		public int InputId;
		public int InputValue;
		public DateTimeOffset DateUtc;
	}
	public enum InputType { Group, Zone, Output };
	#endregion
	#region ConnectChangedEventHandler & ConnectChangedEventArgs & ConnectType
	/// <summary>Delegate for device connection-state change notifications.</summary>
	public delegate void ConnectChangedEventHandler(object sender, ConnectChangedEventArgs e);
	/// <summary>Data for a connection-state change event.</summary>
	public class ConnectChangedEventArgs : EventArgs
	{
		public ConnectChangedEventArgs(string deviceName, ConnectType connected, string info)
		{
			DeviceName = deviceName;
			Connected = connected;
			Info = info;
		}
		public string DeviceName;
		public ConnectType Connected;
		public string Info;
	}
	public enum ConnectType { Unknown = 0, Connected = 1, Disconnected = 2, Default = 3, Downloading = 4 };
	#endregion
	#region AccessControlEventHandler & AccessControlEventArgs
	/// <summary>Delegate for badge-read (access granted or denied) notifications.</summary>
	public delegate void AccessControlEventHandler(object sender, AccessControlEventArgs e);
	/// <summary>Data for a badge-read event from an access-control reader.</summary>
	public class AccessControlEventArgs : EventArgs
	{
		public AccessControlEventArgs(string deviceName, int readerId, ReaderStatus status, int userId, string badgeId, DateTimeOffset dateUtc)
		{
			DeviceName = deviceName;
			ReaderId = readerId;
			Status = status;
			UserId = userId;
			BadgeId = badgeId;
			DateUtc = dateUtc;
		}
		public string DeviceName;
		/// <summary>Reader number on the controller, 1..<see cref="Device.MAX_READER"/> (variable <c>&lt;device&gt;.ReaderNN</c>).</summary>
		public int ReaderId;
		public ReaderStatus Status;
		public int UserId;
		public string BadgeId;
		public DateTimeOffset DateUtc;
	}
	#endregion
}

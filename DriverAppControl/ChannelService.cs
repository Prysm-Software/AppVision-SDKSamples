using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriverAppControl
{
	/// <summary>
	/// For communication service.
	/// </summary>
	class ChannelService
	{
		/// <summary>
		/// Delegates for trace and error
		/// </summary>
		internal Action<string, bool> TraceDelegate = null;
		internal Action<string, bool> ErrorDelegate = null;

		internal bool IsOpened
		{
			get { return _IsOpened; } 
		}
		bool _IsOpened=false;
		
		internal ChannelService(string address, string parameters, int timeout, int maxRetry)
		{
			_password = Helper.GetParameter("PASSWORD", parameters);
		}
		internal void Open()
		{
			_IsOpened = true;
		}
		internal void Close()
		{
			_IsOpened = false;
		}
		internal void Holiday_Reset()
		{ }
		internal bool Holiday_Update(int id, DateTime date)
		{
			return false;
		}
		internal void Holiday_Complete()
		{
		}
		internal void Timetable_Reset()
		{ }
		internal bool Timetable_Update(DeviceTimetable t)
		{
			return false;
		}
		internal void Timetable_Complete()
		{
		}
		internal void Profile_Reset()
		{ }
		internal bool Profile_Update(DeviceProfile p)
		{
			return false;
		}
		internal void Profile_Complete()
		{
		}
		internal void User_Reset()
		{ }
		internal bool User_Update(DeviceUser u)
		{
			return false;
		}
		internal bool User_Delete(int deviceUserId)
		{
			return false;
		}
		internal void User_Complete()
		{
		}
		internal void DownloadParameters(string parameters)
		{
		}

		#region Trace(),Error()
		void Trace(string text, bool sendToServer = false)
		{
			TraceDelegate(text, sendToServer);
		}
		void Error(string text, bool sendToServer = true)
		{
			ErrorDelegate(text, sendToServer);
		}
		#endregion

		string _password;
	}
}

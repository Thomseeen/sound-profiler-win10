using CSCore.CoreAudioAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace SoundProfiler
{
	public static class SoundController
	{
		public static void WriteSoundProfileToFile(Dictionary<string, Dictionary<string, float>> profiles, string profile)
		{
			using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render))
			using (var sessionEnumerator = sessionManager.GetSessionEnumerator())
			{
				foreach (var session in sessionEnumerator)
				{
					string processname;
					using (var session2 = session.QueryInterface<AudioSessionControl2>())
					{
						processname = session2.IsSystemSoundSession ? "system" : session2.Process.ProcessName.ToLower();
					}
					using (var simpleAudioVolume = session.QueryInterface<SimpleAudioVolume>())
					{
						simpleAudioVolume.GetMasterVolumeNative(out float volume);
						if (!profiles[profile].ContainsKey(processname))
						{
							profiles[profile].Add(processname, volume);
						}
					}
				}
			}
			File.WriteAllText(Properties.Settings.Default.ProfileFile, JsonConvert.SerializeObject(profiles));
		}
		public static void SetSoundProfileFromFile(Dictionary<string, Dictionary<string, float>> profiles, string profile)
		{
			using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render))
			using (var sessionEnumerator = sessionManager.GetSessionEnumerator())
			{
				foreach (var session in sessionEnumerator)
				{
					string processname;
					using (var session2 = session.QueryInterface<AudioSessionControl2>())
					{
						processname = session2.IsSystemSoundSession ? "system" : session2.Process.ProcessName.ToLower();
					}
					if(profiles[profile].ContainsKey(processname))
					{
						using (var simpleAudioVolume = session.QueryInterface<SimpleAudioVolume>())
						{
							Guid guid;
							guid = Guid.NewGuid();
							simpleAudioVolume.SetMasterVolumeNative(profiles[profile][processname], guid);
						}
					}
				}
			}
		}
		public static void PrintInfo()
		{
			using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render))
			using (var sessionEnumerator = sessionManager.GetSessionEnumerator())
			{
				foreach (var session in sessionEnumerator)
				{
					using (var session2 = session.QueryInterface<AudioSessionControl2>())
					{
						Debug.WriteLine(session2.IsSystemSoundSession ? "system" : session2.Process.ProcessName.ToLower());
					}
					using (var simpleAudioVolume = session.QueryInterface<SimpleAudioVolume>())
					{
						simpleAudioVolume.GetMasterVolumeNative(out float volume);
						Debug.WriteLine(volume);
					}
				}
			}
		}
		private static AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow)
		{
			using (var enumerator = new MMDeviceEnumerator())
			{
				using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia))
				{
					var sessionManager = AudioSessionManager2.FromMMDevice(device);
					return sessionManager;
				}
			}
		}
	}
}

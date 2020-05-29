using System.IO;
using System.Windows;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace SoundProfiler
{
	/// <summary>
	/// Interaktionslogik für MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		Dictionary<string, Dictionary<string, float>> profiles = new Dictionary<string, Dictionary<string, float>>();
		public MainWindow(string[] args)
		{
			InitializeComponent();
			GetProfilesFromFile();
			foreach(string arg in args)
			{
				if (profiles.ContainsKey(arg.Trim('/')))
				{
					Properties.Settings.Default.LastProfile = arg.Trim('/');
					Properties.Settings.Default.Save();
					string profile = arg.Trim('/');
					Thread reader = new Thread(() => SoundController.SetSoundProfileFromFile(profiles, profile));
					reader.Start();
					while (reader.IsAlive) ;
					Close();
				}
			}
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void ProfileApply_Click(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.LastProfile = (string)ProfileSelector.SelectedItem;
			Properties.Settings.Default.Save();
			string profile = (string) ProfileSelector.SelectedItem;
			Thread reader = new Thread(() => SoundController.SetSoundProfileFromFile(profiles, profile));
			reader.Start();
			while (reader.IsAlive) ;
		}
		private void ProfileDelete_Click(object sender, RoutedEventArgs e)
		{
			string profile = (string)ProfileSelector.SelectedItem;
			if (profiles.ContainsKey(profile))
			{
				profiles.Remove(profile);
				File.WriteAllText(Properties.Settings.Default.ProfileFile, JsonConvert.SerializeObject(profiles));
				ShowProfiles();
			}
		}
		private void ProfileSave_Click(object sender, RoutedEventArgs e)
		{
			Regex regex = new Regex(@"^[a-zA-Z0-9_+-]*$");
			string name = ProfilerNamer.Text;
			if(regex.IsMatch(name, 0) && name != string.Empty)
			{
				if (profiles.ContainsKey(name))
				{
					if(MessageBox.Show("Profil bereits vorhanden. Überschreiben?", "Überschreiben", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
					{
						profiles[name].Clear();
						Thread writer = new Thread(() => SoundController.WriteSoundProfileToFile(profiles, name));
						writer.Start();
						while (writer.IsAlive) ;
						GetProfilesFromFile();
					}
				}
				else
				{
					profiles.Add(name, new Dictionary<string, float>());
					Thread writer = new Thread(() => SoundController.WriteSoundProfileToFile(profiles, name));
					writer.Start();
					while (writer.IsAlive) ;
					GetProfilesFromFile();
				}
			}
			else
			{
				MessageBox.Show("Ungültiger Profilname!");
			}
		}
		private void GetProfilesFromFile()
		{
			if (!File.Exists(Properties.Settings.Default.ProfileFile))
			{
				File.WriteAllText(Properties.Settings.Default.ProfileFile, JsonConvert.SerializeObject(profiles));
			}
			string rawprofiles = File.ReadAllText(Properties.Settings.Default.ProfileFile);
			profiles = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, float>>>(rawprofiles);
			ShowProfiles();
		}
		private void ShowProfiles()
		{
			ProfileSelector.Items.Clear();
			foreach (var profilename in profiles)
			{
				ProfileSelector.Items.Add(profilename.Key);
			}
			if (profiles.ContainsKey(Properties.Settings.Default.LastProfile))
			{
				ProfileSelector.SelectedItem = Properties.Settings.Default.LastProfile;
			}
		}
	}
}

using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Path = System.IO.Path;
using System.Windows.Controls.Primitives;
using Microsoft.VisualBasic;

namespace MXBikesSetupDuperWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    class IniFile   // revision 11
    {
        string Path;
        string EXE = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniFile(string IniPath = null)
        {
            Path = new FileInfo(IniPath ?? EXE + ".ini").FullName;
        }

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }

    public partial class MainWindow : Window
    {
        string 
            modsFolder, 
            pibosoDir, 
            profilesDir, 
            selectedProfileDir, profileSetupsDir,
            selectedTrack, selectedTrackDir,
            selectedBike, selectedBikeDir,
            selectedSetup, selectedSetupPath;

        int copyType;
        string[] tracks;
        List<string> modTracks = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            initPiboso();
        }

        void initPiboso()
        {
            pibosoDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PiBoSo", "MX Bikes");

            // Or specify a specific name in a specific dir
            var MyIni = new IniFile(Path.Join(pibosoDir, "global.ini"));

            if (MyIni.KeyExists("folder", "mods"))
            {
                modsFolder = MyIni.Read("folder", "mods");
            }

            if (modsFolder == null)
                modsFolder = Path.Join(pibosoDir, "mods");

            string tracksFolder = Path.Join(modsFolder, "tracks");
            if (Directory.Exists(tracksFolder))
            {

                string[] subFolders = Directory.GetDirectories(tracksFolder);

                foreach (string subFolder in subFolders)
                {
                    foreach (string pkzFile in Directory.GetFiles(subFolder, "*.pkz"))
                    {
                        modTracks.Add(Path.GetFileNameWithoutExtension(pkzFile));
                    }
                    foreach (string trackDir in Directory.GetDirectories(subFolder))
                    {
                        modTracks.Add(Path.GetFileName(trackDir));
                    }
                }
            }

            if (modTracks.Count > 0)
            {
                modTracks = modTracks.Distinct().ToList();
            }

            profilesDir = Path.Join(pibosoDir, "profiles");

            Debug.WriteLine(profilesDir);

            // check if folder exists, if not, abort
            if (!Directory.Exists(profilesDir))
            {
                MessageBox.Show("Profiles folder not found! Aborting.");

                kill();
            }

            // continue
            string[] folders;

            folders = Directory.GetDirectories(profilesDir);

            foreach (string folder in folders)
            {
                cbSourceProfile.Items.Add(Path.GetFileName(folder));
            }

            cbSourceProfile.Text = $"{cbSourceProfile.Items.Count.ToString()} profiles found, choose one";
        }

        void kill()
        {
            System.Environment.Exit(1);
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            if (cbSourceProfile.SelectedIndex > -1)
            {
                cbSourceProfile_SelectionChanged(cbSourceProfile, null);
            }
        }

        private void cbSourceProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var thisCb = (sender as ComboBox);
            if (thisCb.SelectedItem == null)
                return;

            thisCb.IsEditable = false;

            string selectedProfile = thisCb.SelectedItem.ToString();

            selectedProfileDir = Path.Join(profilesDir, selectedProfile);

            profileSetupsDir = Path.Join(selectedProfileDir, "setups");

            cbSourceTrack.Items.Clear();
            cbSourceTrack.IsEnabled = false;
            cbTargetTrack.ItemsSource = null;
            cbTargetTrack.IsEnabled = false;
            cbSourceBike.Items.Clear();
            cbSourceBike.IsEnabled = false;
            lblSetupInfo.Content = "";

            // check if has setups folder
            if (!Directory.Exists(profileSetupsDir))
            {
                MessageBox.Show("No setups folder found for this profile!");

                return;
            }

            // continue with tracks
            tracks = new DirectoryInfo(profileSetupsDir)
                .EnumerateDirectories()
                .OrderBy(d => Directory.GetLastWriteTime(d.FullName))
                .Select(d => Path.GetFileName(d.FullName))
                .ToArray();

            // check if saved setups found
            if (tracks.Length == 0)
            {
                MessageBox.Show("No saved setups!");

                return;
            }

            // continue
            cbSourceTrack.IsEnabled = true;

            foreach (string track in tracks.Reverse())
            {
                cbSourceTrack.Items.Add(track);
            }
            cbTargetTrack.ItemsSource = tracks
                .OrderBy(x => x)
                .ToList();

            cbSourceTrack.SelectedIndex = 0;
            cbTargetTrack.SelectedIndex = 0;
        }

        private void cbSourceTrack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var thisCb = (sender as ComboBox);
            if (thisCb.SelectedItem == null)
                return;

            selectedTrack = thisCb.SelectedItem.ToString();

            selectedTrackDir = Path.Join(profileSetupsDir, selectedTrack);

            string[] bikes = new DirectoryInfo(selectedTrackDir).EnumerateDirectories().OrderBy(f => f.LastWriteTime).ThenBy(f => f.Name).Select(f => f.FullName).ToArray();

            cbSourceBike.Items.Clear();
            cbSourceBike.IsEnabled = false;
            cbSourceSetup.Items.Clear();
            cbSourceSetup.IsEnabled = false;
            lblSetupInfo.Content = "";

            // check if setups found
            if (bikes.Length == 0)
            {
                MessageBox.Show("No saved setups found for this track!");

                return;
            }

            // continue
            cbSourceBike.IsEnabled = true;

            foreach (string bike in bikes.Reverse())
            {
                cbSourceBike.Items.Add(Path.GetFileName(bike));
            }

            cbSourceBike.SelectedIndex = 0;
        }

        private void cbSourceBike_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var thisCb = (sender as ComboBox);
            if (thisCb.SelectedItem == null)
                return;

            selectedBike = thisCb.SelectedItem.ToString();

            selectedBikeDir = Path.Join(selectedTrackDir, selectedBike);

            string[] setups = new DirectoryInfo(selectedBikeDir).GetFiles("*.stp").OrderBy(f => f.LastWriteTime).ThenBy(f => f.Name).Select(f => f.FullName).ToArray();

            cbSourceSetup.Items.Clear();
            cbSourceSetup.IsEnabled = false;
            lblSetupInfo.Content = "";

            // check if stp files found
            if (setups.Length == 0)
            {
                MessageBox.Show("No setups found!");

                return;
            }

            // continue
            cbSourceSetup.IsEnabled = true;

            foreach (string setup in setups.Reverse())
            {
                cbSourceSetup.Items.Add(Path.GetFileNameWithoutExtension(setup));
            }

            cbSourceSetup.SelectedIndex = 0;
        }

        private void cbSourceSetup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var thisCb = (sender as ComboBox);
            if (thisCb.SelectedItem == null)
                return;

            selectedSetup = thisCb.SelectedItem.ToString();

            selectedSetupPath = Path.Join(selectedBikeDir, selectedSetup + ".stp");

            // get setup file infos
            string createdDate = File.GetCreationTime(selectedSetupPath).ToString("G");
            string changedDate = File.GetLastWriteTime(selectedSetupPath).ToString("G");

            lblSetupInfo.Content = $"{createdDate}\r\n{changedDate}";

            if (rdSpecificTrack.IsChecked == true)
            {
                cbTargetTrack.IsEnabled = true;
            }
        }

        private void rdSpecificTrack_Checked(object sender, RoutedEventArgs e)
        {
            cbTargetTrack.IsEnabled = true;
        }

        private void rdAllTracks_Checked(object sender, RoutedEventArgs e)
        {
            cbTargetTrack.IsEnabled = false;
        }

        void getCopyMode()
        {
            if (rdSpecificTrack.IsChecked == true)
            {
                if (cbSourceTrack.SelectedIndex > -1)
                    cbTargetTrack.IsEnabled = true; copyType = 0;
            }
            else
            {
                cbTargetTrack.IsEnabled = false;
                copyType = 1;
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (cbTargetTrack.SelectedItem == null)
                return;

            ContentDialogResult result;
            string targetDest, targetTrack, targetBike;

            string selectedTargetTrack = cbTargetTrack.SelectedItem.ToString();

            getCopyMode();

            switch (copyType)
            {
                case 0:
                    if (cbTargetTrack.SelectedIndex == -1)
                    {
                        MessageBox.Show("You have to select a track first!"); return;
                    }

                    if (cbTargetTrack.SelectedItem.ToString() == cbSourceTrack.SelectedItem.ToString())
                    {
                        MessageBox.Show("You can't copy the same setup to the same track..."); return;
                    }

                    targetTrack = Path.Join(profileSetupsDir, selectedTargetTrack);

                    targetBike = Path.Join(targetTrack, selectedBike);

                    if (!Directory.Exists(targetBike))
                        Directory.CreateDirectory(targetBike);

                    targetDest = Path.Join(targetBike, selectedSetup + ".stp");

                    if (File.Exists(targetDest))
                    {
                        result = (ContentDialogResult)MessageBox.Show("Setup with same name exists here already. Overwrite?", "Hold up!", MessageBoxButton.YesNo);

                        if (result != ContentDialogResult.Primary)
                        {
                            string promptMsg = "New name for the setup:";
                            tryAgain:
                            string newName = Interaction.InputBox(promptMsg, "Change setup name");

                            if (newName == "")
                                return;

                            if (newName.ToLower() == selectedSetup.ToLower())
                            {
                                promptMsg = "A setup with that name exists already.";
                                goto tryAgain;
                            }

                            targetDest = Path.Join(targetBike, newName + ".stp");
                        }
                    }

                    File.Copy(selectedSetupPath, @targetDest, true);

                    MessageBox.Show($"Setup copied to {selectedTargetTrack}!", "Done!", MessageBoxButton.OK, MessageBoxImage.Information);

                    break;
                case 1:
                    result = (ContentDialogResult)MessageBox.Show("This action will overwrite existing setups and create new folders for every track you have installed. Do you want to continue?", "Hold up!", MessageBoxButton.YesNo);

                    if (result == ContentDialogResult.Secondary)
                        return;

                    List<string> allTracks = new List<string>();

                    // get already saved tracks
                    foreach (string track in Directory.GetDirectories(profileSetupsDir))
                    {
                        allTracks.Add(Path.GetFileName(track));
                    }

                    if (modTracks.Count > 0)
                    {
                        allTracks.AddRange(modTracks);
                        allTracks.Distinct().ToList();
                    }

                    foreach (string track in allTracks.ToArray())
                    {
                        Debug.WriteLine(track);

                        if (selectedTrack == track)
                            continue;

                        targetDest = Path.Join(profileSetupsDir, track, selectedBike);

                        if (!Directory.Exists(targetDest))
                            Directory.CreateDirectory(@targetDest);

                        File.Copy(selectedSetupPath, @Path.Join(targetDest, selectedSetup + ".stp"), true);
                    }

                    MessageBox.Show("Setup copied to all tracks!", "Done!", MessageBoxButton.OK, MessageBoxImage.Information);

                    break;
            }
        }
    }
}

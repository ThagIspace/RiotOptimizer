using System;
using System.Collections.Generic;
using System.IO;
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

namespace ValorantResFix
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            StartUP();
        }

        private void StartUP()
        {
            List<string> SettingsFileList = new List<string>();

            string dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            dir = System.IO.Path.Combine(dir, "VALORANT", "Saved", "Config");

            if (Directory.Exists(dir))
            {
                try
                {
                    // Find all GameUserSettings.ini under VALORANT/Saved/Config
                    // Supports both modern WindowsClient and legacy Windows paths across all accounts
                    string[] iniFiles = Directory.GetFiles(dir, "GameUserSettings.ini", SearchOption.AllDirectories);
                    foreach (var file in iniFiles)
                    {
                        string lower = file.ToLower();
                        if (!lower.Contains("crashreportclient") && !lower.Contains("backup"))
                        {
                            SettingsFileList.Add(file);
                        }
                    }
                }
                catch
                {
                    // Fallback to folder enumeration if recursive search encounters an issue
                    string[] users = Directory.GetDirectories(dir);
                    foreach (var u in users)
                    {
                        string winClient = System.IO.Path.Combine(u, "WindowsClient", "GameUserSettings.ini");
                        string winLegacy = System.IO.Path.Combine(u, "Windows", "GameUserSettings.ini");

                        if (File.Exists(winClient))
                        {
                            SettingsFileList.Add(winClient);
                        }
                        else if (File.Exists(winLegacy))
                        {
                            SettingsFileList.Add(winLegacy);
                        }
                    }
                }

                // Remove duplicate paths (case-insensitive)
                SettingsFileList = SettingsFileList.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                if (SettingsFileList.Count < 1) // if No GameSettings File found
                {
                    MyBaseFrame.Navigate(new ErrorPage());
                }
                else
                {
                    MyBaseFrame.Navigate(new Home(SettingsFileList));
                }
            }
            else // if Valorant folder is not found in AppData Dir
            {
                MyBaseFrame.Navigate(new ErrorPage());
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void MinBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "ValorantResFix v2.0\n\n" +
                "A lightweight utility to adjust internal graphics & resolution scale for VALORANT to boost FPS on low-end hardware.\n\n" +
                "Author / Credits:\n" +
                "• Original Creator: xFahim (https://github.com/xFahim/ValorantResFix)\n" +
                "• Maintained & Updated by: ThagIspace (https://github.com/ThagIspace/ValorantResFix)\n\n" +
                "Safe & Vanguard compliant (client config modification only).",
                "About ValorantResFix",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}

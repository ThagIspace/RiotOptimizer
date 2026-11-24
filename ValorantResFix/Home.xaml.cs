using IniParser;
using IniParser.Model;
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
using System.Windows.Shapes;

namespace ValorantResFix
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        List<string> _fnames = new List<string>();
        List<ScalabilityGroups> _SG = new List<ScalabilityGroups>();
        ScalabilityGroups backupData = new ScalabilityGroups();

        // ini parse
        FileIniDataParser parser = new FileIniDataParser();
        List<IniData> _iniList = new List<IniData>();

        public Home(List<string> fl)
        {
            InitializeComponent();

            _fnames = fl;

            StartUP();
        }

        private int ParseIniInt(string val, int defaultVal)
        {
            if (string.IsNullOrWhiteSpace(val)) return defaultVal;
            decimal result;
            if (decimal.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
            {
                return (int)result;
            }
            return defaultVal;
        }

        private void StartUP()
        {
            TotalFileTxt.Text = _fnames.Count.ToString();

            // load Data
            foreach (var s in _fnames)
            {
                try
                {
                    IniData data = parser.ReadFile(s);
                    ScalabilityGroups sg = new ScalabilityGroups();

                    var sgSection = data["ScalabilityGroups"];
                    if (sgSection == null)
                    {
                        data.Sections.AddSection("ScalabilityGroups");
                        sgSection = data["ScalabilityGroups"];
                    }

                    // ResolutionQuality defaults to 100 if omitted in ini
                    sg.ResolutionQuality = ParseIniInt(sgSection["sg.ResolutionQuality"], 100);
                    sg.ViewDistanceQuality = ParseIniInt(sgSection["sg.ViewDistanceQuality"], 3);
                    sg.AntiAliasingQuality = ParseIniInt(sgSection["sg.AntiAliasingQuality"], 3);
                    sg.ShadowQuality = ParseIniInt(sgSection["sg.ShadowQuality"], 3);
                    sg.PostProcessQuality = ParseIniInt(sgSection["sg.PostProcessQuality"], 3);
                    sg.TextureQuality = ParseIniInt(sgSection["sg.TextureQuality"], 0);
                    sg.EffectsQuality = ParseIniInt(sgSection["sg.EffectsQuality"], 3);
                    sg.FoliageQuality = ParseIniInt(sgSection["sg.FoliageQuality"], 3);
                    sg.ShadingQuality = ParseIniInt(sgSection["sg.ShadingQuality"], 3);
                    sg.FilePath = s;

                    _SG.Add(sg);
                    _iniList.Add(data);
                }
                catch
                {
                    continue;
                }
            }

            // View Data 
            if (_SG.Count < 1)
            {
                MessageBox.Show("Could not parse config files. Please ensure Valorant has run at least once.", "Error");
                Application.Current.Shutdown();
                return;
            }
            else
            {
                // set the first elements data to the view 
                ResqualityBox.Text = _SG[0].ResolutionQuality.ToString();
                ViewDistanceQBox.Text = _SG[0].ViewDistanceQuality.ToString();
                AntiAliasisBox.Text = _SG[0].AntiAliasingQuality.ToString();
                ShadowQBox.Text = _SG[0].ShadowQuality.ToString();
                PostProcessBox.Text = _SG[0].PostProcessQuality.ToString();
                TextureQBox.Text = _SG[0].TextureQuality.ToString();
                EffectQBox.Text = _SG[0].EffectsQuality.ToString();
                FoliageQBox.Text = _SG[0].FoliageQuality.ToString();
                ShadingQBox.Text = _SG[0].ShadingQuality.ToString();
            }

            // check backup 
            if (File.Exists("Backup.dat"))
            {
                backupData = (ScalabilityGroups)Shelper.DeserializeObj("Backup.dat");

                if (backupData == null)
                {
                    MessageBox.Show("Failed parsing Backup File.\nBackup file might be corrupted. Creating new backup from current settings.", "Notice");
                    Shelper.SerializeObj(_SG[0], "Backup.dat");
                    backupData = _SG[0];
                }
            }
            else // writes backup
            {
                if (Shelper.SerializeObj(_SG[0], "Backup.dat") == 1)
                {
                    backupData = _SG[0];
                }
                else
                {
                    MessageBox.Show("Something went wrong while keeping the backup.\nPlease restart the application.", "Notice");
                }
            }

            LoadGrid.Visibility = Visibility.Collapsed;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            ScalabilityGroups sg = new ScalabilityGroups();

            // 1st validation - try convert str to int
            try
            {
                sg.ResolutionQuality = Int32.Parse(ResqualityBox.Text);
                sg.ViewDistanceQuality = Int32.Parse(ViewDistanceQBox.Text);
                sg.AntiAliasingQuality = Int32.Parse(AntiAliasisBox.Text);
                sg.ShadowQuality = Int32.Parse(ShadowQBox.Text);
                sg.PostProcessQuality = Int32.Parse(PostProcessBox.Text);
                sg.TextureQuality = Int32.Parse(TextureQBox.Text);
                sg.EffectsQuality = Int32.Parse(EffectQBox.Text);
                sg.FoliageQuality = Int32.Parse(FoliageQBox.Text);
                sg.ShadingQuality = Int32.Parse(ShadingQBox.Text);
            }
            catch
            {
                MessageBox.Show("Invalid Data Entered.\nMake sure all fields have valid integer values and try again.", "Error");
                return;
            }

            // 2nd validation - value within range
            if (sg.ResolutionQuality < 0 || sg.ResolutionQuality > 100)
            {
                MessageBox.Show("Invalid Value for Resolution Quality.\nValue must be between 0 and 100", "Error");
                return;
            }

            if (sg.ViewDistanceQuality < 0 || sg.ViewDistanceQuality > 5)
            {
                MessageBox.Show("Invalid Value for View Distance Quality.\nValue must be between 0 and 5", "Error");
                return;
            }

            if (sg.AntiAliasingQuality < 0 || sg.AntiAliasingQuality > 5)
            {
                MessageBox.Show("Invalid Value for Anti-Aliasing Quality.\nValue must be between 0 and 5", "Error");
                return;
            }

            if (sg.ShadowQuality < 0 || sg.ShadowQuality > 5)
            {
                MessageBox.Show("Invalid Value for Shadow Quality.\nValue must be between 0 and 5", "Error");
                return;
            }

            if (sg.PostProcessQuality < 0 || sg.PostProcessQuality > 5)
            {
                MessageBox.Show("Invalid Value for Post-Process Quality.\nValue must be between 0 and 5", "Error");
                return;
            }

            if (sg.TextureQuality < 0 || sg.TextureQuality > 5)
            {
                MessageBox.Show("Invalid Value for Texture Quality.\nValue must be between 0 and 5", "Error");
                return;
            }

            if (sg.EffectsQuality < 0 || sg.EffectsQuality > 5)
            {
                MessageBox.Show("Invalid Value for Effects Quality.\nValue must be between 0 and 5", "Error");
                return;
            }

            if (sg.FoliageQuality < 0 || sg.FoliageQuality > 5)
            {
                MessageBox.Show("Invalid Value for Foliage Quality.\nValue must be between 0 and 5", "Error");
                return;
            }

            if (sg.ShadingQuality < 0 || sg.ShadingQuality > 5)
            {
                MessageBox.Show("Invalid Value for Shading Quality.\nValue must be between 0 and 5", "Error");
                return;
            }

            SaveChanges(sg);
            MessageBox.Show("Settings Successfully Changed!", "Done");
        }

        private void ApplyLowBtn_Click(object sender, RoutedEventArgs e)
        {
            ScalabilityGroups sg = new ScalabilityGroups
            {
                ResolutionQuality = 60,
                ViewDistanceQuality = 3,
                AntiAliasingQuality = 2,
                ShadowQuality = 3,
                PostProcessQuality = 3,
                TextureQuality = 0,
                EffectsQuality = 3,
                FoliageQuality = 3,
                ShadingQuality = 3
            };

            SaveChanges(sg);

            MessageBox.Show("Low Settings Applied.\nNB: This is the recommended minimum quality for low-end PCs while maintaining playability.", "Done");

            ResqualityBox.Text = sg.ResolutionQuality.ToString();
            ViewDistanceQBox.Text = sg.ViewDistanceQuality.ToString();
            AntiAliasisBox.Text = sg.AntiAliasingQuality.ToString();
            ShadowQBox.Text = sg.ShadowQuality.ToString();
            PostProcessBox.Text = sg.PostProcessQuality.ToString();
            TextureQBox.Text = sg.TextureQuality.ToString();
            EffectQBox.Text = sg.EffectsQuality.ToString();
            FoliageQBox.Text = sg.FoliageQuality.ToString();
            ShadingQBox.Text = sg.ShadingQuality.ToString();
        }

        private void resDefBtn_Click(object sender, RoutedEventArgs e)
        {
            ScalabilityGroups sg = new ScalabilityGroups
            {
                ResolutionQuality = backupData.ResolutionQuality,
                ViewDistanceQuality = backupData.ViewDistanceQuality,
                AntiAliasingQuality = backupData.AntiAliasingQuality,
                ShadowQuality = backupData.ShadowQuality,
                PostProcessQuality = backupData.PostProcessQuality,
                TextureQuality = backupData.TextureQuality,
                EffectsQuality = backupData.EffectsQuality,
                FoliageQuality = backupData.FoliageQuality,
                ShadingQuality = backupData.ShadingQuality
            };

            SaveChanges(sg);

            MessageBox.Show("Default values restored successfully.", "Done");

            ResqualityBox.Text = sg.ResolutionQuality.ToString();
            ViewDistanceQBox.Text = sg.ViewDistanceQuality.ToString();
            AntiAliasisBox.Text = sg.AntiAliasingQuality.ToString();
            ShadowQBox.Text = sg.ShadowQuality.ToString();
            PostProcessBox.Text = sg.PostProcessQuality.ToString();
            TextureQBox.Text = sg.TextureQuality.ToString();
            EffectQBox.Text = sg.EffectsQuality.ToString();
            FoliageQBox.Text = sg.FoliageQuality.ToString();
            ShadingQBox.Text = sg.ShadingQuality.ToString();
        }

        // helper method to save ini
        private void SaveChanges(ScalabilityGroups sg)
        {
            try
            {
                for (int i = 0; i < _iniList.Count; i++)
                {
                    if (_iniList[i]["ScalabilityGroups"] == null)
                    {
                        _iniList[i].Sections.AddSection("ScalabilityGroups");
                    }

                    _iniList[i]["ScalabilityGroups"]["sg.ResolutionQuality"] = sg.ResolutionQuality + ".000000";
                    _iniList[i]["ScalabilityGroups"]["sg.ViewDistanceQuality"] = sg.ViewDistanceQuality.ToString();
                    _iniList[i]["ScalabilityGroups"]["sg.AntiAliasingQuality"] = sg.AntiAliasingQuality.ToString();
                    _iniList[i]["ScalabilityGroups"]["sg.ShadowQuality"] = sg.ShadowQuality.ToString();
                    _iniList[i]["ScalabilityGroups"]["sg.PostProcessQuality"] = sg.PostProcessQuality.ToString();
                    _iniList[i]["ScalabilityGroups"]["sg.TextureQuality"] = sg.TextureQuality.ToString();
                    _iniList[i]["ScalabilityGroups"]["sg.EffectsQuality"] = sg.EffectsQuality.ToString();
                    _iniList[i]["ScalabilityGroups"]["sg.FoliageQuality"] = sg.FoliageQuality.ToString();
                    _iniList[i]["ScalabilityGroups"]["sg.ShadingQuality"] = sg.ShadingQuality.ToString();

                    string filePath = _fnames[i];
                    if (File.Exists(filePath))
                    {
                        var attr = File.GetAttributes(filePath);

                        // unset read-only before writing
                        if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            File.SetAttributes(filePath, attr & ~FileAttributes.ReadOnly);
                        }

                        parser.WriteFile(filePath, _iniList[i]);

                        // set read-only to prevent game from reverting custom resolution scale
                        File.SetAttributes(filePath, attr | FileAttributes.ReadOnly);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong while saving settings: " + ex.Message, "Error");
                return;
            }
        }
    }
}

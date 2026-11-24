using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private List<string> _fnames = new List<string>();
        private List<ScalabilityGroups> _SG = new List<ScalabilityGroups>();
        private ScalabilityGroups backupData = new ScalabilityGroups();

        // ini parser
        private FileIniDataParser parser = new FileIniDataParser();
        private List<IniData> _iniList = new List<IniData>();

        private string BackupFilePath
        {
            get
            {
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup.dat");
            }
        }

        public Home(List<string> fl)
        {
            InitializeComponent();

            _fnames = fl ?? new List<string>();

            Loc.OnLanguageChanged += ApplyLocalization;
            ApplyLocalization();

            StartUP();
        }

        private void ApplyLocalization()
        {
            if (Loc.IsVi)
            {
                TotalAccountsLbl.Text = "Tổng số tài khoản";
                SaveBtn.Content = "Lưu thay đổi";
                SaveBtn.ToolTip = "Lưu thiết lập vào tất cả tài khoản";
                ApplyLowBtn.Content = "Tối ưu FPS (Low)";
                ApplyLowBtn.ToolTip = "Cài đặt cấu hình thấp nhất để tối đa hóa FPS";
                ResDefBtnText.Text = "Khôi phục gốc";
                resDefBtn.ToolTip = "Khôi phục tất cả tài khoản về cài đặt gốc và mở khóa file";
                LoadTxt.Text = "Đang tải dữ liệu, vui lòng đợi...";
                ValScaleHintTxt.Text = "• Mức đồ họa: 0 = Thấp nhất (Low), 1 = Trung bình, 2 = Cao, 3 = Tối đa (Ultra) | Độ phân giải: 0 - 100%";
            }
            else
            {
                TotalAccountsLbl.Text = "Total Accounts";
                SaveBtn.Content = "Save Changes";
                SaveBtn.ToolTip = "Save custom settings to all accounts";
                ApplyLowBtn.Content = "Apply Low (FPS)";
                ApplyLowBtn.ToolTip = "Optimal low settings for maximum FPS";
                ResDefBtnText.Text = "Rollback Default";
                resDefBtn.ToolTip = "Rollback all accounts to original settings and unlock files";
                LoadTxt.Text = "Loading Data, Please wait...";
                ValScaleHintTxt.Text = "• Graphics scale: 0 = Low, 1 = Medium, 2 = High, 3 = Ultra | Resolution Quality: 0 - 100%";
            }
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

        private bool IsValorantRunning()
        {
            try
            {
                Process[] procs = Process.GetProcessesByName("VALORANT-Win64-Shipping");
                if (procs != null && procs.Length > 0) return true;

                Process[] procs2 = Process.GetProcessesByName("VALORANT");
                return procs2 != null && procs2.Length > 0;
            }
            catch
            {
                return false;
            }
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
                    sg.GlobalIlluminationQuality = ParseIniInt(sgSection["sg.GlobalIlluminationQuality"], 3);
                    sg.ReflectionQuality = ParseIniInt(sgSection["sg.ReflectionQuality"], 3);
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
                MessageBox.Show("Không thể đọc file cấu hình. Hãy đảm bảo VALORANT đã được khởi chạy ít nhất một lần.", "Lỗi");
                Application.Current.Shutdown();
                return;
            }
            else
            {
                // set the first element's data to the view 
                ResqualityBox.Text = _SG[0].ResolutionQuality.ToString();
                ViewDistanceQBox.Text = _SG[0].ViewDistanceQuality.ToString();
                AntiAliasisBox.Text = _SG[0].AntiAliasingQuality.ToString();
                ShadowQBox.Text = _SG[0].ShadowQuality.ToString();
                PostProcessBox.Text = _SG[0].PostProcessQuality.ToString();
                TextureQBox.Text = _SG[0].TextureQuality.ToString();
                EffectQBox.Text = _SG[0].EffectsQuality.ToString();
                FoliageQBox.Text = _SG[0].FoliageQuality.ToString();
                ShadingQBox.Text = _SG[0].ShadingQuality.ToString();
                GIQualityBox.Text = _SG[0].GlobalIlluminationQuality.ToString();
                ReflectionQBox.Text = _SG[0].ReflectionQuality.ToString();
            }

            // check backup 
            string backupPath = BackupFilePath;
            if (File.Exists(backupPath))
            {
                backupData = (ScalabilityGroups)Shelper.DeserializeObj(backupPath);

                if (backupData == null)
                {
                    Shelper.SerializeObj(_SG[0], backupPath);
                    backupData = _SG[0];
                }
            }
            else // writes backup
            {
                if (Shelper.SerializeObj(_SG[0], backupPath) == 1)
                {
                    backupData = _SG[0];
                }
            }

            LoadGrid.Visibility = Visibility.Collapsed;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsValorantRunning())
            {
                var answer = MessageBox.Show(
                    Loc.T("VALORANT hiện đang chạy!\n\nVui lòng đóng VALORANT trước khi lưu để tránh bị game ghi đè mất cấu hình khi thoát.\n\nBạn có muốn tiếp tục lưu không?",
                          "VALORANT is currently running!\n\nPlease close VALORANT before applying changes, otherwise the game may overwrite them upon closing.\n\nDo you want to save anyway?"),
                    Loc.T("Cảnh báo: VALORANT đang chạy", "VALORANT Running"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (answer != MessageBoxResult.Yes) return;
            }

            ScalabilityGroups sg = new ScalabilityGroups();

            // 1st validation - try convert str to int
            try
            {
                sg.ResolutionQuality = Int32.Parse(ResqualityBox.Text.Trim());
                sg.ViewDistanceQuality = Int32.Parse(ViewDistanceQBox.Text.Trim());
                sg.AntiAliasingQuality = Int32.Parse(AntiAliasisBox.Text.Trim());
                sg.ShadowQuality = Int32.Parse(ShadowQBox.Text.Trim());
                sg.PostProcessQuality = Int32.Parse(PostProcessBox.Text.Trim());
                sg.TextureQuality = Int32.Parse(TextureQBox.Text.Trim());
                sg.EffectsQuality = Int32.Parse(EffectQBox.Text.Trim());
                sg.FoliageQuality = Int32.Parse(FoliageQBox.Text.Trim());
                sg.ShadingQuality = Int32.Parse(ShadingQBox.Text.Trim());
                sg.GlobalIlluminationQuality = Int32.Parse(GIQualityBox.Text.Trim());
                sg.ReflectionQuality = Int32.Parse(ReflectionQBox.Text.Trim());
            }
            catch
            {
                MessageBox.Show(
                    Loc.T("Dữ liệu nhập không hợp lệ.\nVui lòng đảm bảo tất cả các ô là số nguyên hợp lệ.",
                          "Invalid Data Entered.\nMake sure all fields have valid integer numbers and try again."),
                    Loc.T("Lỗi", "Error"));
                return;
            }

            // 2nd validation - value within range
            if (sg.ResolutionQuality < 0 || sg.ResolutionQuality > 100)
            {
                MessageBox.Show(Loc.T("Giá trị Resolution Quality không hợp lệ (0-100).", "Invalid Value for Resolution Quality (0-100)."), Loc.T("Lỗi", "Error"));
                return;
            }

            if (sg.ViewDistanceQuality < 0 || sg.ViewDistanceQuality > 5)
            {
                MessageBox.Show(Loc.T("Giá trị View Distance Quality không hợp lệ (0-5).", "Invalid Value for View Distance Quality (0-5)."), Loc.T("Lỗi", "Error"));
                return;
            }

            if (sg.AntiAliasingQuality < 0 || sg.AntiAliasingQuality > 5)
            {
                MessageBox.Show(Loc.T("Giá trị Anti-Aliasing Quality không hợp lệ (0-5).", "Invalid Value for Anti-Aliasing Quality (0-5)."), Loc.T("Lỗi", "Error"));
                return;
            }

            if (sg.ShadowQuality < 0 || sg.ShadowQuality > 5)
            {
                MessageBox.Show(Loc.T("Giá trị Shadow Quality không hợp lệ (0-5).", "Invalid Value for Shadow Quality (0-5)."), Loc.T("Lỗi", "Error"));
                return;
            }

            if (sg.PostProcessQuality < 0 || sg.PostProcessQuality > 5)
            {
                MessageBox.Show(Loc.T("Giá trị Post-Process Quality không hợp lệ (0-5).", "Invalid Value for Post-Process Quality (0-5)."), Loc.T("Lỗi", "Error"));
                return;
            }

            if (sg.TextureQuality < 0 || sg.TextureQuality > 5)
            {
                MessageBox.Show(Loc.T("Giá trị Texture Quality không hợp lệ (0-5).", "Invalid Value for Texture Quality (0-5)."), Loc.T("Lỗi", "Error"));
                return;
            }

            if (sg.EffectsQuality < 0 || sg.EffectsQuality > 5)
            {
                MessageBox.Show(Loc.T("Giá trị Effects Quality không hợp lệ (0-5).", "Invalid Value for Effects Quality (0-5)."), Loc.T("Lỗi", "Error"));
                return;
            }

            if (sg.FoliageQuality < 0 || sg.FoliageQuality > 5)
            {
                MessageBox.Show(Loc.T("Giá trị Foliage Quality không hợp lệ (0-5).", "Invalid Value for Foliage Quality (0-5)."), Loc.T("Lỗi", "Error"));
                return;
            }

            if (sg.ShadingQuality < 0 || sg.ShadingQuality > 5)
            {
                MessageBox.Show(Loc.T("Giá trị Shading Quality không hợp lệ (0-5).", "Invalid Value for Shading Quality (0-5)."), Loc.T("Lỗi", "Error"));
                return;
            }

            if (sg.GlobalIlluminationQuality < 0 || sg.GlobalIlluminationQuality > 5)
            {
                MessageBox.Show(Loc.T("Giá trị Global Illumination Quality không hợp lệ (0-5).", "Invalid Value for Global Illumination Quality (0-5)."), Loc.T("Lỗi", "Error"));
                return;
            }

            if (sg.ReflectionQuality < 0 || sg.ReflectionQuality > 5)
            {
                MessageBox.Show(Loc.T("Giá trị Reflection Quality không hợp lệ (0-5).", "Invalid Value for Reflection Quality (0-5)."), Loc.T("Lỗi", "Error"));
                return;
            }

            SaveChanges(sg);
            MessageBox.Show(
                Loc.T("Đã lưu thiết lập thành công!\nFile cấu hình đã được khóa Read-Only để chống game tự reset.",
                      "Settings Successfully Changed!\nConfig files locked as Read-Only to prevent reset."),
                Loc.T("Hoàn tất", "Done"));
        }

        private void ApplyLowBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsValorantRunning())
            {
                var answer = MessageBox.Show(
                    Loc.T("VALORANT hiện đang chạy!\n\nVui lòng đóng VALORANT trước khi áp dụng cấu hình.\n\nBạn có muốn tiếp tục áp dụng không?",
                          "VALORANT is currently running!\n\nPlease close VALORANT before applying changes.\n\nDo you want to apply anyway?"),
                    Loc.T("Cảnh báo: VALORANT đang chạy", "VALORANT Running"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (answer != MessageBoxResult.Yes) return;
            }

            // Maximum FPS competitive preset
            ScalabilityGroups sg = new ScalabilityGroups
            {
                ResolutionQuality = 60,
                ViewDistanceQuality = 1,
                AntiAliasingQuality = 0,
                ShadowQuality = 0,
                PostProcessQuality = 0,
                TextureQuality = 0,
                EffectsQuality = 0,
                FoliageQuality = 0,
                ShadingQuality = 0,
                GlobalIlluminationQuality = 0,
                ReflectionQuality = 0
            };

            SaveChanges(sg);

            MessageBox.Show(
                Loc.T("Đã áp dụng cấu hình tối ưu FPS thành công!\nResolution Quality đặt về 60% và toàn bộ đồ họa ở mức thấp nhất cho tất cả tài khoản.",
                      "Low Settings Applied Successfully!\nResolution set to 60% and all graphics options set to Low for maximum FPS across all accounts."),
                Loc.T("Hoàn tất", "Done"));

            ResqualityBox.Text = sg.ResolutionQuality.ToString();
            ViewDistanceQBox.Text = sg.ViewDistanceQuality.ToString();
            AntiAliasisBox.Text = sg.AntiAliasingQuality.ToString();
            ShadowQBox.Text = sg.ShadowQuality.ToString();
            PostProcessBox.Text = sg.PostProcessQuality.ToString();
            TextureQBox.Text = sg.TextureQuality.ToString();
            EffectQBox.Text = sg.EffectsQuality.ToString();
            FoliageQBox.Text = sg.FoliageQuality.ToString();
            ShadingQBox.Text = sg.ShadingQuality.ToString();
            GIQualityBox.Text = sg.GlobalIlluminationQuality.ToString();
            ReflectionQBox.Text = sg.ReflectionQuality.ToString();
        }

        private void resDefBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsValorantRunning())
            {
                var answer = MessageBox.Show(
                    Loc.T("VALORANT hiện đang chạy!\n\nVui lòng đóng VALORANT trước khi khôi phục thiết lập.\n\nBạn có muốn tiếp tục không?",
                          "VALORANT is currently running!\n\nPlease close VALORANT before restoring settings.\n\nDo you want to restore anyway?"),
                    Loc.T("Cảnh báo: VALORANT đang chạy", "VALORANT Running"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (answer != MessageBoxResult.Yes) return;
            }

            ScalabilityGroups sg = new ScalabilityGroups();
            if (backupData != null)
            {
                sg.ResolutionQuality = backupData.ResolutionQuality;
                sg.ViewDistanceQuality = backupData.ViewDistanceQuality;
                sg.AntiAliasingQuality = backupData.AntiAliasingQuality;
                sg.ShadowQuality = backupData.ShadowQuality;
                sg.PostProcessQuality = backupData.PostProcessQuality;
                sg.TextureQuality = backupData.TextureQuality;
                sg.EffectsQuality = backupData.EffectsQuality;
                sg.FoliageQuality = backupData.FoliageQuality;
                sg.ShadingQuality = backupData.ShadingQuality;
                sg.GlobalIlluminationQuality = backupData.GlobalIlluminationQuality;
                sg.ReflectionQuality = backupData.ReflectionQuality;
            }
            else
            {
                // Official Default Valorant Settings
                sg.ResolutionQuality = 100;
                sg.ViewDistanceQuality = 3;
                sg.AntiAliasingQuality = 3;
                sg.ShadowQuality = 3;
                sg.PostProcessQuality = 3;
                sg.TextureQuality = 3;
                sg.EffectsQuality = 3;
                sg.FoliageQuality = 3;
                sg.ShadingQuality = 3;
                sg.GlobalIlluminationQuality = 3;
                sg.ReflectionQuality = 3;
            }

            // Save and unlock Read-Only so game can manage settings normally
            SaveChanges(sg, true);

            MessageBox.Show(
                Loc.T("Đã khôi phục cài đặt gốc thành công!\n\nTất cả tài khoản đã được đưa về cấu hình ban đầu và file đã được mở khóa (bỏ Read-Only).",
                      "Default values restored successfully!\n\nAll accounts have been reverted to original defaults, and config files have been unlocked (Read-Only removed)."),
                Loc.T("Khôi phục hoàn tất", "Rollback Successful"),
                MessageBoxButton.OK);

            ResqualityBox.Text = sg.ResolutionQuality.ToString();
            ViewDistanceQBox.Text = sg.ViewDistanceQuality.ToString();
            AntiAliasisBox.Text = sg.AntiAliasingQuality.ToString();
            ShadowQBox.Text = sg.ShadowQuality.ToString();
            PostProcessBox.Text = sg.PostProcessQuality.ToString();
            TextureQBox.Text = sg.TextureQuality.ToString();
            EffectQBox.Text = sg.EffectsQuality.ToString();
            FoliageQBox.Text = sg.FoliageQuality.ToString();
            ShadingQBox.Text = sg.ShadingQuality.ToString();
            GIQualityBox.Text = sg.GlobalIlluminationQuality.ToString();
            ReflectionQBox.Text = sg.ReflectionQuality.ToString();
        }

        // helper method to save ini
        private void SaveChanges(ScalabilityGroups sg, bool unlockReadOnly = false)
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
                    _iniList[i]["ScalabilityGroups"]["sg.GlobalIlluminationQuality"] = sg.GlobalIlluminationQuality.ToString();
                    _iniList[i]["ScalabilityGroups"]["sg.ReflectionQuality"] = sg.ReflectionQuality.ToString();

                    if (i < _SG.Count)
                    {
                        _SG[i].ResolutionQuality = sg.ResolutionQuality;
                        _SG[i].ViewDistanceQuality = sg.ViewDistanceQuality;
                        _SG[i].AntiAliasingQuality = sg.AntiAliasingQuality;
                        _SG[i].ShadowQuality = sg.ShadowQuality;
                        _SG[i].PostProcessQuality = sg.PostProcessQuality;
                        _SG[i].TextureQuality = sg.TextureQuality;
                        _SG[i].EffectsQuality = sg.EffectsQuality;
                        _SG[i].FoliageQuality = sg.FoliageQuality;
                        _SG[i].ShadingQuality = sg.ShadingQuality;
                        _SG[i].GlobalIlluminationQuality = sg.GlobalIlluminationQuality;
                        _SG[i].ReflectionQuality = sg.ReflectionQuality;
                    }

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

                        if (unlockReadOnly)
                        {
                            // Keep unlocked (remove Read-Only) so game can freely edit settings
                            File.SetAttributes(filePath, attr & ~FileAttributes.ReadOnly);
                        }
                        else
                        {
                            // Set Read-Only to prevent game from reverting custom resolution scale
                            File.SetAttributes(filePath, attr | FileAttributes.ReadOnly);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra khi lưu thiết lập: " + ex.Message, "Lỗi");
                return;
            }
        }
    }
}

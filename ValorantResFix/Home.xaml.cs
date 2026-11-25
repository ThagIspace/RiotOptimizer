using IniParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        internal const string ConfigName = "GameUserSettings.ini";
        internal const string BackupKind = "valorant";

        private readonly List<string> _fnames;
        private readonly FileIniDataParser parser = new FileIniDataParser();

        private static string GetBackupPath(string path)
        {
            return ConfigFiles.BackupPath(path, BackupKind);
        }

        public Home(List<string> fl)
        {
            InitializeComponent();

            _fnames = fl.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

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
                ResDefBtnText.Text = "Khôi phục bản sao lưu";
                BrowseBtn.Content = "Chọn file...";
                resDefBtn.ToolTip = "Khôi phục các file có bản sao lưu và mở khóa file";
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
                ResDefBtnText.Text = "Restore Backup";
                BrowseBtn.Content = "Browse...";
                resDefBtn.ToolTip = "Restore and unlock configs with backups";
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
            var failedFiles = new List<string>();
            var loaded = new List<ScalabilityGroups>();
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

                    loaded.Add(sg);
                }
                catch (Exception ex)
                {
                    failedFiles.Add(s + ": " + ex.Message);
                }
            }

            TotalFileTxt.Text = loaded.Count.ToString();
            SaveBtn.IsEnabled = loaded.Count > 0;
            ApplyLowBtn.IsEnabled = loaded.Count > 0;
            resDefBtn.IsEnabled = _fnames.Count > 0;
            if (loaded.Count > 0)
                ShowSettings(loaded[0]);

            if (failedFiles.Count > 0)
                MessageBox.Show(
                    Loc.T("Chưa đọc được các file sau. Bạn vẫn có thể khôi phục file có bản sao lưu:\n",
                          "The following files could not be read. You can still restore files with a backup:\n") + string.Join("\n", failedFiles),
                    Loc.T("Cảnh báo", "Warning"));

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

            if (!SaveChanges(sg)) return;
            MessageBox.Show(
                Loc.T("Đã lưu thiết lập cho các file đọc được!\nFile cấu hình đã được khóa Read-Only để chống game tự reset.",
                      "Settings saved for readable files!\nConfig files locked as Read-Only to prevent reset."),
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

            if (!SaveChanges(sg)) return;

            MessageBox.Show(
                Loc.T("Đã áp dụng cấu hình tối ưu FPS thành công!\nResolution Quality đặt về 60% và toàn bộ đồ họa ở mức thấp nhất cho các file đọc được.",
                      "Low Settings Applied Successfully!\nResolution set to 60% and all graphics options set to Low for maximum FPS across readable files."),
                Loc.T("Hoàn tất", "Done"));

            ShowSettings(sg);
        }

        private void ShowSettings(ScalabilityGroups sg)
        {
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

            var missing = new List<string>();
            var available = new List<string>();
            foreach (string path in _fnames)
            {
                (File.Exists(GetBackupPath(path)) ? available : missing).Add(path);
            }
            if (available.Count == 0)
            {
                string warningText = Loc.T("Không có bản sao lưu cho các file đã chọn.", "No backups exist for the selected files.");
                if (HasLegacyBackup())
                    warningText += "\n\n" + LegacyBackupNotice();
                MessageBox.Show(warningText, Loc.T("Lỗi", "Error"));
                return;
            }
            string warning = Loc.T(
                "Khôi phục sẽ ghi đè toàn bộ file cấu hình bằng bản sao lưu. Các thay đổi khác trong game sau khi sao lưu sẽ bị mất.",
                "Restoring replaces the entire configuration file with its backup. Other in-game changes made after the backup will be lost.");
            warning += Loc.T("\n\nCác file sẽ được khôi phục:\n", "\n\nFiles to restore:\n") + string.Join("\n", available);
            if (missing.Count > 0)
                warning += Loc.T("\n\nCác file không có bản sao lưu sẽ được bỏ qua:\n", "\n\nFiles without backups will be skipped:\n") + string.Join("\n", missing);
            if (MessageBox.Show(warning, Loc.T("Xác nhận khôi phục", "Confirm restore"), MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            var errors = new List<string>();
            int restored = 0;
            foreach (string path in available)
            {
                try
                {
                    ConfigFiles.Restore(path, GetBackupPath(path));
                    restored++;
                }
                catch (Exception ex)
                {
                    errors.Add(path + ": " + ex.Message);
                }
            }
            StartUP();
            string result = Loc.T("Đã khôi phục: ", "Restored: ") + restored + "/" + available.Count;
            if (missing.Count > 0) result += Loc.T("\nKhông có bản sao lưu: ", "\nMissing backups: ") + missing.Count;
            if (errors.Count > 0) result += Loc.T("\nLỗi:\n", "\nErrors:\n") + string.Join("\n", errors);
            MessageBox.Show(result, Loc.T("Kết quả khôi phục", "Restore result"));
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = ConfigFiles.Browse(ConfigName, BackupKind);
                if (path == null) return;
                if (!_fnames.Contains(path, StringComparer.OrdinalIgnoreCase))
                    _fnames.Add(path);
                StartUP();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Loc.T("Lỗi khi mở file: ", "Error opening file: ") + ex.Message, Loc.T("Lỗi", "Error"));
            }
        }

        private static bool HasLegacyBackup()
        {
            return File.Exists("Backup.dat") ||
                   File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup.dat"));
        }

        private static string LegacyBackupNotice()
        {
            return Loc.T(
                "Đã tìm thấy Backup.dat của bản cũ. Nếu đã tối ưu bằng bản cũ, hãy dùng bản cũ để khôi phục trên đúng cấu hình trước khi lưu bằng bản này. Bản này sẽ sao lưu nội dung hiện có của từng file.",
                "A legacy Backup.dat was found. If you applied settings with the old version, use that version to restore the matching config before saving here. This version backs up each file's current contents.");
        }

        private bool SaveChanges(ScalabilityGroups sg)
        {
            if (HasLegacyBackup() && _fnames.Any(path => File.Exists(path) && !File.Exists(GetBackupPath(path))))
            {
                if (MessageBox.Show(LegacyBackupNotice(), Loc.T("Tiếp tục lưu?", "Continue saving?"),
                    MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
                    return false;
            }

            var saved = new List<string>();
            var errors = new List<string>();
            foreach (string path in _fnames)
            {
                try
                {
                    var data = parser.ReadFile(path);
                    var settings = data["ScalabilityGroups"];
                    if (settings == null)
                    {
                        data.Sections.AddSection("ScalabilityGroups");
                        settings = data["ScalabilityGroups"];
                    }

                    settings["sg.ResolutionQuality"] = sg.ResolutionQuality + ".000000";
                    settings["sg.ViewDistanceQuality"] = sg.ViewDistanceQuality.ToString();
                    settings["sg.AntiAliasingQuality"] = sg.AntiAliasingQuality.ToString();
                    settings["sg.ShadowQuality"] = sg.ShadowQuality.ToString();
                    settings["sg.PostProcessQuality"] = sg.PostProcessQuality.ToString();
                    settings["sg.TextureQuality"] = sg.TextureQuality.ToString();
                    settings["sg.EffectsQuality"] = sg.EffectsQuality.ToString();
                    settings["sg.FoliageQuality"] = sg.FoliageQuality.ToString();
                    settings["sg.ShadingQuality"] = sg.ShadingQuality.ToString();
                    settings["sg.GlobalIlluminationQuality"] = sg.GlobalIlluminationQuality.ToString();
                    settings["sg.ReflectionQuality"] = sg.ReflectionQuality.ToString();

                    ConfigFiles.Save(path, GetBackupPath(path), temp => parser.WriteFile(temp, data));
                    saved.Add(path);
                }
                catch (Exception ex)
                {
                    errors.Add(path + ": " + ex.Message);
                }
            }

            if (errors.Count == 0) return true;
            string result = Loc.T("Đã lưu và khóa: ", "Saved and locked: ") + saved.Count + "/" + _fnames.Count;
            if (saved.Count > 0) result += "\n" + string.Join("\n", saved);
            result += Loc.T("\n\nCó lỗi:\n", "\n\nErrors:\n") + string.Join("\n", errors);
            MessageBox.Show(result, Loc.T("Kết quả lưu", "Save result"));
            return false;
        }
    }
}

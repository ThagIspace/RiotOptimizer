using IniParser;
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

namespace ValorantResFix
{
    /// <summary>
    /// Interaction logic for LoLPage.xaml
    /// </summary>
    public partial class LoLPage : Page
    {
        private const string ConfigName = "game.cfg";
        private const string BackupKind = "lol";
        private string _cfgPath;
        private readonly FileIniDataParser _parser = new FileIniDataParser();

        private static string GetBackupPath(string path)
        {
            return ConfigFiles.BackupPath(path, BackupKind);
        }

        public LoLPage()
        {
            InitializeComponent();

            // LoL game.cfg uses Windows ANSI encoding — must set parser encoding
            // to avoid corrupting non-ASCII characters when writing back the file
            _parser.Parser.Configuration.SkipInvalidLines = true;
            _parser.Parser.Configuration.AllowDuplicateKeys = true;
            _parser.Parser.Configuration.OverrideDuplicateKeys = true;

            Loc.OnLanguageChanged += ApplyLocalization;
            ApplyLocalization();

            StartUP();
        }

        private void ApplyLocalization()
        {
            if (Loc.IsVi)
            {
                CfgPathHeader.Text = "Đường dẫn game.cfg:";
                BrowseLoLBtn.Content = "Chọn file...";
                DispModeLbl.Text = "Chế độ hiển thị";
                ShadowQLbl.Text = "Đổ bóng (0 - 4)";
                CharQLbl.Text = "Chi tiết tướng (0 - 4)";
                EnvQLbl.Text = "Chi tiết bản đồ (0 - 4)";
                EffectsQLbl.Text = "Chi tiết hiệu ứng (0 - 4)";

                ComboFullscreen.Content = "Toàn màn hình";
                ComboBorderless.Content = "Không viền";
                ComboWindowed.Content = "Cửa sổ";

                DisableGodraysCheck.Content = "Tắt tia nắng mặt trời (ShowGodray=0)";
                HideEyeCandyCheck.Content = "Tắt sinh vật cảnh trên sông (HideEyeCandy=1)";
                DisableHUDAnimCheck.Content = "Tắt chuyển động thanh máu (EnableHUDAnim=0)";
                DisableFXAACheck.Content = "Tắt khử răng cưa (EnableFXAA=0)";
                DisableEternalsCheck.Content = "Tắt thông báo Cột Mốc Thần Thoại (Chống khựng)";
                DisableVSyncCheck.Content = "Tắt V-Sync (Giảm trễ chuột tối đa)";

                SaveBtn.Content = "Lưu thay đổi";
                SaveBtn.ToolTip = "Lưu các thiết lập vào game.cfg";
                ApplyLowBtn.Content = "Tối ưu FPS (Low)";
                ApplyLowBtn.ToolTip = "Tự động tắt toàn bộ hiệu ứng nặng để đạt FPS tối đa";
                ResDefBtnLoLText.Text = "Khôi phục bản sao lưu";
                resDefBtn.ToolTip = "Khôi phục bản sao lưu game.cfg và mở khóa file";
                LoLScaleHintTxt.Text = "• Mức đồ họa: 0 = Rất thấp (Tắt), 1 = Thấp, 2 = Trung bình, 3 = Cao, 4 = Rất cao";
            }
            else
            {
                CfgPathHeader.Text = "game.cfg path:";
                BrowseLoLBtn.Content = "Browse...";
                DispModeLbl.Text = "Display Mode";
                ShadowQLbl.Text = "Shadow Quality (0 - 4)";
                CharQLbl.Text = "Character Quality (0 - 4)";
                EnvQLbl.Text = "Environment Quality (0 - 4)";
                EffectsQLbl.Text = "Effects Quality (0 - 4)";

                ComboFullscreen.Content = "Fullscreen";
                ComboBorderless.Content = "Borderless";
                ComboWindowed.Content = "Windowed";

                DisableGodraysCheck.Content = "Disable Sun Godrays (ShowGodray=0)";
                HideEyeCandyCheck.Content = "Hide River & Map EyeCandy (HideEyeCandy=1)";
                DisableHUDAnimCheck.Content = "Disable HUD Health Animations (EnableHUDAnim=0)";
                DisableFXAACheck.Content = "Disable Anti-Aliasing (EnableFXAA=0)";
                DisableEternalsCheck.Content = "Disable Eternals Milestones (Prevents Hitching)";
                DisableVSyncCheck.Content = "Disable V-Sync (Lowest Input Latency)";

                SaveBtn.Content = "Save Changes";
                SaveBtn.ToolTip = "Save settings to game.cfg";
                ApplyLowBtn.Content = "Apply Low (FPS)";
                ApplyLowBtn.ToolTip = "Automatically disable heavy graphics for maximum FPS";
                ResDefBtnLoLText.Text = "Restore Backup";
                resDefBtn.ToolTip = "Restore and unlock game.cfg from its backup";
                LoLScaleHintTxt.Text = "• Graphics scale: 0 = Very Low (Off), 1 = Low, 2 = Medium, 3 = High, 4 = Very High";
            }
        }

        public void StartUP()
        {
            string path = FindLoLConfigPath();
            if (path != null)
            {
                LoadConfig(path);
            }
            else
            {
                _cfgPath = null;
                SaveBtn.IsEnabled = false;
                ApplyLowBtn.IsEnabled = false;
                resDefBtn.IsEnabled = false;
                PathBox.Text = Loc.T("Không tự động tìm thấy game.cfg. Vui lòng bấm 'Chọn file...'", "Could not find game.cfg automatically. Please click 'Browse...'");
            }
        }

        public static string FindLoLConfigPath()
        {
            // 1. Registry detection
            try
            {
                string[] regKeys = new string[]
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                    @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
                };

                foreach (var rk in regKeys)
                {
                    using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(rk))
                    {
                        if (key != null)
                        {
                            foreach (var sub in key.GetSubKeyNames())
                            {
                                using (var subKey = key.OpenSubKey(sub))
                                {
                                    if (subKey != null)
                                    {
                                        string name = subKey.GetValue("DisplayName") as string;
                                        string path = subKey.GetValue("InstallLocation") as string;
                                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(path))
                                        {
                                            if (name.Contains("League of Legends") || name.Contains("Liên Minh Huyền Thoại"))
                                            {
                                                string cfg = System.IO.Path.Combine(path, "Config", "game.cfg");
                                                if (File.Exists(cfg)) return cfg;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch {}

            // 2. Scan common paths across all fixed drives
            try
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        string root = drive.RootDirectory.FullName;
                        string[] candidates = new string[]
                        {
                            System.IO.Path.Combine(root, "Games", "Riot Games", "League of Legends", "Config", "game.cfg"),
                            System.IO.Path.Combine(root, "Riot Games", "League of Legends", "Config", "game.cfg"),
                            System.IO.Path.Combine(root, "League of Legends", "Config", "game.cfg"),
                            System.IO.Path.Combine(root, "Program Files", "Riot Games", "League of Legends", "Config", "game.cfg"),
                            System.IO.Path.Combine(root, "Program Files (x86)", "Riot Games", "League of Legends", "Config", "game.cfg")
                        };

                        foreach (var cand in candidates)
                        {
                            if (File.Exists(cand)) return cand;
                        }
                    }
                }
            }
            catch {}

            return null;
        }

        private int ParseInt(string val, int defaultVal)
        {
            if (string.IsNullOrWhiteSpace(val)) return defaultVal;
            decimal res;
            if (decimal.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out res))
            {
                return (int)res;
            }
            return defaultVal;
        }

        private bool LoadConfig(string path)
        {
            _cfgPath = path;
            PathBox.Text = path;
            SaveBtn.IsEnabled = false;
            ApplyLowBtn.IsEnabled = false;
            resDefBtn.IsEnabled = true;
            if (!File.Exists(path) && File.Exists(GetBackupPath(path)))
                return false;

            try
            {
                var loadedData = _parser.ReadFile(path, Encoding.Default);

                // Performance
                var perf = loadedData["Performance"];
                int shadow = perf != null ? ParseInt(perf["ShadowQuality"], 0) : 0;
                int character = perf != null ? ParseInt(perf["CharacterQuality"], 1) : 1;
                int env = perf != null ? ParseInt(perf["EnvironmentQuality"], 1) : 1;
                int fx = perf != null ? ParseInt(perf["EffectsQuality"], 1) : 1;
                int fxaa = perf != null ? ParseInt(perf["EnableFXAA"], 0) : 0;
                int hudAnim = perf != null ? ParseInt(perf["EnableHUDAnimations"], 0) : 0;

                ShadowQBox.Text = shadow.ToString();
                CharacterQBox.Text = character.ToString();
                EnvironmentQBox.Text = env.ToString();
                EffectsQBox.Text = fx.ToString();

                DisableFXAACheck.IsChecked = (fxaa == 0);
                DisableHUDAnimCheck.IsChecked = (hudAnim == 0);

                // General
                var general = loadedData["General"];
                int winMode = general != null ? ParseInt(general["WindowMode"], 0) : 0;
                int vsync = general != null ? ParseInt(general["WaitForVerticalSync"], 0) : 0;
                int godray = general != null ? ParseInt(general["ShowGodray"], 0) : 0;
                int eyecandy = general != null ? ParseInt(general["HideEyeCandy"], 1) : 1;

                // Select WindowMode in ComboBox
                if (winMode == 0) WindowModeCombo.SelectedIndex = 0; // Fullscreen
                else if (winMode == 2) WindowModeCombo.SelectedIndex = 1; // Borderless
                else WindowModeCombo.SelectedIndex = 2; // Windowed

                DisableGodraysCheck.IsChecked = (godray == 0);
                HideEyeCandyCheck.IsChecked = (eyecandy == 1);
                DisableVSyncCheck.IsChecked = (vsync == 0);

                // HUD
                var hud = loadedData["HUD"];
                int eternals = hud != null ? ParseInt(hud["EternalsMilestoneDisplayMode"], 0) : 0;
                DisableEternalsCheck.IsChecked = (eternals == 0);
                SaveBtn.IsEnabled = true;
                ApplyLowBtn.IsEnabled = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Loc.T("Không thể đọc file game.cfg: ", "Unable to read game.cfg: ") + ex.Message + "\n\n" +
                    Loc.T("Bạn có thể khôi phục file nếu có bản sao lưu.", "You can restore the file if a backup exists."),
                    Loc.T("Lỗi", "Error"));
                return false;
            }
        }

        private bool IsLoLRunning()
        {
            try
            {
                // GetProcessesByName doesn't work reliably with spaces on all Windows versions
                // Use GetProcesses() and filter by process name instead
                foreach (var proc in Process.GetProcesses())
                {
                    try
                    {
                        string name = proc.ProcessName;
                        if (name != null &&
                            (name.Equals("League of Legends", StringComparison.OrdinalIgnoreCase) ||
                             name.Equals("LeagueClient", StringComparison.OrdinalIgnoreCase) ||
                             name.Equals("LeagueClientUx", StringComparison.OrdinalIgnoreCase)))
                        {
                            return true;
                        }
                    }
                    catch { }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_cfgPath) || !File.Exists(_cfgPath))
            {
                MessageBox.Show(
                    Loc.T("Không tìm thấy file game.cfg. Vui lòng bấm 'Chọn file...' để chỉ định đường dẫn.",
                          "game.cfg not found. Please click 'Browse...' to select your file."),
                    Loc.T("Lỗi", "Error"));
                return;
            }

            if (IsLoLRunning())
            {
                var ans = MessageBox.Show(
                    Loc.T("Liên Minh Huyền Thoại hiện đang chạy!\n\nVui lòng đóng game trước khi lưu để tránh bị game ghi đè mất cấu hình khi thoát.\n\nBạn có muốn tiếp tục lưu không?",
                          "League of Legends is currently running!\n\nPlease close the game before saving, otherwise LoL will overwrite your changes.\n\nDo you want to save anyway?"),
                    Loc.T("Cảnh báo: LMHT đang chạy", "LoL is Running"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (ans != MessageBoxResult.Yes) return;
            }

            try
            {
                int shadow = int.Parse(ShadowQBox.Text.Trim());
                int character = int.Parse(CharacterQBox.Text.Trim());
                int env = int.Parse(EnvironmentQBox.Text.Trim());
                int fx = int.Parse(EffectsQBox.Text.Trim());

                if (shadow < 0 || shadow > 4 || character < 0 || character > 4 || env < 0 || env > 4 || fx < 0 || fx > 4)
                {
                    MessageBox.Show(
                        Loc.T("Các giá trị đồ họa phải nằm trong khoảng từ 0 đến 4.", "Quality values must be between 0 and 4."),
                        Loc.T("Lỗi", "Error"));
                    return;
                }

                // WindowMode
                int winMode = 0;
                if (WindowModeCombo.SelectedIndex == 1) winMode = 2;
                else if (WindowModeCombo.SelectedIndex == 2) winMode = 1;

                SaveLoLConfig(
                    shadow, character, env, fx,
                    winMode,
                    DisableGodraysCheck.IsChecked == true,
                    HideEyeCandyCheck.IsChecked == true,
                    DisableHUDAnimCheck.IsChecked == true,
                    DisableFXAACheck.IsChecked == true,
                    DisableEternalsCheck.IsChecked == true,
                    DisableVSyncCheck.IsChecked == true);

                MessageBox.Show(
                    Loc.T("Đã lưu cài đặt Liên Minh Huyền Thoại thành công!\nFile cấu hình đã được khóa Read-Only để chống game tự reset.",
                          "League of Legends settings successfully saved!\nFile locked as Read-Only to prevent reset."),
                    Loc.T("Hoàn tất", "Done"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Loc.T("Dữ liệu nhập không hợp lệ hoặc lỗi lưu: ", "Invalid input or error saving: ") + ex.Message,
                    Loc.T("Lỗi", "Error"));
            }
        }

        private void ApplyLowBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_cfgPath) || !File.Exists(_cfgPath))
            {
                MessageBox.Show(
                    Loc.T("Không tìm thấy file game.cfg. Vui lòng bấm 'Chọn file...' để chỉ định đường dẫn.",
                          "game.cfg not found. Please click 'Browse...' to select your file."),
                    Loc.T("Lỗi", "Error"));
                return;
            }

            if (IsLoLRunning())
            {
                var ans = MessageBox.Show(
                    Loc.T("Liên Minh Huyền Thoại hiện đang chạy!\n\nVui lòng đóng game trước khi áp dụng cấu hình.\n\nBạn có muốn tiếp tục không?",
                          "League of Legends is currently running!\n\nPlease close the game before applying changes.\n\nDo you want to apply anyway?"),
                    Loc.T("Cảnh báo: LMHT đang chạy", "LoL is Running"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (ans != MessageBoxResult.Yes) return;
            }

            try
            {
                // Maximum FPS preset for LoL
                SaveLoLConfig(
                    shadow: 0,
                    character: 0,
                    env: 0,
                    fx: 0,
                    winMode: 0, // Exclusive Fullscreen for maximum GPU priority
                    disableGodray: true,
                    hideEyeCandy: true,
                    disableHUDAnim: true,
                    disableFXAA: true,
                    disableEternals: true,
                    disableVSync: true);

                // Update UI
                ShadowQBox.Text = "0";
                CharacterQBox.Text = "0";
                EnvironmentQBox.Text = "0";
                EffectsQBox.Text = "0";
                WindowModeCombo.SelectedIndex = 0; // Fullscreen
                DisableGodraysCheck.IsChecked = true;
                HideEyeCandyCheck.IsChecked = true;
                DisableHUDAnimCheck.IsChecked = true;
                DisableFXAACheck.IsChecked = true;
                DisableEternalsCheck.IsChecked = true;
                DisableVSyncCheck.IsChecked = true;

                MessageBox.Show(
                    Loc.T("Đã áp dụng cấu hình tối ưu FPS cho LMHT thành công!\nToàn bộ đồ họa đã giảm xuống thấp nhất và tắt các hiệu ứng phụ trong giao tranh.",
                          "Optimal Low Settings Applied!\nAll graphics lowered and ambient distractors disabled for maximum FPS in teamfights."),
                    Loc.T("Hoàn tất", "Done"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Loc.T("Lỗi khi áp dụng thiết lập: ", "Error applying settings: ") + ex.Message,
                    Loc.T("Lỗi", "Error"));
            }
        }

        private void resDefBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_cfgPath)) return;

            if (IsLoLRunning())
            {
                var ans = MessageBox.Show(
                    Loc.T("Liên Minh Huyền Thoại hiện đang chạy!\n\nVui lòng đóng game trước khi khôi phục thiết lập.\n\nBạn có muốn tiếp tục không?",
                          "League of Legends is currently running!\n\nPlease close the game before restoring.\n\nDo you want to restore anyway?"),
                    Loc.T("Cảnh báo: LMHT đang chạy", "LoL is Running"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (ans != MessageBoxResult.Yes) return;
            }

            try
            {
                string backupPath = GetBackupPath(_cfgPath);
                if (!File.Exists(backupPath))
                {
                    MessageBox.Show(Loc.T("Không tìm thấy bản sao lưu cho file này.", "No backup exists for this file."), Loc.T("Lỗi", "Error"));
                    return;
                }
                var answer = MessageBox.Show(
                    Loc.T("Khôi phục sẽ ghi đè toàn bộ file cấu hình bằng bản sao lưu. Các thay đổi khác trong game sau khi sao lưu sẽ bị mất. Tiếp tục?",
                          "Restoring replaces the entire configuration file with its backup. Other in-game changes made after the backup will be lost. Continue?"),
                    Loc.T("Xác nhận khôi phục", "Confirm restore"), MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (answer != MessageBoxResult.Yes) return;
                ConfigFiles.Restore(_cfgPath, backupPath);

                if (!LoadConfig(_cfgPath)) return;
                MessageBox.Show(
                    Loc.T("Đã khôi phục bản sao lưu LMHT thành công và mở khóa file (bỏ Read-Only)!",
                          "LoL backup restored successfully and file unlocked!"),
                    Loc.T("Hoàn tất", "Done"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Loc.T("Lỗi khi khôi phục bản sao lưu: ", "Error restoring backup: ") + ex.Message,
                    Loc.T("Lỗi", "Error"));
            }
        }

        private void SaveLoLConfig(
            int shadow, int character, int env, int fx,
            int winMode,
            bool disableGodray, bool hideEyeCandy, bool disableHUDAnim,
            bool disableFXAA, bool disableEternals, bool disableVSync)
        {
            var currentData = _parser.ReadFile(_cfgPath, Encoding.Default);

            // [Performance]
            if (currentData["Performance"] == null) currentData.Sections.AddSection("Performance");
            currentData["Performance"]["ShadowQuality"] = shadow.ToString();
            currentData["Performance"]["CharacterQuality"] = character.ToString();
            currentData["Performance"]["EnvironmentQuality"] = env.ToString();
            currentData["Performance"]["EffectsQuality"] = fx.ToString();
            currentData["Performance"]["EnableFXAA"] = disableFXAA ? "0" : "1";
            currentData["Performance"]["EnableHUDAnimations"] = disableHUDAnim ? "0" : "1";

            // [General]
            if (currentData["General"] == null) currentData.Sections.AddSection("General");
            currentData["General"]["WindowMode"] = winMode.ToString();
            currentData["General"]["WaitForVerticalSync"] = disableVSync ? "0" : "1";
            currentData["General"]["ShowGodray"] = disableGodray ? "0" : "1";
            currentData["General"]["HideEyeCandy"] = hideEyeCandy ? "1" : "0";

            // [HUD]
            if (currentData["HUD"] == null) currentData.Sections.AddSection("HUD");
            currentData["HUD"]["EternalsMilestoneDisplayMode"] = disableEternals ? "0" : "2";

            ConfigFiles.Save(_cfgPath, GetBackupPath(_cfgPath),
                temp => _parser.WriteFile(temp, currentData, Encoding.Default));
        }

        private void BrowseLoLBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = ConfigFiles.Browse(ConfigName, BackupKind);
                if (path != null)
                    LoadConfig(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở file: " + ex.Message, "Lỗi");
            }
        }
    }
}

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

namespace ValorantResFix
{
    /// <summary>
    /// Interaction logic for LoLPage.xaml
    /// </summary>
    public partial class LoLPage : Page
    {
        private string _cfgPath = null;
        private FileIniDataParser _parser = new FileIniDataParser();
        private IniData _iniData = null;

        private string BackupFilePath
        {
            get
            {
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup_LoL.cfg");
            }
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
                ResDefBtnLoLText.Text = "Khôi phục gốc";
                resDefBtn.ToolTip = "Khôi phục file game.cfg về mặc định và mở khóa file";
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
                ResDefBtnLoLText.Text = "Rollback Default";
                resDefBtn.ToolTip = "Revert game.cfg to default and unlock file";
                LoLScaleHintTxt.Text = "• Graphics scale: 0 = Very Low (Off), 1 = Low, 2 = Medium, 3 = High, 4 = Very High";
            }
        }

        public void StartUP()
        {
            _cfgPath = FindLoLConfigPath();

            if (!string.IsNullOrEmpty(_cfgPath) && File.Exists(_cfgPath))
            {
                LoadConfig(_cfgPath);
            }
            else
            {
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

        private void LoadConfig(string path)
        {
            try
            {
                _cfgPath = path;
                PathBox.Text = path;

                _iniData = _parser.ReadFile(path, Encoding.Default);

                // Create backup if not exists
                string backupPath = BackupFilePath;
                if (!File.Exists(backupPath))
                {
                    File.Copy(path, backupPath, true);
                }

                // Performance
                var perf = _iniData["Performance"];
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
                var general = _iniData["General"];
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
                var hud = _iniData["HUD"];
                int eternals = hud != null ? ParseInt(hud["EternalsMilestoneDisplayMode"], 0) : 0;
                DisableEternalsCheck.IsChecked = (eternals == 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đọc file game.cfg: " + ex.Message, "Lỗi");
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
                    DisableVSyncCheck.IsChecked == true,
                    unlockReadOnly: false);

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
                    disableVSync: true,
                    unlockReadOnly: false);

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
            if (string.IsNullOrEmpty(_cfgPath) || !File.Exists(_cfgPath)) return;

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
                string backupPath = BackupFilePath;
                var attr = File.GetAttributes(_cfgPath);
                if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(_cfgPath, attr & ~FileAttributes.ReadOnly);
                }

                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, _cfgPath, true);
                }
                else
                {
                    // Fallback default
                    SaveLoLConfig(
                        shadow: 1, character: 2, env: 2, fx: 2,
                        winMode: 0,
                        disableGodray: false, hideEyeCandy: false,
                        disableHUDAnim: false, disableFXAA: false,
                        disableEternals: false, disableVSync: true,
                        unlockReadOnly: true);
                }

                // Remove Read-Only attribute
                attr = File.GetAttributes(_cfgPath);
                File.SetAttributes(_cfgPath, attr & ~FileAttributes.ReadOnly);

                LoadConfig(_cfgPath);
                MessageBox.Show(
                    Loc.T("Đã khôi phục cài đặt LMHT gốc thành công và mở khóa file (bỏ Read-Only)!",
                          "Default LoL settings restored successfully and file unlocked!"),
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
            bool disableFXAA, bool disableEternals, bool disableVSync,
            bool unlockReadOnly)
        {
            if (_iniData == null) _iniData = _parser.ReadFile(_cfgPath, Encoding.Default);

            // [Performance]
            if (_iniData["Performance"] == null) _iniData.Sections.AddSection("Performance");
            _iniData["Performance"]["ShadowQuality"] = shadow.ToString();
            _iniData["Performance"]["CharacterQuality"] = character.ToString();
            _iniData["Performance"]["EnvironmentQuality"] = env.ToString();
            _iniData["Performance"]["EffectsQuality"] = fx.ToString();
            _iniData["Performance"]["EnableFXAA"] = disableFXAA ? "0" : "1";
            _iniData["Performance"]["EnableHUDAnimations"] = disableHUDAnim ? "0" : "1";

            // [General]
            if (_iniData["General"] == null) _iniData.Sections.AddSection("General");
            _iniData["General"]["WindowMode"] = winMode.ToString();
            _iniData["General"]["WaitForVerticalSync"] = disableVSync ? "0" : "1";
            _iniData["General"]["ShowGodray"] = disableGodray ? "0" : "1";
            _iniData["General"]["HideEyeCandy"] = hideEyeCandy ? "1" : "0";

            // [HUD]
            if (_iniData["HUD"] == null) _iniData.Sections.AddSection("HUD");
            _iniData["HUD"]["EternalsMilestoneDisplayMode"] = disableEternals ? "0" : "2";

            var attr = File.GetAttributes(_cfgPath);
            if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                File.SetAttributes(_cfgPath, attr & ~FileAttributes.ReadOnly);
            }

            _parser.WriteFile(_cfgPath, _iniData, Encoding.Default);

            if (unlockReadOnly)
            {
                File.SetAttributes(_cfgPath, attr & ~FileAttributes.ReadOnly);
            }
            else
            {
                File.SetAttributes(_cfgPath, attr | FileAttributes.ReadOnly);
            }
        }

        private void BrowseLoLBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "LoL Config (game.cfg)|game.cfg|Config Files (*.cfg;*.ini)|*.cfg;*.ini|All Files (*.*)|*.*",
                    Title = Loc.T("Chọn file game.cfg của Liên Minh Huyền Thoại", "Select League of Legends game.cfg")
                };

                bool? res = dlg.ShowDialog();
                if (res == true)
                {
                    LoadConfig(dlg.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở file: " + ex.Message, "Lỗi");
            }
        }
    }
}

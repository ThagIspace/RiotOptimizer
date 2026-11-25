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
        private Page _valPage = null;
        private Page _lolPage = null;
        private bool _isValActive = true;

        public MainWindow()
        {
            InitializeComponent();
            Loc.OnLanguageChanged += ApplyLocalization;
            ApplyLocalization();
            StartUP();
        }

        private void LangBtn_Click(object sender, RoutedEventArgs e)
        {
            Loc.Toggle();
        }

        private void ApplyLocalization()
        {
            LangTxt.Text = Loc.IsVi ? "VI" : "EN";

            if (Loc.IsVi)
            {
                NoticeBtn.ToolTip = "Lưu ý quan trọng khi sử dụng";
                AboutBtn.ToolTip = "Thông tin phần mềm";
                MinBtn.ToolTip = "Thu nhỏ";
                ExitBtn.ToolTip = "Đóng";
                LangBtn.ToolTip = "Đổi ngôn ngữ (Anh - Việt)";

                NoticeTitleTxt.Text = "LƯU Ý QUAN TRỌNG KHI SỬ DỤNG";
                NoticeItem1Title.Text = "1. Đóng game trước khi lưu hoặc áp dụng thiết lập";
                NoticeItem1Body1.Text = "• Hãy tắt hoàn toàn VALORANT hoặc Liên Minh Huyền Thoại trước khi bấm 'Lưu thay đổi' hoặc 'Tối ưu FPS (Low)'.";
                NoticeItem1Body2.Text = "• Nếu game đang chạy, khi thoát ra game có thể tự động ghi đè và làm mất các tùy chỉnh vừa áp dụng.";

                NoticeItem2Title.Text = "2. Cơ chế khóa file Chỉ đọc (Read-Only)";
                NoticeItem2Body1.Text = "• Sau khi Lưu hoặc Tối ưu FPS, phần mềm sẽ tự động bật thuộc tính Read-Only cho file cấu hình.";
                NoticeItem2Body2.Text = "• Việc này giúp giữ nguyên tỷ lệ Resolution Quality (độ phân giải nội bộ) dưới 100% không bị game tự reset khi vào trận.";

                NoticeItem3Title.Text = "3. Cách khôi phục bản sao lưu";
                NoticeItem3Body1.Text = "• Nếu bạn muốn chỉnh lại đồ họa trong menu game như bình thường, bấm 'Khôi phục bản sao lưu'.";
                NoticeItem3Body2.Text = "• Tool sẽ tự động mở khóa file (tắt Read-Only) và chép lại bản sao đã lưu của file cấu hình.";

                NoticeItem4Title.Text = "4. Hỗ trợ nhiều tài khoản (VALORANT)";
                NoticeItem4Body1.Text = "• Ứng dụng tự động quét và đồng bộ thiết lập tối ưu cho TẤT CẢ các tài khoản VALORANT có trên máy cùng một lúc.";

                NoticeItem5Title.Text = "5. Cam kết an toàn 100% (Tuân thủ Vanguard)";
                NoticeItem5Body1.Text = "• Công cụ CHỈ chỉnh sửa các file cấu hình text (.ini / game.cfg) hợp lệ của game.";
                NoticeItem5Body2.Text = "• Tuyệt đối KHÔNG can thiệp bộ nhớ RAM, KHÔNG inject DLL hay chỉnh sửa file game, an toàn tuyệt đối với tài khoản.";

                CloseNoticeBtnTxt.Text = "Đã hiểu & Đóng";
            }
            else
            {
                NoticeBtn.ToolTip = "Important usage notices";
                AboutBtn.ToolTip = "About RiotOptimizer";
                MinBtn.ToolTip = "Minimize";
                ExitBtn.ToolTip = "Close";
                LangBtn.ToolTip = "Switch Language (English / Vietnamese)";

                NoticeTitleTxt.Text = "IMPORTANT USAGE NOTICES";
                NoticeItem1Title.Text = "1. Close game before saving or applying settings";
                NoticeItem1Body1.Text = "• Please exit VALORANT or League of Legends completely before clicking 'Save Changes' or 'Apply Low (FPS)'.";
                NoticeItem1Body2.Text = "• If the game is running, it may overwrite your configuration upon closing.";

                NoticeItem2Title.Text = "2. Read-Only file locking mechanism";
                NoticeItem2Body1.Text = "• After Saving or Applying Low settings, the app locks the config file as Read-Only.";
                NoticeItem2Body2.Text = "• This prevents the game from resetting custom Resolution Quality (< 100%) when loading a match.";

                NoticeItem3Title.Text = "3. How to restore a backup";
                NoticeItem3Body1.Text = "• To restore a saved config and configure graphics inside the game normally, click 'Restore Backup'.";
                NoticeItem3Body2.Text = "• The tool removes the Read-Only lock and restores the saved copy of the config.";

                NoticeItem4Title.Text = "4. Multi-account synchronization (VALORANT)";
                NoticeItem4Body1.Text = "• Automatically detects and applies settings to ALL VALORANT accounts installed on this PC simultaneously.";

                NoticeItem5Title.Text = "5. 100% Safe & Vanguard Compliant";
                NoticeItem5Body1.Text = "• Only modifies standard game client text configuration files (.ini / game.cfg).";
                NoticeItem5Body2.Text = "• NEVER touches RAM, injects DLLs, or modifies binaries. Completely compliant with Riot Vanguard.";

                CloseNoticeBtnTxt.Text = "Understood & Close";
            }
        }

        public void StartUP()
        {
            List<string> settingsFileList = ScanAllDrives();

            if (settingsFileList.Count < 1)
            {
                _valPage = new ErrorPage();
            }
            else
            {
                _valPage = new Home(settingsFileList);
            }

            MyBaseFrame.Navigate(_valPage);
            UpdateTabStyles();
        }

        internal void ShowValorantFiles(List<string> paths)
        {
            _valPage = new Home(paths);
            MyBaseFrame.Navigate(_valPage);
        }

        private void TabValBtn_Click(object sender, RoutedEventArgs e)
        {
            _isValActive = true;
            UpdateTabStyles();
            NoticeOverlay.Visibility = Visibility.Collapsed;
            MyBaseFrame.Navigate(_valPage);
        }

        private void TabLoLBtn_Click(object sender, RoutedEventArgs e)
        {
            _isValActive = false;
            UpdateTabStyles();
            NoticeOverlay.Visibility = Visibility.Collapsed;
            if (_lolPage == null)
            {
                _lolPage = new LoLPage();
            }
            MyBaseFrame.Navigate(_lolPage);
        }

        private void NoticeBtn_Click(object sender, RoutedEventArgs e)
        {
            NoticeOverlay.Visibility = (NoticeOverlay.Visibility == Visibility.Visible) 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        private void CloseNoticeBtn_Click(object sender, RoutedEventArgs e)
        {
            NoticeOverlay.Visibility = Visibility.Collapsed;
        }

        private void NoticeOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == NoticeOverlay)
            {
                NoticeOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateTabStyles()
        {
            Brush activeBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#87CEFA")); // LightSkyBlue
            Brush inactiveBorder = Brushes.Transparent;
            Brush activeBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D2D2D"));
            Brush inactiveBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF222222"));
            Brush activeFg = Brushes.White;
            Brush inactiveFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999"));

            if (_isValActive)
            {
                TabValBtn.BorderBrush = activeBorder;
                TabValBtn.Background = activeBg;
                TabValBtn.Foreground = activeFg;

                TabLoLBtn.BorderBrush = inactiveBorder;
                TabLoLBtn.Background = inactiveBg;
                TabLoLBtn.Foreground = inactiveFg;
            }
            else
            {
                TabLoLBtn.BorderBrush = activeBorder;
                TabLoLBtn.Background = activeBg;
                TabLoLBtn.Foreground = activeFg;

                TabValBtn.BorderBrush = inactiveBorder;
                TabValBtn.Background = inactiveBg;
                TabValBtn.Foreground = inactiveFg;
            }
        }

        public static List<string> ScanAllDrives()
        {
            List<string> found = new List<string>();

            // 1. Current user LocalAppData
            try
            {
                string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                ScanDirectory(System.IO.Path.Combine(localApp, "VALORANT", "Saved", "Config"), found);
            }
            catch {}

            // 2. Other user profiles under C:\Users\*
            try
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string usersDir = System.IO.Path.GetDirectoryName(userProfile);
                if (Directory.Exists(usersDir))
                {
                    foreach (var uDir in Directory.GetDirectories(usersDir))
                    {
                        string testPath = System.IO.Path.Combine(uDir, "AppData", "Local", "VALORANT", "Saved", "Config");
                        ScanDirectory(testPath, found);
                    }
                }
            }
            catch {}

            // 3. Scan all fixed drives
            try
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        string[] candidates = new string[]
                        {
                            System.IO.Path.Combine(drive.RootDirectory.FullName, "VALORANT", "Saved", "Config"),
                            System.IO.Path.Combine(drive.RootDirectory.FullName, "Riot Games", "VALORANT", "Saved", "Config"),
                            System.IO.Path.Combine(drive.RootDirectory.FullName, "Games", "VALORANT", "Saved", "Config")
                        };

                        foreach (var cand in candidates)
                        {
                            ScanDirectory(cand, found);
                        }
                    }
                }
            }
            catch {}

            return found.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static void ScanDirectory(string dir, List<string> results)
        {
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir)) return;

            try
            {
                string[] files = Directory.GetFiles(dir, "GameUserSettings.ini", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    string lower = f.ToLower();
                    if (!lower.Contains("crashreportclient") && !lower.Contains("backup"))
                    {
                        results.Add(f);
                    }
                }
            }
            catch
            {
                try
                {
                    foreach (var sub in Directory.GetDirectories(dir))
                    {
                        string c1 = System.IO.Path.Combine(sub, "WindowsClient", "GameUserSettings.ini");
                        string c2 = System.IO.Path.Combine(sub, "Windows", "GameUserSettings.ini");
                        if (File.Exists(c1)) results.Add(c1);
                        else if (File.Exists(c2)) results.Add(c2);
                    }
                }
                catch {}
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
            if (Loc.IsVi)
            {
                MessageBox.Show(
                    "RiotOptimizer v2.5\n\n" +
                    "Công cụ mã nguồn mở tối ưu FPS tất cả-trong-một cho VALORANT và Liên Minh Huyền Thoại.\n\n" +
                    "Tính năng chính:\n" +
                    "• VALORANT: Hạ độ phân giải nội bộ (Resolution Quality), chỉnh đầy đủ thông số đồ họa, đồng bộ tự động tất cả tài khoản.\n" +
                    "• Liên Minh Huyền Thoại: Cấu hình giảm trễ input tối đa, tắt ánh sáng mặt trời/sinh vật cảnh, tối ưu giao diện HUD.\n" +
                    "• Khôi phục bản sao lưu cho cả 2 tựa game.\n\n" +
                    "Tác giả & Đóng góp:\n" +
                    "• Tác giả gốc: xFahim (https://github.com/xFahim/ValorantResFix)\n" +
                    "• Phát triển & Mở rộng: ThagIspace (https://github.com/ThagIspace/RiotOptimizer)\n\n" +
                    "100% An toàn, tuân thủ Riot Vanguard (chỉ can thiệp cấu hình client).",
                    "Thông tin phần mềm - RiotOptimizer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    "RiotOptimizer v2.5\n\n" +
                    "An open-source all-in-one optimizer for Riot Games titles (VALORANT & League of Legends).\n\n" +
                    "Key Features:\n" +
                    "• VALORANT: Internal resolution scaling, all scalability groups, automatic multi-account sync.\n" +
                    "• League of Legends: Low-latency presets, godrays/eyecandy removal, HUD optimizations.\n" +
                    "• Backup restoration for both games.\n\n" +
                    "Credits:\n" +
                    "• Original Creator: xFahim (https://github.com/xFahim/ValorantResFix)\n" +
                    "• Maintained & Expanded by: ThagIspace (https://github.com/ThagIspace/RiotOptimizer)\n\n" +
                    "Safe & Vanguard compliant (client configuration tuning only).",
                    "About RiotOptimizer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}

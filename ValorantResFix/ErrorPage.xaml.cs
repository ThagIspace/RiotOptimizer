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

namespace ValorantResFix
{
    /// <summary>
    /// Interaction logic for ErrorPage.xaml
    /// </summary>
    public partial class ErrorPage : Page
    {
        public ErrorPage()
        {
            InitializeComponent();
            Loc.OnLanguageChanged += ApplyLocalization;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (Loc.IsVi)
            {
                ErrorTitleTxt.Text = "Không tìm thấy file GameUserSettings.ini";
                ErrorDescTxt.Text = "Hệ thống đã quét thư mục AppData và các ổ đĩa nhưng chưa tìm thấy file cấu hình.";
                ErrorBullet1.Text = "• Hãy đảm bảo bạn đã mở VALORANT ít nhất 1 lần trên máy này.";
                ErrorBullet2.Text = "• Hoặc chọn thủ công file GameUserSettings.ini bên dưới:";
                BrowseBtn.Content = "Chọn file thủ công";
                RescanBtn.Content = "Quét lại toàn bộ ổ đĩa";
            }
            else
            {
                ErrorTitleTxt.Text = "Could Not Find GameUserSettings.ini";
                ErrorDescTxt.Text = "We scanned your standard AppData and drives but couldn't locate the file.";
                ErrorBullet1.Text = "• Make sure VALORANT has been launched at least once on this PC.";
                ErrorBullet2.Text = "• Or select your GameUserSettings.ini manually below:";
                BrowseBtn.Content = "Browse File Manually";
                RescanBtn.Content = "Rescan All Drives";
            }
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Filter = "Valorant Config (GameUserSettings.ini)|GameUserSettings.ini|INI Files (*.ini)|*.ini|All Files (*.*)|*.*";
                dlg.Title = Loc.T("Chọn file GameUserSettings.ini của VALORANT", "Select GameUserSettings.ini");

                bool? result = dlg.ShowDialog();
                if (result == true)
                {
                    if (this.NavigationService != null)
                    {
                        this.NavigationService.Navigate(new Home(new List<string> { dlg.FileName }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Loc.T("Lỗi khi mở file: ", "Error opening file: ") + ex.Message, Loc.T("Lỗi", "Error"));
            }
        }

        private void RescanBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> found = MainWindow.ScanAllDrives();
            if (found != null && found.Count > 0)
            {
                if (this.NavigationService != null)
                {
                    this.NavigationService.Navigate(new Home(found));
                }
            }
            else
            {
                MessageBox.Show(
                    Loc.T("Quét xong. Vẫn không tìm thấy GameUserSettings.ini tự động.\nVui lòng bấm 'Chọn file thủ công' để chỉ định file cấu hình.",
                          "Rescan complete. Could not find GameUserSettings.ini automatically.\nPlease use 'Browse File Manually' to locate your file."),
                    Loc.T("Thông báo", "Notice"));
            }
        }
    }
}

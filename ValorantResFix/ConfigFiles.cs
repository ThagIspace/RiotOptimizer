using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace ValorantResFix
{
    internal static class ConfigFiles
    {
        internal static string BackupPath(string path, string game)
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RiotOptimizer", "Backups");
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(Path.GetFullPath(path).ToUpperInvariant()));
                return Path.Combine(dir, BitConverter.ToString(hash).Replace("-", "") + "." + game + ".bak");
            }
        }

        internal static string Browse(string fileName, string game)
        {
            var dialog = new OpenFileDialog
            {
                Title = Loc.T("Chọn file cấu hình hoặc nhập đường dẫn cũ để khôi phục", "Select the config file or enter its former path to restore"),
                Filter = fileName + "|" + fileName,
                FileName = fileName,
                CheckFileExists = false,
                CheckPathExists = false
            };
            if (dialog.ShowDialog() != true) return null;

            string path = dialog.FileName;
            if (File.Exists(BackupPath(path, game)) ||
                (File.Exists(path) && string.Equals(Path.GetFileName(path), fileName, StringComparison.OrdinalIgnoreCase)))
                return path;

            MessageBox.Show(
                Loc.T("Hãy chọn đúng tên file cấu hình. Nếu file đã mất, nhập đường dẫn ban đầu có bản sao lưu.",
                      "Select the expected config filename. If it is missing, enter its original path with an existing backup."),
                Loc.T("Lỗi", "Error"));
            return null;
        }

        internal static void Save(string path, string backup, Action<string> write)
        {
            Update(path, temp =>
            {
                write(temp);
                if (!File.Exists(backup)) CreateBackup(path, backup);
            }, true, false);
        }

        internal static void Restore(string path, string backup)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            Update(path, temp => CopyContents(backup, temp), false, true);
        }

        private static void CreateBackup(string path, string backup)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(backup));
            string temp = TempPath(backup);
            try
            {
                CopyContents(path, temp);
                try
                {
                    File.Move(temp, backup);
                }
                catch (IOException) when (File.Exists(backup))
                {
                    // Another instance finished creating the first backup.
                }
            }
            finally
            {
                DeleteTempFile(temp);
            }
        }

        private static void Update(string path, Action<string> write, bool readOnly, bool allowMissing)
        {
            string temp = TempPath(path);
            string previous = temp + ".previous";
            try
            {
                write(temp);
                if (allowMissing && !File.Exists(path))
                {
                    File.Move(temp, path);
                    return;
                }

                var original = File.GetAttributes(path);
                var writable = original & ~FileAttributes.ReadOnly;
                if (original != writable) SetAttributes(path, writable);
                try
                {
                    File.Replace(temp, path, previous);
                }
                catch (Exception replaceError)
                {
                    try
                    {
                        if (!File.Exists(path) && File.Exists(previous)) File.Move(previous, path);
                        if (original != writable && File.Exists(path)) SetAttributes(path, original);
                    }
                    catch (Exception recoveryError) when (recoveryError is IOException || recoveryError is UnauthorizedAccessException)
                    {
                        throw new IOException(replaceError.Message + " " + recoveryError.Message + " (" + previous + ")", replaceError);
                    }
                    throw;
                }

                try
                {
                    SetAttributes(path, readOnly ? writable | FileAttributes.ReadOnly : writable);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    throw new IOException(
                        Loc.T("Đã thay nội dung file nhưng không đặt được thuộc tính. File trước khi thay: ",
                              "File contents were replaced but attributes could not be set. Previous file: ") +
                        previous + ". " + ex.Message, ex);
                }
                DeleteTempFile(previous);
            }
            finally
            {
                DeleteTempFile(temp);
            }
        }

        private static void CopyContents(string source, string temp)
        {
            using (var input = File.OpenRead(source))
            using (var output = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                input.CopyTo(output);
            }
        }

        private static string TempPath(string path)
        {
            return Path.Combine(Path.GetDirectoryName(path), ".riotoptimizer-" + Guid.NewGuid().ToString("N") + ".tmp");
        }

        private static void SetAttributes(string path, FileAttributes attributes)
        {
            attributes &= ~FileAttributes.Normal;
            File.SetAttributes(path, attributes == 0 ? FileAttributes.Normal : attributes);
        }

        private static void DeleteTempFile(string temp)
        {
            try { File.Delete(temp); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}

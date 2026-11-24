using System;

namespace ValorantResFix
{
    public enum AppLanguage
    {
        Vietnamese,
        English
    }

    public static class Loc
    {
        private static AppLanguage _current = AppLanguage.Vietnamese;

        public static AppLanguage Current
        {
            get { return _current; }
            set { _current = value; }
        }

        public static event Action OnLanguageChanged;

        public static bool IsVi
        {
            get { return _current == AppLanguage.Vietnamese; }
        }

        public static void Toggle()
        {
            _current = (_current == AppLanguage.Vietnamese) ? AppLanguage.English : AppLanguage.Vietnamese;
            if (OnLanguageChanged != null)
            {
                OnLanguageChanged();
            }
        }

        public static string T(string vi, string en)
        {
            return IsVi ? vi : en;
        }
    }
}

using UnityEditor;

namespace net.puk06.TexStackEditor.Editor.Localization
{
    internal static class LocalizationManager
    {
        private const string LanguageKey = "TexStackEditor_CurrentLanguage";

        internal static string CurrentLanguage
        {
            get => EditorPrefs.GetString(LanguageKey, "ja-JP");
            set => EditorPrefs.SetString(LanguageKey, value);
        }
    }
}

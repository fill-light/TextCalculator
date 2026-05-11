using System;
using System.Drawing;
using System.IO;

namespace Calculator
{
    public class AppSettings
    {
        public string Theme { get; set; } = "Light";
        public string UIFontName { get; set; } = "Segoe UI";
        public float UIFontSize { get; set; } = 9f;
        public string EditorFontName { get; set; } = "Consolas";
        public float EditorFontSize { get; set; } = 11f;

        private static string SettingsPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Calculator",
                "settings.ini");

        public static AppSettings Load()
        {
            var s = new AppSettings();
            try
            {
                if (!File.Exists(SettingsPath)) return s;
                foreach (var line in File.ReadAllLines(SettingsPath))
                {
                    var parts = line.Split('=');
                    if (parts.Length != 2) continue;
                    string key = parts[0].Trim();
                    string val = parts[1].Trim();
                    switch (key)
                    {
                        case "Theme": s.Theme = val; break;
                        case "UIFontName": s.UIFontName = val; break;
                        case "UIFontSize":
                            float.TryParse(val, out float us);
                            s.UIFontSize = us > 0 ? us : 9f; break;
                        case "EditorFontName": s.EditorFontName = val; break;
                        case "EditorFontSize":
                            float.TryParse(val, out float es);
                            s.EditorFontSize = es > 0 ? es : 11f; break;
                    }
                }
            }
            catch { }
            return s;
        }

        public void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllLines(SettingsPath, new[]
                {
                    $"Theme={Theme}",
                    $"UIFontName={UIFontName}",
                    $"UIFontSize={UIFontSize}",
                    $"EditorFontName={EditorFontName}",
                    $"EditorFontSize={EditorFontSize}"
                });
            }
            catch { }
        }

        public Font GetUIFont()
        {
            try { return new Font(UIFontName, UIFontSize); }
            catch { return new Font("Arial", 9f); }
        }

        public Font GetEditorFont()
        {
            try { return new Font(EditorFontName, EditorFontSize); }
            catch { return new Font("Courier New", 11f); }
        }

        public AppTheme GetTheme()
        {
            return Enum.TryParse(Theme, out AppTheme t) ? t : AppTheme.Light;
        }

        public void SetFont(Font uiFont, Font editorFont)
        {
            UIFontName = uiFont.Name;
            UIFontSize = uiFont.Size;
            EditorFontName = editorFont.Name;
            EditorFontSize = editorFont.Size;
        }

        public void SetTheme(AppTheme theme)
        {
            Theme = theme.ToString();
        }
    }
}
using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Calculator
{
    public class AppSettings
    {
        // ── 저장할 값들 ───────────────────────────────────────
        public string  Theme          { get; set; } = "Light";

        public string  UIFontName     { get; set; } = "Segoe UI";
        public float   UIFontSize     { get; set; } = 9f;

        public string  EditorFontName { get; set; } = "Consolas";
        public float   EditorFontSize { get; set; } = 11f;

        // ── 파일 경로 ─────────────────────────────────────────
        private static string SettingsPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Calculator",
                "settings.json");

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            WriteIndented        = true,
            Converters           = { new JsonStringEnumConverter() }
        };

        // ── 불러오기 ──────────────────────────────────────────
        public static AppSettings Load()
        {
            try
            {
                string path = SettingsPath;
                if (!File.Exists(path)) return new AppSettings();
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<AppSettings>(json, _jsonOpts)
                       ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        // ── 저장 ─────────────────────────────────────────────
        public void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(SettingsPath)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string json = JsonSerializer.Serialize(this, _jsonOpts);
                File.WriteAllText(SettingsPath, json);
            }
            catch { /* 저장 실패는 무시 */ }
        }

        // ── 헬퍼: Font ↔ Settings 변환 ───────────────────────
        public Font GetUIFont()
        {
            try   { return new Font(UIFontName, UIFontSize); }
            catch { return new Font("Arial", 9f); }
        }

        public Font GetEditorFont()
        {
            try   { return new Font(EditorFontName, EditorFontSize); }
            catch { return new Font("Courier New", 11f); }
        }

        public AppTheme GetTheme()
        {
            return Enum.TryParse<AppTheme>(Theme, out var t) ? t : AppTheme.Light;
        }

        public void SetFont(Font uiFont, Font editorFont)
        {
            UIFontName     = uiFont.Name;
            UIFontSize     = uiFont.Size;
            EditorFontName = editorFont.Name;
            EditorFontSize = editorFont.Size;
        }

        public void SetTheme(AppTheme theme)
        {
            Theme = theme.ToString();
        }
    }
}

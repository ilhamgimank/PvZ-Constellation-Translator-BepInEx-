using UnityEngine;
using System.Text.RegularExpressions;
using PvZStarSignTranslator.Managers;

namespace PvZStarSignTranslator.Features
{
    public static class TextDumper
    {
        public static bool EnableDump = true;

        // Variabel toggle pemisah deteksi per komponen
        public static bool EnableIMGUI = true;
        public static bool EnableUGUI = true;
        public static bool EnableNGUI = false;
        public static bool EnableTextMesh = true;
        public static bool EnableTMP = true;
        public static bool EnableFairyGUI = false;

        // [FIX] Fungsi untuk mengekstrak teks komponen apa saja menggunakan Reflection
        public static bool TryGetReflectionText(Component component, out string text)
        {
            text = null;
            if (component == null) return false;

            var prop = component.GetType().GetProperty("text");
            if (prop != null && prop.PropertyType == typeof(string))
            {
                text = prop.GetValue(component, null) as string;
                return true;
            }
            return false;
        }

        // Mendeteksi dan menyimpan teks Mandarin yang belum ada di database
        public static void DumpText(string text, string _sourceType = "Text")
        {
            if (string.IsNullOrWhiteSpace(text) || !EnableDump) return;

            // Pengecekan tipe komponen untuk mengizinkan atau menahan dump
            if (_sourceType == "IMGUI" && !EnableIMGUI) return;
            if (_sourceType.Contains("Text") && !_sourceType.Contains("Mesh") && !EnableUGUI) return; // UGUI Text
            if (_sourceType.Contains("TextMeshPro") && !EnableTMP) return;
            if (_sourceType == "TextMesh" && !EnableTextMesh) return;
            if (_sourceType == "NGUI" && !EnableNGUI) return;
            if (_sourceType == "FairyGUI" && !EnableFairyGUI) return;

            // Filter: Hanya ambil teks yang mengandung karakter Mandarin
            if (!Regex.IsMatch(text, @"\p{IsCJKUnifiedIdeographs}")) return;

            // Abaikan jika sudah diterjemahkan
            if (TranslationManager.Translations.ContainsKey(text)) return;

            try
            {
                string path = System.IO.Path.Combine(FileManager.DumpsFolder, "untranslated_strings.json");
                if (!System.IO.File.Exists(path)) return;

                string cleanText = EscapeForJson(text);
                string json = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);

                if (json.Contains("\"" + cleanText + "\":")) return;

                int insertPos = json.IndexOf('{') + 1;
                string comma = json.Contains(":") ? "," : "";
                string newEntry = string.Format("\n  \"{0}\": \"{1}\"{2}", cleanText, cleanText, comma);

                json = json.Insert(insertPos, newEntry);
                System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);

                Plugin.Log.LogInfo(string.Format("New text dumped [{0}]: {1}", _sourceType, text));
            }
            catch (System.Exception ex) { Plugin.Log.LogError("Dump Failed: " + ex.Message); }
        }

        public static string EscapeForJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }
    }
}
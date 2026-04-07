#pragma warning disable IDE0060 // Remove unused parameter
using UnityEngine;
using System.Text.RegularExpressions;
using PvZStarSignTranslator.Managers;

namespace PvZStarSignTranslator.Features
{
    public static class TextDumper
    {
        public static bool EnableDump = true;

        // Prefix unused parameter with underscore to fix IDE0060
        public static void DumpText(string text, string _sourceType = "Text")
        {
            if (string.IsNullOrWhiteSpace(text) || !EnableDump) return;

            if (!Regex.IsMatch(text, @"\p{IsCJKUnifiedIdeographs}")) return;
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
                // Fix String Interpolation
                string newEntry = string.Format("\n  \"{0}\": \"{1}\"{2}", cleanText, cleanText, comma);

                json = json.Insert(insertPos, newEntry);
                System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);

                Plugin.Log.LogInfo("Added to dump: " + text);
            }
            catch (System.Exception ex) { Plugin.Log.LogError("Dump Error: " + ex.Message); }
        }

        public static bool TryGetReflectionText(GameObject obj, string componentName, out string result)
        {
            result = "";
            Component comp = obj.GetComponent(componentName);
            if (comp != null)
            {
                var prop = comp.GetType().GetProperty("text");
                if (prop != null)
                {
                    result = prop.GetValue(comp, null) as string;
                    return true;
                }
            }
            return false;
        }

        public static string EscapeForJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }
    }
}
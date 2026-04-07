using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace PvZStarSignTranslator.Managers
{
    public static class TranslationManager
    {
        public static Dictionary<string, string> Translations = new Dictionary<string, string>();
        public static Dictionary<string, string> RegexTranslations = new Dictionary<string, string>();
        public static Dictionary<string, string> UIOverrides = new Dictionary<string, string>();

        public static void LoadTranslations()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Translations.Clear();
            RegexTranslations.Clear();
            UIOverrides.Clear();

            LoadJsonStrings();
            LoadRegexTranslations();
            LoadUIOverrides();

            sw.Stop();
            // Menggunakan string.Format untuk log
            Plugin.Log.LogInfo(string.Format("[Translation] Loaded: {0} Strings, {1} Regex in {2} ms.",
                Translations.Count, RegexTranslations.Count, sw.ElapsedMilliseconds));
        }

        private static void LoadJsonStrings()
        {
            string path = Path.Combine(FileManager.CurrentLanguagePath, "Strings", "translation_strings.json");
            if (!File.Exists(path)) return;

            try
            {
                string jsonContent = File.ReadAllText(path, System.Text.Encoding.UTF8);
                var matches = Regex.Matches(jsonContent, @"""((?:[^""\\]|\\.)*)""\s*:\s*""((?:[^""\\]|\\.)*)""");

                foreach (Match match in matches)
                {
                    string original = UnescapeJson(match.Groups[1].Value);
                    string translation = UnescapeJson(match.Groups[2].Value);
                    if (!string.IsNullOrEmpty(translation) && original != translation)
                    {
                        Translations[original] = translation;
                    }
                }
            }
            catch (Exception ex) { Plugin.Log.LogError("Failed to load json: " + ex.Message); }
        }

        private static void LoadRegexTranslations()
        {
            string path = Path.Combine(FileManager.CurrentLanguagePath, "Strings", "translation_regexs.json");
            if (!File.Exists(path)) return;

            try
            {
                string jsonContent = File.ReadAllText(path, System.Text.Encoding.UTF8);
                var matches = Regex.Matches(jsonContent, @"""((?:[^""\\]|\\.)*)""\s*:\s*""((?:[^""\\]|\\.)*)""");

                foreach (Match match in matches)
                {
                    string pattern = UnescapeJson(match.Groups[1].Value);
                    string translation = UnescapeJson(match.Groups[2].Value);
                    translation = translation.Replace("{0}", "$1").Replace("{1}", "$2");

                    if (!string.IsNullOrEmpty(translation)) RegexTranslations[pattern] = translation;
                }
            }
            catch (Exception ex) { Plugin.Log.LogError("Failed to load regex: " + ex.Message); }
        }

        private static void LoadUIOverrides()
        {
            string path = Path.Combine(FileManager.CurrentLanguagePath, "Strings", "ui_overrides.json");
            if (!File.Exists(path)) return;

            try
            {
                string jsonContent = File.ReadAllText(path, System.Text.Encoding.UTF8);
                var matches = Regex.Matches(jsonContent, @"""((?:[^""\\]|\\.)*)""\s*:\s*""((?:[^""\\]|\\.)*)""");

                foreach (Match match in matches)
                {
                    string uiPath = match.Groups[1].Value.Trim();
                    string props = match.Groups[2].Value.Trim();
                    if (!string.IsNullOrEmpty(props)) UIOverrides[uiPath] = props;
                }
            }
            catch (Exception ex) { Plugin.Log.LogError("Failed to load ui_overrides: " + ex.Message); }
        }

        private static string UnescapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\\"", "\"").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t").Replace("\\\\", "\\");
        }
    }
}
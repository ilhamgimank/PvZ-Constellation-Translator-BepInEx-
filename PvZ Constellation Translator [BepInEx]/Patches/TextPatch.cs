using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using PvZStarSignTranslator.Managers;
using PvZStarSignTranslator.Features;

namespace PvZStarSignTranslator.Patches
{
    public static class TextPatch
    {
        public static bool IsApplying = false;

        // Cache untuk menyimpan teks asli sebelum diterjemahkan agar tidak hilang saat ganti bahasa
        public static Dictionary<Component, string> OriginalTextCache = new Dictionary<Component, string>();

        public static void PatchAll(Harmony harmony)
        {
            try
            {
                var setter = AccessTools.PropertySetter(typeof(Text), "text");
                harmony.Patch(setter, prefix: new HarmonyMethod(typeof(TextPatch), nameof(GenericPrefix)));

                // Patch OnEnable agar teks langsung diterjemahkan saat UI muncul
                var onEnable = AccessTools.Method(typeof(Text), "OnEnable");
                if (onEnable != null)
                {
                    harmony.Patch(onEnable, postfix: new HarmonyMethod(typeof(TextPatch), nameof(UGUI_OnEnable_Postfix)));
                }
                Plugin.Log.LogInfo("Hooked UGUI Text system.");
            }
            catch { }

            try
            {
                System.Type tmpType = AccessTools.TypeByName("TMPro.TMP_Text");
                if (tmpType != null)
                {
                    var setter = AccessTools.PropertySetter(tmpType, "text");
                    harmony.Patch(setter, prefix: new HarmonyMethod(typeof(TextPatch), nameof(GenericPrefix)));

                    var onEnable = AccessTools.Method(tmpType, "OnEnable");
                    if (onEnable != null)
                    {
                        harmony.Patch(onEnable, postfix: new HarmonyMethod(typeof(TextPatch), nameof(TMP_OnEnable_Postfix)));
                    }
                    Plugin.Log.LogInfo("Hooked TextMeshPro system.");
                }
            }
            catch { }
        }

        private static void GenericPrefix(Component __instance, ref string value)
        {
            if (string.IsNullOrEmpty(value) || IsApplying || __instance == null) return;

            // Simpan teks asli ke cache jika belum ada
            if (!OriginalTextCache.ContainsKey(__instance))
            {
                OriginalTextCache[__instance] = value;
            }

            TextDumper.DumpText(value, __instance.GetType().Name);

            // Proses translasi
            if (TranslationManager.Translations.TryGetValue(value.Trim(), out string translated))
            {
                value = translated;
            }
            else
            {
                foreach (var regexEntry in TranslationManager.RegexTranslations)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(value, regexEntry.Key))
                    {
                        value = System.Text.RegularExpressions.Regex.Replace(value, regexEntry.Key, regexEntry.Value);
                        break;
                    }
                }
            }
        }

        private static void UGUI_OnEnable_Postfix(Text __instance)
        {
            if (__instance == null || string.IsNullOrEmpty(__instance.text)) return;
            string val = __instance.text;
            GenericPrefix(__instance, ref val);
            if (__instance.text != val)
            {
                IsApplying = true;
                __instance.text = val;
                IsApplying = false;
            }
        }

        private static void TMP_OnEnable_Postfix(Component __instance)
        {
            if (__instance == null) return;
            var prop = __instance.GetType().GetProperty("text");
            if (prop != null)
            {
                string val = prop.GetValue(__instance, null) as string;
                if (string.IsNullOrEmpty(val)) return;

                GenericPrefix(__instance, ref val);
                if (val != (prop.GetValue(__instance, null) as string))
                {
                    IsApplying = true;
                    prop.SetValue(__instance, val, null);
                    IsApplying = false;
                }
            }
        }

        // Fungsi vital untuk memperbarui semua teks di layar saat ganti bahasa
        public static void RefreshAllTexts()
        {
            try
            {
                // Bersihkan cache dari objek yang sudah hancur (dead components)
                List<Component> deadKeys = new List<Component>();
                foreach (var kvp in OriginalTextCache) if (kvp.Key == null) deadKeys.Add(kvp.Key);
                foreach (var k in deadKeys) OriginalTextCache.Remove(k);

                // Update UGUI
                Text[] allTexts = Resources.FindObjectsOfTypeAll<Text>();
                foreach (Text t in allTexts)
                {
                    if (t == null || t.gameObject.scene.name == null) continue;
                    if (OriginalTextCache.TryGetValue(t, out string orig))
                    {
                        string translated = orig;
                        GenericPrefix(t, ref translated);
                        IsApplying = true;
                        t.text = translated;
                        IsApplying = false;
                    }
                }

                // Update TMPro
                System.Type tmpType = AccessTools.TypeByName("TMPro.TMP_Text");
                if (tmpType != null)
                {
                    UnityEngine.Object[] allTMPs = Resources.FindObjectsOfTypeAll(tmpType);
                    var prop = tmpType.GetProperty("text");
                    foreach (UnityEngine.Object obj in allTMPs)
                    {
                        Component tmp = obj as Component;
                        if (tmp == null || tmp.gameObject.scene.name == null || prop == null) continue;
                        if (OriginalTextCache.TryGetValue(tmp, out string orig))
                        {
                            string translated = orig;
                            GenericPrefix(tmp, ref translated);
                            IsApplying = true;
                            prop.SetValue(tmp, translated, null);
                            IsApplying = false;
                        }
                    }
                }
                Plugin.Log.LogInfo("All screen texts refreshed successfully.");
            }
            catch (Exception ex) { Plugin.Log.LogError("RefreshAllTexts Error: " + ex.Message); }
        }
    }
}
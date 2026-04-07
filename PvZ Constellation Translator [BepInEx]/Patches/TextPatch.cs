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

        public static void PatchAll(Harmony harmony)
        {
            try
            {
                var setter = AccessTools.PropertySetter(typeof(Text), "text");
                harmony.Patch(setter, prefix: new HarmonyMethod(typeof(TextPatch), nameof(GenericPrefix)));
                Plugin.Log.LogInfo("Hooked UGUI Text.");
            }
            catch { }

            try
            {
                System.Type tmpType = AccessTools.TypeByName("TMPro.TMP_Text");
                if (tmpType != null)
                {
                    var setter = AccessTools.PropertySetter(tmpType, "text");
                    harmony.Patch(setter, prefix: new HarmonyMethod(typeof(TextPatch), nameof(GenericPrefix)));
                    Plugin.Log.LogInfo("Hooked TextMeshPro.");
                }
            }
            catch { }
        }

        private static void GenericPrefix(Component __instance, ref string value)
        {
            if (string.IsNullOrEmpty(value) || IsApplying) return;

            TextDumper.DumpText(value, __instance.GetType().Name);

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
    }
}
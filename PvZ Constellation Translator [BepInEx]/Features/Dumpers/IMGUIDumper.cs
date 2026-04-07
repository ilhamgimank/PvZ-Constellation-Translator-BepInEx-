using HarmonyLib;
using UnityEngine;
using PvZStarSignTranslator.Features;

namespace PvZStarSignTranslator.Patches.Dumpers
{
    public static class TextDumper_IMGUI
    {
        public static void Patch(Harmony harmony)
        {
            try
            {
                // Menangkap semua string yang masuk ke sistem UI lawas Unity (GUI.Label, GUI.Box, dll)
                var method = AccessTools.Method(typeof(GUIContent), "Temp", new[] { typeof(string) });
                if (method != null)
                {
                    harmony.Patch(method, prefix: new HarmonyMethod(typeof(TextDumper_IMGUI), nameof(Prefix)));
                    Plugin.Log.LogInfo("IMGUI Text Dumper initialized.");
                }
            }
            catch { }
        }

        private static void Prefix(ref string t)
        {
            if (TextDumper.EnableIMGUI && !string.IsNullOrEmpty(t))
            {
                TextDumper.DumpText(t, "IMGUI");
            }
        }
    }
}
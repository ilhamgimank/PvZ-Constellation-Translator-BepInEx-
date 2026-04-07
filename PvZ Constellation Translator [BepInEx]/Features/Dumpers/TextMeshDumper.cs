using HarmonyLib;
using UnityEngine;
using PvZStarSignTranslator.Features;

namespace PvZStarSignTranslator.Patches.Dumpers
{
    public static class TextDumper_TextMesh
    {
        public static void Patch(Harmony harmony)
        {
            try
            {
                var setter = AccessTools.PropertySetter(typeof(TextMesh), "text");
                if (setter != null)
                {
                    harmony.Patch(setter, prefix: new HarmonyMethod(typeof(TextDumper_TextMesh), nameof(Prefix)));
                    Plugin.Log.LogInfo("TextMesh (Legacy 3D) Dumper initialized.");
                }
            }
            catch { }
        }

        private static void Prefix(ref string value)
        {
            if (TextDumper.EnableTextMesh && !string.IsNullOrEmpty(value))
            {
                TextDumper.DumpText(value, "TextMesh");
            }
        }
    }
}
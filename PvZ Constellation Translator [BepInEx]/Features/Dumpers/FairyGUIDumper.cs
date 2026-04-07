using HarmonyLib;
using PvZStarSignTranslator.Features;

namespace PvZStarSignTranslator.Patches.Dumpers
{
    public static class TextDumper_FairyGUI
    {
        public static void Patch(Harmony harmony)
        {
            try
            {
                var type = AccessTools.TypeByName("FairyGUI.GTextField");
                if (type != null)
                {
                    var setter = AccessTools.PropertySetter(type, "text");
                    if (setter != null)
                    {
                        harmony.Patch(setter, prefix: new HarmonyMethod(typeof(TextDumper_FairyGUI), nameof(Prefix)));
                        Plugin.Log.LogInfo("FairyGUI Dumper initialized.");
                    }
                }
            }
            catch { }
        }

        private static void Prefix(ref string value)
        {
            if (TextDumper.EnableFairyGUI && !string.IsNullOrEmpty(value))
            {
                TextDumper.DumpText(value, "FairyGUI");
            }
        }
    }
}
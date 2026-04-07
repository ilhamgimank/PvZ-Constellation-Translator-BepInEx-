using HarmonyLib;
using PvZStarSignTranslator.Features;

namespace PvZStarSignTranslator.Patches.Dumpers
{
    public static class TextDumper_NGUI
    {
        public static void Patch(Harmony harmony)
        {
            try
            {
                var type = AccessTools.TypeByName("UILabel");
                if (type != null)
                {
                    var setter = AccessTools.PropertySetter(type, "text");
                    if (setter != null)
                    {
                        harmony.Patch(setter, prefix: new HarmonyMethod(typeof(TextDumper_NGUI), nameof(Prefix)));
                        Plugin.Log.LogInfo("NGUI Dumper initialized.");
                    }
                }
            }
            catch { }
        }

        private static void Prefix(ref string value)
        {
            if (TextDumper.EnableNGUI && !string.IsNullOrEmpty(value))
            {
                TextDumper.DumpText(value, "NGUI");
            }
        }
    }
}
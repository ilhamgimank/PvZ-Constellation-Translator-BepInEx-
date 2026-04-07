using HarmonyLib;
using PvZStarSignTranslator.Features;

namespace PvZStarSignTranslator.Patches.Dumpers
{
    public static class TextDumper_TMP
    {
        public static void Patch(Harmony harmony)
        {
            try
            {
                // Mencari tipe kelas TextMeshPro (TMP_Text) dari library pihak ketiga
                var tmpType = AccessTools.TypeByName("TMPro.TMP_Text");
                if (tmpType != null)
                {
                    // Menghubungkan (hook) ke fungsi 'set_text' dari TextMeshPro
                    var setter = AccessTools.PropertySetter(tmpType, "text");
                    if (setter != null)
                    {
                        harmony.Patch(setter, prefix: new HarmonyMethod(typeof(TextDumper_TMP), nameof(Prefix)));
                        Plugin.Log.LogInfo("TextMeshPro (TMP) Dumper initialized.");
                    }
                    else
                    {
                        Plugin.Log.LogWarning("TextMeshPro (TMP) Dumper: Failed to find property setter 'text'.");
                    }
                }
                else
                {
                    Plugin.Log.LogWarning("TextMeshPro (TMP) Dumper: TMPro library not found in this game.");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError("Failed to initialize TextMeshPro Dumper: " + ex.Message);
            }
        }

        // Fungsi ini akan dipanggil setiap kali game mencoba mengubah teks TextMeshPro
        private static void Prefix(ref string value)
        {
            // Jika fitur dumper untuk TMP diaktifkan dan teksnya tidak kosong
            if (TextDumper.EnableTMP && !string.IsNullOrEmpty(value))
            {
                // Kirim teksnya ke sistem Auto Dumper dengan label sumber "TextMeshPro"
                TextDumper.DumpText(value, "TextMeshPro");
            }
        }
    }
}
using UnityEngine;

namespace PvZStarSignTranslator.Features.Dumpers
{
    public static class TMPDumper
    {
        // Mengekstrak teks dari TextMeshPro menggunakan reflection agar aman
        public static string Extract(GameObject obj)
        {
            if (TextDumper.TryGetReflectionText(obj, "TMPro.TextMeshProUGUI", out string tmpUI)) return tmpUI;
            if (TextDumper.TryGetReflectionText(obj, "TMPro.TextMeshPro", out string tmp3D)) return tmp3D;
            return "";
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace PvZStarSignTranslator.Features.Dumpers
{
    public static class UGUIDumper
    {
        // Mengekstrak teks dari komponen UI Text bawaan Unity
        public static string Extract(GameObject obj)
        {
            if (obj.TryGetComponent<Text>(out var uiText)) return uiText.text;
            return "";
        }
    }
}
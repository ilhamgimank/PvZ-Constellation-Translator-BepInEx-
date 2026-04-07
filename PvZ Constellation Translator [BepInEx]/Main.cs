using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using PvZStarSignTranslator.Managers;
using PvZStarSignTranslator.Features;
using PvZStarSignTranslator.Patches;

namespace PvZStarSignTranslator
{
    // Update ke v0.2.1: Polish UI (Dialog Konfirmasi & Notifikasi)
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo(string.Format("Initializing {0} v{1}...", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION));

            // 1. Inisialisasi struktur file
            FileManager.Initialize();

            // 2. Memuat data translasi
            TranslationManager.LoadTranslations();

            // 3. Mengaktifkan Harmony Patching
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            TextPatch.PatchAll(_harmony);

            Log.LogInfo(string.Format("Mod engine v{0} is now active.", MyPluginInfo.PLUGIN_VERSION));
        }

        private void Update()
        {
            // Menjalankan scanner (Ctrl + RMB)
            PathDetector.HandleInput();

            // Menjalankan logika menu bahasa di dalam game
            LanguageMenu.Update();
        }
    }

    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "com.ilhamgimank.pvz.starsign.translator";
        public const string PLUGIN_NAME = "PvZ Constellation Translator";
        public const string PLUGIN_VERSION = "0.2.1"; // Versi naik ke 0.2.1
    }
}
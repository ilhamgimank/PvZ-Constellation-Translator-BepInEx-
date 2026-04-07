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
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            // Menggunakan string.Format untuk kompatibilitas C# 7.3
            Log.LogInfo(string.Format("Starting {0} v{1}...", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION));

            FileManager.Initialize();
            TranslationManager.LoadTranslations();

            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            TextPatch.PatchAll(_harmony);

            Log.LogInfo("All systems are online. Localization patches applied.");
        }

        private void Update()
        {
            PathDetector.HandleInput();
        }
    }

    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "com.ilhamgimank.pvz.starsign.translator";
        public const string PLUGIN_NAME = "PvZ Constellation Translator";
        public const string PLUGIN_VERSION = "0.1.0";
    }
}
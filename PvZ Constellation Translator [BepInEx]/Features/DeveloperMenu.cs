using UnityEngine;
using PvZStarSignTranslator.Managers;
using PvZStarSignTranslator.Patches;
using System.Runtime.InteropServices;

namespace PvZStarSignTranslator.Features
{
    // Komponen MonoBehaviour untuk menampilkan menu In-Game Developer Toolkit (F12)
    public class DeveloperMenu : MonoBehaviour
    {
        public static bool IsOpen = false;
        public static bool EnableTextTranslation = true;
        public static bool EnableUIOverrides = true;
        public static bool EnableRegex = true;

        public static bool IsConsoleVisible = true;

        private Rect windowRect;
        private bool isWindowInitialized = false;
        private Vector2 scrollPosition = Vector2.zero;

        private int queueTextCount = 0;
        private int translatedTextCount = 0;
        private float statusUpdateTimer = 0f;

        [DllImport("kernel32.dll")]
        private static extern System.IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        public static void SetConsoleVisibility(bool show)
        {
            try
            {
                System.IntPtr handle = GetConsoleWindow();
                if (handle != System.IntPtr.Zero) ShowWindow(handle, show ? SW_SHOW : SW_HIDE);
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogWarning(string.Format("[Console] Gagal mengubah visibilitas konsol: {0}", ex.Message));
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                IsOpen = !IsOpen;
                if (IsOpen) UpdateStatusCounts();
            }

            if (Input.GetKeyDown(KeyCode.F5)) ReloadTranslationsCommand();

            if (IsOpen)
            {
                statusUpdateTimer -= Time.unscaledDeltaTime;
                if (statusUpdateTimer <= 0)
                {
                    statusUpdateTimer = 2f;
                    UpdateStatusCounts();
                }
            }
        }

        private void ReloadTranslationsCommand()
        {
            Plugin.Log.LogInfo("[Command] Memuat ulang kamus terjemahan dari file JSON...");
            TranslationManager.LoadTranslations();

            TextPatch.OriginalTextCache.Clear();
            TextPatch.RefreshAllTexts();
            UpdateStatusCounts();

            NotificationSystem.CreateNotificationUI("Translation Reloaded!", Color.green);
        }

        private void UpdateStatusCounts()
        {
            string dumpsPath = System.IO.Path.Combine(FileManager.DumpsFolder, "untranslated_strings.json");
            queueTextCount = GetJsonEntryCount(dumpsPath, "\": \"");

            string translationPath = System.IO.Path.Combine(FileManager.LocalizationFolder, LanguageMenu.CurrentLanguage, "Strings", "translation_strings.json");
            translatedTextCount = GetJsonEntryCount(translationPath, "\": \"");
        }

        private int GetJsonEntryCount(string filePath, string keyToCount)
        {
            if (!System.IO.File.Exists(filePath)) return 0;
            try
            {
                string content = System.IO.File.ReadAllText(filePath);
                int count = 0, index = 0;
                while ((index = content.IndexOf(keyToCount, index)) != -1)
                {
                    count++;
                    index += keyToCount.Length;
                }
                return count;
            }
            catch { return 0; }
        }

        void OnGUI()
        {
            if (!IsOpen) return;

            if (!isWindowInitialized)
            {
                float windowHeight = Mathf.Min(800f, Screen.height - 40f);
                windowRect = new Rect(Screen.width - 380, 20, 360, windowHeight);
                isWindowInitialized = true;
            }

            GUI.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            windowRect = GUI.Window(999, windowRect, DrawWindow, "PvZ Constellation Toolkit");
        }

        void DrawWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, windowRect.width - 35, 20));
            if (GUI.Button(new Rect(windowRect.width - 30, 2, 25, 20), "X")) IsOpen = false;

            GUILayout.Space(10);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Mod Version: {0}", MyPluginInfo.PLUGIN_VERSION), GUILayout.Width(150));
            GUILayout.FlexibleSpace();
            GUILayout.Label("Game Version: " + Application.version, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight });
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUIStyle langStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            langStyle.normal.textColor = Color.green;
            GUILayout.Label("Active Language : " + LanguageMenu.CurrentLanguage, langStyle);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            // --- CUSTOM FONT (BELUM ADA - DISABLE) ---
            GUI.enabled = false;
            GUILayout.BeginVertical("box");
            DrawSectionHeader("---- Custom Font Settings ----");
            GUILayout.Button("Browse Custom Font... (WIP)", GUILayout.Height(35));
            GUILayout.Label("Feature not yet implemented in StarSign.", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = Color.gray } });
            GUILayout.EndVertical();
            GUI.enabled = true;
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            DrawSectionHeader("---- Command Panel ----");
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Reload\nTranslation [F5]", GUILayout.Height(40))) ReloadTranslationsCommand();

            GUI.enabled = false;
            GUILayout.Button("Reload\nTexture [F6]", GUILayout.Height(40));
            GUILayout.Button("Manual\nHook Text [F7]", GUILayout.Height(40));
            GUI.enabled = true;

            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            bool prevConsoleState = IsConsoleVisible;
            IsConsoleVisible = DrawToggle("Show Console (CMD) Window", IsConsoleVisible);
            if (IsConsoleVisible != prevConsoleState) SetConsoleVisibility(IsConsoleVisible);

            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            DrawSectionHeader("---- Text Translation Settings ----");

            EnableTextTranslation = DrawToggle("Enable Text Translation", EnableTextTranslation);

            GUI.enabled = false;
            DrawToggle("Enable Almanac Translation", false);
            DrawToggle("Auto Indent Almanac Text", false);
            GUI.enabled = true;

            EnableUIOverrides = DrawToggle("Enable UI Overrides", EnableUIOverrides);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUI.enabled = false;
            GUILayout.BeginVertical("box");
            DrawSectionHeader("---- Image & Texture Settings ----");
            DrawToggle("1. Use Modded Textures", false);
            GUILayout.Space(5);
            GUILayout.Label("2. Texture Dumper:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            DrawToggle("   ↳ Normal Scanning (Auto)", false);
            GUILayout.BeginHorizontal();
            GUILayout.Label("   ↳ Advanced Scanning (Mass)", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold }, GUILayout.Width(220));
            GUILayout.FlexibleSpace();
            GUILayout.Button("DUMP NOW", new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold }, GUILayout.Width(90));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.enabled = true;
            GUILayout.Space(10);

            GUI.enabled = false;
            GUILayout.BeginVertical("box");
            DrawSectionHeader("---- Almanac Dumping ----");
            DrawToggle("Enable Almanac Dumper", false);
            GUILayout.EndVertical();
            GUI.enabled = true;
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            DrawSectionHeader("---- Text Dumping & Scanning ----");

            bool prevDumpState = TextDumper.EnableDump;
            TextDumper.EnableDump = DrawToggle("Enable Text Auto-Dumper", TextDumper.EnableDump);
            if (TextDumper.EnableDump != prevDumpState)
            {
                NotificationSystem.CreateNotificationUI(string.Format("Auto Dumper {0}", TextDumper.EnableDump ? "ON" : "OFF"), TextDumper.EnableDump ? Color.green : Color.red);
            }

            PathDetector.IsEnabled = DrawToggle("Enable Path Scanner (Ctrl+RMB)", PathDetector.IsEnabled);
            PathDetector.IsAdvanced = DrawToggle("Enable Adv. Scanner Details", PathDetector.IsAdvanced);
            EnableRegex = DrawToggle("Enable Regex Processing", EnableRegex);

            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            DrawSectionHeader("---- Text Detection ----");
            TextDumper.EnableIMGUI = DrawToggle("Enable IMGUI", TextDumper.EnableIMGUI);
            TextDumper.EnableUGUI = DrawToggle("Enable UGUI", TextDumper.EnableUGUI);
            TextDumper.EnableNGUI = DrawToggle("Enable NGUI", TextDumper.EnableNGUI);
            TextDumper.EnableTextMesh = DrawToggle("Enable TextMesh", TextDumper.EnableTextMesh);
            TextDumper.EnableTMP = DrawToggle("Enable TextMeshPro", TextDumper.EnableTMP);
            TextDumper.EnableFairyGUI = DrawToggle("Enable FairyGUI", TextDumper.EnableFairyGUI);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUI.enabled = false;
            GUILayout.BeginVertical("box");
            DrawSectionHeader("---- Special Features ----");
            DrawToggle("Enable Auto Translate (Google)", false);
            DrawToggle("Enable Currency Conversion", false);
            GUILayout.EndVertical();
            GUI.enabled = true;
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            DrawSectionHeader("---- Status ----");
            DrawStatusRow("Queue Text (To Translate)", queueTextCount.ToString());
            DrawStatusRow("Text Translated (JSON)", translatedTextCount.ToString());

            GUI.enabled = false;
            DrawStatusRow("Plants Dumped", "N/A", 70f);
            DrawStatusRow("Zombies Dumped", "N/A", 70f);
            GUI.enabled = true;

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawSectionHeader(string title)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUILayout.Label(title, style);
            GUILayout.Space(5);
        }

        private bool DrawToggle(string label, bool value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold }, GUILayout.Width(240));
            GUILayout.FlexibleSpace();

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.normal.textColor = value ? Color.green : Color.red;

            if (GUILayout.Button(value ? "ON" : "OFF", btnStyle, GUILayout.Width(50))) value = !value;
            GUILayout.EndHorizontal();
            return value;
        }

        private void DrawStatusRow(string label, string value, float width = 50f)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.FlexibleSpace();
            GUILayout.Label(value, new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold }, GUILayout.Width(width));
            GUILayout.EndHorizontal();
        }
    }
}
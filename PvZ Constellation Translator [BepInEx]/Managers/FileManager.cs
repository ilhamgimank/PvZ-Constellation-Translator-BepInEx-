using System.IO;
using System.Text;
using BepInEx;

namespace PvZStarSignTranslator.Managers
{
    public static class FileManager
    {
        public static string RootFolder;
        public static string LocalizationFolder;
        public static string DumpsFolder;
        public static string CurrentLanguagePath;

        public static void Initialize()
        {
            RootFolder = Path.Combine(Paths.PluginPath, "PvZ Constellation Translator");
            LocalizationFolder = Path.Combine(RootFolder, "Localization");
            DumpsFolder = Path.Combine(RootFolder, "Dumps");
            CurrentLanguagePath = Path.Combine(LocalizationFolder, "Bahasa Indonesia");

            CreateDirectorySafe(RootFolder);
            CreateDirectorySafe(LocalizationFolder);
            CreateDirectorySafe(DumpsFolder);
            CreateDirectorySafe(CurrentLanguagePath);
            CreateDirectorySafe(Path.Combine(CurrentLanguagePath, "Strings"));

            InitializeBaseFiles();

            Plugin.Log.LogInfo("File system structure initialized.");
        }

        private static void CreateDirectorySafe(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        private static void InitializeBaseFiles()
        {
            string stringPath = Path.Combine(CurrentLanguagePath, "Strings", "translation_strings.json");
            if (!File.Exists(stringPath))
            {
                string defaultJson = "{\n  \"Adventure\": \"Petualangan\",\n  \"Options\": \"Pengaturan\"\n}";
                File.WriteAllText(stringPath, defaultJson, Encoding.UTF8);
            }

            string dumpPath = Path.Combine(DumpsFolder, "untranslated_strings.json");
            if (!File.Exists(dumpPath)) File.WriteAllText(dumpPath, "{\n\n}", Encoding.UTF8);
        }
    }
}
#pragma warning disable IDE0031, IDE0270

using BepInEx.Configuration;
using PvZStarSignTranslator.Managers;
using PvZStarSignTranslator.Patches;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace PvZStarSignTranslator.Features
{
    // Class statis ini bertugas menangani seluruh logika Menu Bahasa di dalam game
    public static class LanguageMenu
    {
        // ==========================================================
        // VARIABEL OBJEK UI
        // ==========================================================
        private static GameObject customDropdownBtn;
        private static GameObject dropdownListPanel;
        private static GameObject languageLabelObj;
        private static GameObject languageConfirmPanel;

        public static bool isDropdownOpen = false;
        public static string CurrentLanguage = "Chinese";

        // ==========================================================
        // VARIABEL KONFIGURASI
        // ==========================================================
        public static ConfigFile ModConfig;
        public static ConfigEntry<string> ConfigLanguage;
        public static ConfigEntry<int> ConfigLabelFontSize;
        public static ConfigEntry<float> ConfigLabelOffsetX;
        public static ConfigEntry<float> ConfigLabelOffsetY;
        public static ConfigEntry<int> ConfigButtonFontSize;
        public static ConfigEntry<int> ConfigDialogTitleFontSize;

        public static void InitConfig()
        {
            string configPath = Path.Combine(FileManager.RootFolder, "config.ini");
            ModConfig = new ConfigFile(configPath, true);

            ConfigLanguage = ModConfig.Bind("General", "Language", "Chinese", "Bahasa yang aktif saat ini.");
            ConfigLabelFontSize = ModConfig.Bind("UI Settings", "LabelFontSize", 35, "Ukuran teks label 'Language'.");
            ConfigLabelOffsetX = ModConfig.Bind("UI Settings", "LabelOffsetX", 0f, "Geseran posisi X (Kanan/Kiri) untuk label.");
            ConfigLabelOffsetY = ModConfig.Bind("UI Settings", "LabelOffsetY", 65f, "Geseran posisi Y (Atas/Bawah) untuk label.");
            ConfigButtonFontSize = ModConfig.Bind("UI Settings", "ButtonFontSize", 30, "Ukuran teks di dalam tombol dropdown.");
            ConfigDialogTitleFontSize = ModConfig.Bind("UI Settings", "DialogTitleFontSize", 40, "Ukuran teks judul pada dialog konfirmasi bahasa.");

            CurrentLanguage = ConfigLanguage.Value;
        }

        public static void Update()
        {
            if (ModConfig == null) InitConfig();

            GameObject targetBtn = GameObject.Find("/菜单界面画布/面板/调节关卡面板/按键区/一键获取所有植物按键");

            if (targetBtn != null && targetBtn.activeInHierarchy)
            {
                if (customDropdownBtn == null) CreateDropdownMenu(targetBtn);
            }
            else if (customDropdownBtn != null && customDropdownBtn.activeSelf)
            {
                HideAllUI();
            }
        }

        private static void HideAllUI()
        {
            customDropdownBtn?.SetActive(false);
            languageLabelObj?.SetActive(false);
            dropdownListPanel?.SetActive(false);
            languageConfirmPanel?.SetActive(false);
            isDropdownOpen = false;
        }

        private static void CreateDropdownMenu(GameObject targetBtn)
        {
            customDropdownBtn = Object.Instantiate(targetBtn, targetBtn.transform.parent);
            customDropdownBtn.name = "LanguageDropdownMenu";

            RectTransform rt = customDropdownBtn.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + rt.rect.width + 20f, rt.anchoredPosition.y);

            Button btn = customDropdownBtn.GetComponent<Button>();
            btn.onClick = new Button.ButtonClickedEvent();
            btn.onClick.AddListener(ToggleDropdown);

            CreateLanguageLabel(rt);
            UpdateMenuVisuals();
        }

        private static void CreateLanguageLabel(RectTransform buttonRt)
        {
            languageLabelObj = new GameObject("LanguageLabelText");
            languageLabelObj.transform.SetParent(customDropdownBtn.transform.parent, false);

            Text labelTxt = languageLabelObj.AddComponent<Text>();
            labelTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelTxt.fontSize = ConfigLabelFontSize.Value;
            labelTxt.color = Color.white;
            labelTxt.alignment = TextAnchor.MiddleCenter;
            labelTxt.horizontalOverflow = HorizontalWrapMode.Overflow;

            languageLabelObj.AddComponent<Outline>().effectColor = Color.black;

            RectTransform labelRt = languageLabelObj.GetComponent<RectTransform>();
            labelRt.sizeDelta = new Vector2(buttonRt.rect.width, 40);

            labelRt.anchoredPosition = new Vector2(
                buttonRt.anchoredPosition.x + ConfigLabelOffsetX.Value,
                buttonRt.anchoredPosition.y + ConfigLabelOffsetY.Value
            );

            UpdateLabelText();
        }

        private static void UpdateLabelText()
        {
            if (languageLabelObj == null) return;
            Text txt = languageLabelObj.GetComponent<Text>();
            txt.text = (CurrentLanguage == "Indonesian") ? "Bahasa:" : (CurrentLanguage == "Chinese" ? "语言:" : "Language:");
        }

        private static void UpdateMenuVisuals()
        {
            if (customDropdownBtn == null) return;

            string arrow = isDropdownOpen ? " ▲" : " ▼";
            string fullText = CurrentLanguage + arrow;

            // Deteksi dan Modifikasi untuk UGUI Text
            Text t = customDropdownBtn.GetComponentInChildren<Text>();
            if (t != null)
            {
                t.text = fullText;
                t.resizeTextForBestFit = false;
                t.fontSize = ConfigButtonFontSize.Value;
                Plugin.Log.LogInfo("[Language Menu] Tipe teks terdeteksi: UGUI Text pada tombol dropdown utama.");
            }

            // Deteksi dan Modifikasi untuk TextMeshPro
            System.Type tmpType = HarmonyLib.AccessTools.TypeByName("TMPro.TMP_Text");
            if (tmpType != null)
            {
                var tmpComp = customDropdownBtn.GetComponentInChildren(tmpType);
                if (tmpComp != null)
                {
                    tmpType.GetProperty("text")?.SetValue(tmpComp, fullText, null);
                    tmpType.GetProperty("enableAutoSizing")?.SetValue(tmpComp, false, null);
                    tmpType.GetProperty("fontSize")?.SetValue(tmpComp, (float)ConfigButtonFontSize.Value, null);
                    Plugin.Log.LogInfo("[Language Menu] Tipe teks terdeteksi: TextMeshPro (TMP) pada tombol dropdown utama.");
                }
            }

            // Deteksi dan Modifikasi untuk Legacy TextMesh (3D Text)
            TextMesh tm = customDropdownBtn.GetComponentInChildren<TextMesh>();
            if (tm != null)
            {
                tm.text = fullText;
                Plugin.Log.LogInfo("[Language Menu] Tipe teks terdeteksi: TextMesh (Legacy 3D) pada tombol dropdown utama.");
            }
        }

        public static void ToggleDropdown()
        {
            isDropdownOpen = !isDropdownOpen;

            if (dropdownListPanel == null && isDropdownOpen) GenerateLanguageList();

            if (dropdownListPanel != null) dropdownListPanel.SetActive(isDropdownOpen);

            UpdateMenuVisuals();
        }

        private static void GenerateLanguageList()
        {
            dropdownListPanel = new GameObject("LanguageListPanel");
            dropdownListPanel.transform.SetParent(customDropdownBtn.transform, false);

            RectTransform listRt = dropdownListPanel.AddComponent<RectTransform>();

            listRt.anchorMin = new Vector2(0.5f, 0);
            listRt.anchorMax = new Vector2(0.5f, 0);
            listRt.pivot = new Vector2(0.5f, 1);
            listRt.anchoredPosition = new Vector2(0, -10);

            VerticalLayoutGroup vlg = dropdownListPanel.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 10;

            CreateLanguageItem("Chinese");
            CreateLanguageItem("English");
            CreateLanguageItem("Indonesian");

            RectTransform btnRt = customDropdownBtn.GetComponent<RectTransform>();
            listRt.sizeDelta = new Vector2(btnRt.rect.width, (3 * (btnRt.rect.height + 10)) + 10);
        }

        private static void CreateLanguageItem(string langName)
        {
            GameObject item = Object.Instantiate(customDropdownBtn, dropdownListPanel.transform);
            item.name = "Item_" + langName;

            Transform childPanel = item.transform.Find("LanguageListPanel");
            if (childPanel != null) Object.Destroy(childPanel.gameObject);

            // Deteksi untuk Item List: UGUI Text
            Text t = item.GetComponentInChildren<Text>();
            if (t != null)
            {
                t.text = langName;
                t.resizeTextForBestFit = false;
                t.fontSize = ConfigButtonFontSize.Value;
                Plugin.Log.LogInfo("[Language Menu] Tipe teks terdeteksi: UGUI Text pada item list '" + langName + "'.");
            }

            // Deteksi untuk Item List: TextMeshPro
            System.Type tmpType = HarmonyLib.AccessTools.TypeByName("TMPro.TMP_Text");
            if (tmpType != null)
            {
                var tmpComp = item.GetComponentInChildren(tmpType);
                if (tmpComp != null)
                {
                    tmpType.GetProperty("text")?.SetValue(tmpComp, langName, null);
                    tmpType.GetProperty("enableAutoSizing")?.SetValue(tmpComp, false, null);
                    tmpType.GetProperty("fontSize")?.SetValue(tmpComp, (float)ConfigButtonFontSize.Value, null);
                    Plugin.Log.LogInfo("[Language Menu] Tipe teks terdeteksi: TextMeshPro (TMP) pada item list '" + langName + "'.");
                }
            }

            // Deteksi untuk Item List: Legacy TextMesh (3D Text)
            TextMesh tm = item.GetComponentInChildren<TextMesh>();
            if (tm != null)
            {
                tm.text = langName;
                Plugin.Log.LogInfo("[Language Menu] Tipe teks terdeteksi: TextMesh (Legacy 3D) pada item list '" + langName + "'.");
            }

            Button btn = item.GetComponent<Button>();
            btn.onClick = new Button.ButtonClickedEvent();
            btn.onClick.AddListener(() => OnLanguageSelected(langName));
        }

        // ==========================================================
        // EKSEKUSI PEMILIHAN BAHASA & DIALOG KONFIRMASI
        // ==========================================================
        private static void OnLanguageSelected(string langName)
        {
            isDropdownOpen = false;
            dropdownListPanel?.SetActive(false);
            UpdateMenuVisuals();

            if (CurrentLanguage == langName)
            {
                string msg = langName == "Indonesian" ? "Bahasa ini sudah digunakan!" : (langName == "Chinese" ? "该语言已在使用！" : "Language already in use!");
                NotificationSystem.CreateNotificationUI(msg, Color.yellow);
                return;
            }

            ShowConfirmDialog(langName);
        }

        private static void ShowConfirmDialog(string targetLang)
        {
            if (languageConfirmPanel != null) Object.Destroy(languageConfirmPanel);

            GameObject mainCanvas = GameObject.Find("/菜单界面画布");
            if (mainCanvas == null)
            {
                Plugin.Log.LogWarning("Main Canvas tidak ditemukan, langsung ubah bahasa.");
                ApplyLanguage(targetLang);
                return;
            }

            // 1. Background Hitam Transparan (Blocker)
            languageConfirmPanel = new GameObject("LanguageConfirmPanel");
            languageConfirmPanel.transform.SetParent(mainCanvas.transform, false);
            languageConfirmPanel.transform.SetAsLastSibling();

            RectTransform bgRt = languageConfirmPanel.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            bgRt.localScale = Vector3.one;

            Image bgImg = languageConfirmPanel.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.85f);
            languageConfirmPanel.AddComponent<Button>();

            // 2. Kotak Dialog
            GameObject dialogBox = new GameObject("DialogBox");
            dialogBox.transform.SetParent(languageConfirmPanel.transform, false);
            RectTransform boxRt = dialogBox.AddComponent<RectTransform>();
            boxRt.localScale = Vector3.one; // Pastikan panel tidak gepeng
            boxRt.anchorMin = new Vector2(0.5f, 0.5f);
            boxRt.anchorMax = new Vector2(0.5f, 0.5f);
            boxRt.pivot = new Vector2(0.5f, 0.5f);
            boxRt.anchoredPosition = Vector2.zero;

            Image boxImg = dialogBox.AddComponent<Image>();

            // Cek path yang ada spasinya maupun tidak (mengantisipasi log scanner)
            GameObject bgSource = GameObject.Find("/菜单界面画布/面板/调节关卡面板/面板区/一键获取所有植物面 板/背景");
            if (bgSource == null) bgSource = GameObject.Find("/菜单界面画布/面板/调节关卡面板/面板区/一键获取所有植物面板/背景");

            if (bgSource != null && bgSource.TryGetComponent<Image>(out Image sourceImg) && sourceImg.sprite != null)
            {
                boxRt.sizeDelta = new Vector2(800, 520); // Skala proporsional 800x520 (aslinya 970x700)
                boxImg.sprite = sourceImg.sprite;
                boxImg.type = sourceImg.type;
                boxImg.color = Color.white; // Kembalikan ke warna tekstur asli
            }
            else
            {
                // Fallback kalau sprite gagal di-load
                boxRt.sizeDelta = new Vector2(750, 400);
                boxImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
                dialogBox.AddComponent<Outline>().effectColor = Color.white;
            }

            // 3. Teks Judul
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(dialogBox.transform, false);
            Text titleTxt = titleObj.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleTxt.fontSize = ConfigDialogTitleFontSize.Value;
            titleTxt.color = Color.white;
            titleTxt.alignment = TextAnchor.MiddleCenter;

            string titleMsg = $"Change language to {targetLang}?";
            if (CurrentLanguage == "Indonesian") titleMsg = $"Ganti bahasa ke {targetLang}?";
            else if (CurrentLanguage == "Chinese") titleMsg = $"将语言更改为 {targetLang}？";
            titleTxt.text = titleMsg;
            titleTxt.AddComponent<Outline>().effectColor = Color.black; // Biar lebih kebaca

            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.localScale = Vector3.one;
            titleRt.anchorMin = new Vector2(0.5f, 0.5f);
            titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.pivot = new Vector2(0.5f, 0.5f);
            titleRt.sizeDelta = new Vector2(700, 100);
            titleRt.anchoredPosition = new Vector2(0, 100); // Digeser agak ke atas

            // 4. Tombol Confirm
            GameObject btnConfirm = Object.Instantiate(customDropdownBtn, dialogBox.transform);
            btnConfirm.name = "BtnConfirm";
            RectTransform confRt = btnConfirm.GetComponent<RectTransform>();

            // [FIX] Reset Scale & Atur Ulang Ukuran agar Tombol Presisi
            confRt.localScale = Vector3.one;
            confRt.anchorMin = new Vector2(0.5f, 0.5f);
            confRt.anchorMax = new Vector2(0.5f, 0.5f);
            confRt.pivot = new Vector2(0.5f, 0.5f);
            confRt.sizeDelta = new Vector2(240, 80); // Set ukuran proporsional
            confRt.anchoredPosition = new Vector2(-160, -100); // Kiri Bawah

            string confStr = (CurrentLanguage == "Indonesian") ? "Iya" : (CurrentLanguage == "Chinese" ? "确定" : "Yes");
            SetButtonText(btnConfirm, confStr);

            Button confAction = btnConfirm.GetComponent<Button>();
            confAction.onClick = new Button.ButtonClickedEvent();
            confAction.onClick.AddListener(() => ApplyLanguage(targetLang));

            // 5. Tombol Cancel
            GameObject btnCancel = Object.Instantiate(customDropdownBtn, dialogBox.transform);
            btnCancel.name = "BtnCancel";
            RectTransform cancRt = btnCancel.GetComponent<RectTransform>();

            // [FIX] Reset Scale & Atur Ulang Ukuran agar Tombol Presisi
            cancRt.localScale = Vector3.one;
            cancRt.anchorMin = new Vector2(0.5f, 0.5f);
            cancRt.anchorMax = new Vector2(0.5f, 0.5f);
            cancRt.pivot = new Vector2(0.5f, 0.5f);
            cancRt.sizeDelta = new Vector2(240, 80); // Set ukuran proporsional
            cancRt.anchoredPosition = new Vector2(160, -100); // Kanan Bawah

            string cancStr = (CurrentLanguage == "Indonesian") ? "Ga" : (CurrentLanguage == "Chinese" ? "取消" : "No");
            SetButtonText(btnCancel, cancStr);

            Button cancAction = btnCancel.GetComponent<Button>();
            cancAction.onClick = new Button.ButtonClickedEvent();
            cancAction.onClick.AddListener(() => Object.Destroy(languageConfirmPanel));
        }

        private static void SetButtonText(GameObject btnObj, string newText)
        {
            Transform listChild = btnObj.transform.Find("LanguageListPanel");
            if (listChild != null) Object.Destroy(listChild.gameObject);

            // Deteksi untuk Tombol di Dialog: UGUI Text
            Text t = btnObj.GetComponentInChildren<Text>();
            if (t != null)
            {
                t.text = newText;
                t.resizeTextForBestFit = false;
                t.fontSize = ConfigButtonFontSize.Value; // Gunakan ukuran teks dari config
                t.alignment = TextAnchor.MiddleCenter;
                Plugin.Log.LogInfo("[Language Menu] Tipe teks terdeteksi: UGUI Text pada tombol dialog '" + btnObj.name + "'.");
            }

            // Deteksi untuk Tombol di Dialog: TextMeshPro
            System.Type tmpType = HarmonyLib.AccessTools.TypeByName("TMPro.TMP_Text");
            if (tmpType != null)
            {
                var tmpComp = btnObj.GetComponentInChildren(tmpType);
                if (tmpComp != null)
                {
                    tmpType.GetProperty("text")?.SetValue(tmpComp, newText, null);
                    tmpType.GetProperty("enableAutoSizing")?.SetValue(tmpComp, false, null);
                    tmpType.GetProperty("fontSize")?.SetValue(tmpComp, (float)ConfigButtonFontSize.Value, null);
                    Plugin.Log.LogInfo("[Language Menu] Tipe teks terdeteksi: TextMeshPro (TMP) pada tombol dialog '" + btnObj.name + "'.");
                }
            }

            // Deteksi untuk Tombol di Dialog: Legacy TextMesh (3D Text)
            TextMesh tm = btnObj.GetComponentInChildren<TextMesh>();
            if (tm != null)
            {
                tm.text = newText;
                Plugin.Log.LogInfo("[Language Menu] Tipe teks terdeteksi: TextMesh (Legacy 3D) pada tombol dialog '" + btnObj.name + "'.");
            }
        }

        private static void ApplyLanguage(string langName)
        {
            CurrentLanguage = langName;
            ConfigLanguage.Value = langName;
            ModConfig.Save();

            TranslationManager.LoadTranslations();
            UpdateLabelText();
            TextPatch.RefreshAllTexts();
            UpdateMenuVisuals();

            if (languageConfirmPanel != null) Object.Destroy(languageConfirmPanel);

            string msg = langName == "Indonesian" ? "Bahasa berhasil diubah!" : (langName == "Chinese" ? "语言更改成功！" : "Language changed successfully!");
            NotificationSystem.CreateNotificationUI(msg, Color.green);

            Plugin.Log.LogInfo(string.Format("Language applied: {0}", langName));
        }
    }
}
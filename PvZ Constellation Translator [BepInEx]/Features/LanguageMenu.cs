#pragma warning disable IDE0031, IDE0044 // Add readonly modifier, Remove unnecessary assignment

using UnityEngine;
using UnityEngine.UI;
using System.IO;
using BepInEx.Configuration;
using PvZStarSignTranslator.Managers;
using PvZStarSignTranslator.Patches;

namespace PvZStarSignTranslator.Features
{
    public static class LanguageMenu
    {
        private static GameObject customDropdownBtn;
        private static GameObject dropdownListPanel;
        private static GameObject languageLabelObj;
        private static GameObject languageConfirmPanel;

        public static bool isDropdownOpen = false;
        public static string CurrentLanguage = "Chinese";

        public static ConfigFile ModConfig;
        public static ConfigEntry<string> ConfigLanguage;
        public static ConfigEntry<int> ConfigLabelFontSize;
        public static ConfigEntry<float> ConfigLabelOffsetY;

        public static void InitConfig()
        {
            // Gunakan path internal project
            string configPath = Path.Combine(FileManager.RootFolder, "config.ini");
            ModConfig = new ConfigFile(configPath, true);

            ConfigLanguage = ModConfig.Bind("General", "Language", "Chinese", "Current active language.");
            ConfigLabelFontSize = ModConfig.Bind("UI Settings", "LabelFontSize", 35, "Size of the 'Language' label.");
            ConfigLabelOffsetY = ModConfig.Bind("UI Settings", "LabelOffsetY", 65f, "Y position of the label.");

            CurrentLanguage = ConfigLanguage.Value;
        }

        public static void Update()
        {
            if (ModConfig == null) InitConfig();

            // Path tombola target di PvZ Constellation
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
            labelRt.anchoredPosition = new Vector2(buttonRt.anchoredPosition.x, buttonRt.anchoredPosition.y + ConfigLabelOffsetY.Value);

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

            // Update Legacy Text
            Text t = customDropdownBtn.GetComponentInChildren<Text>();
            if (t != null) t.text = fullText;

            // Update TMP via Reflection
            System.Type tmpType = HarmonyLib.AccessTools.TypeByName("TMPro.TMP_Text");
            if (tmpType != null)
            {
                var tmpComp = customDropdownBtn.GetComponentInChildren(tmpType);
                if (tmpComp != null) tmpType.GetProperty("text")?.SetValue(tmpComp, fullText, null);
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
            listRt.anchorMin = new Vector2(0.5f, 1);
            listRt.anchorMax = new Vector2(0.5f, 1);
            listRt.pivot = new Vector2(0.5f, 0);
            listRt.anchoredPosition = new Vector2(0, 10);

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

            Text t = item.GetComponentInChildren<Text>();
            if (t != null) t.text = langName;

            System.Type tmpType = HarmonyLib.AccessTools.TypeByName("TMPro.TMP_Text");
            if (tmpType != null)
            {
                var tmpComp = item.GetComponentInChildren(tmpType);
                if (tmpComp != null) tmpType.GetProperty("text")?.SetValue(tmpComp, langName, null);
            }

            Button btn = item.GetComponent<Button>();
            btn.onClick = new Button.ButtonClickedEvent();
            btn.onClick.AddListener(() => OnLanguageSelected(langName));
        }

        private static void OnLanguageSelected(string langName)
        {
            isDropdownOpen = false;
            dropdownListPanel?.SetActive(false);

            if (CurrentLanguage != langName) ApplyLanguage(langName);
            UpdateMenuVisuals();
        }

        private static void ApplyLanguage(string langName)
        {
            CurrentLanguage = langName;
            ConfigLanguage.Value = langName;
            ModConfig.Save();

            TranslationManager.LoadTranslations();
            UpdateLabelText();

            // Sekarang memanggil fungsi yang sudah ada di TextPatch
            TextPatch.RefreshAllTexts();

            Plugin.Log.LogInfo(string.Format("Language applied: {0}", langName));
        }
    }
}
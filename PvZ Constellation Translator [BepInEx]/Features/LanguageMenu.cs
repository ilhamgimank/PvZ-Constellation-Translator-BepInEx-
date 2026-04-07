#pragma warning disable IDE0031 // Menyembunyikan peringatan Visual Studio tentang pengecekan null yang bisa disingkat

using UnityEngine;
using UnityEngine.UI;
using System.IO;
using BepInEx.Configuration; // Wajib untuk menggunakan fitur ConfigFile (config.ini)
using PvZStarSignTranslator.Managers;
using PvZStarSignTranslator.Patches;

namespace PvZStarSignTranslator.Features
{
    // Class statis ini bertugas menangani seluruh logika Menu Bahasa di dalam game
    public static class LanguageMenu
    {
        // ==========================================================
        // VARIABEL OBJEK UI (Menyimpan referensi elemen yang kita buat)
        // ==========================================================
        private static GameObject customDropdownBtn;   // Tombol utama menu bahasa
        private static GameObject dropdownListPanel;   // Panel background yang isinya daftar bahasa (Indo, Eng, dll)
        private static GameObject languageLabelObj;    // Teks tulisan "Language:" di atas tombol
        private static GameObject languageConfirmPanel;// Panel pop-up konfirmasi

        // Variabel Status & Data
        public static bool isDropdownOpen = false;     // Menyimpan status apakah daftar dropdown lagi kebuka atau ketutup
        public static string CurrentLanguage = "Chinese"; // Bahasa bawaan saat game baru pertama kali dibuka

        // ==========================================================
        // VARIABEL KONFIGURASI (Untuk menyimpan pengaturan ke config.ini)
        // ==========================================================
        public static ConfigFile ModConfig;
        public static ConfigEntry<string> ConfigLanguage;       // Menyimpan bahasa terakhir yang dipilih user
        public static ConfigEntry<int> ConfigLabelFontSize;     // Mengatur ukuran font tulisan "Language:"
        public static ConfigEntry<float> ConfigLabelOffsetX;    // Mengatur geseran posisi Kanan/Kiri tulisan "Language:"
        public static ConfigEntry<float> ConfigLabelOffsetY;    // Mengatur geseran posisi Atas/Bawah tulisan "Language:"
        public static ConfigEntry<int> ConfigButtonFontSize;    // Mengatur ukuran font di dalam tombol dropdown
        public static ConfigEntry<int> ConfigDialogTitleFontSize; // Mengatur ukuran judul dialog konfirmasi

        // ==========================================================
        // FUNGSI INISIALISASI KONFIGURASI
        // ==========================================================
        public static void InitConfig()
        {
            // Menentukan lokasi file config.ini (BepInEx/plugins/PvZ Constellation Translator/config.ini)
            string configPath = Path.Combine(FileManager.RootFolder, "config.ini");
            ModConfig = new ConfigFile(configPath, true);

            // Bind berfungsi untuk membaca config, atau membuatnya dengan nilai default jika belum ada
            ConfigLanguage = ModConfig.Bind("General", "Language", "Chinese", "Bahasa yang aktif saat ini.");
            ConfigLabelFontSize = ModConfig.Bind("UI Settings", "LabelFontSize", 35, "Ukuran teks label 'Language'.");
            ConfigLabelOffsetX = ModConfig.Bind("UI Settings", "LabelOffsetX", 0f, "Geseran posisi X (Kanan/Kiri) untuk label.");
            ConfigLabelOffsetY = ModConfig.Bind("UI Settings", "LabelOffsetY", 65f, "Geseran posisi Y (Atas/Bawah) untuk label.");
            ConfigButtonFontSize = ModConfig.Bind("UI Settings", "ButtonFontSize", 30, "Ukuran teks di dalam tombol dropdown (Indonesian, English, dll).");
            ConfigDialogTitleFontSize = ModConfig.Bind("UI Settings", "DialogTitleFontSize", 40, "Ukuran teks judul pada dialog konfirmasi bahasa.");

            // Set bahasa saat ini sesuai dengan yang ada di file config.ini
            CurrentLanguage = ConfigLanguage.Value;
        }

        // ==========================================================
        // FUNGSI UPDATE (Dipanggil terus-menerus setiap frame game oleh Plugin.cs)
        // ==========================================================
        public static void Update()
        {
            // Pastikan config sudah dimuat. Jika belum, muat sekarang.
            if (ModConfig == null) InitConfig();

            // KUNCI UTAMA: Kita mencari tombol asli game (Dapatkan Semua Tanaman) untuk dijadikan patokan
            GameObject targetBtn = GameObject.Find("/菜单界面画布/面板/调节关卡面板/按键区/一键获取所有植物按键");

            // Jika tombol asli game KETEMU dan SEDANG TAMPIL di layar...
            if (targetBtn != null && targetBtn.activeInHierarchy)
            {
                // ...dan tombol mod kita belum dibuat, maka BUAT TOMBOLNYA SEKARANG.
                if (customDropdownBtn == null) CreateDropdownMenu(targetBtn);
            }
            // Tapi jika tombol asli game NGILANG (misal user nutup panel pengaturan)...
            else if (customDropdownBtn != null && customDropdownBtn.activeSelf)
            {
                // ...maka SEMBUNYIKAN juga semua UI mod kita biar gak melayang sendirian di layar.
                HideAllUI();
            }
        }

        // Fungsi bantuan untuk menyembunyikan semua objek UI mod kita
        private static void HideAllUI()
        {
            customDropdownBtn?.SetActive(false);
            languageLabelObj?.SetActive(false);
            dropdownListPanel?.SetActive(false);
            languageConfirmPanel?.SetActive(false); // Tutup dialog konfirmasi kalau panel ditutup
            isDropdownOpen = false;
        }

        // ==========================================================
        // PEMBUATAN UI UTAMA (Tombol Menu)
        // ==========================================================
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

            Text t = customDropdownBtn.GetComponentInChildren<Text>();
            if (t != null)
            {
                t.text = fullText;
                t.resizeTextForBestFit = false;
                t.fontSize = ConfigButtonFontSize.Value;
            }

            System.Type tmpType = HarmonyLib.AccessTools.TypeByName("TMPro.TMP_Text");
            if (tmpType != null)
            {
                var tmpComp = customDropdownBtn.GetComponentInChildren(tmpType);
                if (tmpComp != null)
                {
                    tmpType.GetProperty("text")?.SetValue(tmpComp, fullText, null);
                    tmpType.GetProperty("enableAutoSizing")?.SetValue(tmpComp, false, null);
                    tmpType.GetProperty("fontSize")?.SetValue(tmpComp, (float)ConfigButtonFontSize.Value, null);
                }
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

            Text t = item.GetComponentInChildren<Text>();
            if (t != null)
            {
                t.text = langName;
                t.resizeTextForBestFit = false;
                t.fontSize = ConfigButtonFontSize.Value;
            }

            System.Type tmpType = HarmonyLib.AccessTools.TypeByName("TMPro.TMP_Text");
            if (tmpType != null)
            {
                var tmpComp = item.GetComponentInChildren(tmpType);
                if (tmpComp != null)
                {
                    tmpType.GetProperty("text")?.SetValue(tmpComp, langName, null);
                    tmpType.GetProperty("enableAutoSizing")?.SetValue(tmpComp, false, null);
                    tmpType.GetProperty("fontSize")?.SetValue(tmpComp, (float)ConfigButtonFontSize.Value, null);
                }
            }

            Button btn = item.GetComponent<Button>();
            btn.onClick = new Button.ButtonClickedEvent();
            btn.onClick.AddListener(() => OnLanguageSelected(langName));
        }

        // ==========================================================
        // EKSEKUSI PEMILIHAN BAHASA & DIALOG KONFIRMASI
        // ==========================================================
        // Dipanggil saat salah satu bahasa di-klik
        private static void OnLanguageSelected(string langName)
        {
            isDropdownOpen = false;
            dropdownListPanel?.SetActive(false);
            UpdateMenuVisuals();

            // CEGAH SPAM: Jika bahasa yang dipilih SAMA dengan yang dipakai, munculkan notifikasi penolakan
            if (CurrentLanguage == langName)
            {
                string msg = langName == "Indonesian" ? "Bahasa ini sudah digunakan!" : (langName == "Chinese" ? "该语言已在使用！" : "Language already in use!");
                NotificationSystem.CreateNotificationUI(msg, Color.yellow);
                return;
            }

            // Jika beda, panggil dialog konfirmasi!
            ShowConfirmDialog(langName);
        }

        // Fungsi untuk membuat dan menampilkan Pop-Up Konfirmasi
        private static void ShowConfirmDialog(string targetLang)
        {
            // Hancurkan panel lama jika ada, biar selalu fresh
            if (languageConfirmPanel != null) Object.Destroy(languageConfirmPanel);

            // Cari Canvas Utama di game StarSign untuk menempelkan pop-up kita
            GameObject mainCanvas = GameObject.Find("/菜单界面画布");
            if (mainCanvas == null)
            {
                Plugin.Log.LogWarning("Main Canvas tidak ditemukan, langsung ubah bahasa tanpa dialog.");
                ApplyLanguage(targetLang);
                return;
            }

            // 1. Buat Background Hitam Transparan (Blocker) layar penuh
            languageConfirmPanel = new GameObject("LanguageConfirmPanel");
            languageConfirmPanel.transform.SetParent(mainCanvas.transform, false);
            languageConfirmPanel.transform.SetAsLastSibling(); // Paling depan

            RectTransform bgRt = languageConfirmPanel.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

            Image bgImg = languageConfirmPanel.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.85f); // Hitam transparan

            // Tambah Button kosong agar klik tidak tembus ke UI di belakangnya
            languageConfirmPanel.AddComponent<Button>();

            // 2. Buat Kotak Dialog di tengah layar
            GameObject dialogBox = new GameObject("DialogBox");
            dialogBox.transform.SetParent(languageConfirmPanel.transform, false);
            RectTransform boxRt = dialogBox.AddComponent<RectTransform>();
            boxRt.sizeDelta = new Vector2(750, 400); // Ukuran kotak dialog
            boxRt.anchoredPosition = Vector2.zero;

            Image boxImg = dialogBox.AddComponent<Image>();
            boxImg.color = new Color(0.15f, 0.15f, 0.2f, 1f); // Warna Navy gelap biar elegan
            dialogBox.AddComponent<Outline>().effectColor = Color.white; // Border putih

            // 3. Buat Teks Judul
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(dialogBox.transform, false);
            Text titleTxt = titleObj.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("Cambria.ttf");
            titleTxt.fontSize = ConfigDialogTitleFontSize.Value;
            titleTxt.color = Color.white;
            titleTxt.alignment = TextAnchor.MiddleCenter;

            // Sesuaikan bahasa teks dialog dengan bahasa yang SEDANG AKTIF
            string titleMsg = $"Change language to {targetLang}?";
            if (CurrentLanguage == "Indonesian") titleMsg = $"Ganti bahasa ke {targetLang}?";
            else if (CurrentLanguage == "Chinese") titleMsg = $"将语言更改为 {targetLang}？";
            titleTxt.text = titleMsg;

            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.sizeDelta = new Vector2(700, 100);
            titleRt.anchoredPosition = new Vector2(0, 80); // Posisikan di atas

            // 4. Kloning Tombol untuk Konfirmasi (Biar bentuknya mirip tombol game asli)
            GameObject btnConfirm = Object.Instantiate(customDropdownBtn, dialogBox.transform);
            btnConfirm.name = "BtnConfirm";
            RectTransform confRt = btnConfirm.GetComponent<RectTransform>();
            confRt.anchoredPosition = new Vector2(-160, -80); // Kiri Bawah

            // Atur teks Confirm
            string confStr = (CurrentLanguage == "Indonesian") ? "Iya" : (CurrentLanguage == "Chinese" ? "确定" : "Yes");
            SetButtonText(btnConfirm, confStr);

            Button confAction = btnConfirm.GetComponent<Button>();
            confAction.onClick = new Button.ButtonClickedEvent();
            confAction.onClick.AddListener(() => ApplyLanguage(targetLang));

            // 5. Kloning Tombol untuk Cancel
            GameObject btnCancel = Object.Instantiate(customDropdownBtn, dialogBox.transform);
            btnCancel.name = "BtnCancel";
            RectTransform cancRt = btnCancel.GetComponent<RectTransform>();
            cancRt.anchoredPosition = new Vector2(160, -80); // Kanan Bawah

            // Atur teks Cancel
            string cancStr = (CurrentLanguage == "Indonesian") ? "Ga" : (CurrentLanguage == "Chinese" ? "取消" : "No");
            SetButtonText(btnCancel, cancStr);

            Button cancAction = btnCancel.GetComponent<Button>();
            cancAction.onClick = new Button.ButtonClickedEvent();
            cancAction.onClick.AddListener(() => Object.Destroy(languageConfirmPanel)); // Tutup panel jika batal
        }

        // Fungsi bantuan untuk mengubah teks di tombol kloningan
        private static void SetButtonText(GameObject btnObj, string newText)
        {
            // Hapus list anakan kalau ada
            Transform listChild = btnObj.transform.Find("LanguageListPanel");
            if (listChild != null) Object.Destroy(listChild.gameObject);

            // Ganti teks biasa
            Text t = btnObj.GetComponentInChildren<Text>();
            if (t != null) t.text = newText;

            // Ganti teks TMPro
            System.Type tmpType = HarmonyLib.AccessTools.TypeByName("TMPro.TMP_Text");
            if (tmpType != null)
            {
                var tmpComp = btnObj.GetComponentInChildren(tmpType);
                if (tmpComp != null) tmpType.GetProperty("text")?.SetValue(tmpComp, newText, null);
            }
        }

        // Fungsi pamungkas untuk mengubah sistem game ke bahasa baru
        private static void ApplyLanguage(string langName)
        {
            // Simpan bahasa
            CurrentLanguage = langName;
            ConfigLanguage.Value = langName;
            ModConfig.Save();

            // Terjemahkan!
            TranslationManager.LoadTranslations();
            UpdateLabelText();
            TextPatch.RefreshAllTexts();
            UpdateMenuVisuals();

            // Hancurkan panel konfirmasi
            if (languageConfirmPanel != null) Object.Destroy(languageConfirmPanel);

            // Memanggil Notifikasi dari file NotificationSystem.cs
            string msg = langName == "Indonesian" ? "Bahasa berhasil diubah!" : (langName == "Chinese" ? "语言更改成功！" : "Language changed successfully!");
            NotificationSystem.CreateNotificationUI(msg, Color.green);

            Plugin.Log.LogInfo(string.Format("Language applied: {0}", langName));
        }
    }
}
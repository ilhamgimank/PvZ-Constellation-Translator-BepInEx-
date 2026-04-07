using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using HarmonyLib;

namespace PvZStarSignTranslator.Features
{
    public static class PathDetector
    {
        public static bool IsEnabled = true;
        public static bool IsAdvanced = true; // Set true untuk mendapatkan info detail koordinat & layout

        public static string LastScannedSpriteName = "";
        public static string PickedSpriteName = "";

        // Mendeteksi input pengguna untuk memicu fungsi scanning atau picking
        public static void HandleInput()
        {
            if (!IsEnabled) return;

            // CTRL + Klik Kanan: Menjalankan pemindaian objek (Teks & Path)
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(1))
            {
                ScanObjectUnderMouse();
            }

            // ALT + Klik Kanan: Mengambil nama sprite/gambar saja
            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetMouseButtonDown(1))
            {
                PickTextureUnderMouse();
            }
        }

        // Fungsi untuk mengambil nama tekstur/sprite di bawah kursor
        private static void PickTextureUnderMouse()
        {
            PickedSpriteName = "";
            bool found = false;

            if (EventSystem.current != null)
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                foreach (RaycastResult result in results)
                {
                    if (TryGetSpriteName(result.gameObject))
                    {
                        found = true;
                        break;
                    }
                }
            }

            // Jika tidak ditemukan di UI, coba cek objek 3D (SpriteRenderer)
            if (!found)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = Physics.RaycastAll(ray);
                foreach (var hit in hits)
                {
                    if (TryGetSpriteName(hit.collider.gameObject))
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(PickedSpriteName))
            {
                Plugin.Log.LogInfo(string.Format("[Texture Picker] Selected: {0}", PickedSpriteName));
            }
            else
            {
                Plugin.Log.LogWarning("[Texture Picker] No Sprite Found!");
            }
        }

        private static bool TryGetSpriteName(GameObject obj)
        {
            if (obj == null) return false;

            if (obj.TryGetComponent<Image>(out var img) && img.sprite != null)
            {
                PickedSpriteName = img.sprite.name;
                return true;
            }

            if (obj.TryGetComponent<SpriteRenderer>(out var sr) && sr.sprite != null)
            {
                PickedSpriteName = sr.sprite.name;
                return true;
            }

            return false;
        }

        // Fungsi utama pemindaian objek di bawah mouse (Absolute Scanner)
        private static void ScanObjectUnderMouse()
        {
            Plugin.Log.LogInfo("---------------------------------------------");
            string mode = IsAdvanced ? "Advanced Scanner" : "Basic Scanner";
            Plugin.Log.LogInfo(string.Format("Path Detector - {0} Active", mode));
            Plugin.Log.LogInfo("---------------------------------------------");

            int foundCount = 0;
            Vector2 mousePos = Input.mousePosition;
            LastScannedSpriteName = "";

            // 1. SCAN TEKS (Tanpa peduli Raycast Target)

            // Scan UGUI Text
            Text[] allTexts = Object.FindObjectsOfType<Text>();
            foreach (Text t in allTexts)
            {
                if (t.gameObject.activeInHierarchy && IsPointInsideRectTransform(t.rectTransform, mousePos))
                {
                    PrintLog(t.gameObject, t.text, "UI.Text");
                    foundCount++;
                }
            }

            // Scan TextMeshPro UI (Menggunakan Reflection)
            System.Type tmpType = AccessTools.TypeByName("TMPro.TextMeshProUGUI");
            if (tmpType != null)
            {
                Object[] allTMPs = Object.FindObjectsOfType(tmpType);
                foreach (Object tmpObj in allTMPs)
                {
                    Component tmp = tmpObj as Component;
                    if (tmp != null && tmp.gameObject.activeInHierarchy)
                    {
                        RectTransform rt = tmp.GetComponent<RectTransform>();
                        if (rt != null && IsPointInsideRectTransform(rt, mousePos))
                        {
                            var prop = tmpType.GetProperty("text");
                            string txtVal = prop?.GetValue(tmp, null) as string;
                            PrintLog(tmp.gameObject, txtVal, "TextMeshProUGUI");
                            foundCount++;
                        }
                    }
                }
            }

            // Scan TextMesh 3D
            TextMesh[] allMesh = Object.FindObjectsOfType<TextMesh>();
            foreach (TextMesh tm in allMesh)
            {
                if (tm.gameObject.activeInHierarchy && IsObjectUnderMouse(tm.gameObject))
                {
                    PrintLog(tm.gameObject, tm.text, "TextMesh3D");
                    foundCount++;
                }
            }

            // 2. RAYCAST SCANNER (Cadangan untuk Gambar/Komponen lainnya)
            if (foundCount == 0 || IsAdvanced)
            {
                if (EventSystem.current != null)
                {
                    PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = mousePos };
                    List<RaycastResult> results = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointerData, results);

                    foreach (RaycastResult result in results)
                    {
                        // Hindari log ganda jika sudah terdeteksi sebagai teks
                        if (result.gameObject.GetComponent<Text>() == null && result.gameObject.GetComponent("TMPro.TMP_Text") == null)
                        {
                            if (CheckAndLog(result.gameObject)) foundCount++;
                        }
                    }
                }
            }

            if (foundCount == 0) Plugin.Log.LogWarning("No relevant component found at this position.");
            Plugin.Log.LogInfo(string.Format("Scan Complete. Found {0} object(s).", foundCount));
            Plugin.Log.LogInfo("---------------------------------------------");
        }

        // Mengecek apakah posisi mouse berada di dalam area RectTransform UI di layar
        private static bool IsPointInsideRectTransform(RectTransform rectTransform, Vector2 screenPoint)
        {
            Camera cam = null;
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = canvas.worldCamera ?? Camera.main;
            }
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, cam);
        }

        // Mengecek apakah mouse berada di atas objek 3D atau SpriteRenderer
        private static bool IsObjectUnderMouse(GameObject obj)
        {
            if (!obj.TryGetComponent<Renderer>(out var r)) return false;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return r.bounds.IntersectRay(ray);
        }

        // Memeriksa komponen spesifik dan mencatat datanya
        private static bool CheckAndLog(GameObject obj)
        {
            if (obj == null) return false;

            string textContent = "N/A";
            string type = "Unknown";
            bool isTarget = false;

            // Cek InputField
            if (obj.TryGetComponent<InputField>(out var inputField))
            {
                textContent = inputField.text;
                type = "UI.InputField";
                isTarget = true;
            }

            // Cek Image/Sprite
            if (obj.TryGetComponent<Image>(out var img) && img.sprite != null)
            {
                LastScannedSpriteName = img.sprite.name;
                if (!isTarget) { type = "UI.Image"; textContent = img.sprite.name; isTarget = true; }
            }
            else if (obj.TryGetComponent<SpriteRenderer>(out var sr) && sr.sprite != null)
            {
                LastScannedSpriteName = sr.sprite.name;
                if (!isTarget) { type = "SpriteRenderer"; textContent = sr.sprite.name; isTarget = true; }
            }

            if (isTarget || (IsAdvanced && obj.GetComponent<RectTransform>() != null))
            {
                PrintLog(obj, textContent, type);
                return true;
            }

            return false;
        }

        // Mencetak hasil pemindaian ke log BepInEx
        private static void PrintLog(GameObject obj, string text, string type)
        {
            string path = GetPath(obj.transform);
            string jsonKey = EscapeForJson(text);

            if (IsAdvanced)
            {
                string extra = "";
                if (obj.TryGetComponent<RectTransform>(out var rect))
                {
                    extra += string.Format("\nPos (X, Y)  : {0:F1}, {1:F1}", rect.anchoredPosition.x, rect.anchoredPosition.y);
                    extra += string.Format("\nSize (W, H) : {0:F0}, {1:F0}", rect.rect.width, rect.rect.height);
                    extra += string.Format("\nPivot       : {0}", rect.pivot);
                }

                string spriteInfo = string.IsNullOrEmpty(LastScannedSpriteName) ? "" : string.Format("\nSprite Name : {0}", LastScannedSpriteName);

                Plugin.Log.LogInfo(string.Format("Type: {0} | Path: {1}\nContent: {2}\nJSON Key: \"{3}\"{4}{5}",
                    type, path, text, jsonKey, extra, spriteInfo));
            }
            else
            {
                Plugin.Log.LogInfo(string.Format("[FOUND] Type: {0} | Content: {1}", type, text));
                Plugin.Log.LogInfo(string.Format("        Path: {0}", path));
                if (!string.IsNullOrEmpty(LastScannedSpriteName))
                    Plugin.Log.LogInfo(string.Format("        Sprite: {0}", LastScannedSpriteName));
            }
        }

        // Mengamankan string agar bisa langsung dicopas ke file JSON terjemahan
        private static string EscapeForJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        // Mendapatkan path hierarki lengkap dari objek Unity
        public static string GetPath(Transform current)
        {
            if (current.parent == null) return "/" + current.name;
            return GetPath(current.parent) + "/" + current.name;
        }
    }
}
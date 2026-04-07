using UnityEngine;
using UnityEngine.UI;

namespace PvZStarSignTranslator.Features
{
    // ==========================================================
    // SISTEM NOTIFIKASI ELEGAN (FLOATING TEXT)
    // ==========================================================
    public static class NotificationSystem
    {
        public static void CreateNotificationUI(string message, Color textColor)
        {
            // Buat kanvas khusus notifikasi yang selalu ada di atas (Overlay)
            GameObject notifCanvasObj = GameObject.Find("TranslatorNotifCanvas");
            if (notifCanvasObj == null)
            {
                notifCanvasObj = new GameObject("TranslatorNotifCanvas");
                Canvas canvas = notifCanvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9999; // Prioritas paling depan

                CanvasScaler scaler = notifCanvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                Object.DontDestroyOnLoad(notifCanvasObj); // Biar nggak hancur pas pindah level
            }

            // Buat Panel Background Hitam Transparan
            GameObject notifObj = new GameObject("NotifPanel");
            notifObj.transform.SetParent(notifCanvasObj.transform, false);

            Image bg = notifObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.85f);

            // Auto-layout agar box hitam menyesuaikan panjang teks
            HorizontalLayoutGroup hlg = notifObj.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.padding = new RectOffset(30, 30, 15, 15);

            ContentSizeFitter csf = notifObj.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Buat Teks Notifikasinya
            GameObject textObj = new GameObject("NotifText");
            textObj.transform.SetParent(notifObj.transform, false);

            Text txt = textObj.AddComponent<Text>();
            txt.text = message;
            txt.color = textColor;
            txt.fontSize = 35;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Tambahkan Shadow/Outline Hitam
            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);

            // Posisi awal notifikasi (Di tengah atas layar)
            RectTransform notifRt = notifObj.GetComponent<RectTransform>();
            notifRt.pivot = new Vector2(0.5f, 0.5f);
            notifRt.anchorMin = new Vector2(0.5f, 0.5f);
            notifRt.anchorMax = new Vector2(0.5f, 0.5f);
            notifRt.anchoredPosition = new Vector2(0, 150);

            // Tambahkan Script Animasi (Fader)
            notifObj.AddComponent<NotificationFader>();
        }
    }

    // ==========================================================
    // SCRIPT ANIMASI NOTIFIKASI
    // ==========================================================
    public class NotificationFader : MonoBehaviour
    {
        private float stayTimer = 1.5f; // Tahan di layar selama 1.5 detik
        private CanvasGroup cg;
        private RectTransform rt;

        void Start()
        {
            cg = gameObject.AddComponent<CanvasGroup>();
            rt = GetComponent<RectTransform>();
        }

        void Update()
        {
            if (stayTimer > 0)
            {
                stayTimer -= Time.unscaledDeltaTime;
                rt.anchoredPosition += new Vector2(0, 10f * Time.unscaledDeltaTime); // Gerak pelan ke atas
            }
            else
            {
                cg.alpha -= Time.unscaledDeltaTime * 2f; // Pudar perlahan
                rt.anchoredPosition += new Vector2(0, 25f * Time.unscaledDeltaTime); // Gerak cepat ke atas

                if (cg.alpha <= 0)
                {
                    Destroy(gameObject); // Hapus jika sudah transparan penuh
                }
            }
        }
    }
}
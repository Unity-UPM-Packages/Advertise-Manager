using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

namespace TheLegends.Base.Ads
{
    public class AdLayoutHelper : MonoBehaviour
    {
        public Canvas mainCanvas; // Kéo Canvas của bạn vào đây
        public RectTransform mainContentRect; // Kéo RectTransform gốc của bạn vào đây
        public Image adBackground;

        // Bạn có thể gọi hàm này khi cần điều chỉnh layout (truyền 60 cho Banner)
        public void AdjustLayoutForNativeBanner(float bannerHeightInDpOrPt = 60f)
        {
            if (mainCanvas == null || mainContentRect == null)
            {
                Debug.LogError("Canvas or RectTransform is not set!");
                return;
            }

            // Áp dụng công thức quy đổi tương thích cả Android (DP) & iOS (Points)
            float bannerHeightInCanvasUnits = GetCanvasUnitsFromDpOrPt(bannerHeightInDpOrPt);

            // Giả sử mainContentRect được neo vào 4 góc (stretch-stretch)
            // Đẩy cạnh dưới của nó lên một khoảng bằng chiều cao của banner
            Vector2 offsetMin = mainContentRect.offsetMin;
            offsetMin.y = bannerHeightInCanvasUnits;
            mainContentRect.offsetMin = offsetMin;

            if (adBackground != null)
            {
                adBackground.rectTransform.sizeDelta = new Vector2(mainContentRect.rect.width, bannerHeightInCanvasUnits);
            }

            Debug.Log($"Calculated banner height in Canvas Units: {bannerHeightInCanvasUnits}. Adjusting bottom padding.");
        }

        /// <summary>
        /// Chuyển đổi giá trị DP (Android - base 160 DPI) hoặc Points (iOS - base 163 DPI)
        /// thành đơn vị tương đương trong hệ thống Canvas của Unity.
        /// </summary>
        /// <param name="value">Giá trị DP (Android) hoặc PT (iOS), mặc định 60 cho Banner.</param>
        /// <returns>Giá trị tương đương trong đơn vị Canvas.</returns>
        public float GetCanvasUnitsFromDpOrPt(float value)
        {
            // Lấy DPI của màn hình. Trả về giá trị mặc định nếu chạy trong Editor
            float dpi = Screen.dpi;
            if (dpi <= 0)
            {
                // DPI mặc định trong Unity Editor để test preview
                dpi = 320f;
                Debug.LogWarning($"Screen.dpi is 0 (likely in Editor). Using default value: {dpi}");
            }

            // 1. Mốc Base DPI quy ước:
            // - Android: 160 DPI (mdpi)
            // - iOS: 163 DPI (1x non-retina base của Apple)
            float baseDpi = 160f;
#if UNITY_IOS && !UNITY_EDITOR
            baseDpi = 163f;
#elif UNITY_ANDROID && !UNITY_EDITOR
            baseDpi = 160f;
#endif

            // 2. Tính density thực tế của thiết bị
            float density = dpi / baseDpi;

            // 3. Chuyển đổi DP/PT sang Physical Pixels thực tế trên màn hình
            float pixels = value * density;

            // 4. Chuyển đổi Physical Pixels sang đơn vị Canvas của Unity
            float scaleFactor = (mainCanvas != null && mainCanvas.scaleFactor > 0) ? mainCanvas.scaleFactor : 1f;
            float canvasUnits = pixels / scaleFactor;

            return canvasUnits;
        }

        // Backward compatibility cho code cũ gọi GetCanvasUnitsFromDp
        public float GetCanvasUnitsFromDp(float dp)
        {
            return GetCanvasUnitsFromDpOrPt(dp);
        }
    }
}
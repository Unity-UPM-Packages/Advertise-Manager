using TheLegends.Base.Ads.NativeDynamicUI;
using UnityEngine;

public class Stage2 : MonoBehaviour
{
    // Bấm chuột phải vào Script này bên Inspector và bấm "Test Chạy Máy Quét"
    [ContextMenu("Test Chạy Máy Quét (Mồi JSON)")]
    public void TestScan()
    {
        // 1. Nhét chính cái GameObject này vào Máy Quét
        DynamicAdsCacheManager.InitializeAndCache(this.gameObject);

        // 2. Móc túi lấy cục JSON ra xem thử
        string json = DynamicAdsCacheManager.GetLayoutJson(this.gameObject.name);

        // 3. In rập khuôn ra Console
        Debug.Log("==== MÁY QUÉT JSON RENDER THÀNH CÔNG ====");
        Debug.Log(json);

        // 4. Mở luôn thư mục chứa File Ảnh PNG để bạn tận mắt chiêm ngưỡng (Chỉ áp dụng trên máy tính Windows/Mac)
        string cachePath = Application.persistentDataPath + "/DynamicAdsCache";
        Application.OpenURL("file://" + cachePath);
    }
}

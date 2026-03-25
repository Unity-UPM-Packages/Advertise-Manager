# KẾ HOẠCH TRIỂN KHAI: DYNAMIC NATIVE ADS (UNITY SANG NATIVE)

Bản kế hoạch này mô tả quy trình chuyển đổi kiến trúc UI quảng cáo Native từ việc thiết kế bằng file XML (Android) / XIB (iOS) sang phương pháp **thiết kế 100% trên Unity Canvas**, xuất ra cấu hình JSON tĩnh và Native tự động render (vẽ lại) dựa trên cấu hình đó.

**Mục tiêu cốt lõi:**
1. **Thiết kế tập trung:** Unity xử lý toàn bộ UI (bo góc, màu sắc, hình ảnh Custom, vị trí).
2. **Tối ưu hiệu năng:** Trích xuất JSON bằng C# chạy 1 lần lúc khởi tạo (Runtime Init).
3. **Đồng bộ hoàn hảo:** Hỗ trợ co giãn màn hình tự động bằng Anchor, bẫy click với hitbox tùy chỉnh.

---

## GIAI ĐOẠN 1: THIẾT KẾ CẤU TRÚC DỮ LIỆU & ENUM (UNITY C#)

**Mục tiêu:** Định nghĩa giao thức dữ liệu (Schema) chung giữa Unity và Native.

### Bước 1.1: Định nghĩa Enum Thành phần
```csharp
public enum NativeAdElement
{
    RootAdView = 0, // Điểm neo giới hạn vùng tương tác click của toàn quảng cáo         
    // Các thành phần nội dung tiêu chuẩn AdMob
    Headline = 1, CallToAction = 2, MediaView = 3, IconView = 4, Body = 5, Advertiser = 6, StarRating = 7, Price = 8, Store = 9,
    // Nhãn đánh dấu bắt buộc theo Policy của Google (Chữ "Ad" tĩnh)
    AdAttribution = 10,
    // Decorators mở rộng (Native xử lý ngoại vi)
    Decorator_CloseButton = 11, Decorator_CountdownText = 12, Decorator_RadialTimer = 13
}

public enum AdZLayer
{
    BannerLayer = 10, MRECLayer = 20, PopupLayer = 50, FullscreenLayer = 100
}
```

### Bước 1.2: Cấu trúc JSON Model (Data Class Code)
Sử dụng **Composite Pattern**: mỗi Element có thể chứa đồng thời thông số Không gian (`rectTransform`), Hình ảnh (`image`), và Chữ viết (`text`).
```json
{
  "layoutId": "Banner_Default",
  "elements": [
    {
      "elementType": "CallToAction",
      "rectTransform": {
        "anchorMin": {"x": 1.0, "y": 1.0}, "anchorMax": {"x": 1.0, "y": 1.0},
        "offsetMin": {"x": -200.0, "y": -80.0}, "offsetMax": {"x": -20.0, "y": -20.0},
        "pivot": {"x": 1.0, "y": 1.0}, "rotationZ": 0.0,
        "scaleX": 1.0, "scaleY": 1.0
      },
      "image": { 
        "color": "#FF0A64FF", 
        "cornerRadius": 16.0, 
        "imagePath": "/data/user/0/com.app.game/files/DynamicAdsCache/btn_cta.png", 
        "isRadialFill": false 
      },
      "text": { 
        "textContent": "INSTALL", "color": "#FFFFFFFF", "fontSize": 24.0, "alignment": "MiddleCenter", "isBold": true 
      }
    }
  ]
}
```

### Bước 1.3: Hướng Dẫn Thiết Kế Cơ Bản (Dành cho Designer)

Để lớp C# trích xuất hoạt động hoàn hảo, Layout vẽ trên Unity Canvas nên tuân thủ sơ đồ cấp bậc (Hierarchy) và cách bố trí Component tương ứng. Dưới đây là bảng tra cứu:

**Mẫu Sơ Đồ Cây (Tree Hierarchy) Của Một Banner Điển Hình:**
```text
Canvas (Có thể là Canvas trống hoặc Screen Space)
└── Khung_Banner_Tổng (Gắn Tag: RootAdView) (Có: Image làm nền xanh đen)
    │
    ├── Hinh_Bieu_Tuong (Gắn Tag: IconView) (Có: Image khung vuông)
    │
    ├── Chu_Tieu_De_Game (Gắn Tag: Headline) (Có: Text "Quảng cáo thử nghiệm")
    │
    ├── Chu_Mo_Ta (Gắn Tag: Body) (Có: Text "Cài đặt ngay...", Font nhỏ)
    │
    ├── Nut_Bam_Cai_Dat (Gắn Tag: CallToAction) (Có: Image màu nổi)
    │   └── Text_Action (Có: Text "Install") 
    │       -> LƯU Ý: Không cần gắn Tag, Exporter tự động nội suy xuyên thấu lấy Font của con.
    │
    └── Nhan_AdMob_Luat (Gắn Tag: AdAttribution) (Có: Image nền phụ)
        └── Text_Ad (Có: Text "Ad", Chữ trắng) 
            -> LƯU Ý: Không cần gắn Tag, tương tự như trên.
```

**Bảng Tra Cứu Ánh Xạ UI Component Sang Native:**

| Giá trị Thẻ (`NativeAdElement`) | Yêu cầu Component (Của Cha hoặc Con) | Vai trò sau khi sang Native Android (`AdMob`) |
| :--- | :--- | :--- |
| `RootAdView` | **`Image`** (Phông nền, Bo góc) | Neo kích thước tổng & Bắt Click Event phủ toàn quảng cáo. Kích thước động mượt mà bằng Canvas Anchors. |
| `IconView` | **`Image`** (Hình Avatar vuông) | OS Override đè hoàn toàn bằng Logo App thực tế kéo từ Internet. |
| `Headline` | **`Text`** (Cỡ chữ to) | OS Override đè TEXT bằng Tên Game, nhưng **GIỮ NGUYÊN** cấu trúc Font/Size/Màu mà Unity đã nặn. |
| `CallToAction` | **`Image`** (Box Nút) + **`Text`** (Chữ) | Rất linh hoạt: OS Override đoạt TEXT ghi đè ("Install"), nhưng mượn Font của Text Con và Màu/Bo Viền của Image Cha. |
| `Body` | **`Text`** (Chữ nhỏ, Wrap đoạn) | OS Override đè TEXT bằng miêu tả Game tải từ Google về. |
| `MediaView` | **`Image`** (Tỷ lệ 16:9, Cực to) | Đây là chỗ Video Gameplay hoặc Ảnh Bia ngang được bắn vào (Thường dùng cho Fullscreen/MREC). |
| `AdAttribution` | **`Image`** + **`Text`** (Chữ "Ad") | TĨNH. Giữ nguyên gốc 100% tài sản Unity (không gán biến Override) để lách Policy. |
| `Decorator_...` | Tùy biên độ thiết kế | Chạy logic độc lập (Vòng xoay thời gian, Nút tắt Game). Không đụng độ với luồng SDK Google. |

---

## GIAI ĐOẠN 2: HỆ THỐNG SMART EXPORTER (UNITY C# RUNTIME)

**Mục tiêu:** Quét giao diện Unity Canvas, tiến hành gộp dữ liệu thành các Element JSON. Giải quyết bài toán Tải Đồ Họa Nặng mà không block CPU.

### Bước 2.1: Kịch bản Gắn nhãn
Cung cấp MonoBehaviour `DynamicNativeMark.cs` yêu cầu truyền giá trị `NativeAdElement`. Bổ sung biến `public float customCornerRadius = 0f`.

### Bước 2.2: Trình Quét và Phân rã (Chính sách Zero-Base64)
- Hàm `GenerateJson(Transform root)` duyệt cây Heirarchy theo chiến lược **Đệ quy Depth-first**. Tính toán `anchor/offset` dựa trên hệ quy chiếu màn hình thực tế nhằm khử sai số từ các LayoutGroup rỗng.
- **Smart Grouping:** Gọi API `GetComponentsInChildren<Image/Text>()` để gom Màu, Font và Hình ảnh thành 1 cụm.
- **Trích xuất Đồ họa Vật lý:** Tuyệt đối KHÔNG gầm chuỗi Base64 dài khổng lồ vào JSON nhằm chống sập cầu JNI C++ qua Android. Gặp Custom Image: C# dùng `EncodeToPNG()` lưu thành file gốc đẩy vào thư mục `Application.persistentDataPath + "/DynamicAdsCache/tên_ảnh.png"`. JSON chỉ chép đúng cái đường dẫn `imagePath` siêu nhẹ.
*(Ghi chú Bảo Mật: Thư mục `persistentDataPath` nằm gọn trong Sandbox App trên Hệ điều hành, hoàn toàn không yêu cầu cấp quyền Storage (Quyền Files/Media) từ User. File rác này tự hủy sạch sẽ khi người dùng gỡ cài đặt Game).*

### Bước 2.3: Thuật Toán Cache Invalidation (Chống Nghẽn CPU Khởi Động)
Hàm nén PNG nặng nề không được phép chạy phung phí mỗi lần mở App. Chúng ta áp dụng thuật toán đối chiếu phiên bản `Application.buildGUID`:
```csharp
string currentBuildGUID = Application.buildGUID; 
string lastCachedGUID = PlayerPrefs.GetString("AdsUI_CachedBuildGUID", "");

if (currentBuildGUID != lastCachedGUID) {
    // 1. Bản Cài Mới tinh HOẶC Chạy Update từ Google Play/AppStore
    // -> Tiến hành Quét sạch Thư mục Cache /DynamicAdsCache/ cũ để đổ rác.
    // -> Force Export mảng byte EncodeToPNG() toàn bộ Image đè xuống phân vùng đĩa lại từ đầu.
    PlayerPrefs.SetString("AdsUI_CachedBuildGUID", currentBuildGUID); // Khóa dấu an toàn.
} else {
    // 2. User chơi Game lần thứ 2, 3...
    // -> Cache Hit: Bỏ lơ sạch sẽ quá trình lưu PNG. Chỉ trích xuất Layout Node JSON nhẹ vài KB đi kèm `imagePath` cũ. (Zero-Lag Khởi động Màn hình Splash).
}
```
Hoàn thiện đóng gói JSON String và đưa vào Dictionary RAM tĩnh của C#.

---

## GIAI ĐOẠN 3: KIỂM SOÁT LAYER WINDOW MANAGER TĨNH (ANDROID INIT)

**Mục tiêu:** Phân tầng Z-Order cấp độ hệ điều hành (VD: Cấm Banner đè mất phần màn hình Interstitial). Định lý Window Root.

### Bước 3.1: Hệ thống Container Vô Hình & Cơ Chế Vuốt Xuyên Thấu (Touch Pass-through)
Lúc khởi tạo Base Module Plugin của Native, chèn 3 FrameLayout vào ViewRoot App. Đồng thời **BẮT BUỘC** tắt cờ bắt sự kiện để không làm Game Unity bị "Mù cảm ứng" khi thùng chứa đang rỗng:
```kotlin
val layerBanner = FrameLayout(activity).apply { isClickable = false; isFocusable = false }
val layerMrec = FrameLayout(activity).apply { isClickable = false; isFocusable = false }
val layerInter = FrameLayout(activity).apply { isClickable = false; isFocusable = false }

// Khóa vĩnh viễn quyền đè lớp Z-Index cấp Hệ điều hành
rootView.addView(layerBanner) 
rootView.addView(layerMrec)   
rootView.addView(layerInter) 
```
*(Ghi chú: Nhờ tắt 2 cờ Focus/Click, mọi thao tác vuốt/chạm của người chơi khi không trúng vào UI Quảng cáo sẽ tự động đánh thủng xuyên qua các lớp FrameLayout này và rớt xuống `SurfaceView` của Unity Game bình thường).* 

### Bước 3.2: Module Toán học `UnityRectLayout`
Lập trình Custom `ViewGroup` tên `DynamicAdBuilderLayout` xử lý Responsive Layout Params. Override `onLayout()` để quy đổi `(Anchor + Offset * ScreenRatio)` sang Device Pixel. Mâm hệ quy chiếu Screen Android kích thước vật lý trùng khớp với Screen Unity nên triệt tiêu hoàn toàn lỗi lệch Tai thỏ Notch/SafeArea.

---

## GIAI ĐOẠN 4: GIAO TIẾP VÀ KHỞI TỘ LỆNH ĐIỀU PHỐI (C# BUILDER BRIDGE)

**Mục tiêu:** C# điều hướng hiển thị và gửi thông số chuẩn quy đầu cuối sang Native qua JNI.

**1. Hành vi `.WithLayout(string layoutId)`**
- Trích xuất String Json từ Cache tĩnh ở Bước 2.3 đính kèm theo.

**2. Tiêu điểm `.WithLayer(AdZLayer layer)`**
- Lựa chọn mức độ ưu tiên vùng chứa (10, 20, 100). Đổ luồng vào Container tạo sẵn lúc Init.

**3. Khối lệnh `.WithCountdown(float timeSeconds)`**
- Nạp thời lượng đếm ngược cho Native Animation Timer.

---

## GIAI ĐOẠN 5: PARSER TOÁN HỌC & RENDER UI ĐỒ HỌA (ANDROID)

**Mục tiêu:** Native Parser xử lý giải mã JSON, nặn Đồ họa tự động. Thay thế hoàn toàn Layout XML truyền thống.

### Bước 5.1: Xâu Chuỗi Render Array (Flat Sequence Traversal)
- Hàm `buildUIFromJson()` duyệt vòng lặp Element tuần tự. Lệnh `addView()` tịnh tiến đảm bảo hiển thị đúng Trật tự Đè Lóp Mắt nhìn y hệt Unity Hierarchy.
- Tự động hóa thiết lập cấu trúc nền Shape `GradientDrawable` bo viền tĩnh và gán Font chữ vào TextView bên trong. 

### Bước 5.2: Load File Path Tĩnh (Custom Texture Loading)
- Dò tìm tham số `imagePath`: Thay vì nổ RAM giải Base64, Android gọi lệnh Load Bitmap Truyền thống `BitmapFactory.decodeFile(imagePath)` trỏ thẳng vào Bộ Nhớ Sandbox cục bộ mượt mà không Lag Thread. (HOẶC Sử dụng Custom Image Loader như `Glide`). Ánh xạ thành công Asset C# Custom UI hiển thị Lên Native ImageView.

---

## GIAI ĐOẠN 6: BƠM DỮ LIỆU ĐÍCH ADMOB & BẪY HITBOX CLICK KÉO DÀI

**Mục tiêu:** Mồi móng dữ liệu AdMob động. Xử lý chức năng Hitbox Bẫy Click Rộng mà không xài Collider 2D Vận lý.

### Bước 6.1: Khởi Tạo Lãnh Thổ (Bounding Root Bounds)
Lọc Node JSON mảng mang tag `RootAdView`. Lấy Dimension hình chữ nhật đo lường khởi tạo: `val mainAdView = NativeAdView(context)`. Re-Parenting nhét toàn bộ lưới Layout Rendered của Giai đoạn 5 đè sập vô con biến Node này. `RootAdView` này nắm cờ giữ trọng trách Thước Cảm Biến Touch Raycast Native.

### Bước 6.2: Mapping Bắt Luồng Component
Trỏ Reference ánh xạ Component ảo lên AdMob SDK: `mainAdView.headlineView = tvHeadline`. Gán `mainAdView.callToActionView = frameDienTichKhongLo`. 
*(Lưu ý Pháp lý AdMob: Riêng Component mang thẻ `AdAttribution` (ví dụ Nền vàng + Text "Ad"), Code Native chỉ thực hiện AddView vẽ lên màn hình ở GĐ 5. **TUYỆT ĐỐI LÀM NGƠ** không gán Bind nó vào bất kì hàm nào của Core `mainAdView` tại bước GĐ 6 này. Nhờ đó đối tượng giữ bản chất vẽ Hình Khối Tĩnh (Static Rendering), thỏa mãn Policy của Google).* 

### Bước 6.3: Phun Data Bơm Luồng Trực Tiếp 
Thực thi lệnh Binding: `mainAdView.setNativeAd(adLoaded)`. Toàn bộ Placeholder Text Lõi Giả Lập sẽ bị Override chép gán đè Tài Sản Text Quảng Cáo thật của AdMob. 

---

## GIAI ĐOẠN 7: DECORATORS, HOẠT ẢNH THỜI GIAN NATIVE LỆNH HỦY (LIFECYCLE)

**Mục tiêu:** Xử lý Logic thời gian đếm số và vòng tròn quạt khuyết mượt mà chạy trên HĐH Native độc lập. Vòng đời Xóa thả Garbage Collection.

### Bước 7.1: Xây Dựng `RadialFillImageView` Đặc Diễn
OS Android xây dựng Custom Component ViewGroup `RadialFillImageView` (Render cho cờ Boolean `isRadialFill = true`).
Thuật toán nội tại cắt lát Vector bằng Toán Hình Học `Canvas.clipPath(ArchPieSlicePath)` dập lên Image Path đã bóc tại Giai Đoạn 5. Trơn nhẵn siêu Flat.

### Bước 7.2: Hệ Cơ Phân Hạch Thời Gian (OS Logic Ticker)
Dựa tham số Float `WithCountdown(5.0f)`: Android Thread Tách Lập Mutil đẻ lệnh Delay `CountDownTimer(duration, 16)`:
- **Refresh `onTick(...)` (~60FPS):** System ép Parameter Update `sweepAngle` cắt biến % góc nan quạt. Kích Event Redraw `invalidate()` xoay góc cực mượt không lag Thread Update 60FPS Game Unity. Cập Nhật Value Integer Label Component `Decorator_CountdownText`.
- **Hết Giờ `onFinish(...)`:** Giá trị text 0. Khóa biến Touch Unblocked qua Event:
```kotlin
closeBtn.setOnClickListener {
   // Drop Object Rác Phía Native
   targetBucketContainer.removeView(currentDynamicAdLayout)
   
   // Bridge Tin Nhắn (JNI Call Game):
   UnityPlayer.UnitySendMessage("AdsManager", "OnNativeAdClosed", "layoutId string")
}
```

Kiến trúc Hệ Cốt Lõi Framework Xuất Mảng Dòng Xuyên Nền Tảng Chấm Dứt Mãn Nhãn Không Góc Chết! Hoàn thành Spec Build Package.

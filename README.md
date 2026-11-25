# RiotOptimizer v2.5

An open-source, lightweight all-in-one optimizer for Riot Games titles (**VALORANT** & **League of Legends**). Boosts FPS, reduces input latency, and stabilizes framerates for low-end PCs and competitive players.

---

## 🚀 Tính năng nổi bật (Features)

### 🎯 VALORANT
- **Hạ Resolution Quality:** Chỉnh độ phân giải nội bộ (ví dụ: 60%, 70%) giúp tăng FPS đột biến mà vẫn giữ độ phân giải màn hình sắc nét.
- **Tùy chỉnh 11 Scalability Groups:** Đầy đủ các thiết lập ẩn (`GlobalIllumination`, `ReflectionQuality`, `ViewDistance`, `Shading`,...).
- **Đồng bộ đa tài khoản tự động:** Tự động quét toàn bộ ổ đĩa và áp dụng cấu hình cho tất cả tài khoản VALORANT trên máy.
- **Tự động khóa Read-Only:** Ngăn game tự reset cấu hình đồ họa khi vào trận.

### ⚔️ LEAGUE OF LEGENDS (Liên Minh Huyền Thoại)
- **Tối ưu độ trễ (Input Latency):** Thiết lập Fullscreen độc quyền, tắt V-Sync.
- **Tắt hiệu ứng làm tụt FPS:** Tắt tia nắng mặt trời (`ShowGodray=0`), tắt sinh vật cảnh sông (`HideEyeCandy=1`), tắt hoạt ảnh thanh máu (`EnableHUDAnimations=0`), tắt khử răng cưa FXAA.
- **Chống khựng giao tranh:** Tắt thông báo Cột Mốc Thần Thoại (`EternalsMilestoneDisplayMode=0`).

### 🛡️ Tiện ích chung
- **Khôi phục bản sao lưu:** Chép lại file được lưu trước lần chỉnh đầu tiên trên mỗi đường dẫn và bỏ thuộc tính Read-Only.
- **Bảng Lưu ý (Notice):** Tích hợp hướng dẫn sử dụng và lưu ý quan trọng ngay trên giao diện.
- **Giao diện Tiếng Việt thân thiện:** Dễ sử dụng cho game thủ Việt Nam.

---

## 🛡️ An Toàn & Tuân Thủ Riot Vanguard (Safety)
- **100% An toàn:** Không can thiệp bộ nhớ RAM, không inject DLL hay sửa đổi file thực thi của game.
- **Hợp lệ:** Chỉ chỉnh sửa các file cấu hình client hợp lệ của Riot (`GameUserSettings.ini` và `game.cfg`).

---

## 📦 Hướng Dẫn Sử Dụng (How to Use)

Nhánh `main` chứa mã nguồn v2.5. Để dùng phiên bản này, mở `ValorantResFix.sln` trong Visual Studio trên Windows với thành phần **.NET desktop development** và **.NET Framework 4.6.1 targeting pack**, khôi phục gói NuGet rồi build cấu hình **Release**. Chạy `ValorantResFix/bin/Release/RiotOptimizer.exe`.

[Gói tải sẵn v2.0](https://github.com/ThagIspace/RiotOptimizer/releases/tag/v2.0) chỉ hỗ trợ VALORANT.

1. Tắt VALORANT / LMHT trước khi mở tool.
2. Mở file **`RiotOptimizer.exe`**.
3. Chọn tab game muốn tối ưu (**VALORANT** hoặc **LEAGUE OF LEGENDS**).
4. Bấm **Tối ưu FPS (Low)** để tự động áp dụng cấu hình tối đa FPS, hoặc tự chỉnh tay rồi bấm **Lưu thay đổi**.
5. Vào game và trải nghiệm!

Để hoàn tác, bấm **Khôi phục bản sao lưu**. Thao tác này chép lại toàn bộ file từ bản sao, nên các chỉnh sửa thực hiện sau thời điểm sao lưu sẽ mất. Nếu file bị xóa, bấm **Chọn file** rồi nhập đường dẫn ban đầu của file.

Nếu đã tối ưu VALORANT bằng v2.0, hãy dùng bản cũ và file `Backup.dat` của đúng bản cài đó để khôi phục cấu hình tương ứng trước khi lưu lần đầu bằng v2.5. Bản v2.5 sao lưu nội dung đang có trên từng đường dẫn. `Backup.dat` chỉ lưu một bộ thiết lập và có thể không khớp với các tài khoản khác.

---

## 🤝 Credits & Acknowledgments
- **Original Author & Project:** [xFahim](https://github.com/xFahim) ([xFahim/ValorantResFix](https://github.com/xFahim/ValorantResFix))
- **Maintained & Expanded as RiotOptimizer by:** [ThagIspace](https://github.com/ThagIspace) ([ThagIspace/RiotOptimizer](https://github.com/ThagIspace/RiotOptimizer))

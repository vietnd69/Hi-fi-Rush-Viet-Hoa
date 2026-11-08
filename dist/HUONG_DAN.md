# Hi-Fi RUSH – Bản Việt hóa (v1.0)

Bản dịch thay thế **tiếng Anh** trong game: chọn ngôn ngữ **English** thì hiện tiếng Việt.
Gồm 8.494 dòng (thoại, menu, hướng dẫn, mô tả vật phẩm…) và font Google có đủ dấu tiếng Việt.

## Cách 1 (Nhanh nhất – Khuyên dùng): Cài tự động bằng `HiFiRush_VietHoa_Setup.exe`

1. Mở trực tiếp file **`HiFiRush_VietHoa_Setup.exe`**.
2. Bộ cài sẽ **tự động quét và điền sẵn thư mục game trên Steam** (nếu cài ở nơi khác, bấm **Chọn thư mục...** và trỏ tới thư mục `Hi-Fi RUSH`).
3. Bấm **CÀI ĐẶT VIỆT HÓA** (tích hợp sẵn toàn bộ bản dịch, bộ font tiếng Việt, Overdub loader và cấu hình chống lỗi Tutorial).
4. Mở game bình thường từ Steam và thưởng thức! Khi muốn gỡ bỏ về nguyên bản, chỉ cần mở lại `HiFiRush_VietHoa_Setup.exe` và bấm **Gỡ Việt hóa**.

## Cách 2: Cài thủ công qua Overdub

Dùng **Overdub** (https://github.com/akmubi/overdub, mod loader dành riêng cho Hi-Fi RUSH). Nó nạp được file `.pak` và container IoStore (`.utoc` + `.ucas`) ngay từ lúc game khởi động, nên không cần Reloaded-II hay Unreal Essentials.

1. Tải `overdub-3.0.0.steam.zip` (bản Steam) tại https://github.com/akmubi/overdub/releases/tag/v3.0.0, giải nén `overdub.dll` và `XAPOFX1_5.dll` vào `<game>\Hibiki\Binaries\Win64\`. Xóa mọi DLL proxy khác ở đó (UE4SS, `dinput8.dll`…).
2. Chép thư mục `B_Overdub\Win64\mods\vietnamese.hifirush` vào `<game>\Hibiki\Binaries\Win64\mods\`.
3. Chạy game bình thường từ Steam. Không cần tham số `-fileopenlog`.
4. Vào Options → ngôn ngữ văn bản để **English**.

Overdub ghi nhật ký vào `Win64\overdub.log`. Mod Việt hóa mount thành công khi log có dòng `Pak filename: mods/vietnamese.hifirush/assets/VietHoa_P.pak` và `IOSTORE MOUNT ... Status: OK` ngay sau đó. Nút backtick (`` ` ``) mở bảng quản lý mod của Overdub; có thể tắt các mod Overdub kèm theo (cheats…) ở đó nếu không dùng.

Gỡ cài đặt: xóa `overdub.dll`, `XAPOFX1_5.dll` và thư mục `mods\vietnamese.hifirush` (cùng `overdub-config.ini`, `overdub.log` nếu muốn).

## Font

Font gốc của game **thiếu 92 chữ cái tiếng Việt** (ả ạ ơ ư ấ ầ ẩ ẫ ậ ắ… ỵ ỷ ỹ), thiếu thì ký tự đó biến mất (vd "lưu" thành "lu"). Kiểm tra trong game (Lua của Overdub) cho thấy menu và hội thoại dùng các font **FOT-UDKakugo / FOT-NewRodin** chứ không phải AvenirNextCyr/TNG, nên bản này vá cả nhóm này: giữ nguyên chữ Nhật/Hán, thay toàn bộ chữ Latin bằng **Google Sans Flex** (đủ chữ Việt, độ đậm khớp từng font: UL 200 … U 1000). AvenirNextCyr được thay bằng Google Sans Flex, TNG Gothic bằng **Fjalla One**. Giấy phép OFL nằm trong `LICENSES`. Xem `xem_truoc_font.png`.

## Ngôn ngữ mới "vi"

Gói thêm văn hóa `vi` (locres + `Game.locmeta` khai báo thêm `vi`). Engine đã nhận (`GetLocalizedCultures` có `vi`), nhưng danh sách trong Options → Text Language do game tự dựng, chưa thêm được dòng "Tiếng Việt" vào đó. Hiện vẫn dùng cách cũ: chọn **English** thì hiện tiếng Việt.
## Cách cài tạm thời – chỉ chữ (không font)

Chạy `A_ThuCong\CAI_DAT.bat` rồi thêm `-fileopenlog` vào Launch Options trên Steam. Chữ có dấu đặc biệt sẽ biến mất vì thiếu font. Gỡ: `A_ThuCong\GO_CAI_DAT.bat`, bỏ `-fileopenlog`.

## Không dùng nữa: Reloaded-II + Unreal Essentials

Đã thử: Reloaded-II chỉ tiêm được sau khi game mount xong các gói, nên chỉ chuyển hướng được file chữ (locres) chứ không nạp được font. Ngoài ra Reloaded-II chạy trên .NET 10 làm game báo lỗi "Hi-Fi RUSH has stopped working" khi thoát. Nếu đã cài, hãy gỡ: bỏ `Reloaded-II`, chỉ dùng Overdub.

## Chưa được dịch
- Chữ nằm trong **ảnh/texture** hoặc asset không đi qua locres (vd. "Press Any Button" ở màn hình tiêu đề).
- Giọng nói (voice) giữ nguyên.
- 560 dòng `[DISABLE]` là nội dung game không dùng.

## Lưu ý
- Overdub gắn với phiên bản game (Steam build 20567064 đã thử). Game hoặc Overdub cập nhật có thể cần tải lại bản Overdub mới; dựng lại gói bằng `tools\package.py`.
- Mọi thao tác chỉ thêm file; không sửa file gốc của game. Dùng cho bản game bạn sở hữu.


# Cài Việt hóa Hi-Fi RUSH v2

1. Thoát game. Mở **HiFiRush_VietHoa_v2_Setup.exe**.
2. Bộ cài tự tìm game; nếu chưa thấy, bấm **Chọn thư mục** và chọn thư mục Hi-Fi RUSH đã cài.
3. Bấm **Cài đặt Việt hóa**, chờ thông báo hoàn tất.
4. Mở game qua Steam. Đặt ngôn ngữ chữ là **English** để dùng bản Việt hóa.

Bộ cài đã kèm bản dịch 8.509 dòng, font và Overdub; không cần giải nén ZIP hay tự chép file.
ZIP `HiFiRush_VietHoa_v2_Test_Overdub.zip` chỉ là gói mod kiểm thử cho máy đã có loader,
khác gói Full của v1. Dùng EXE ở trên khi cài thông thường.

## Gỡ hoặc phục hồi

Thoát game, mở lại đúng EXE, chọn cùng thư mục và bấm **Gỡ Việt hóa**.
Bộ cài phục hồi các file về trạng thái trước khi cài v2, bao gồm bản Việt hóa cũ nếu trước đó đã có.
Không xóa file `.viet-hoa-v2-backup.zip` trong thư mục game: đó là bản sao lưu để phục hồi.
Save và mod khác không bị gỡ.

Nếu báo file đã đổi sau khi cài, dừng thao tác và giữ bản sao lưu; không tự xóa file đó.
Nếu Windows cảnh báo ứng dụng chưa được nhận diện: bộ cài chưa ký số. Kiểm đúng nguồn tải
và SHA-256 trong `SHA256SUMS.txt` trước khi quyết định mở **More info → Run anyway**.
Nếu báo thiếu quyền, chỉ chạy bằng **Run as administrator** khi thư mục game cần quyền đó.

## Phạm vi đã kiểm

Đã thử chính EXE trên thư mục game mô phỏng: cài, cài lại, chuyển từ v1, giữ mod/cấu hình khác,
gỡ và phục hồi đúng từng byte, từ chối thư mục sai hoặc file đã bị thay đổi.
Chưa xác nhận v2 chạy trong game; build nguồn quan sát là Steam 20567064.

Để xác nhận trong game: mở menu/cài đặt và một cảnh có phụ đề, kiểm dấu tiếng Việt và tràn khung;
thử tải save/chuyển màn; sau khi gỡ kiểm game mở lại được. Khi báo lỗi, gửi ảnh cùng tên EXE,
phiên bản game, cảnh gặp lỗi và file `Hibiki/Binaries/Win64/overdub.log`.

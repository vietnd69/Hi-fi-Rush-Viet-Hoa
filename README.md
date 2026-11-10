# Hi-Fi RUSH Việt hóa v2

Bản dịch **8.509 dòng**, kèm font tiếng Việt và Overdub trong một bộ cài Windows.

## Tải và cài đặt

Tải **[HiFiRush_VietHoa_v2_Setup.exe](https://github.com/vietnd69/Hi-fi-Rush-Viet-Hoa/releases/latest/download/HiFiRush_VietHoa_v2_Setup.exe)** tại [release mới nhất](https://github.com/vietnd69/Hi-fi-Rush-Viet-Hoa/releases/latest).
Bộ cài mới nằm trong [dist](dist/) và [GitHub Releases](https://github.com/vietnd69/Hi-fi-Rush-Viet-Hoa/releases/latest).

1. Thoát game rồi mở EXE.
2. Kiểm tra thư mục game được nhận diện; bấm **Chọn thư mục** nếu cần.
3. Bấm **Cài đặt Việt hóa** và chờ hoàn tất.
4. Mở game qua Steam, đặt ngôn ngữ chữ là **English**.

Không cần giải nén ZIP hoặc cài công cụ phát triển. Xem [hướng dẫn cài/gỡ](dist/HUONG_DAN_v2.md).

## Gỡ và phục hồi

Thoát game, mở lại đúng bộ cài v2, chọn cùng thư mục và bấm **Gỡ Việt hóa**.
Các file được phục hồi về trạng thái trước khi cài v2, gồm bản Việt hóa cũ nếu đã có.
Giữ file `.viet-hoa-v2-backup.zip` trong thư mục game để phục hồi. Bộ cài giữ save và mod khác;
file đã bị sửa sau khi cài sẽ được giữ lại và báo lỗi thay vì bị xóa hoặc ghi đè.

## Thay đổi ở v2

- Đóng gói đúng bản dịch v2 đã QA, font và loader; luồng tạo bộ cài mặc định không còn lấy tài nguyên v1.
- Tự sao lưu loader, cấu hình và file Việt hóa cũ có xung đột trước khi cài.
- Kiểm tra payload và snapshot bản dịch trước khi dựng EXE; kiểm tra hash trước khi cài lại hoặc gỡ.

ZIP kiểm thử v2 trước đây chỉ chứa mod và cần loader có sẵn, khác gói Full của v1.
Dùng EXE ở trên để cài thông thường. Các gói cũ thuộc v1;
[bản phát hành v1](https://github.com/vietnd69/Hi-fi-Rush-Viet-Hoa/releases/tag/v1.0.0) được giữ để tham khảo.

## Phạm vi kiểm chứng

Đã thử chính EXE trên thư mục mô phỏng: cài mới, cài lại, chuyển từ v1, giữ mod/cấu hình khác,
gỡ phục hồi đúng từng byte, từ chối thư mục sai và file bị sửa sau khi cài.
**Chưa xác nhận chạy trong game thật**, gồm font, khung chữ và ổn định khi chơi.
Build nguồn được quan sát: **Steam 20567064**. Chưa có bằng chứng v2 tương thích phiên bản khác.

## Kiểm tra file

Bộ cài chưa ký số. Repo công khai chỉ chứa bản phân phối và hướng dẫn cho người chơi.
Đối chiếu SHA-256 của file tải về với [RELEASE_SHA256SUMS.txt](https://github.com/vietnd69/Hi-fi-Rush-Viet-Hoa/releases/download/v2.0.0/RELEASE_SHA256SUMS.txt).

SHA-256 của EXE: `04588f90ef178f309d6c25b3f4af13051ee52341e222cc46c7f6e07ce0047896`.

# Quan-ly-trung-tam-ngoai-ngu

Website quản lý trung tâm ngoại ngữ bằng ASP.NET Core MVC (.NET 8).

## Trạng thái hiện tại

- Public pages đã có đầy đủ.
- Dashboard theo vai trò `Admin`, `Staff`, `Teacher` đã có.
- Backend đọc dữ liệu thật từ SQL Server dựa trên schema trong [`db.txt`](C:\DoAn\db.txt).
- Login quản trị đọc bảng `Accounts`.
- Đăng ký học viên mới ghi vào bảng `Students`.
- Tin tức vẫn dùng dữ liệu seed nội bộ vì `db.txt` chưa có bảng riêng cho bài viết.
- Nhiều form CRUD quản trị hiện vẫn là form demo giao diện; dữ liệu danh sách đã lên DB nhưng thao tác `POST/PUT/DELETE` chưa nối hết.

## Kết nối database

Connection string hiện được cấu hình trong:

- `Quan-ly-trung-tam-ngoai-ngu/appsettings.json`
- `Quan-ly-trung-tam-ngoai-ngu/appsettings.Development.json`

Thông tin đang dùng theo `db.txt`:

- Server: `100.89.159.46,1433`
- Database: `LanguageCenterDB`
- User: `sa`

## Cách dựng DB

1. Mở file [`db.txt`](C:\DoAn\db.txt).
2. Chạy script đó trên SQL Server để tạo `LanguageCenterDB` và seed dữ liệu cơ bản.
3. Chạy project ASP.NET Core.

## Cách chạy project

```bash
dotnet build .\Quan-ly-trung-tam-ngoai-ngu\Quan-ly-trung-tam-ngoai-ngu.csproj
dotnet run --project .\Quan-ly-trung-tam-ngoai-ngu\Quan-ly-trung-tam-ngoai-ngu.csproj
```

## Ghi chú kỹ thuật

- Lớp dữ liệu đang dùng `SqlServerDataService`.
- Nếu SQL Server không truy cập được, service sẽ fallback về `MockDataService` để giao diện không bị trắng trang.
- Do sandbox offline của môi trường hiện tại, `dotnet build` không chạy được theo luồng restore chuẩn; cú pháp C# đã được kiểm tra lại bằng Roslyn compiler với reference pack `net8.0`.

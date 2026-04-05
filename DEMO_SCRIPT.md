# DEMO_SCRIPT

## 1. Mục tiêu demo

- chứng minh hệ thống đã có DB thật
- chứng minh auth/role thật
- chứng minh CRUD lõi chạy được
- chứng minh giáo viên có flow dạy học thực tế
- chứng minh bản final đủ để nộp cuối kỳ

## 2. Phân công gợi ý cho nhóm

- Phúc
  - giới thiệu đề tài, kiến trúc, UI, flow tổng thể
- Danh
  - giải thích database, EF Core, role, auth, CRUD thật
- Pháp
  - thao tác demo nghiệp vụ và hỗ trợ phần Q&A

## 3. Tài khoản demo

- Admin: `admin / 123456`
- Staff: `staff01 / 123456`
- Teacher: `t001 / 123456`

## 4. Dữ liệu nên chuẩn bị sẵn

- hệ thống đã có course, teacher, class, enrollment seed
- lớp của teacher `T001`:
  - `KX0405110604`
  - `KX0405110215`
- enrollment có thể dùng nhanh:
  - `43`

## 5. Kịch bản demo từng bước

1. Mở trang đăng nhập.
2. Đăng nhập bằng `admin`.
3. Vào Dashboard Admin, giới thiệu các module chính.
4. Vào `Khóa học`, tạo một khóa học mới.
5. Vào `Giáo viên`, tạo hoặc mở hồ sơ giáo viên.
6. Vào `Lớp học`, tạo lớp mới và gán giáo viên.
7. Đăng xuất.
8. Đăng nhập bằng `staff01`.
9. Vào `Học viên`, thêm một học viên mới.
10. Vào `Ghi danh`, ghi danh học viên vào lớp.
11. Vào `Thu học phí`, tạo biên nhận mới.
12. Đăng xuất.
13. Đăng nhập bằng `t001`.
14. Vào `Lịch dạy`, tạo một buổi học.
15. Vào `Điểm danh`, ghi nhận điểm danh.
16. Vào `Bài kiểm tra`, tạo bài kiểm tra mới.
17. Vào `Nhập điểm`, nhập kết quả cho học viên.
18. Đăng xuất.
19. Đăng nhập lại bằng `admin`.
20. Mở `Báo cáo` hoặc `Dashboard Admin` để chốt thống kê cuối.

## 6. Ai nói phần nào

### Phúc

- giới thiệu đề tài
- giải thích luồng người dùng
- nhấn mạnh giao diện, layout, area, tính sẵn sàng để demo

### Danh

- giải thích DB thật
- giải thích mapping EF Core
- giải thích authentication và role
- giải thích unique/FK/computed column

### Pháp

- thao tác CRUD demo
- thao tác flow Teacher
- thao tác tạo bài kiểm tra và nhập điểm

## 7. Điểm cần nhấn mạnh khi thuyết trình

- project không còn dùng mock runtime
- dữ liệu được lưu thật xuống SQL Server
- role protection là thật
- stage 3 đã nâng cấp password hashing
- `Exams` và `ExamResults` đã tách riêng để phản ánh nghiệp vụ rõ hơn

## 8. Phương án dự phòng khi demo

- nếu không muốn tạo quá nhiều dữ liệu mới, dùng seed sẵn:
  - class `KX0405110604`
  - enrollment `43`
- nếu mạng nội bộ chậm:
  - ưu tiên demo route/list/detail trước
  - sau đó demo 1 thao tác create thật ở `Exams` và `ExamResults`

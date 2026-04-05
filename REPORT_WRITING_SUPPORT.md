# REPORT_WRITING_SUPPORT

## 1. Danh sách module hệ thống

- Public site
  - trang chủ, giới thiệu, khóa học, lớp học, tin tức, liên hệ, đăng nhập, đăng ký, hồ sơ
- Accounts
  - quản lý tài khoản và phân quyền
- Students
  - quản lý học viên
- Teachers
  - quản lý giáo viên
- Courses
  - quản lý khóa học
- Classes
  - quản lý lớp học
- Enrollments
  - ghi danh học viên vào lớp
- Receipts
  - thu học phí và quản lý biên nhận
- ClassSessions
  - tạo và quản lý buổi học
- Attendances
  - điểm danh theo buổi
- Exams
  - quản lý bài kiểm tra
- ExamResults
  - nhập kết quả học tập
- Dashboard/Reports
  - thống kê cho Admin, Staff, Teacher

## 2. Mô tả ngắn từng module

- Accounts:
  - lưu tài khoản đăng nhập hệ thống và role
- Students:
  - lưu hồ sơ học viên
- Teachers:
  - lưu hồ sơ giáo viên
- Courses:
  - định nghĩa chương trình đào tạo
- Classes:
  - mở lớp cụ thể theo course và teacher
- Enrollments:
  - liên kết học viên với lớp học
- Receipts:
  - ghi nhận các lần thanh toán học phí
- ClassSessions:
  - quản lý từng buổi học cụ thể
- Attendances:
  - ghi nhận chuyên cần theo enrollment và session
- Exams:
  - tạo danh mục bài kiểm tra
- ExamResults:
  - lưu điểm số theo enrollment và exam

## 3. Vai trò người dùng

- Admin
  - quản trị toàn hệ thống
  - quản lý account, teacher, course, class, report
- Staff
  - thêm học viên
  - ghi danh
  - thu học phí
- Teacher
  - xem lớp phụ trách
  - tạo buổi học
  - điểm danh
  - tạo bài kiểm tra
  - nhập điểm

## 4. Các bảng dữ liệu chính

- `Accounts`
- `Students`
- `Teachers`
- `Courses`
- `Classes`
- `Enrollments`
- `Receipts`
- `ClassSessions`
- `Attendances`
- `Exams`
- `ExamResults`

## 5. Luồng nghiệp vụ chính

1. Admin tạo khóa học
2. Admin tạo lớp học và gán giáo viên
3. Staff thêm học viên
4. Staff ghi danh học viên vào lớp
5. Staff thu học phí và tạo biên nhận
6. Teacher tạo buổi học
7. Teacher điểm danh
8. Teacher tạo bài kiểm tra
9. Teacher nhập điểm
10. Admin xem dashboard và báo cáo

## 6. Danh sách màn hình quan trọng

- Đăng nhập
- Dashboard Admin
- Dashboard Staff
- Dashboard Teacher
- Quản lý tài khoản
- Quản lý học viên
- Quản lý giáo viên
- Quản lý khóa học
- Quản lý lớp học
- Ghi danh
- Thu học phí
- Buổi học
- Điểm danh
- Bài kiểm tra
- Kết quả bài kiểm tra
- Báo cáo

## 7. Điểm nổi bật kỹ thuật của project

- ASP.NET Core MVC rõ area theo role
- EF Core + SQL Server thật
- cookie authentication + role protection
- password hashing chuẩn ở stage 3
- startup backfill cho account legacy
- computed column `FinalFee` được map trong EF
- module `Exams` và `ExamResults` đã tách độc lập

## 8. Hướng phát triển tương lai

- chuẩn hóa async/await toàn bộ read/write flow
- tách connection string khỏi source code
- cho phép chọn trực tiếp `ExamId` khi nhập kết quả
- thêm export Excel/PDF
- thêm upload avatar
- thêm audit log nghiệp vụ

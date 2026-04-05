# STAGE_3_FINALIZATION_REPORT

## 1. Tổng quan

- Mục tiêu giai đoạn 3:
  - chuẩn hóa kiến trúc sau stage 2
  - tăng độ an toàn cho authentication/password
  - làm rõ module `Exams` và `ExamResults`
  - dọn nợ kỹ thuật đủ để bản final dễ demo, dễ chụp hình, dễ viết báo cáo
- Phạm vi thực hiện:
  - source hiện tại trong workspace
  - `STAGE_2_AUDIT_REPORT.md`
  - script SQL mới nhất trong `C:/Users/ADMIN/Documents/script.ipynb`
  - database thật `LanguageCenterDB`
- Baseline từ stage 2 audit:
  - SQL Server, EF Core, cookie auth, CRUD lõi và dashboard thật đã chạy được
  - nợ kỹ thuật chính còn lại là password plain text/dev style, tên service legacy, module Exams chưa độc lập, async chưa xuyên suốt, legacy services cũ gây nhiễu

## 2. Những vấn đề tồn đọng từ giai đoạn 2

| Vấn đề | Ảnh hưởng | Ưu tiên |
| --- | --- | --- |
| `Accounts.PasswordHash` còn dữ liệu plain text | Rủi ro bảo mật, khó bảo vệ khi demo/nộp | Rất cao |
| `IMockDataService`, `IDemoAuthService` sai bản chất runtime | Khó giải thích kiến trúc | Cao |
| `Services/Sql` và `Services/Mocks` cũ vẫn nằm trong compile path | Gây nhiễu khi đọc source và refactor | Cao |
| `Exams` và `ExamResults` đang trộn vào một flow nhập điểm | Khó chụp màn hình, khó giải thích nghiệp vụ | Cao |
| async/await chưa chuẩn hóa toàn dự án | Kỹ thuật chưa sạch final | Trung bình |
| UI menu/route phần bài kiểm tra chưa rõ | Ảnh hưởng trải nghiệm demo | Trung bình |

## 3. Công việc đã thực hiện trong giai đoạn 3

### 3.1 Refactor kiến trúc

- Đổi service contract runtime:
  - `IMockDataService` -> `ILanguageCenterReadService`
  - `IDemoAuthService` -> `IAccountAuthService`
- Đồng bộ lại DI trong `Program.cs`.
- Giữ nguyên source legacy để tham chiếu, nhưng loại khỏi compile:
  - `Services/Mocks/**`
  - `Services/Sql/**`
- Mục tiêu của thay đổi này là làm code final phản ánh đúng bản chất: app đang chạy bằng EF Core thật, không còn runtime mock/ADO.NET.

### 3.2 Password/auth hardening

- Thêm `IAccountPasswordService` và `AccountPasswordService`.
- Auth runtime giờ dùng `PasswordHasher<AccountEntity>` của ASP.NET Core để hash và verify mật khẩu.
- `EfAuthService` đã chuyển sang async cho login/register.
- `AccountController` dùng async cho login/register.
- Khi admin tạo hoặc cập nhật tài khoản trong module `Accounts`, mật khẩu mới được hash trước khi lưu.
- Thêm startup task `Stage3StartupTasks.ApplySecurityBackfillAsync()`:
  - khi app khởi động, các tài khoản còn `PasswordHash` dạng legacy sẽ được backfill sang hash chuẩn
  - giúp thống nhất dữ liệu demo, tránh trạng thái “một số tài khoản đã hash, một số chưa hash”
- Kết quả test thực tế:
  - `admin`, `admin2`, `staff01`, `t001` đã chuyển từ legacy sang hashed trong DB

### 3.3 Hoàn thiện module Exams độc lập

- Bổ sung model/domain:
  - `Exam`
  - `ExamInput`
- Bổ sung service methods:
  - `ILanguageCenterReadService.GetExams()`
  - `ILanguageCenterManagementService.GetExam/SaveExam/DeleteExam`
- `EfLanguageCenterReadService` đã có query thật cho `Exams`.
- `EfLanguageCenterManagementService` đã có CRUD thật cho `Exams`.
- Tách route/module:
  - `ExamsController` mới dùng cho quản lý bài kiểm tra
  - controller cũ được đổi thành `ExamResultsController`
- Cập nhật menu/sidebar:
  - Admin:
    - `Bài kiểm tra`
    - `Kết quả kiểm tra`
  - Teacher:
    - `Bài kiểm tra`
    - `Nhập điểm`
- Đã test flow thật:
  - Teacher đăng nhập
  - tạo bài kiểm tra mới
  - nhập kết quả cho enrollment thật
  - kiểm tra lại bằng SQL: có bản ghi mới trong `Exams` và `ExamResults`

### 3.4 Validation, error handling, logging

- Giữ validation server-side trong management service.
- Bổ sung thông báo lỗi thân thiện hơn ở các điểm stage 3 chỉnh mạnh:
  - xóa bài kiểm tra đã có kết quả
  - lớp học không tồn tại
  - loại bài kiểm tra không hợp lệ
  - max score không hợp lệ
- Thêm logging rõ hơn ở auth/password:
  - login fail
  - password storage upgraded
  - startup security backfill

### 3.5 UI/demo cleanup

- Menu dashboard được tách rõ lại theo nghiệp vụ.
- Route mới rõ ràng hơn:
  - `/Admin/Exams`
  - `/Admin/ExamResults`
  - `/Teacher/Exams`
  - `/Teacher/ExamResults`
- Quick action của Teacher Dashboard được cập nhật theo route mới cho nhập điểm.

## 4. Ma trận before/after

| Hạng mục | Trước giai đoạn 3 | Sau giai đoạn 3 | Ghi chú |
| --- | --- | --- | --- |
| Auth service naming | `IDemoAuthService` | `IAccountAuthService` | Phản ánh đúng runtime |
| Read service naming | `IMockDataService` | `ILanguageCenterReadService` | Hết nhầm với mock |
| Password storage | plain text/dev style | hashed chuẩn + backfill startup | Cải thiện lớn nhất của stage 3 |
| Legacy services | còn compile cùng app | loại khỏi compile | Giữ source để tham khảo |
| Exams module | trộn với nhập điểm | tách riêng `Exams` và `ExamResults` | Dễ demo/báo cáo hơn |
| Auth flow | sync ở auth service | async ở login/register | Chuẩn hơn stage 2 |
| Sidebar/menu | một mục điểm số chung | tách bài kiểm tra và kết quả | Rõ nghiệp vụ |

## 5. File đã tạo / sửa

### File tạo mới

- `Quan-ly-trung-tam-ngoai-ngu/Services/Interfaces/IAccountAuthService.cs`
  - interface auth runtime mới
- `Quan-ly-trung-tam-ngoai-ngu/Services/Interfaces/ILanguageCenterReadService.cs`
  - interface read runtime mới
- `Quan-ly-trung-tam-ngoai-ngu/Services/Interfaces/IAccountPasswordService.cs`
  - contract cho password hashing/verification
- `Quan-ly-trung-tam-ngoai-ngu/Services/Security/AccountPasswordService.cs`
  - triển khai password hasher chuẩn
- `Quan-ly-trung-tam-ngoai-ngu/Infrastructure/Stage3StartupTasks.cs`
  - startup backfill cho legacy password
- `Quan-ly-trung-tam-ngoai-ngu/Areas/Admin/Controllers/ExamsController.cs`
  - module bài kiểm tra cho Admin
- `Quan-ly-trung-tam-ngoai-ngu/Areas/Teacher/Controllers/ExamsController.cs`
  - module bài kiểm tra cho Teacher

### File sửa chính

- `Quan-ly-trung-tam-ngoai-ngu/Program.cs`
- `Quan-ly-trung-tam-ngoai-ngu/Quan-ly-trung-tam-ngoai-ngu.csproj`
- `Quan-ly-trung-tam-ngoai-ngu/Controllers/AccountController.cs`
- `Quan-ly-trung-tam-ngoai-ngu/Models/AuthModels.cs`
- `Quan-ly-trung-tam-ngoai-ngu/Models/AppDomainModels.cs`
- `Quan-ly-trung-tam-ngoai-ngu/Models/ManagementModels.cs`
- `Quan-ly-trung-tam-ngoai-ngu/Services/Ef/EfAuthService.cs`
- `Quan-ly-trung-tam-ngoai-ngu/Services/Ef/EfLanguageCenterReadService.cs`
- `Quan-ly-trung-tam-ngoai-ngu/Services/Ef/EfLanguageCenterManagementService.cs`
- `Quan-ly-trung-tam-ngoai-ngu/Services/Interfaces/ILanguageCenterManagementService.cs`
- `Quan-ly-trung-tam-ngoai-ngu/Areas/Admin/Controllers/OperationsAcademicControllers.cs`
- `Quan-ly-trung-tam-ngoai-ngu/Areas/Teacher/Controllers/TeacherControllers.cs`
- `Quan-ly-trung-tam-ngoai-ngu/Views/Shared/Partials/_DashboardSidebar.cshtml`

### File sửa đồng bộ do rename contract

- Các controller/base controller/public controller đang inject read/auth service runtime đã được cập nhật tên interface cho đồng nhất.

## 6. Kiến trúc cuối cùng

### DbContext

- `ApplicationDbContext` tiếp tục là trục EF Core thật.
- Dùng DB thật `LanguageCenterDB`.

### Entities

- Giữ 11 bảng nghiệp vụ:
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

### ViewModels/Inputs

- Bổ sung `ExamInput` để `Exams` độc lập với `ExamResultInput`.
- Bổ sung `Exam` domain model để màn hình quản lý bài kiểm tra có dữ liệu riêng.

### Services

- `ILanguageCenterReadService`
  - service đọc dữ liệu runtime cho dashboard/list/detail
- `IAccountAuthService`
  - auth runtime theo DB thật
- `IAccountPasswordService`
  - hash/verify/migration logic cho password
- `ILanguageCenterManagementService`
  - CRUD nghiệp vụ lõi

### Auth

- Cookie authentication thật
- claims theo `Account`
- password hashing chuẩn
- startup backfill cho account legacy

### Role protection

- `Admin`, `Staff`, `Teacher`
- unauthorized truy cập route area sẽ bị redirect về khu phù hợp hoặc login

### Logging

- dùng logging built-in
- stage 3 tập trung log cho auth/password/startup backfill

### Reports/export

- chưa bổ sung export Excel/PDF thật ở stage 3 lần này
- không phải blocker cho demo final

## 7. Kết quả kiểm thử

### Build

- `dotnet build`: PASS
- sau thay đổi cuối cùng: `0 Warning(s), 0 Error(s)`

### Login/logout

- Login thật bằng DB:
  - `admin / 123456` -> `302 /Admin`
  - `staff01 / 123456` -> `302 /Staff`
  - `t001 / 123456` -> `302 /Teacher`
- route area không đúng role:
  - admin truy cập `/Teacher/Exams` -> `302 /Admin`
- unauthenticated:
  - `/Admin/Exams` -> `302 /Account/Login`
  - `/Admin/ExamResults` -> `302 /Account/Login`
  - `/Teacher/Exams` -> `302 /Account/Login`

### Password hardening

- Query SQL xác nhận:
  - `admin` -> `Hashed`
  - `admin2` -> `Hashed`
  - `staff01` -> `Hashed`
  - `t001` -> `Hashed`

### Exam flow

- Teacher login: PASS
- Tạo bài kiểm tra mới qua `/Teacher/Exams/Create`: PASS
- Nhập kết quả qua `/Teacher/ExamResults/Create`: PASS
- SQL xác nhận:
  - có record mới trong `Exams`
  - có record mới trong `ExamResults`

### Regression

- dashboard/admin area không bị vỡ route
- sidebar mới hoạt động
- route public login vẫn dùng được

## 8. Tài khoản và dữ liệu demo

### Tài khoản demo

- Admin: `admin / 123456`
- Staff: `staff01 / 123456`
- Teacher: `t001 / 123456`

### Seed data dùng được cho demo

- lớp của giáo viên `T001`
  - `KX0405110604`
  - `KX0405110215`
- enrollment dùng được để nhập điểm/thu học phí/test flow:
  - `EnrollmentId = 43`

### Dữ liệu tối thiểu để chạy demo

- Tạo lớp cần:
  - 1 course
  - 1 teacher
- Ghi danh cần:
  - 1 student
  - 1 class
- Thu học phí cần:
  - 1 enrollment
- Điểm danh cần:
  - 1 class session
  - 1 enrollment đúng lớp
- Nhập điểm cần:
  - 1 exam
  - 1 enrollment

## 9. Gợi ý chụp màn hình báo cáo

1. Trang đăng nhập
   - thể hiện auth thật theo role
2. Dashboard Admin
   - thể hiện hệ thống tổng quan
3. Quản lý tài khoản
   - thể hiện phân quyền
4. Quản lý học viên
   - thể hiện CRUD thật
5. Quản lý khóa học
   - thể hiện nghiệp vụ đào tạo
6. Quản lý lớp học
   - thể hiện liên kết course-teacher-class
7. Ghi danh
   - thể hiện enrollment thật
8. Thu học phí
   - thể hiện receipts và công nợ
9. Bài kiểm tra
   - điểm mới của stage 3
10. Kết quả bài kiểm tra
   - điểm mới của stage 3
11. Điểm danh
   - thể hiện attendance thật
12. Dashboard Teacher
   - thể hiện role-specific flow

## 10. Gợi ý nội dung để viết báo cáo cuối kỳ

### Chương 1

- lý do chọn đề tài quản lý trung tâm ngoại ngữ
- mục tiêu hệ thống
- phạm vi người dùng: Admin, Staff, Teacher
- công nghệ sử dụng: ASP.NET Core MVC, EF Core, SQL Server

### Chương 2

- phân tích nghiệp vụ:
  - tài khoản
  - học viên
  - giáo viên
  - khóa học
  - lớp học
  - ghi danh
  - học phí
  - buổi học
  - điểm danh
  - bài kiểm tra
  - kết quả học tập
- use case theo role
- luồng demo chính

### Chương 3

- thiết kế hệ thống
- kiến trúc MVC + service layer
- thiết kế database và quan hệ giữa 11 bảng
- giải thích soft delete, unique index, FK, computed column `FinalFee`

### Chương 4/5

- cài đặt hệ thống
- mô tả các màn hình chính
- mô tả auth flow và role protection
- mô tả CRUD thật và dashboard thật
- đánh giá kết quả
- hạn chế và hướng phát triển

### UML/database/module nên nhắc tới

- use case diagram theo 3 role
- ERD 11 bảng
- activity flow cho:
  - ghi danh
  - thu học phí
  - điểm danh
  - tạo bài kiểm tra + nhập điểm

## 11. Nợ kỹ thuật còn lại

- async/await chưa được chuẩn hóa xuyên suốt toàn bộ read/dashboard/controller layer
  - không blocker demo
  - nhưng vẫn là technical debt thật
- connection string vẫn đang nằm trong `appsettings*.json`
  - nên tách khỏi source nếu còn thời gian
- module `ExamResults` hiện vẫn nhập lại metadata exam khi tạo kết quả
  - đã tốt hơn stage 2 nhờ tách module
  - nhưng nếu còn thời gian có thể đổi sang chọn `ExamId` trực tiếp
- logging hiện ở mức built-in đủ dùng
  - chưa có persistent file log riêng

## 12. Kết luận cuối

- Project hiện đã đạt mức final đủ tốt để:
  - build
  - demo
  - chụp màn hình
  - viết báo cáo cuối kỳ
- Stage 3 đã xử lý xong các điểm yếu lớn nhất của stage 2:
  - password/auth hardening
  - đổi tên contract legacy
  - loại legacy services khỏi compile
  - tách `Exams` ra độc lập
  - route/menu rõ hơn cho demo
- Kết luận thực tế:
  - bản hiện tại **đủ điều kiện để nộp và thuyết trình final**
  - chưa phải mức “kiến trúc tuyệt đối sạch 100%” vì async/read layer và cấu hình bí mật vẫn còn nợ kỹ thuật nhẹ

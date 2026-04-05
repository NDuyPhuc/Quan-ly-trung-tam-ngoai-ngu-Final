using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Mocks;

public class MockDataService : ILanguageCenterReadService
{
    private readonly IReadOnlyList<DemoAccount> _accounts;
    private readonly List<Student> _students;
    private readonly IReadOnlyList<Teacher> _teachers;
    private readonly IReadOnlyList<Course> _courses;
    private readonly IReadOnlyList<CourseClass> _classes;
    private readonly IReadOnlyList<Enrollment> _enrollments;
    private readonly IReadOnlyList<Receipt> _receipts;
    private readonly IReadOnlyList<TuitionDebt> _debts;
    private readonly IReadOnlyList<ClassSession> _sessions;
    private readonly IReadOnlyList<AttendanceRecord> _attendance;
    private readonly IReadOnlyList<ExamResult> _examResults;
    private readonly IReadOnlyList<NewsArticle> _articles;
    private readonly object _syncRoot = new();

    public MockDataService()
    {
        _accounts =
        [
            new DemoAccount { Id = 1, FullName = "Phúc Nguyễn", Email = "admin@demo.com", Password = "123456", Role = AppConstants.Roles.Admin, Department = "Ban quản trị", Phone = "0901001001", Status = "Đang hoạt động" },
            new DemoAccount { Id = 2, FullName = "Danh Trần", Email = "staff@demo.com", Password = "123456", Role = AppConstants.Roles.Staff, Department = "Giáo vụ", Phone = "0901001002", Status = "Đang hoạt động" },
            new DemoAccount { Id = 3, FullName = "Pháp Lê", Email = "teacher@demo.com", Password = "123456", Role = AppConstants.Roles.Teacher, Department = "Giảng viên", Phone = "0901001003", Status = "Đang hoạt động" },
            new DemoAccount { Id = 4, FullName = "Nguyễn Mai Anh", Email = "mai.anh@northstar.vn", Password = "123456", Role = AppConstants.Roles.Teacher, Department = "IELTS", Phone = "0902003004", Status = "Tạm khóa" }
        ];

        _courses =
        [
            new Course
            {
                Id = 1,
                Code = "TOEIC-650",
                Slug = "toeic-650-plus",
                Name = "TOEIC 650+",
                Level = "Trung cấp",
                Duration = "14 tuần",
                ScheduleSummary = "Tối 2-4-6",
                TuitionFee = 6800000,
                Status = "Đang tuyển sinh",
                ShortDescription = "Lộ trình luyện đề và chiến lược xử lý các phần 5-7 cho mục tiêu 650+.",
                Description = "Khóa học tập trung nền tảng ngữ pháp, từ vựng học thuật và kỹ thuật giải đề với tiến độ rõ ràng theo tuần.",
                TargetOutput = "Mục tiêu đầu ra 650+ sau 3,5 tháng cùng 3 bài thi thử.",
                NextOpening = "15/04/2026",
                StudentCount = 92,
                Objectives = ["Củng cố các phần 1-4", "Tăng tốc xử lý các phần 5-7", "Rèn chiến lược thời gian", "Làm quen đề thi chuẩn"],
                Highlights = ["Luyện đề có phân tích lỗi", "Báo cáo tiến độ hàng tuần", "Bộ thẻ từ vựng", "Thi thử cuối khóa"]
            },
            new Course
            {
                Id = 2,
                Code = "IELTS-FOUNDATION",
                Slug = "ielts-foundation",
                Name = "IELTS Nền Tảng",
                Level = "Sơ cấp",
                Duration = "16 tuần",
                ScheduleSummary = "Tối 3-5-7",
                TuitionFee = 9200000,
                Status = "Đang tuyển sinh",
                ShortDescription = "Xây nền tảng toàn diện 4 kỹ năng cho học viên mới bắt đầu IELTS.",
                Description = "Khóa học làm quen cấu trúc đề IELTS, xây dựng vốn từ theo chủ đề và phát triển phát âm, phản xạ giao tiếp song song kỹ năng học thuật.",
                TargetOutput = "Mục tiêu đầu ra 4.5 - 5.0 IELTS để chuyển tiếp lên khóa chuyên sâu.",
                NextOpening = "20/04/2026",
                StudentCount = 114,
                Objectives = ["Nắm cấu trúc 4 kỹ năng", "Xây từ vựng theo chủ đề", "Cải thiện phát âm", "Luyện viết đoạn cơ bản"],
                Highlights = ["Bài kiểm tra ngắn mỗi 2 tuần", "Câu lạc bộ nói cuối tuần", "Video tự học", "Phản hồi từng bài viết"]
            },
            new Course
            {
                Id = 3,
                Code = "COMM-APPLY",
                Slug = "giao-tiep-ung-dung",
                Name = "Tiếng Anh Giao Tiếp Ứng Dụng",
                Level = "Cơ bản",
                Duration = "12 tuần",
                ScheduleSummary = "Tối 2-4-6",
                TuitionFee = 5200000,
                Status = "Khai giảng sớm",
                ShortDescription = "Rèn phản xạ giao tiếp đi làm, thuyết trình ngắn và hội thoại thường ngày.",
                Description = "Chương trình thiết kế cho sinh viên và người đi làm cần giao tiếp tự tin trong ngữ cảnh thực tế như họp nhóm, gửi email và chăm sóc khách hàng.",
                TargetOutput = "Tự tin giao tiếp các tình huống công việc cơ bản và trình bày ngắn trước nhóm.",
                NextOpening = "25/04/2026",
                StudentCount = 67,
                Objectives = ["Tăng phản xạ nghe nói", "Luyện hội thoại tình huống", "Mở rộng mẫu câu công việc", "Thực hành thuyết trình"],
                Highlights = ["Thực hành tình huống thực tế", "Phản hồi nói trực tiếp", "Bộ mẫu câu công việc", "Chuyên đề cuối khóa"]
            },
            new Course
            {
                Id = 4,
                Code = "IELTS-INTENSIVE",
                Slug = "ielts-intensive-65",
                Name = "IELTS Chuyên Sâu 6.5+",
                Level = "Nâng cao",
                Duration = "18 tuần",
                ScheduleSummary = "Tối 2-4-6",
                TuitionFee = 13800000,
                Status = "Sắp mở lớp",
                ShortDescription = "Luyện chuyên sâu Đọc hiểu, Viết Task 2 và Nói band 6.5+.",
                Description = "Khóa học chuyên sâu cho học viên đã có nền 5.0 trở lên, tập trung tăng band theo tiêu chí chấm điểm và chiến lược làm bài cá nhân hóa.",
                TargetOutput = "Đầu ra mục tiêu 6.5+ với lộ trình kiểm tra định kỳ và chữa bài 1-1.",
                NextOpening = "05/05/2026",
                StudentCount = 41,
                Objectives = ["Phân tích tiêu chí chấm điểm", "Tăng tốc Đọc hiểu và Nghe hiểu", "Nâng cấp cấu trúc bài viết", "Cải thiện tính mạch lạc"],
                Highlights = ["Chữa bài viết 1-1", "Bộ đề cập nhật", "Thi nói thử với giáo viên", "Báo cáo chiến lược cá nhân"]
            }
        ];

        _teachers =
        [
            new Teacher { Id = 1, Code = "GV001", FullName = "Nguyễn Mai Anh", Email = "mai.anh@northstar.vn", Phone = "0903111222", Specialty = "IELTS", Qualification = "ThS. Ngôn ngữ Anh", Status = "Đang giảng dạy", AssignedClassCount = 3 },
            new Teacher { Id = 2, Code = "GV002", FullName = "Trần Minh Khang", Email = "minh.khang@northstar.vn", Phone = "0903222333", Specialty = "TOEIC", Qualification = "Cử nhân Sư phạm Anh", Status = "Đang giảng dạy", AssignedClassCount = 2 },
            new Teacher { Id = 3, Code = "GV003", FullName = "Lê Thảo Vy", Email = "thao.vy@northstar.vn", Phone = "0903333444", Specialty = "Giao tiếp", Qualification = "TESOL", Status = "Đang giảng dạy", AssignedClassCount = 2 },
            new Teacher { Id = 4, Code = "GV004", FullName = "Phạm Quốc Bảo", Email = "quoc.bao@northstar.vn", Phone = "0903444555", Specialty = "IELTS", Qualification = "CELTA", Status = "Nghỉ phép", AssignedClassCount = 1 }
        ];

        _classes =
        [
            new CourseClass { Id = 1, Code = "TOEIC-2404-A1", CourseName = "TOEIC 650+", TeacherName = "Trần Minh Khang", Schedule = "T2-T4-T6 | 18:30 - 20:30", Room = "P.201", Status = "Đang hoạt động", Capacity = 25, Enrolled = 21, StartDate = new DateTime(2026, 4, 15), EndDate = new DateTime(2026, 7, 20) },
            new CourseClass { Id = 2, Code = "IELTS-2404-F1", CourseName = "IELTS Nền Tảng", TeacherName = "Nguyễn Mai Anh", Schedule = "T3-T5-T7 | 18:00 - 20:00", Room = "P.305", Status = "Đang hoạt động", Capacity = 24, Enrolled = 22, StartDate = new DateTime(2026, 4, 20), EndDate = new DateTime(2026, 8, 10) },
            new CourseClass { Id = 3, Code = "COMM-2404-B2", CourseName = "Tiếng Anh Giao Tiếp Ứng Dụng", TeacherName = "Lê Thảo Vy", Schedule = "T2-T4-T6 | 19:00 - 21:00", Room = "P.102", Status = "Sắp khai giảng", Capacity = 20, Enrolled = 14, StartDate = new DateTime(2026, 4, 25), EndDate = new DateTime(2026, 7, 18) },
            new CourseClass { Id = 4, Code = "IELTS-2505-I1", CourseName = "IELTS Chuyên Sâu 6.5+", TeacherName = "Nguyễn Mai Anh", Schedule = "T2-T4-T6 | 20:00 - 22:00", Room = "P.401", Status = "Mở đăng ký", Capacity = 18, Enrolled = 9, StartDate = new DateTime(2026, 5, 5), EndDate = new DateTime(2026, 9, 8) },
            new CourseClass { Id = 5, Code = "TOEIC-2403-C1", CourseName = "TOEIC 650+", TeacherName = "Trần Minh Khang", Schedule = "T7-CN | 08:00 - 11:00", Room = "P.204", Status = "Đã đủ chỗ", Capacity = 20, Enrolled = 20, StartDate = new DateTime(2026, 3, 10), EndDate = new DateTime(2026, 6, 14) }
        ];

        _students =
        [
            new Student { Id = 1, Code = "HV001", FullName = "Ngô Quỳnh Như", Email = "quynh.nhu@gmail.com", Phone = "0911222333", Level = "Tiền trung cấp", Status = "Đang học", CourseName = "TOEIC 650+", ClassCode = "TOEIC-2404-A1", JoinedOn = new DateTime(2026, 4, 1), TuitionFee = 6800000, PaidAmount = 4000000, DebtAmount = 2800000 },
            new Student { Id = 2, Code = "HV002", FullName = "Lý Gia Hân", Email = "gia.han@gmail.com", Phone = "0911444555", Level = "Sơ cấp", Status = "Đang học", CourseName = "IELTS Nền Tảng", ClassCode = "IELTS-2404-F1", JoinedOn = new DateTime(2026, 4, 2), TuitionFee = 9200000, PaidAmount = 9200000, DebtAmount = 0 },
            new Student { Id = 3, Code = "HV003", FullName = "Đỗ Khánh Linh", Email = "khanh.linh@gmail.com", Phone = "0911777888", Level = "Nhập môn", Status = "Bảo lưu", CourseName = "Tiếng Anh Giao Tiếp Ứng Dụng", ClassCode = "COMM-2404-B2", JoinedOn = new DateTime(2026, 3, 28), TuitionFee = 5200000, PaidAmount = 2600000, DebtAmount = 2600000 },
            new Student { Id = 4, Code = "HV004", FullName = "Phan Hoàng Nam", Email = "hoang.nam@gmail.com", Phone = "0911999000", Level = "Trung cấp", Status = "Hoàn thành", CourseName = "TOEIC 650+", ClassCode = "TOEIC-2403-C1", JoinedOn = new DateTime(2026, 2, 10), TuitionFee = 6800000, PaidAmount = 6800000, DebtAmount = 0 },
            new Student { Id = 5, Code = "HV005", FullName = "Trịnh Bảo Châu", Email = "bao.chau@gmail.com", Phone = "0911888777", Level = "Trung cao cấp", Status = "Đang học", CourseName = "IELTS Chuyên Sâu 6.5+", ClassCode = "IELTS-2505-I1", JoinedOn = new DateTime(2026, 4, 2), TuitionFee = 13800000, PaidAmount = 6000000, DebtAmount = 7800000 }
        ];

        _enrollments =
        [
            new Enrollment { Id = 1, EnrollmentCode = "GD240401", StudentName = "Ngô Quỳnh Như", CourseName = "TOEIC 650+", ClassCode = "TOEIC-2404-A1", EnrolledOn = new DateTime(2026, 4, 1), Status = "Đã xếp lớp", PaymentStatus = "Đóng một phần", TotalFee = 6800000, PaidAmount = 4000000 },
            new Enrollment { Id = 2, EnrollmentCode = "GD240402", StudentName = "Lý Gia Hân", CourseName = "IELTS Nền Tảng", ClassCode = "IELTS-2404-F1", EnrolledOn = new DateTime(2026, 4, 2), Status = "Hoàn tất", PaymentStatus = "Đã thanh toán", TotalFee = 9200000, PaidAmount = 9200000 },
            new Enrollment { Id = 3, EnrollmentCode = "GD240403", StudentName = "Trịnh Bảo Châu", CourseName = "IELTS Chuyên Sâu 6.5+", ClassCode = "IELTS-2505-I1", EnrolledOn = new DateTime(2026, 4, 2), Status = "Chờ xác nhận", PaymentStatus = "Đóng cọc", TotalFee = 13800000, PaidAmount = 6000000 },
            new Enrollment { Id = 4, EnrollmentCode = "GD240404", StudentName = "Đỗ Khánh Linh", CourseName = "Tiếng Anh Giao Tiếp Ứng Dụng", ClassCode = "COMM-2404-B2", EnrolledOn = new DateTime(2026, 3, 28), Status = "Đã xếp lớp", PaymentStatus = "Còn nợ", TotalFee = 5200000, PaidAmount = 2600000 }
        ];

        _receipts =
        [
            new Receipt { Id = 1, ReceiptCode = "PT240401", StudentName = "Ngô Quỳnh Như", ClassCode = "TOEIC-2404-A1", PaidOn = new DateTime(2026, 4, 1), Amount = 4000000, PaymentMethod = "Chuyển khoản", Status = "Đã ghi nhận" },
            new Receipt { Id = 2, ReceiptCode = "PT240402", StudentName = "Lý Gia Hân", ClassCode = "IELTS-2404-F1", PaidOn = new DateTime(2026, 4, 2), Amount = 9200000, PaymentMethod = "Tiền mặt", Status = "Đã in biên nhận" },
            new Receipt { Id = 3, ReceiptCode = "PT240403", StudentName = "Trịnh Bảo Châu", ClassCode = "IELTS-2505-I1", PaidOn = new DateTime(2026, 4, 2), Amount = 6000000, PaymentMethod = "Chuyển khoản", Status = "Đã ghi nhận" }
        ];

        _debts =
        [
            new TuitionDebt { Id = 1, StudentName = "Ngô Quỳnh Như", CourseName = "TOEIC 650+", TotalFee = 6800000, PaidAmount = 4000000, RemainingAmount = 2800000, DueDate = new DateTime(2026, 4, 18), Status = "Sắp đến hạn" },
            new TuitionDebt { Id = 2, StudentName = "Đỗ Khánh Linh", CourseName = "Tiếng Anh Giao Tiếp Ứng Dụng", TotalFee = 5200000, PaidAmount = 2600000, RemainingAmount = 2600000, DueDate = new DateTime(2026, 4, 12), Status = "Quá hạn" },
            new TuitionDebt { Id = 3, StudentName = "Trịnh Bảo Châu", CourseName = "IELTS Chuyên Sâu 6.5+", TotalFee = 13800000, PaidAmount = 6000000, RemainingAmount = 7800000, DueDate = new DateTime(2026, 4, 28), Status = "Đang theo dõi" }
        ];

        _sessions =
        [
            new ClassSession { Id = 1, ClassCode = "TOEIC-2404-A1", Topic = "Nghe hiểu phần 3 - suy luận thông tin", SessionDate = new DateTime(2026, 4, 3), TimeSlot = "18:30 - 20:30", Room = "P.201", Status = "Hôm nay" },
            new ClassSession { Id = 2, ClassCode = "IELTS-2404-F1", Topic = "Viết Task 1 - mô tả biểu đồ", SessionDate = new DateTime(2026, 4, 4), TimeSlot = "18:00 - 20:00", Room = "P.305", Status = "Sắp diễn ra" },
            new ClassSession { Id = 3, ClassCode = "COMM-2404-B2", Topic = "Giao tiếp công sở cơ bản", SessionDate = new DateTime(2026, 4, 5), TimeSlot = "19:00 - 21:00", Room = "P.102", Status = "Đã lên lịch" }
        ];

        _attendance =
        [
            new AttendanceRecord { Id = 1, ClassCode = "TOEIC-2404-A1", SessionTopic = "Nghe hiểu phần 3 - suy luận thông tin", StudentName = "Ngô Quỳnh Như", AttendanceDate = new DateTime(2026, 4, 3), Status = "Có mặt", Note = "Đúng giờ" },
            new AttendanceRecord { Id = 2, ClassCode = "TOEIC-2404-A1", SessionTopic = "Nghe hiểu phần 3 - suy luận thông tin", StudentName = "Phan Hoàng Nam", AttendanceDate = new DateTime(2026, 4, 3), Status = "Muộn", Note = "Đến trễ 10 phút" },
            new AttendanceRecord { Id = 3, ClassCode = "IELTS-2404-F1", SessionTopic = "Viết Task 1 - mô tả biểu đồ", StudentName = "Lý Gia Hân", AttendanceDate = new DateTime(2026, 4, 4), Status = "Có mặt", Note = "Tích cực phát biểu" }
        ];

        _examResults =
        [
            new ExamResult { Id = 1, ClassCode = "TOEIC-2404-A1", StudentName = "Ngô Quỳnh Như", ExamType = "Giữa kỳ", Score = 78, AverageScore = 81, Result = "Đạt" },
            new ExamResult { Id = 2, ClassCode = "IELTS-2404-F1", StudentName = "Lý Gia Hân", ExamType = "Bài kiểm tra tiến độ", Score = 6.0m, AverageScore = 5.8m, Result = "Đạt" },
            new ExamResult { Id = 3, ClassCode = "COMM-2404-B2", StudentName = "Đỗ Khánh Linh", ExamType = "Đánh giá nói", Score = 5.5m, AverageScore = 5.2m, Result = "Cần cải thiện" },
            new ExamResult { Id = 4, ClassCode = "IELTS-2505-I1", StudentName = "Trịnh Bảo Châu", ExamType = "Đầu vào", Score = 6.0m, AverageScore = 6.0m, Result = "Đạt" }
        ];

        _articles =
        [
            new NewsArticle
            {
                Id = 1,
                Slug = "lich-khai-giang-thang-4",
                Title = "Lịch khai giảng các khóa tháng 4/2026",
                Category = "Thông báo",
                Summary = "Cập nhật các lớp TOEIC, IELTS và Giao tiếp bắt đầu trong tháng 4 cùng ưu đãi giữ chỗ sớm.",
                Content = "Trung tâm NorthStar English chính thức mở đăng ký cho các lớp khai giảng tháng 4. Học viên có thể lựa chọn các khung giờ tối 2-4-6, 3-5-7 hoặc cuối tuần tùy theo lịch học và làm việc. Chương trình ưu đãi áp dụng cho học viên đăng ký trước ngày khai giảng 7 ngày.",
                Author = "Phòng giáo vụ",
                PublishedOn = new DateTime(2026, 4, 1),
                IsFeatured = true
            },
            new NewsArticle
            {
                Id = 2,
                Slug = "workshop-chien-luoc-toeic",
                Title = "Buổi chia sẻ miễn phí: Chiến lược đạt TOEIC 700+",
                Category = "Sự kiện",
                Summary = "Buổi chia sẻ cách học và quản lý thời gian khi làm bài TOEIC dành cho sinh viên năm cuối.",
                Content = "Buổi chia sẻ tập trung vào các chiến lược cải thiện nhanh điểm Đọc hiểu và Nghe hiểu, kết hợp phần giải đáp trực tiếp với giáo viên TOEIC tại trung tâm. Người tham dự sẽ được tặng bộ tài liệu ôn tập nhanh và phiếu học thử.",
                Author = "Ban học thuật",
                PublishedOn = new DateTime(2026, 3, 28),
                IsFeatured = true
            },
            new NewsArticle
            {
                Id = 3,
                Slug = "kinh-nghiem-hoc-ielts-cho-nguoi-moi",
                Title = "Kinh nghiệm học IELTS cho người mới bắt đầu",
                Category = "Chia sẻ học tập",
                Summary = "5 lưu ý quan trọng khi bắt đầu lộ trình IELTS từ nền tảng cơ bản.",
                Content = "Với người mới bắt đầu IELTS, điều quan trọng là xác định mục tiêu đầu ra, đánh giá đúng trình độ và chọn lộ trình phù hợp. Bên cạnh đó, cần dành thời gian xây dựng phát âm, từ vựng nền và thói quen tiếp xúc với tiếng Anh mỗi ngày.",
                Author = "Nguyễn Mai Anh",
                PublishedOn = new DateTime(2026, 3, 25),
                IsFeatured = false
            }
        ];
    }

    public IReadOnlyList<DemoAccount> GetAccounts() => _accounts;
    public IReadOnlyList<Student> GetStudents() => _students;
    public IReadOnlyList<Teacher> GetTeachers() => _teachers;
    public IReadOnlyList<Course> GetCourses() => _courses;
    public IReadOnlyList<CourseClass> GetClasses() => _classes;
    public IReadOnlyList<Enrollment> GetEnrollments() => _enrollments;
    public IReadOnlyList<Receipt> GetReceipts() => _receipts;
    public IReadOnlyList<TuitionDebt> GetDebts() => _debts;
    public IReadOnlyList<ClassSession> GetSessions() => _sessions;
    public IReadOnlyList<AttendanceRecord> GetAttendanceRecords() => _attendance;
    public IReadOnlyList<ExamResult> GetExamResults() => _examResults;
    public IReadOnlyList<NewsArticle> GetNewsArticles() => _articles;

    public StudentRegistrationResult RegisterStudent(string fullName, string email, string phone)
    {
        var normalizedEmail = email.Trim();

        lock (_syncRoot)
        {
            var exists = _students.Any(x => x.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)) ||
                         _accounts.Any(x => x.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                return StudentRegistrationResult.Fail("Email nay da ton tai trong du lieu demo.");
            }

            var nextId = _students.Count == 0 ? 1 : _students.Max(x => x.Id) + 1;
            var nextCodeNumber = _students
                .Select(x => ExtractStudentNumber(x.Code))
                .DefaultIfEmpty(0)
                .Max() + 1;
            var studentCode = $"HV{nextCodeNumber:000}";

            _students.Add(new Student
            {
                Id = nextId,
                Code = studentCode,
                FullName = fullName.Trim(),
                Email = normalizedEmail,
                Phone = phone.Trim(),
                Level = "Cho tu van",
                Status = "Moi dang ky",
                CourseName = "Chua chon khoa",
                ClassCode = "Chua xep lop",
                JoinedOn = DateTime.Now,
                TuitionFee = 0,
                PaidAmount = 0,
                DebtAmount = 0
            });

            return StudentRegistrationResult.Success(
                $"SQL Server dang khong kha dung nen he thong da ghi nhan dang ky o che do demo voi ma {studentCode}.",
                studentCode);
        }
    }

    private static int ExtractStudentNumber(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return 0;
        }

        var digits = new string(code.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var value) ? value : 0;
    }
}

/*
    Extended seed data for LanguageCenterDB
    - Run this after the schema script in db.txt
    - Safe to run multiple times
    - Sample login accounts after seeding:
        admin / 123456
        admin2 / 123456
        staff01 / 123456
        staff02 / 123456
        staff03 / 123456
        t001 -> t010 / 123456
*/

USE LanguageCenterDB;
GO

SET NOCOUNT ON;
GO

DECLARE @SeedReferenceDate DATE = '2026-04-04';

BEGIN TRY
    BEGIN TRAN;

    DECLARE @AccountsSource TABLE
    (
        Username NVARCHAR(50),
        PasswordHash NVARCHAR(255),
        FullName NVARCHAR(100),
        Email NVARCHAR(100),
        Phone NVARCHAR(20),
        Role NVARCHAR(20),
        IsActive BIT,
        Status TINYINT,
        CreatedAt DATETIME2
    );

    INSERT INTO @AccountsSource VALUES
    (N'admin', N'123456', N'Administrator', N'admin@languagecenter.local', N'0900000000', N'Admin', 1, 1, '2026-01-01T08:00:00'),
    (N'admin2', N'123456', N'Nguyễn Đức An', N'admin2@languagecenter.local', N'0900000011', N'Admin', 1, 1, '2026-01-05T08:00:00'),
    (N'staff01', N'123456', N'Phạm Thu Hằng', N'staff01@languagecenter.local', N'0900000021', N'Staff', 1, 1, '2026-01-08T08:00:00'),
    (N'staff02', N'123456', N'Lê Thanh Phương', N'staff02@languagecenter.local', N'0900000022', N'Staff', 1, 1, '2026-01-10T08:00:00'),
    (N'staff03', N'123456', N'Trần Hoài Nam', N'staff03@languagecenter.local', N'0900000023', N'Staff', 1, 1, '2026-01-12T08:00:00'),
    (N't001', N'123456', N'Nguyễn Minh Anh', N'minhanh.teacher@gmail.com', N'0911111111', N'Teacher', 1, 1, '2026-01-15T08:00:00'),
    (N't002', N'123456', N'Trần Hoàng Phúc', N'hoangphuc.teacher@gmail.com', N'0922222222', N'Teacher', 1, 1, '2026-01-16T08:00:00'),
    (N't003', N'123456', N'Lê Thu Trang', N'thu.trang.teacher@gmail.com', N'0930000003', N'Teacher', 1, 1, '2026-01-17T08:00:00'),
    (N't004', N'123456', N'Phạm Gia Bảo', N'gia.bao.teacher@gmail.com', N'0930000004', N'Teacher', 1, 1, '2026-01-18T08:00:00'),
    (N't005', N'123456', N'Võ Khánh Linh', N'khanh.linh.teacher@gmail.com', N'0930000005', N'Teacher', 1, 1, '2026-01-19T08:00:00'),
    (N't006', N'123456', N'Đỗ Quốc Việt', N'quoc.viet.teacher@gmail.com', N'0930000006', N'Teacher', 1, 1, '2026-01-20T08:00:00'),
    (N't007', N'123456', N'Nguyễn Bảo Châu', N'bao.chau.teacher@gmail.com', N'0930000007', N'Teacher', 1, 1, '2026-01-21T08:00:00'),
    (N't008', N'123456', N'Bùi Thị Thu Hà', N'thu.ha.teacher@gmail.com', N'0930000008', N'Teacher', 1, 1, '2026-01-22T08:00:00'),
    (N't009', N'123456', N'Trần Đức Huy', N'duc.huy.teacher@gmail.com', N'0930000009', N'Teacher', 1, 1, '2026-01-23T08:00:00'),
    (N't010', N'123456', N'Hoàng Nhật Quang', N'nhat.quang.teacher@gmail.com', N'0930000010', N'Teacher', 1, 1, '2026-01-24T08:00:00');

    MERGE dbo.Accounts AS target
    USING @AccountsSource AS source
        ON target.Username = source.Username
    WHEN MATCHED THEN
        UPDATE SET
            PasswordHash = source.PasswordHash,
            FullName = source.FullName,
            Email = source.Email,
            Phone = source.Phone,
            Role = source.Role,
            IsActive = source.IsActive,
            Status = source.Status,
            IsDeleted = 0,
            UpdatedAt = SYSDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (Username, PasswordHash, FullName, Email, Phone, Role, IsActive, Status, IsDeleted, CreatedAt)
        VALUES (source.Username, source.PasswordHash, source.FullName, source.Email, source.Phone, source.Role, source.IsActive, source.Status, 0, source.CreatedAt);

    DECLARE @TeacherSource TABLE
    (
        TeacherCode NVARCHAR(20),
        FullName NVARCHAR(100),
        Phone NVARCHAR(20),
        Email NVARCHAR(100),
        Specialization NVARCHAR(100),
        Status TINYINT,
        CreatedAt DATETIME2
    );

    INSERT INTO @TeacherSource VALUES
    (N'T001', N'Nguyễn Minh Anh', N'0911111111', N'minhanh.teacher@gmail.com', N'English Communication', 1, '2026-01-15T08:00:00'),
    (N'T002', N'Trần Hoàng Phúc', N'0922222222', N'hoangphuc.teacher@gmail.com', N'IELTS Foundation', 1, '2026-01-16T08:00:00'),
    (N'T003', N'Lê Thu Trang', N'0930000003', N'thu.trang.teacher@gmail.com', N'Basic English', 1, '2026-01-17T08:00:00'),
    (N'T004', N'Phạm Gia Bảo', N'0930000004', N'gia.bao.teacher@gmail.com', N'TOEIC', 1, '2026-01-18T08:00:00'),
    (N'T005', N'Võ Khánh Linh', N'0930000005', N'khanh.linh.teacher@gmail.com', N'IELTS Intensive', 1, '2026-01-19T08:00:00'),
    (N'T006', N'Đỗ Quốc Việt', N'0930000006', N'quoc.viet.teacher@gmail.com', N'Business English', 1, '2026-01-20T08:00:00'),
    (N'T007', N'Nguyễn Bảo Châu', N'0930000007', N'bao.chau.teacher@gmail.com', N'Kids English', 1, '2026-01-21T08:00:00'),
    (N'T008', N'Bùi Thị Thu Hà', N'0930000008', N'thu.ha.teacher@gmail.com', N'Grammar Foundation', 1, '2026-01-22T08:00:00'),
    (N'T009', N'Trần Đức Huy', N'0930000009', N'duc.huy.teacher@gmail.com', N'TOEIC Intensive', 1, '2026-01-23T08:00:00'),
    (N'T010', N'Hoàng Nhật Quang', N'0930000010', N'nhat.quang.teacher@gmail.com', N'Speaking & Presentation', 1, '2026-01-24T08:00:00');

    MERGE dbo.Teachers AS target
    USING @TeacherSource AS source
        ON target.TeacherCode = source.TeacherCode
    WHEN MATCHED THEN
        UPDATE SET
            FullName = source.FullName,
            Phone = source.Phone,
            Email = source.Email,
            Specialization = source.Specialization,
            Status = source.Status,
            IsDeleted = 0,
            UpdatedAt = SYSDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (TeacherCode, FullName, Phone, Email, Specialization, Status, IsDeleted, CreatedAt)
        VALUES (source.TeacherCode, source.FullName, source.Phone, source.Email, source.Specialization, source.Status, 0, source.CreatedAt);

    DECLARE @StudentSource TABLE
    (
        StudentCode NVARCHAR(20),
        FullName NVARCHAR(100),
        DateOfBirth DATE,
        Gender NVARCHAR(10),
        Phone NVARCHAR(20),
        Email NVARCHAR(100),
        Address NVARCHAR(255),
        Status TINYINT,
        CreatedAt DATETIME2
    );

    INSERT INTO @StudentSource VALUES
    (N'S001', N'Lê Thị Lan', '2005-05-10', N'Nữ', N'0933333333', N'lan.student@gmail.com', N'Hồ Chí Minh', 1, '2026-01-05T08:00:00'),
    (N'S002', N'Phạm Quốc Huy', '2004-09-21', N'Nam', N'0944444444', N'huy.student@gmail.com', N'Hồ Chí Minh', 1, '2026-01-06T08:00:00'),
    (N'S003', N'Nguyễn Minh Khoa', '2006-02-14', N'Nam', N'0903000003', N'minh.khoa@gmail.com', N'Thủ Đức, Hồ Chí Minh', 1, '2026-01-12T08:00:00'),
    (N'S004', N'Trần Bảo Ngọc', '2005-07-03', N'Nữ', N'0903000004', N'bao.ngoc@gmail.com', N'Quận 7, Hồ Chí Minh', 1, '2026-01-15T08:00:00'),
    (N'S005', N'Võ Gia Huy', '2004-11-11', N'Nam', N'0903000005', N'gia.huy@gmail.com', N'Quận 12, Hồ Chí Minh', 1, '2026-01-18T08:00:00'),
    (N'S006', N'Lê Khánh Linh', '2005-01-25', N'Nữ', N'0903000006', N'khanh.linh@student.com', N'Bình Thạnh, Hồ Chí Minh', 1, '2026-01-20T08:00:00'),
    (N'S007', N'Phan Tuấn Kiệt', '2003-12-19', N'Nam', N'0903000007', N'tuan.kiet@gmail.com', N'Gò Vấp, Hồ Chí Minh', 1, '2026-01-22T08:00:00'),
    (N'S008', N'Đặng Thu Hà', '2005-08-05', N'Nữ', N'0903000008', N'thu.ha@student.com', N'Tân Bình, Hồ Chí Minh', 1, '2026-01-25T08:00:00'),
    (N'S009', N'Hoàng Minh Tâm', '2004-04-30', N'Nam', N'0903000009', N'minh.tam@gmail.com', N'Thủ Đức, Hồ Chí Minh', 1, '2026-01-28T08:00:00'),
    (N'S010', N'Bùi Ngọc Anh', '2005-09-13', N'Nữ', N'0903000010', N'ngoc.anh@gmail.com', N'Quận 3, Hồ Chí Minh', 1, '2026-02-01T08:00:00'),
    (N'S011', N'Nguyễn Quốc Đạt', '2004-02-18', N'Nam', N'0903000011', N'quoc.dat@gmail.com', N'Phú Nhuận, Hồ Chí Minh', 1, '2026-02-04T08:00:00'),
    (N'S012', N'Đỗ Mỹ Tiên', '2006-06-22', N'Nữ', N'0903000012', N'my.tien@gmail.com', N'Quận 10, Hồ Chí Minh', 1, '2026-02-07T08:00:00'),
    (N'S013', N'Trương Gia Bảo', '2005-10-09', N'Nam', N'0903000013', N'gia.bao@student.com', N'Bình Tân, Hồ Chí Minh', 1, '2026-02-10T08:00:00'),
    (N'S014', N'Phạm Thảo Vy', '2004-03-16', N'Nữ', N'0903000014', N'thao.vy@gmail.com', N'Tân Phú, Hồ Chí Minh', 1, '2026-02-13T08:00:00'),
    (N'S015', N'Lý Hoàng Phúc', '2003-08-28', N'Nam', N'0903000015', N'hoang.phuc@student.com', N'Quận 5, Hồ Chí Minh', 1, '2026-02-16T08:00:00'),
    (N'S016', N'Ngô Tường Vi', '2005-12-01', N'Nữ', N'0903000016', N'tuong.vi@gmail.com', N'Quận 1, Hồ Chí Minh', 1, '2026-02-20T08:00:00'),
    (N'S017', N'Nguyễn Thành Đạt', '2004-07-14', N'Nam', N'0903000017', N'thanh.dat@gmail.com', N'Quận 8, Hồ Chí Minh', 1, '2026-02-24T08:00:00'),
    (N'S018', N'Trần Bích Ngọc', '2005-06-06', N'Nữ', N'0903000018', N'bich.ngoc@student.com', N'Nhà Bè, Hồ Chí Minh', 1, '2026-02-28T08:00:00'),
    (N'S019', N'Lê Đức Anh', '2003-11-23', N'Nam', N'0903000019', N'duc.anh@gmail.com', N'Quận 6, Hồ Chí Minh', 1, '2026-03-03T08:00:00'),
    (N'S020', N'Võ Thu Phương', '2004-10-17', N'Nữ', N'0903000020', N'thu.phuong@gmail.com', N'Củ Chi, Hồ Chí Minh', 1, '2026-03-05T08:00:00'),
    (N'S021', N'Mai Khánh Chi', '2006-01-08', N'Nữ', N'0903000021', N'khanh.chi@student.com', N'Hóc Môn, Hồ Chí Minh', 1, '2026-03-10T08:00:00'),
    (N'S022', N'Đinh Tuấn Anh', '2005-04-12', N'Nam', N'0903000022', N'tuan.anh@gmail.com', N'Bình Chánh, Hồ Chí Minh', 1, '2026-03-12T08:00:00'),
    (N'S023', N'Huỳnh Bảo Trân', '2004-06-09', N'Nữ', N'0903000023', N'bao.tran@student.com', N'Quận 11, Hồ Chí Minh', 1, '2026-03-15T08:00:00'),
    (N'S024', N'Châu Minh Quân', '2003-09-27', N'Nam', N'0903000024', N'minh.quan@gmail.com', N'Thủ Đức, Hồ Chí Minh', 1, '2026-03-18T08:00:00');

    MERGE dbo.Students AS target
    USING @StudentSource AS source
        ON target.StudentCode = source.StudentCode
    WHEN MATCHED THEN
        UPDATE SET
            FullName = source.FullName,
            DateOfBirth = source.DateOfBirth,
            Gender = source.Gender,
            Phone = source.Phone,
            Email = source.Email,
            Address = source.Address,
            Status = source.Status,
            IsDeleted = 0,
            UpdatedAt = SYSDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (StudentCode, FullName, DateOfBirth, Gender, Phone, Email, Address, Status, IsDeleted, CreatedAt)
        VALUES (source.StudentCode, source.FullName, source.DateOfBirth, source.Gender, source.Phone, source.Email, source.Address, source.Status, 0, source.CreatedAt);

    DECLARE @CourseSource TABLE
    (
        CourseCode NVARCHAR(20),
        CourseName NVARCHAR(100),
        Description NVARCHAR(500),
        DurationHours INT,
        TuitionFee DECIMAL(18,2),
        Status TINYINT,
        CreatedAt DATETIME2
    );

    INSERT INTO @CourseSource VALUES
    (N'ENG-BASIC', N'Tiếng Anh Cơ Bản', N'Khóa học nền tảng dành cho học viên cần củng cố lại ngữ pháp, từ vựng và phản xạ giao tiếp cơ bản.', 60, 3500000, 1, '2026-01-05T08:00:00'),
    (N'ENG-IELTS', N'IELTS Foundation', N'Khóa học xây nền IELTS cho học viên band 3.0 - 4.0, tập trung đồng đều 4 kỹ năng.', 80, 5500000, 1, '2026-01-06T08:00:00'),
    (N'TOEIC-650', N'TOEIC 650+', N'Lộ trình luyện đề và chiến lược làm bài TOEIC để đạt mục tiêu 650+.', 72, 4800000, 1, '2026-01-08T08:00:00'),
    (N'TOEIC-900', N'TOEIC 900+', N'Khóa nâng cao cho học viên hướng tới điểm TOEIC 850 - 900+.', 90, 7200000, 1, '2026-01-10T08:00:00'),
    (N'IELTS-INTENSIVE', N'IELTS Intensive 6.5+', N'Khóa học tăng tốc cho học viên đã có nền tảng, tập trung band mục tiêu 6.5 trở lên.', 100, 8600000, 1, '2026-01-12T08:00:00'),
    (N'COMM-BUSINESS', N'Business English', N'Tiếng Anh giao tiếp công sở, họp, email và thuyết trình dành cho người đi làm.', 48, 4200000, 1, '2026-01-14T08:00:00'),
    (N'KIDS-STARTER', N'Kids Starter', N'Khóa tiếng Anh thiếu nhi khởi động cho học sinh tiểu học.', 40, 3200000, 1, '2026-01-16T08:00:00'),
    (N'SPEAK-FOUNDATION', N'Speaking Foundation', N'Khóa học tăng phản xạ nói, phát âm và giao tiếp thực tế.', 45, 3800000, 1, '2026-01-18T08:00:00');

    MERGE dbo.Courses AS target
    USING @CourseSource AS source
        ON target.CourseCode = source.CourseCode
    WHEN MATCHED THEN
        UPDATE SET
            CourseName = source.CourseName,
            Description = source.Description,
            DurationHours = source.DurationHours,
            TuitionFee = source.TuitionFee,
            Status = source.Status,
            IsDeleted = 0,
            UpdatedAt = SYSDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (CourseCode, CourseName, Description, DurationHours, TuitionFee, Status, IsDeleted, CreatedAt)
        VALUES (source.CourseCode, source.CourseName, source.Description, source.DurationHours, source.TuitionFee, source.Status, 0, source.CreatedAt);

    DECLARE @ClassSource TABLE
    (
        ClassCode NVARCHAR(20),
        ClassName NVARCHAR(100),
        CourseCode NVARCHAR(20),
        TeacherCode NVARCHAR(20),
        StartDate DATE,
        EndDate DATE,
        ScheduleText NVARCHAR(255),
        Capacity INT,
        Status TINYINT,
        CreatedAt DATETIME2
    );

    INSERT INTO @ClassSource VALUES
    (N'C001', N'Lớp Basic 01', N'ENG-BASIC', N'T001', '2026-04-10', '2026-06-10', N'T2-T4-T6 18:00-20:00', 25, 1, '2026-03-20T08:00:00'),
    (N'C002', N'Lớp Basic 02', N'ENG-BASIC', N'T003', '2026-03-12', '2026-05-12', N'T3-T5 18:00-20:00', 22, 1, '2026-02-25T08:00:00'),
    (N'C003', N'IELTS Foundation 01', N'ENG-IELTS', N'T002', '2026-03-18', '2026-06-18', N'T2-T4-T6 19:00-21:00', 18, 1, '2026-03-01T08:00:00'),
    (N'C004', N'IELTS Intensive 01', N'IELTS-INTENSIVE', N'T005', '2026-05-06', '2026-08-06', N'T3-T5-T7 18:30-20:30', 16, 1, '2026-03-28T08:00:00'),
    (N'C005', N'Business English 01', N'COMM-BUSINESS', N'T006', '2026-03-25', '2026-06-25', N'T2-T4 19:00-21:00', 20, 1, '2026-03-05T08:00:00'),
    (N'C006', N'Kids Starter 01', N'KIDS-STARTER', N'T007', '2026-04-12', '2026-07-12', N'T7-CN 08:00-10:00', 20, 1, '2026-03-25T08:00:00'),
    (N'C007', N'TOEIC 650 01', N'TOEIC-650', N'T004', '2026-02-20', '2026-05-20', N'T2-T4-T6 18:30-20:30', 25, 1, '2026-02-10T08:00:00'),
    (N'C008', N'TOEIC 650 02', N'TOEIC-650', N'T009', '2026-04-20', '2026-07-20', N'T3-T5 18:00-20:00', 25, 1, '2026-03-30T08:00:00'),
    (N'C009', N'TOEIC 900 01', N'TOEIC-900', N'T009', '2026-01-10', '2026-03-25', N'T2-T4 19:00-21:00', 15, 1, '2025-12-20T08:00:00'),
    (N'C010', N'Speaking Foundation 01', N'SPEAK-FOUNDATION', N'T010', '2026-03-10', '2026-05-10', N'CN 08:00-11:00', 18, 1, '2026-02-20T08:00:00'),
    (N'C011', N'IELTS Foundation 02', N'ENG-IELTS', N'T005', '2026-02-05', '2026-04-25', N'T3-T5-T7 18:00-20:00', 18, 0, '2026-01-28T08:00:00'),
    (N'C012', N'Business English 02', N'COMM-BUSINESS', N'T006', '2026-05-15', '2026-08-15', N'T2-T4 18:30-20:30', 20, 1, '2026-04-01T08:00:00');

    ;WITH ClassResolved AS
    (
        SELECT
            src.ClassCode,
            src.ClassName,
            c.Id AS CourseId,
            t.Id AS TeacherId,
            src.StartDate,
            src.EndDate,
            src.ScheduleText,
            src.Capacity,
            src.Status,
            src.CreatedAt
        FROM @ClassSource src
        INNER JOIN dbo.Courses c ON c.CourseCode = src.CourseCode AND c.IsDeleted = 0
        LEFT JOIN dbo.Teachers t ON t.TeacherCode = src.TeacherCode AND t.IsDeleted = 0
    )
    MERGE dbo.Classes AS target
    USING ClassResolved AS source
        ON target.ClassCode = source.ClassCode
    WHEN MATCHED THEN
        UPDATE SET
            ClassName = source.ClassName,
            CourseId = source.CourseId,
            TeacherId = source.TeacherId,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            ScheduleText = source.ScheduleText,
            Capacity = source.Capacity,
            Status = source.Status,
            IsDeleted = 0,
            UpdatedAt = SYSDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (ClassCode, ClassName, CourseId, TeacherId, StartDate, EndDate, ScheduleText, Capacity, Status, IsDeleted, CreatedAt)
        VALUES (source.ClassCode, source.ClassName, source.CourseId, source.TeacherId, source.StartDate, source.EndDate, source.ScheduleText, source.Capacity, source.Status, 0, source.CreatedAt);

    DECLARE @EnrollmentSource TABLE
    (
        StudentCode NVARCHAR(20),
        ClassCode NVARCHAR(20),
        EnrollDate DATE,
        Status NVARCHAR(20),
        TotalFee DECIMAL(18,2),
        DiscountAmount DECIMAL(18,2),
        Note NVARCHAR(255),
        CreatedAt DATETIME2
    );

    INSERT INTO @EnrollmentSource VALUES
    (N'S001', N'C001', '2026-04-02', N'DangHoc', 3500000, 0, N'Ghi danh lớp Basic 01', '2026-04-02T08:00:00'),
    (N'S003', N'C001', '2026-04-01', N'DangHoc', 3500000, 100000, N'Đăng ký sớm', '2026-04-01T08:00:00'),
    (N'S004', N'C001', '2026-04-03', N'DangHoc', 3500000, 300000, N'Có ưu đãi học nhóm', '2026-04-03T08:00:00'),
    (N'S005', N'C001', '2026-04-03', N'DangHoc', 3500000, 0, N'Ghi danh trực tiếp tại trung tâm', '2026-04-03T08:00:00'),
    (N'S006', N'C002', '2026-03-10', N'DangHoc', 3500000, 0, N'Đã nhận tài liệu đầu khóa', '2026-03-10T08:00:00'),
    (N'S007', N'C002', '2026-03-11', N'DangHoc', 3500000, 200000, N'Đăng ký qua tư vấn viên', '2026-03-11T08:00:00'),
    (N'S008', N'C002', '2026-03-12', N'BaoLuu', 3500000, 500000, N'Bảo lưu do lịch cá nhân', '2026-03-12T08:00:00'),
    (N'S009', N'C002', '2026-03-13', N'DangHoc', 3500000, 0, N'Đang theo học đều', '2026-03-13T08:00:00'),
    (N'S002', N'C003', '2026-03-14', N'DangHoc', 5500000, 0, N'Đầu vào IELTS 4.0', '2026-03-14T08:00:00'),
    (N'S010', N'C003', '2026-03-15', N'DangHoc', 5500000, 300000, N'Có cam kết đầu ra nội bộ', '2026-03-15T08:00:00'),
    (N'S011', N'C003', '2026-03-16', N'DangHoc', 5500000, 500000, N'Ưu đãi học viên cũ', '2026-03-16T08:00:00'),
    (N'S012', N'C003', '2026-03-17', N'DangHoc', 5500000, 0, N'Đăng ký theo nhóm bạn', '2026-03-17T08:00:00'),
    (N'S013', N'C003', '2026-03-18', N'DangHoc', 5500000, 1000000, N'Học bổng đầu vào', '2026-03-18T08:00:00'),
    (N'S014', N'C004', '2026-04-01', N'DangHoc', 8600000, 0, N'Chuẩn bị vào lớp tăng tốc', '2026-04-01T08:00:00'),
    (N'S015', N'C004', '2026-04-02', N'DangHoc', 8600000, 600000, N'Ưu đãi đóng sớm', '2026-04-02T08:00:00'),
    (N'S016', N'C004', '2026-04-03', N'DangHoc', 8600000, 0, N'Đăng ký lớp buổi tối', '2026-04-03T08:00:00'),
    (N'S017', N'C005', '2026-03-20', N'DangHoc', 4200000, 0, N'Người đi làm cần cải thiện email', '2026-03-20T08:00:00'),
    (N'S018', N'C005', '2026-03-21', N'DangHoc', 4200000, 200000, N'Đăng ký theo giới thiệu', '2026-03-21T08:00:00'),
    (N'S019', N'C005', '2026-03-22', N'Huy', 4200000, 0, N'Hủy do thay đổi lịch công tác', '2026-03-22T08:00:00'),
    (N'S020', N'C006', '2026-04-02', N'DangHoc', 3200000, 0, N'Phụ huynh đăng ký cuối tuần', '2026-04-02T08:00:00'),
    (N'S021', N'C006', '2026-04-02', N'DangHoc', 3200000, 200000, N'Ưu đãi anh chị em', '2026-04-02T08:00:00'),
    (N'S022', N'C006', '2026-04-03', N'DangHoc', 3200000, 0, N'Đăng ký lớp thiếu nhi', '2026-04-03T08:00:00'),
    (N'S023', N'C007', '2026-02-16', N'DangHoc', 4800000, 0, N'Mục tiêu TOEIC 650+', '2026-02-16T08:00:00'),
    (N'S024', N'C007', '2026-02-17', N'DangHoc', 4800000, 200000, N'Đăng ký sớm đầu khóa', '2026-02-17T08:00:00'),
    (N'S003', N'C007', '2026-02-18', N'DangHoc', 4800000, 0, N'Học song song khóa nền tảng', '2026-02-18T08:00:00'),
    (N'S005', N'C007', '2026-02-19', N'BaoLuu', 4800000, 300000, N'Bảo lưu giữa khóa', '2026-02-19T08:00:00'),
    (N'S006', N'C007', '2026-02-19', N'DangHoc', 4800000, 0, N'Đã tham gia đều các buổi', '2026-02-19T08:00:00'),
    (N'S007', N'C008', '2026-04-02', N'DangHoc', 4800000, 0, N'Chờ khai giảng lớp TOEIC 650 02', '2026-04-02T08:00:00'),
    (N'S008', N'C008', '2026-04-03', N'DangHoc', 4800000, 200000, N'Ghi danh kèm bạn học', '2026-04-03T08:00:00'),
    (N'S009', N'C008', '2026-04-03', N'DangHoc', 4800000, 0, N'Đặt cọc giữ chỗ', '2026-04-03T08:00:00'),
    (N'S010', N'C009', '2026-01-05', N'HoanThanh', 7200000, 500000, N'Đã hoàn thành khóa TOEIC nâng cao', '2026-01-05T08:00:00'),
    (N'S011', N'C009', '2026-01-06', N'HoanThanh', 7200000, 700000, N'Kết thúc khóa với kết quả tốt', '2026-01-06T08:00:00'),
    (N'S012', N'C009', '2026-01-07', N'HoanThanh', 7200000, 0, N'Đã nhận chứng nhận nội bộ', '2026-01-07T08:00:00'),
    (N'S013', N'C010', '2026-03-05', N'DangHoc', 3800000, 0, N'Phát triển kỹ năng nói', '2026-03-05T08:00:00'),
    (N'S014', N'C010', '2026-03-06', N'DangHoc', 3800000, 150000, N'Tham gia lớp nói cuối tuần', '2026-03-06T08:00:00'),
    (N'S015', N'C010', '2026-03-07', N'DangHoc', 3800000, 0, N'Đang theo học đều', '2026-03-07T08:00:00'),
    (N'S016', N'C011', '2026-02-01', N'BaoLuu', 5500000, 0, N'Bảo lưu do lịch thi', '2026-02-01T08:00:00'),
    (N'S017', N'C011', '2026-02-02', N'BaoLuu', 5500000, 300000, N'Tạm nghỉ vì công tác', '2026-02-02T08:00:00'),
    (N'S018', N'C012', '2026-04-03', N'DangHoc', 4200000, 0, N'Chờ khai giảng lớp Business 02', '2026-04-03T08:00:00'),
    (N'S019', N'C012', '2026-04-03', N'DangHoc', 4200000, 0, N'Đăng ký lại sau khi đổi lịch', '2026-04-03T08:00:00'),
    (N'S020', N'C012', '2026-04-04', N'DangHoc', 4200000, 300000, N'Ưu đãi thanh toán sớm', '2026-04-04T08:00:00');

    ;WITH EnrollmentResolved AS
    (
        SELECT
            s.Id AS StudentId,
            c.Id AS ClassId,
            src.EnrollDate,
            src.Status,
            src.TotalFee,
            src.DiscountAmount,
            src.Note,
            src.CreatedAt
        FROM @EnrollmentSource src
        INNER JOIN dbo.Students s ON s.StudentCode = src.StudentCode AND s.IsDeleted = 0
        INNER JOIN dbo.Classes c ON c.ClassCode = src.ClassCode AND c.IsDeleted = 0
    )
    MERGE dbo.Enrollments AS target
    USING EnrollmentResolved AS source
        ON target.StudentId = source.StudentId AND target.ClassId = source.ClassId
    WHEN MATCHED THEN
        UPDATE SET
            EnrollDate = source.EnrollDate,
            Status = source.Status,
            TotalFee = source.TotalFee,
            DiscountAmount = source.DiscountAmount,
            Note = source.Note,
            IsDeleted = 0,
            UpdatedAt = SYSDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (StudentId, ClassId, EnrollDate, Status, TotalFee, DiscountAmount, Note, IsDeleted, CreatedAt)
        VALUES (source.StudentId, source.ClassId, source.EnrollDate, source.Status, source.TotalFee, source.DiscountAmount, source.Note, 0, source.CreatedAt);

    ;WITH EnrollmentData AS
    (
        SELECT
            e.Id AS EnrollmentId,
            src.StudentCode,
            src.ClassCode,
            src.EnrollDate,
            src.Status,
            CAST(src.TotalFee - src.DiscountAmount AS DECIMAL(18,2)) AS FinalFee,
            TRY_CONVERT(INT, RIGHT(src.StudentCode, 3)) AS StudentNo
        FROM @EnrollmentSource src
        INNER JOIN dbo.Students s ON s.StudentCode = src.StudentCode AND s.IsDeleted = 0
        INNER JOIN dbo.Classes c ON c.ClassCode = src.ClassCode AND c.IsDeleted = 0
        INNER JOIN dbo.Enrollments e ON e.StudentId = s.Id AND e.ClassId = c.Id AND e.IsDeleted = 0
    ),
    ReceiptSource AS
    (
        SELECT
            CONCAT(N'R1', RIGHT(StudentCode, 3), RIGHT(ClassCode, 3)) AS ReceiptCode,
            EnrollmentId,
            DATEADD(DAY, 1, CAST(EnrollDate AS DATETIME2)) AS PaymentDate,
            CAST(CASE
                    WHEN Status = N'HoanThanh' THEN ROUND(FinalFee * 0.55, 0)
                    WHEN StudentNo % 5 = 0 THEN FinalFee
                    WHEN StudentNo % 5 = 1 THEN ROUND(FinalFee * 0.70, 0)
                    WHEN StudentNo % 5 = 2 THEN ROUND(FinalFee * 0.50, 0)
                    WHEN StudentNo % 5 = 3 THEN ROUND(FinalFee * 0.35, 0)
                    ELSE ROUND(FinalFee * 0.25, 0)
                 END AS DECIMAL(18,2)) AS Amount,
            CASE StudentNo % 3 WHEN 0 THEN N'Cash' WHEN 1 THEN N'Transfer' ELSE N'Card' END AS PaymentMethod,
            N'Thanh toán đợt 1' AS Note
        FROM EnrollmentData
        WHERE Status <> N'Huy' AND NOT (StudentCode = N'S001' AND ClassCode = N'C001')

        UNION ALL

        SELECT
            CONCAT(N'R2', RIGHT(StudentCode, 3), RIGHT(ClassCode, 3)) AS ReceiptCode,
            EnrollmentId,
            DATEADD(DAY, 15, CAST(EnrollDate AS DATETIME2)) AS PaymentDate,
            CAST(FinalFee - CASE
                    WHEN Status = N'HoanThanh' THEN ROUND(FinalFee * 0.55, 0)
                    WHEN StudentNo % 5 = 0 THEN FinalFee
                    WHEN StudentNo % 5 = 1 THEN ROUND(FinalFee * 0.70, 0)
                    WHEN StudentNo % 5 = 2 THEN ROUND(FinalFee * 0.50, 0)
                    WHEN StudentNo % 5 = 3 THEN ROUND(FinalFee * 0.35, 0)
                    ELSE ROUND(FinalFee * 0.25, 0)
                 END AS DECIMAL(18,2)) AS Amount,
            CASE (StudentNo + 1) % 3 WHEN 0 THEN N'Cash' WHEN 1 THEN N'Transfer' ELSE N'Card' END AS PaymentMethod,
            N'Thanh toán đợt 2' AS Note
        FROM EnrollmentData
        WHERE Status <> N'Huy'
          AND NOT (StudentCode = N'S001' AND ClassCode = N'C001')
          AND (Status = N'HoanThanh' OR StudentNo % 5 IN (0, 1))
    )
    MERGE dbo.Receipts AS target
    USING (SELECT * FROM ReceiptSource WHERE Amount > 0) AS source
        ON target.ReceiptCode = source.ReceiptCode
    WHEN MATCHED THEN
        UPDATE SET
            EnrollmentId = source.EnrollmentId,
            PaymentDate = source.PaymentDate,
            Amount = source.Amount,
            PaymentMethod = source.PaymentMethod,
            Note = source.Note
    WHEN NOT MATCHED THEN
        INSERT (ReceiptCode, EnrollmentId, PaymentDate, Amount, PaymentMethod, Note, CreatedAt)
        VALUES (source.ReceiptCode, source.EnrollmentId, source.PaymentDate, source.Amount, source.PaymentMethod, source.Note, source.PaymentDate);

    DECLARE @SessionSource TABLE
    (
        ClassCode NVARCHAR(20),
        SessionDate DATE,
        Topic NVARCHAR(255),
        Note NVARCHAR(255)
    );

    INSERT INTO @SessionSource VALUES
    (N'C002', '2026-03-12', N'Giới thiệu khóa học và kiểm tra đầu vào', N'Buổi mở đầu'),
    (N'C002', '2026-03-17', N'Ngữ pháp nền tảng và phản xạ nói', N'Thực hành theo cặp'),
    (N'C002', '2026-03-24', N'Listening cơ bản và ghi chú ý chính', N'Bài tập tại lớp'),
    (N'C002', '2026-04-07', N'Reading skills cho trình độ cơ bản', N'Buổi sắp diễn ra'),
    (N'C003', '2026-03-18', N'IELTS orientation và placement', N'Buổi đầu khóa'),
    (N'C003', '2026-03-25', N'Writing Task 1 overview', N'Tập trung từ vựng biểu đồ'),
    (N'C003', '2026-04-01', N'Reading matching headings', N'Luyện tốc độ đọc'),
    (N'C003', '2026-04-08', N'Listening section 2 strategies', N'Buổi sắp diễn ra'),
    (N'C005', '2026-03-25', N'Business small talk', N'Role play theo nhóm'),
    (N'C005', '2026-03-30', N'Email etiquette in English', N'Bài tập email nội bộ'),
    (N'C005', '2026-04-03', N'Meeting language and follow-up', N'Có bài thực hành'),
    (N'C005', '2026-04-15', N'Presentation opening and closing', N'Buổi sắp diễn ra'),
    (N'C007', '2026-02-20', N'TOEIC diagnostic test', N'Kiểm tra đầu khóa'),
    (N'C007', '2026-02-27', N'Part 2 question-response', N'Luyện theo bộ đề'),
    (N'C007', '2026-03-06', N'Part 5 grammar traps', N'Tổng hợp lỗi thường gặp'),
    (N'C007', '2026-04-04', N'Part 3 short conversations', N'Buổi học hôm nay'),
    (N'C009', '2026-01-15', N'TOEIC 900 orientation', N'Khai giảng'),
    (N'C009', '2026-02-05', N'Reading speed and inference', N'Bài test giữa chặng'),
    (N'C009', '2026-03-10', N'Full mock test review', N'Chuẩn bị cuối khóa'),
    (N'C010', '2026-03-15', N'Pronunciation and intonation', N'Warm-up speaking'),
    (N'C010', '2026-03-22', N'Speaking fluency drills', N'Thực hành cặp đôi'),
    (N'C010', '2026-04-05', N'Presentation mini project', N'Buổi sắp diễn ra'),
    (N'C011', '2026-02-05', N'Foundation review and target setting', N'Buổi đầu khóa'),
    (N'C011', '2026-02-12', N'Listening note-taking basics', N'Có worksheet'),
    (N'C011', '2026-03-03', N'Reading true-false-not given', N'Lớp đang tạm dừng'),
    (N'C001', '2026-04-10', N'Khởi động lớp Basic 01', N'Buổi sắp tới'),
    (N'C001', '2026-04-13', N'Phát âm và từ vựng chủ đề gia đình', N'Buổi sắp tới'),
    (N'C004', '2026-05-06', N'IELTS intensive orientation', N'Chưa diễn ra'),
    (N'C006', '2026-04-12', N'Kids alphabet and phonics', N'Buổi sắp tới'),
    (N'C008', '2026-04-20', N'TOEIC 650 class introduction', N'Buổi sắp tới'),
    (N'C012', '2026-05-18', N'Business English onboarding', N'Buổi sắp tới');

    ;WITH SessionResolved AS
    (
        SELECT
            c.Id AS ClassId,
            src.ClassCode,
            src.SessionDate,
            src.Topic,
            src.Note
        FROM @SessionSource src
        INNER JOIN dbo.Classes c ON c.ClassCode = src.ClassCode AND c.IsDeleted = 0
    )
    MERGE dbo.ClassSessions AS target
    USING SessionResolved AS source
        ON target.ClassId = source.ClassId
       AND target.SessionDate = source.SessionDate
       AND ISNULL(target.Topic, N'') = ISNULL(source.Topic, N'')
    WHEN MATCHED THEN
        UPDATE SET Note = source.Note
    WHEN NOT MATCHED THEN
        INSERT (ClassId, SessionDate, Topic, Note, CreatedAt)
        VALUES (source.ClassId, source.SessionDate, source.Topic, source.Note, DATEADD(HOUR, 8, CAST(source.SessionDate AS DATETIME2)));

    ;WITH SessionData AS
    (
        SELECT
            cs.Id AS ClassSessionId,
            src.ClassCode,
            src.SessionDate,
            src.Topic
        FROM @SessionSource src
        INNER JOIN dbo.Classes c ON c.ClassCode = src.ClassCode AND c.IsDeleted = 0
        INNER JOIN dbo.ClassSessions cs
            ON cs.ClassId = c.Id
           AND cs.SessionDate = src.SessionDate
           AND ISNULL(cs.Topic, N'') = ISNULL(src.Topic, N'')
    ),
    AttendanceSource AS
    (
        SELECT
            e.Id AS EnrollmentId,
            sd.ClassSessionId,
            CASE
                WHEN (TRY_CONVERT(INT, RIGHT(src.StudentCode, 3)) + DATEPART(DAY, sd.SessionDate)) % 10 IN (0, 1) THEN N'Absent'
                WHEN (TRY_CONVERT(INT, RIGHT(src.StudentCode, 3)) + DATEPART(DAY, sd.SessionDate)) % 10 IN (2, 3) THEN N'Late'
                ELSE N'Present'
            END AS AttendanceStatus,
            CASE
                WHEN (TRY_CONVERT(INT, RIGHT(src.StudentCode, 3)) + DATEPART(DAY, sd.SessionDate)) % 10 IN (0, 1) THEN N'Vắng buổi học'
                WHEN (TRY_CONVERT(INT, RIGHT(src.StudentCode, 3)) + DATEPART(DAY, sd.SessionDate)) % 10 IN (2, 3) THEN N'Đi học muộn'
                ELSE N'Có mặt đúng giờ'
            END AS Note
        FROM @EnrollmentSource src
        INNER JOIN dbo.Students s ON s.StudentCode = src.StudentCode AND s.IsDeleted = 0
        INNER JOIN dbo.Classes c ON c.ClassCode = src.ClassCode AND c.IsDeleted = 0
        INNER JOIN dbo.Enrollments e ON e.StudentId = s.Id AND e.ClassId = c.Id AND e.IsDeleted = 0
        INNER JOIN SessionData sd ON sd.ClassCode = src.ClassCode
        WHERE src.Status <> N'Huy' AND sd.SessionDate <= @SeedReferenceDate
    )
    MERGE dbo.Attendances AS target
    USING AttendanceSource AS source
        ON target.EnrollmentId = source.EnrollmentId AND target.ClassSessionId = source.ClassSessionId
    WHEN MATCHED THEN
        UPDATE SET
            AttendanceStatus = source.AttendanceStatus,
            Note = source.Note
    WHEN NOT MATCHED THEN
        INSERT (EnrollmentId, ClassSessionId, AttendanceStatus, Note, CreatedAt)
        VALUES (source.EnrollmentId, source.ClassSessionId, source.AttendanceStatus, source.Note, SYSDATETIME());

    DECLARE @ExamSource TABLE
    (
        ClassCode NVARCHAR(20),
        ExamName NVARCHAR(100),
        ExamType NVARCHAR(20),
        ExamDate DATE,
        MaxScore DECIMAL(5,2)
    );

    INSERT INTO @ExamSource VALUES
    (N'C002', N'Basic Progress Test', N'Test', '2026-03-24', 10),
    (N'C003', N'IELTS Writing Quiz', N'Test', '2026-04-01', 10),
    (N'C003', N'IELTS Foundation Midterm', N'Midterm', '2026-04-03', 10),
    (N'C005', N'Business Presentation Check', N'Speaking', '2026-04-03', 10),
    (N'C007', N'TOEIC Mock Test 1', N'Test', '2026-03-18', 100),
    (N'C007', N'TOEIC Midterm', N'Midterm', '2026-04-02', 100),
    (N'C009', N'TOEIC 900 Final', N'Final', '2026-03-20', 100),
    (N'C010', N'Speaking Performance Check', N'Speaking', '2026-03-29', 10),
    (N'C011', N'IELTS Listening Quiz', N'Test', '2026-03-15', 10),
    (N'C011', N'IELTS Foundation Review', N'Midterm', '2026-03-22', 10);

    ;WITH ExamResolved AS
    (
        SELECT
            c.Id AS ClassId,
            src.ClassCode,
            src.ExamName,
            src.ExamType,
            src.ExamDate,
            src.MaxScore
        FROM @ExamSource src
        INNER JOIN dbo.Classes c ON c.ClassCode = src.ClassCode AND c.IsDeleted = 0
    )
    MERGE dbo.Exams AS target
    USING ExamResolved AS source
        ON target.ClassId = source.ClassId
       AND target.ExamName = source.ExamName
       AND target.ExamType = source.ExamType
       AND target.ExamDate = source.ExamDate
    WHEN MATCHED THEN
        UPDATE SET MaxScore = source.MaxScore
    WHEN NOT MATCHED THEN
        INSERT (ClassId, ExamName, ExamType, ExamDate, MaxScore, CreatedAt)
        VALUES (source.ClassId, source.ExamName, source.ExamType, source.ExamDate, source.MaxScore, DATEADD(HOUR, 9, CAST(source.ExamDate AS DATETIME2)));

    ;WITH ExamData AS
    (
        SELECT
            ex.Id AS ExamId,
            src.ClassCode,
            src.ExamName,
            src.ExamDate,
            src.MaxScore
        FROM @ExamSource src
        INNER JOIN dbo.Classes c ON c.ClassCode = src.ClassCode AND c.IsDeleted = 0
        INNER JOIN dbo.Exams ex
            ON ex.ClassId = c.Id
           AND ex.ExamName = src.ExamName
           AND ex.ExamType = src.ExamType
           AND ex.ExamDate = src.ExamDate
    ),
    ExamResultSource AS
    (
        SELECT
            ed.ExamId,
            e.Id AS EnrollmentId,
            CAST(ROUND(ed.MaxScore * ((40 + ((TRY_CONVERT(INT, RIGHT(src.StudentCode, 3)) * 7 + DATEPART(DAY, ed.ExamDate)) % 56)) / 100.0), 2) AS DECIMAL(5,2)) AS Score,
            CASE
                WHEN ROUND(ed.MaxScore * ((40 + ((TRY_CONVERT(INT, RIGHT(src.StudentCode, 3)) * 7 + DATEPART(DAY, ed.ExamDate)) % 56)) / 100.0), 2) >= ed.MaxScore * 0.5 THEN N'Pass'
                ELSE N'Fail'
            END AS ResultStatus,
            CASE
                WHEN ROUND(ed.MaxScore * ((40 + ((TRY_CONVERT(INT, RIGHT(src.StudentCode, 3)) * 7 + DATEPART(DAY, ed.ExamDate)) % 56)) / 100.0), 2) >= ed.MaxScore * 0.5 THEN N'Kết quả ổn định'
                ELSE N'Cần củng cố thêm'
            END AS Note
        FROM @EnrollmentSource src
        INNER JOIN dbo.Students s ON s.StudentCode = src.StudentCode AND s.IsDeleted = 0
        INNER JOIN dbo.Classes c ON c.ClassCode = src.ClassCode AND c.IsDeleted = 0
        INNER JOIN dbo.Enrollments e ON e.StudentId = s.Id AND e.ClassId = c.Id AND e.IsDeleted = 0
        INNER JOIN ExamData ed ON ed.ClassCode = src.ClassCode
        WHERE src.Status <> N'Huy'
    )
    MERGE dbo.ExamResults AS target
    USING ExamResultSource AS source
        ON target.ExamId = source.ExamId AND target.EnrollmentId = source.EnrollmentId
    WHEN MATCHED THEN
        UPDATE SET
            Score = source.Score,
            ResultStatus = source.ResultStatus,
            Note = source.Note
    WHEN NOT MATCHED THEN
        INSERT (ExamId, EnrollmentId, Score, ResultStatus, Note, CreatedAt)
        VALUES (source.ExamId, source.EnrollmentId, source.Score, source.ResultStatus, source.Note, SYSDATETIME());

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;
    THROW;
END CATCH;
GO

SELECT N'Accounts' AS TableName, COUNT(*) AS TotalRows FROM dbo.Accounts WHERE IsDeleted = 0
UNION ALL
SELECT N'Students', COUNT(*) FROM dbo.Students WHERE IsDeleted = 0
UNION ALL
SELECT N'Teachers', COUNT(*) FROM dbo.Teachers WHERE IsDeleted = 0
UNION ALL
SELECT N'Courses', COUNT(*) FROM dbo.Courses WHERE IsDeleted = 0
UNION ALL
SELECT N'Classes', COUNT(*) FROM dbo.Classes WHERE IsDeleted = 0
UNION ALL
SELECT N'Enrollments', COUNT(*) FROM dbo.Enrollments WHERE IsDeleted = 0
UNION ALL
SELECT N'Receipts', COUNT(*) FROM dbo.Receipts
UNION ALL
SELECT N'ClassSessions', COUNT(*) FROM dbo.ClassSessions
UNION ALL
SELECT N'Attendances', COUNT(*) FROM dbo.Attendances
UNION ALL
SELECT N'Exams', COUNT(*) FROM dbo.Exams
UNION ALL
SELECT N'ExamResults', COUNT(*) FROM dbo.ExamResults;
GO

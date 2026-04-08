using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Quan_ly_trung_tam_ngoai_ngu.Data;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Ef;

public sealed class EfLanguageCenterManagementService : ILanguageCenterManagementService
{
    private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase) { "Admin", "Staff", "Teacher" };
    private static readonly HashSet<string> ValidGenders = new(StringComparer.OrdinalIgnoreCase) { "Nam", "Nữ", "Khác" };
    private static readonly HashSet<string> ValidEnrollmentStatuses = new(StringComparer.OrdinalIgnoreCase) { "DangHoc", "BaoLuu", "HoanThanh", "Huy" };
    private static readonly HashSet<string> ValidPaymentMethods = new(StringComparer.OrdinalIgnoreCase) { "Cash", "Transfer", "Card" };
    private static readonly HashSet<string> ValidAttendanceStatuses = new(StringComparer.OrdinalIgnoreCase) { "Present", "Absent", "Late" };
    private static readonly HashSet<string> ValidExamTypes = new(StringComparer.OrdinalIgnoreCase) { "Midterm", "Final", "Speaking", "Test" };
    private static readonly HashSet<string> ValidResultStatuses = new(StringComparer.OrdinalIgnoreCase) { "Pass", "Fail" };

    private readonly ApplicationDbContext _dbContext;
    private readonly IAccountPasswordService _passwordService;
    private readonly ILogger<EfLanguageCenterManagementService> _logger;

    public EfLanguageCenterManagementService(
        ApplicationDbContext dbContext,
        IAccountPasswordService passwordService,
        ILogger<EfLanguageCenterManagementService> logger)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _logger = logger;
    }

    public AccountInput? GetAccount(int id)
    {
        var account = _dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        return account is null
            ? null
            : new AccountInput
            {
                Username = account.Username,
                Password = account.PasswordHash,
                FullName = account.FullName,
                Email = account.Email ?? string.Empty,
                Phone = account.Phone ?? string.Empty,
                Role = account.Role,
                IsActive = account.IsActive
            };
    }

    public ManagementResult SaveAccount(int? id, AccountInput input)
    {
        try
        {
            var username = Required(input.Username, "Tên đăng nhập là bắt buộc.");
            var fullName = Required(input.FullName, "Họ và tên là bắt buộc.");
            var role = Required(input.Role, "Vai trò là bắt buộc.");
            var email = Optional(input.Email);
            var phone = Optional(input.Phone);
            var password = id.HasValue ? (input.Password?.Trim() ?? string.Empty) : Required(input.Password, "Mật khẩu là bắt buộc.");

            if (!ValidRoles.Contains(role))
            {
                return ManagementResult.Fail("Vai trò không hợp lệ.");
            }

            if (_dbContext.Accounts.Any(x => !x.IsDeleted && x.Username == username && (!id.HasValue || x.Id != id.Value)))
            {
                return ManagementResult.Fail("Tên đăng nhập đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(email) &&
                _dbContext.Accounts.Any(x => !x.IsDeleted && x.Email == email && (!id.HasValue || x.Id != id.Value)))
            {
                return ManagementResult.Fail("Email đã tồn tại.");
            }

            if (id.HasValue)
            {
                var account = _dbContext.Accounts.FirstOrDefault(x => x.Id == id.Value && !x.IsDeleted);
                if (account is null)
                {
                    return ManagementResult.Fail("Không tìm thấy tài khoản cần cập nhật.");
                }

                account.Username = username;
                account.FullName = fullName;
                account.Email = email;
                account.Phone = phone;
                account.Role = role;
                account.IsActive = input.IsActive;
                account.Status = input.IsActive ? (byte)1 : (byte)0;
                account.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrWhiteSpace(password))
                {
                    account.PasswordHash = _passwordService.HashPassword(account, password);
                }

                _dbContext.SaveChanges();
                return ManagementResult.Success("Cập nhật tài khoản thành công.");
            }

            var newAccount = new AccountEntity
            {
                Username = username,
                FullName = fullName,
                Email = email,
                Phone = phone,
                Role = role,
                IsActive = input.IsActive,
                Status = input.IsActive ? (byte)1 : (byte)0,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            newAccount.PasswordHash = _passwordService.HashPassword(newAccount, password);
            _dbContext.Accounts.Add(newAccount);

            _dbContext.SaveChanges();
            return ManagementResult.Success("Tạo tài khoản thành công.");
        }
        catch (InvalidOperationException ex)
        {
            return ManagementResult.Fail(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "save account");
        }
    }

    public ManagementResult DeleteAccount(int id) =>
        SoftDelete(_dbContext.Accounts.FirstOrDefault(x => x.Id == id && !x.IsDeleted), "delete account", "Xóa tài khoản thành công.", "Không tìm thấy tài khoản cần xóa.");

    public StudentInput? GetStudent(int id)
    {
        var student = _dbContext.Students.AsNoTracking().FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        return student is null
            ? null
            : new StudentInput
            {
                StudentCode = student.StudentCode,
                FullName = student.FullName,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender ?? string.Empty,
                Phone = student.Phone ?? string.Empty,
                Email = student.Email ?? string.Empty,
                Address = student.Address ?? string.Empty,
                IsActive = student.Status == 1
            };
    }

        public ManagementResult SaveStudent(int? id, StudentInput input)
    {
        try
        {
            var fullName = Required(input.FullName, "Tên học viên là bắt buộc.");
            var email = Optional(input.Email);
            var gender = Optional(input.Gender);

            if (!string.IsNullOrWhiteSpace(gender) && !ValidGenders.Contains(gender))
            {
                return ManagementResult.Fail("Giới tính không hợp lệ.");
            }

            if (!string.IsNullOrWhiteSpace(email) &&
                _dbContext.Students.Any(x => !x.IsDeleted && x.Email == email && (!id.HasValue || x.Id != id.Value)))
            {
                return ManagementResult.Fail("Email học viên đã tồn tại.");
            }

            if (id.HasValue)
            {
                var student = _dbContext.Students.FirstOrDefault(x => x.Id == id.Value && !x.IsDeleted);
                if (student is null)
                {
                    return ManagementResult.Fail("Không tìm thấy học viên cần cập nhật.");
                }

                student.FullName = fullName;
                student.DateOfBirth = input.DateOfBirth;
                student.Gender = gender;
                student.Phone = Optional(input.Phone);
                student.Email = email;
                student.Address = Optional(input.Address);
                student.Status = input.IsActive ? (byte)1 : (byte)0;
                student.UpdatedAt = DateTime.Now;

                _dbContext.SaveChanges();
                return ManagementResult.Success("Cập nhật học viên thành công.");
            }

            var generatedCode = GenerateSequentialCode(
                _dbContext.Students
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.StudentCode),
                "S");

            _dbContext.Students.Add(new StudentEntity
            {
                StudentCode = generatedCode,
                FullName = fullName,
                DateOfBirth = input.DateOfBirth,
                Gender = gender,
                Phone = Optional(input.Phone),
                Email = email,
                Address = Optional(input.Address),
                Status = input.IsActive ? (byte)1 : (byte)0,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            });

            _dbContext.SaveChanges();
            return ManagementResult.Success($"Tạo học viên thành công. Hệ thống đã tự sinh mã {generatedCode}.");
        }
        catch (InvalidOperationException ex)
        {
            return ManagementResult.Fail(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "save student");
        }
    }

    public ManagementResult DeleteStudent(int id) =>
        SoftDelete(_dbContext.Students.FirstOrDefault(x => x.Id == id && !x.IsDeleted), "delete student", "Xóa học viên thành công.", "Không tìm thấy học viên cần xóa.");

    public TeacherInput? GetTeacher(int id)
    {
        var teacher = _dbContext.Teachers.AsNoTracking().FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        return teacher is null
            ? null
            : new TeacherInput
            {
                TeacherCode = teacher.TeacherCode,
                FullName = teacher.FullName,
                Phone = teacher.Phone ?? string.Empty,
                Email = teacher.Email ?? string.Empty,
                Specialization = teacher.Specialization ?? string.Empty,
                IsActive = teacher.Status == 1
            };
    }

    public ManagementResult SaveTeacher(int? id, TeacherInput input)
    {
        try
        {
            var code = Required(input.TeacherCode, "Mã giáo viên là bắt buộc.");
            var fullName = Required(input.FullName, "Tên giáo viên là bắt buộc.");
            var email = Optional(input.Email);

            if (_dbContext.Teachers.Any(x => !x.IsDeleted && x.TeacherCode == code && (!id.HasValue || x.Id != id.Value)))
            {
                return ManagementResult.Fail("Mã giáo viên đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(email) &&
                _dbContext.Teachers.Any(x => !x.IsDeleted && x.Email == email && (!id.HasValue || x.Id != id.Value)))
            {
                return ManagementResult.Fail("Email giáo viên đã tồn tại.");
            }

            if (id.HasValue)
            {
                var teacher = _dbContext.Teachers.FirstOrDefault(x => x.Id == id.Value && !x.IsDeleted);
                if (teacher is null)
                {
                    return ManagementResult.Fail("Không tìm thấy giáo viên cần cập nhật.");
                }

                teacher.TeacherCode = code;
                teacher.FullName = fullName;
                teacher.Phone = Optional(input.Phone);
                teacher.Email = email;
                teacher.Specialization = Optional(input.Specialization);
                teacher.Status = input.IsActive ? (byte)1 : (byte)0;
                teacher.UpdatedAt = DateTime.Now;

                _dbContext.SaveChanges();
                return ManagementResult.Success("Cập nhật giáo viên thành công.");
            }

            _dbContext.Teachers.Add(new TeacherEntity
            {
                TeacherCode = code,
                FullName = fullName,
                Phone = Optional(input.Phone),
                Email = email,
                Specialization = Optional(input.Specialization),
                Status = input.IsActive ? (byte)1 : (byte)0,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            });

            _dbContext.SaveChanges();
            return ManagementResult.Success("Tạo giáo viên thành công.");
        }
        catch (InvalidOperationException ex)
        {
            return ManagementResult.Fail(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "save teacher");
        }
    }

    public ManagementResult DeleteTeacher(int id) =>
        SoftDelete(_dbContext.Teachers.FirstOrDefault(x => x.Id == id && !x.IsDeleted), "delete teacher", "Xóa giáo viên thành công.", "Không tìm thấy giáo viên cần xóa.");

    public CourseInput? GetCourse(int id)
    {
        var course = _dbContext.Courses.AsNoTracking().FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        return course is null
            ? null
            : new CourseInput
            {
                CourseCode = course.CourseCode,
                CourseName = course.CourseName,
                Description = course.Description ?? string.Empty,
                DurationHours = course.DurationHours,
                TuitionFee = course.TuitionFee,
                IsActive = course.Status == 1
            };
    }

    public ManagementResult SaveCourse(int? id, CourseInput input)
    {
        try
        {
            var code = Required(input.CourseCode, "Mã khóa học là bắt buộc.");
            var name = Required(input.CourseName, "Tên khóa học là bắt buộc.");

            if (input.DurationHours <= 0)
            {
                return ManagementResult.Fail("Thời lượng khóa học phải lớn hơn 0.");
            }

            if (input.TuitionFee < 0)
            {
                return ManagementResult.Fail("Học phí phải lớn hơn hoặc bằng 0.");
            }

            if (_dbContext.Courses.Any(x => !x.IsDeleted && x.CourseCode == code && (!id.HasValue || x.Id != id.Value)))
            {
                return ManagementResult.Fail("Mã khóa học đã tồn tại.");
            }

            if (id.HasValue)
            {
                var course = _dbContext.Courses.FirstOrDefault(x => x.Id == id.Value && !x.IsDeleted);
                if (course is null)
                {
                    return ManagementResult.Fail("Không tìm thấy khóa học cần cập nhật.");
                }

                course.CourseCode = code;
                course.CourseName = name;
                course.Description = Optional(input.Description);
                course.DurationHours = input.DurationHours;
                course.TuitionFee = input.TuitionFee;
                course.Status = input.IsActive ? (byte)1 : (byte)0;
                course.UpdatedAt = DateTime.Now;

                _dbContext.SaveChanges();
                return ManagementResult.Success("Cập nhật khóa học thành công.");
            }

            _dbContext.Courses.Add(new CourseEntity
            {
                CourseCode = code,
                CourseName = name,
                Description = Optional(input.Description),
                DurationHours = input.DurationHours,
                TuitionFee = input.TuitionFee,
                Status = input.IsActive ? (byte)1 : (byte)0,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            });

            _dbContext.SaveChanges();
            return ManagementResult.Success("Tạo khóa học thành công.");
        }
        catch (InvalidOperationException ex)
        {
            return ManagementResult.Fail(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "save course");
        }
    }

    public ManagementResult DeleteCourse(int id) =>
        SoftDelete(_dbContext.Courses.FirstOrDefault(x => x.Id == id && !x.IsDeleted), "delete course", "Xóa khóa học thành công.", "Không tìm thấy khóa học cần xóa.");

    public ClassInput? GetClass(int id)
    {
        var courseClass = _dbContext.Classes
            .AsNoTracking()
            .Include(x => x.Course)
            .Include(x => x.Teacher)
            .FirstOrDefault(x => x.Id == id && !x.IsDeleted);

        return courseClass is null
            ? null
            : new ClassInput
            {
                ClassCode = courseClass.ClassCode,
                ClassName = courseClass.ClassName,
                CourseCode = courseClass.Course.CourseCode,
                TeacherCode = courseClass.Teacher?.TeacherCode ?? string.Empty,
                StartDate = courseClass.StartDate,
                EndDate = courseClass.EndDate,
                ScheduleText = courseClass.ScheduleText ?? string.Empty,
                Capacity = courseClass.Capacity,
                IsActive = courseClass.Status == 1
            };
    }

    public ManagementResult SaveClass(int? id, ClassInput input)
    {
        try
        {
            var code = Required(input.ClassCode, "Mã lớp là bắt buộc.");
            var className = Required(input.ClassName, "Tên lớp là bắt buộc.");
            var courseCode = Required(input.CourseCode, "Khóa học là bắt buộc.");
            var teacherCode = Optional(input.TeacherCode);

            if (input.Capacity <= 0)
            {
                return ManagementResult.Fail("Sĩ số tối đa phải lớn hơn 0.");
            }

            if (input.EndDate.Date < input.StartDate.Date)
            {
                return ManagementResult.Fail("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");
            }

            var course = _dbContext.Courses.FirstOrDefault(x => !x.IsDeleted && x.CourseCode == courseCode);
            if (course is null)
            {
                return ManagementResult.Fail("Không tìm thấy khóa học đã chọn.");
            }

            TeacherEntity? teacher = null;
            if (!string.IsNullOrWhiteSpace(teacherCode))
            {
                teacher = _dbContext.Teachers.FirstOrDefault(x => !x.IsDeleted && x.TeacherCode == teacherCode);
                if (teacher is null)
                {
                    return ManagementResult.Fail("Không tìm thấy giáo viên đã chọn.");
                }
            }

            if (_dbContext.Classes.Any(x => !x.IsDeleted && x.ClassCode == code && (!id.HasValue || x.Id != id.Value)))
            {
                return ManagementResult.Fail("Mã lớp đã tồn tại.");
            }

            if (id.HasValue)
            {
                var entity = _dbContext.Classes.FirstOrDefault(x => x.Id == id.Value && !x.IsDeleted);
                if (entity is null)
                {
                    return ManagementResult.Fail("Không tìm thấy lớp học cần cập nhật.");
                }

                entity.ClassCode = code;
                entity.ClassName = className;
                entity.CourseId = course.Id;
                entity.TeacherId = teacher?.Id;
                entity.StartDate = input.StartDate.Date;
                entity.EndDate = input.EndDate.Date;
                entity.ScheduleText = Optional(input.ScheduleText);
                entity.Capacity = input.Capacity;
                entity.Status = input.IsActive ? (byte)1 : (byte)0;
                entity.UpdatedAt = DateTime.Now;

                _dbContext.SaveChanges();
                return ManagementResult.Success("Cập nhật lớp học thành công.");
            }

            _dbContext.Classes.Add(new ClassEntity
            {
                ClassCode = code,
                ClassName = className,
                CourseId = course.Id,
                TeacherId = teacher?.Id,
                StartDate = input.StartDate.Date,
                EndDate = input.EndDate.Date,
                ScheduleText = Optional(input.ScheduleText),
                Capacity = input.Capacity,
                Status = input.IsActive ? (byte)1 : (byte)0,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            });

            _dbContext.SaveChanges();
            return ManagementResult.Success("Tạo lớp học thành công.");
        }
        catch (InvalidOperationException ex)
        {
            return ManagementResult.Fail(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "save class");
        }
    }

    public ManagementResult DeleteClass(int id) =>
        SoftDelete(_dbContext.Classes.FirstOrDefault(x => x.Id == id && !x.IsDeleted), "delete class", "Xóa lớp học thành công.", "Không tìm thấy lớp học cần xóa.");

    public EnrollmentInput? GetEnrollment(int id)
    {
        var enrollment = _dbContext.Enrollments
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Class)
            .FirstOrDefault(x => x.Id == id && !x.IsDeleted);

        return enrollment is null
            ? null
            : new EnrollmentInput
            {
                StudentCode = enrollment.Student.StudentCode,
                ClassCode = enrollment.Class.ClassCode,
                EnrollDate = enrollment.EnrollDate,
                Status = enrollment.Status,
                TotalFee = enrollment.TotalFee,
                DiscountAmount = enrollment.DiscountAmount,
                Note = enrollment.Note ?? string.Empty
            };
    }

        public ManagementResult SaveEnrollment(int? id, EnrollmentInput input)
    {
        try
        {
            var studentCode = Required(input.StudentCode, "Học viên là bắt buộc.");
            var classCode = Required(input.ClassCode, "Lớp học là bắt buộc.");
            var status = Required(input.Status, "Trạng thái là bắt buộc.");

            if (!ValidEnrollmentStatuses.Contains(status))
            {
                return ManagementResult.Fail("Trạng thái ghi danh không hợp lệ.");
            }

            var student = _dbContext.Students.FirstOrDefault(x => !x.IsDeleted && x.StudentCode == studentCode);
            var courseClass = _dbContext.Classes
                .Include(x => x.Course)
                .FirstOrDefault(x => !x.IsDeleted && x.ClassCode == classCode);

            if (student is null || courseClass is null)
            {
                return ManagementResult.Fail("Không tìm thấy học viên hoặc lớp học đã chọn.");
            }

            string? convertedCode = null;
            if (EfServiceMapper.IsConsultationLead(student.StudentCode))
            {
                convertedCode = GenerateSequentialCode(
                    _dbContext.Students
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted && x.Id != student.Id)
                        .Select(x => x.StudentCode),
                    "S");

                student.StudentCode = convertedCode;
                student.UpdatedAt = DateTime.Now;
            }

            var totalFee = input.TotalFee > 0 ? input.TotalFee : courseClass.Course.TuitionFee;
            if (totalFee < 0)
            {
                return ManagementResult.Fail("Tổng học phí phải lớn hơn hoặc bằng 0.");
            }

            if (input.DiscountAmount < 0 || input.DiscountAmount > totalFee)
            {
                return ManagementResult.Fail("Số tiền giảm giá không hợp lệ.");
            }

            if (_dbContext.Enrollments.Any(x => !x.IsDeleted && x.StudentId == student.Id && x.ClassId == courseClass.Id && (!id.HasValue || x.Id != id.Value)))
            {
                return ManagementResult.Fail("Học viên này đã được ghi danh vào lớp đã chọn.");
            }

            if (id.HasValue)
            {
                var entity = _dbContext.Enrollments.FirstOrDefault(x => x.Id == id.Value && !x.IsDeleted);
                if (entity is null)
                {
                    return ManagementResult.Fail("Không tìm thấy ghi danh cần cập nhật.");
                }

                entity.StudentId = student.Id;
                entity.ClassId = courseClass.Id;
                entity.EnrollDate = input.EnrollDate.Date;
                entity.Status = status;
                entity.TotalFee = totalFee;
                entity.DiscountAmount = input.DiscountAmount;
                entity.Note = Optional(input.Note);
                entity.UpdatedAt = DateTime.Now;

                _dbContext.SaveChanges();
                return convertedCode is null
                    ? ManagementResult.Success("Cập nhật ghi danh thành công.")
                    : ManagementResult.Success($"Cập nhật ghi danh thành công. Hồ sơ tư vấn đã được chuyển thành mã học viên {convertedCode}.");
            }

            _dbContext.Enrollments.Add(new EnrollmentEntity
            {
                StudentId = student.Id,
                ClassId = courseClass.Id,
                EnrollDate = input.EnrollDate.Date,
                Status = status,
                TotalFee = totalFee,
                DiscountAmount = input.DiscountAmount,
                Note = Optional(input.Note),
                IsDeleted = false,
                CreatedAt = DateTime.Now
            });

            _dbContext.SaveChanges();
            return convertedCode is null
                ? ManagementResult.Success("Tạo ghi danh thành công.")
                : ManagementResult.Success($"Tạo ghi danh thành công. Hồ sơ tư vấn đã được chuyển thành mã học viên {convertedCode}.");
        }
        catch (InvalidOperationException ex)
        {
            return ManagementResult.Fail(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "save enrollment");
        }
    }

    public ManagementResult DeleteEnrollment(int id) =>
        SoftDelete(_dbContext.Enrollments.FirstOrDefault(x => x.Id == id && !x.IsDeleted), "delete enrollment", "Xóa ghi danh thành công.", "Không tìm thấy ghi danh cần xóa.");

    public ReceiptInput? GetReceipt(int id)
    {
        var receipt = _dbContext.Receipts.AsNoTracking().FirstOrDefault(x => x.Id == id);
        return receipt is null
            ? null
            : new ReceiptInput
            {
                EnrollmentId = receipt.EnrollmentId,
                PaymentDate = receipt.PaymentDate,
                Amount = receipt.Amount,
                PaymentMethod = receipt.PaymentMethod,
                Note = receipt.Note ?? string.Empty
            };
    }

    public ManagementResult SaveReceipt(int? id, ReceiptInput input)
    {
        try
        {
            if (input.EnrollmentId <= 0)
            {
                return ManagementResult.Fail("Ghi danh là bắt buộc.");
            }

            if (input.Amount <= 0)
            {
                return ManagementResult.Fail("Số tiền phải lớn hơn 0.");
            }

            var paymentMethod = Required(input.PaymentMethod, "Phương thức thanh toán là bắt buộc.");
            if (!ValidPaymentMethods.Contains(paymentMethod))
            {
                return ManagementResult.Fail("Phương thức thanh toán không hợp lệ.");
            }

            if (!_dbContext.Enrollments.Any(x => x.Id == input.EnrollmentId && !x.IsDeleted))
            {
                return ManagementResult.Fail("Không tìm thấy ghi danh đã chọn.");
            }

            if (id.HasValue)
            {
                var receipt = _dbContext.Receipts.FirstOrDefault(x => x.Id == id.Value);
                if (receipt is null)
                {
                    return ManagementResult.Fail("Không tìm thấy phiếu thu cần cập nhật.");
                }

                receipt.EnrollmentId = input.EnrollmentId;
                receipt.PaymentDate = input.PaymentDate;
                receipt.Amount = input.Amount;
                receipt.PaymentMethod = paymentMethod;
                receipt.Note = Optional(input.Note);

                _dbContext.SaveChanges();
                return ManagementResult.Success("Cập nhật biên nhận thành công.");
            }

            _dbContext.Receipts.Add(new ReceiptEntity
            {
                ReceiptCode = GenerateSequentialCode(_dbContext.Receipts.Select(x => x.ReceiptCode).ToList(), "R"),
                EnrollmentId = input.EnrollmentId,
                PaymentDate = input.PaymentDate,
                Amount = input.Amount,
                PaymentMethod = paymentMethod,
                Note = Optional(input.Note),
                CreatedAt = DateTime.Now
            });

            _dbContext.SaveChanges();
            return ManagementResult.Success("Tạo biên nhận thành công.");
        }
        catch (InvalidOperationException ex)
        {
            return ManagementResult.Fail(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "save receipt");
        }
    }

    public ManagementResult DeleteReceipt(int id)
    {
        try
        {
            var receipt = _dbContext.Receipts.FirstOrDefault(x => x.Id == id);
            if (receipt is null)
            {
                return ManagementResult.Fail("Không tìm thấy phiếu thu cần xóa.");
            }

            _dbContext.Receipts.Remove(receipt);
            _dbContext.SaveChanges();
            return ManagementResult.Success("Xóa biên nhận thành công.");
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "delete receipt");
        }
    }

    public SessionInput? GetSession(int id)
    {
        var session = _dbContext.ClassSessions
            .AsNoTracking()
            .Include(x => x.Class)
            .FirstOrDefault(x => x.Id == id);

        return session is null
            ? null
            : new SessionInput
            {
                ClassCode = session.Class.ClassCode,
                SessionDate = session.SessionDate,
                Topic = session.Topic ?? string.Empty,
                Note = session.Note ?? string.Empty
            };
    }

    public ManagementResult SaveSession(int? id, SessionInput input)
    {
        try
        {
            var classCode = Required(input.ClassCode, "Lớp học là bắt buộc.");
            var topic = Required(input.Topic, "Chủ đề buổi học là bắt buộc.");
            var courseClass = _dbContext.Classes.FirstOrDefault(x => !x.IsDeleted && x.ClassCode == classCode);

            if (courseClass is null)
            {
                return ManagementResult.Fail("Không tìm thấy lớp học đã chọn.");
            }

            if (id.HasValue)
            {
                var session = _dbContext.ClassSessions.FirstOrDefault(x => x.Id == id.Value);
                if (session is null)
                {
                    return ManagementResult.Fail("Không tìm thấy buổi học cần cập nhật.");
                }

                session.ClassId = courseClass.Id;
                session.SessionDate = input.SessionDate.Date;
                session.Topic = topic;
                session.Note = Optional(input.Note);

                _dbContext.SaveChanges();
                return ManagementResult.Success("Cập nhật buổi học thành công.");
            }

            _dbContext.ClassSessions.Add(new ClassSessionEntity
            {
                ClassId = courseClass.Id,
                SessionDate = input.SessionDate.Date,
                Topic = topic,
                Note = Optional(input.Note),
                CreatedAt = DateTime.Now
            });

            _dbContext.SaveChanges();
            return ManagementResult.Success("Tạo buổi học thành công.");
        }
        catch (InvalidOperationException ex)
        {
            return ManagementResult.Fail(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "save session");
        }
    }

    public ManagementResult DeleteSession(int id)
    {
        try
        {
            var session = _dbContext.ClassSessions.FirstOrDefault(x => x.Id == id);
            if (session is null)
            {
                return ManagementResult.Fail("Không tìm thấy buổi học cần xóa.");
            }

            _dbContext.ClassSessions.Remove(session);
            _dbContext.SaveChanges();
            return ManagementResult.Success("Xóa buổi học thành công.");
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "delete session");
        }
    }

    public AttendanceInput? GetAttendance(int id)
    {
        var attendance = _dbContext.Attendances.AsNoTracking().FirstOrDefault(x => x.Id == id);
        return attendance is null
            ? null
            : new AttendanceInput
            {
                EnrollmentId = attendance.EnrollmentId,
                ClassSessionId = attendance.ClassSessionId,
                AttendanceStatus = attendance.AttendanceStatus,
                Note = attendance.Note ?? string.Empty
            };
    }

    public ManagementResult SaveAttendance(int? id, AttendanceInput input)
    {
        try
        {
            if (input.EnrollmentId <= 0 || input.ClassSessionId <= 0)
            {
                return ManagementResult.Fail("Ghi danh và buổi học là bắt buộc.");
            }

            var attendanceStatus = Required(input.AttendanceStatus, "Trạng thái điểm danh là bắt buộc.");
            if (!ValidAttendanceStatuses.Contains(attendanceStatus))
            {
                return ManagementResult.Fail("Trạng thái điểm danh không hợp lệ.");
            }

            if (!AttendanceBelongsToSessionClass(input.EnrollmentId, input.ClassSessionId))
            {
                return ManagementResult.Fail("Học viên không thuộc lớp của buổi học đã chọn. Vui lòng chọn ghi danh và buổi học cùng lớp.");
            }

            if (id.HasValue)
            {
                var attendance = _dbContext.Attendances.FirstOrDefault(x => x.Id == id.Value);
                if (attendance is null)
                {
                    return ManagementResult.Fail("Không tìm thấy điểm danh cần cập nhật.");
                }

                attendance.EnrollmentId = input.EnrollmentId;
                attendance.ClassSessionId = input.ClassSessionId;
                attendance.AttendanceStatus = attendanceStatus;
                attendance.Note = Optional(input.Note);

                _dbContext.SaveChanges();
                return ManagementResult.Success("Cập nhật điểm danh thành công.");
            }

            var existing = _dbContext.Attendances.FirstOrDefault(x => x.EnrollmentId == input.EnrollmentId && x.ClassSessionId == input.ClassSessionId);
            if (existing is not null)
            {
                existing.AttendanceStatus = attendanceStatus;
                existing.Note = Optional(input.Note);
                _dbContext.SaveChanges();
                return ManagementResult.Success("Cập nhật điểm danh thành công.");
            }

            _dbContext.Attendances.Add(new AttendanceEntity
            {
                EnrollmentId = input.EnrollmentId,
                ClassSessionId = input.ClassSessionId,
                AttendanceStatus = attendanceStatus,
                Note = Optional(input.Note),
                CreatedAt = DateTime.Now
            });

            _dbContext.SaveChanges();
            return ManagementResult.Success("Tạo điểm danh thành công.");
        }
        catch (InvalidOperationException ex)
        {
            return ManagementResult.Fail(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "save attendance");
        }
    }

    public ManagementResult DeleteAttendance(int id)
    {
        try
        {
            var attendance = _dbContext.Attendances.FirstOrDefault(x => x.Id == id);
            if (attendance is null)
            {
                return ManagementResult.Fail("Không tìm thấy điểm danh cần xóa.");
            }

            _dbContext.Attendances.Remove(attendance);
            _dbContext.SaveChanges();
            return ManagementResult.Success("Xóa điểm danh thành công.");
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "delete attendance");
        }
    }

    public ExamInput? GetExam(int id)
    {
        var exam = _dbContext.Exams
            .AsNoTracking()
            .Include(x => x.Class)
            .FirstOrDefault(x => x.Id == id);

        return exam is null
            ? null
            : new ExamInput
            {
                ClassCode = exam.Class.ClassCode,
                ExamName = exam.ExamName,
                ExamType = exam.ExamType,
                ExamDate = exam.ExamDate,
                MaxScore = exam.MaxScore
            };
    }

    public ManagementResult SaveExam(int? id, ExamInput input)
    {
        try
        {
            var classCode = Required(input.ClassCode, "Lớp học là bắt buộc.");
            var examName = Required(input.ExamName, "Tên bài kiểm tra là bắt buộc.");
            var examType = Required(input.ExamType, "Loại bài kiểm tra là bắt buộc.");

            if (!ValidExamTypes.Contains(examType))
            {
                return ManagementResult.Fail("Loại bài kiểm tra không hợp lệ.");
            }

            if (input.MaxScore <= 0)
            {
                return ManagementResult.Fail("Điểm tối đa phải lớn hơn 0.");
            }

            var classEntity = _dbContext.Classes.FirstOrDefault(x => !x.IsDeleted && x.ClassCode == classCode);
            if (classEntity is null)
            {
                return ManagementResult.Fail("Không tìm thấy lớp học đã chọn.");
            }

            var duplicateExists = _dbContext.Exams.Any(x =>
                x.ClassId == classEntity.Id &&
                x.ExamName == examName &&
                x.ExamType == examType &&
                x.ExamDate == input.ExamDate.Date &&
                (!id.HasValue || x.Id != id.Value));

            if (duplicateExists)
            {
                return ManagementResult.Fail("Bài kiểm tra này đã tồn tại cho lớp học đã chọn.");
            }

            if (id.HasValue)
            {
                var exam = _dbContext.Exams.FirstOrDefault(x => x.Id == id.Value);
                if (exam is null)
                {
                    return ManagementResult.Fail("Không tìm thấy bài kiểm tra cần cập nhật.");
                }

                exam.ClassId = classEntity.Id;
                exam.ExamName = examName;
                exam.ExamType = examType;
                exam.ExamDate = input.ExamDate.Date;
                exam.MaxScore = input.MaxScore;

                _dbContext.SaveChanges();
                return ManagementResult.Success("Cập nhật bài kiểm tra thành công.");
            }

            _dbContext.Exams.Add(new ExamEntity
            {
                ClassId = classEntity.Id,
                ExamName = examName,
                ExamType = examType,
                ExamDate = input.ExamDate.Date,
                MaxScore = input.MaxScore,
                CreatedAt = DateTime.Now
            });

            _dbContext.SaveChanges();
            return ManagementResult.Success("Tạo bài kiểm tra thành công.");
        }
        catch (InvalidOperationException ex)
        {
            return ManagementResult.Fail(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "save exam");
        }
    }

    public ManagementResult DeleteExam(int id)
    {
        try
        {
            var exam = _dbContext.Exams
                .Include(x => x.Results)
                .FirstOrDefault(x => x.Id == id);

            if (exam is null)
            {
                return ManagementResult.Fail("Không tìm thấy bài kiểm tra cần xóa.");
            }

            if (exam.Results.Count > 0)
            {
                return ManagementResult.Fail("Không thể xóa bài kiểm tra đã có kết quả. Hãy xóa các dòng điểm trước.");
            }

            _dbContext.Exams.Remove(exam);
            _dbContext.SaveChanges();
            return ManagementResult.Success("Xóa bài kiểm tra thành công.");
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "delete exam");
        }
    }

    public ExamResultInput? GetExamResult(int id)
    {
        var result = _dbContext.ExamResults
            .AsNoTracking()
            .Include(x => x.Exam)
            .FirstOrDefault(x => x.Id == id);

        return result is null
            ? null
            : new ExamResultInput
            {
                EnrollmentId = result.EnrollmentId,
                ExamName = result.Exam.ExamName,
                ExamType = result.Exam.ExamType,
                ExamDate = result.Exam.ExamDate,
                MaxScore = result.Exam.MaxScore,
                Score = result.Score,
                ResultStatus = result.ResultStatus ?? string.Empty,
                Note = result.Note ?? string.Empty
            };
    }

    public ManagementResult SaveExamResult(int? id, ExamResultInput input)
    {
        try
        {
            if (input.EnrollmentId <= 0)
            {
                return ManagementResult.Fail("Ghi danh là bắt buộc.");
            }

            if (input.MaxScore <= 0)
            {
                return ManagementResult.Fail("Điểm tối đa phải lớn hơn 0.");
            }

            if (input.Score < 0)
            {
                return ManagementResult.Fail("Điểm số phải lớn hơn hoặc bằng 0.");
            }

            var examName = Required(input.ExamName, "Tên bài kiểm tra là bắt buộc.");
            var examType = Required(input.ExamType, "Loại bài kiểm tra là bắt buộc.");
            if (!ValidExamTypes.Contains(examType))
            {
                return ManagementResult.Fail("Loại bài kiểm tra không hợp lệ.");
            }

            if (!string.IsNullOrWhiteSpace(input.ResultStatus) && !ValidResultStatuses.Contains(input.ResultStatus))
            {
                return ManagementResult.Fail("Kết quả học tập không hợp lệ.");
            }

            var enrollment = _dbContext.Enrollments.FirstOrDefault(x => x.Id == input.EnrollmentId && !x.IsDeleted);
            if (enrollment is null)
            {
                return ManagementResult.Fail("Không tìm thấy ghi danh đã chọn.");
            }

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var operationResult = ManagementResult.Fail("Không thể lưu điểm số.");

            strategy.Execute(() =>
            {
                using var transaction = _dbContext.Database.BeginTransaction();

                var exam = _dbContext.Exams.FirstOrDefault(x =>
                    x.ClassId == enrollment.ClassId &&
                    x.ExamName == examName &&
                    x.ExamType == examType &&
                    x.ExamDate == input.ExamDate.Date);

                if (exam is null)
                {
                    exam = new ExamEntity
                    {
                        ClassId = enrollment.ClassId,
                        ExamName = examName,
                        ExamType = examType,
                        ExamDate = input.ExamDate.Date,
                        MaxScore = input.MaxScore,
                        CreatedAt = DateTime.Now
                    };
                    _dbContext.Exams.Add(exam);
                    _dbContext.SaveChanges();
                }
                else
                {
                    exam.ExamName = examName;
                    exam.ExamType = examType;
                    exam.ExamDate = input.ExamDate.Date;
                    exam.MaxScore = input.MaxScore;
                    _dbContext.SaveChanges();
                }

                var resultStatus = !string.IsNullOrWhiteSpace(input.ResultStatus)
                    ? input.ResultStatus.Trim()
                    : input.Score >= input.MaxScore * 0.5m ? "Pass" : "Fail";

                if (id.HasValue)
                {
                    var current = _dbContext.ExamResults.FirstOrDefault(x => x.Id == id.Value);
                    if (current is null)
                    {
                        operationResult = ManagementResult.Fail("Không tìm thấy điểm số cần cập nhật.");
                        transaction.Rollback();
                        return;
                    }

                    current.ExamId = exam.Id;
                    current.EnrollmentId = input.EnrollmentId;
                    current.Score = input.Score;
                    current.ResultStatus = Optional(resultStatus);
                    current.Note = Optional(input.Note);
                    _dbContext.SaveChanges();
                    transaction.Commit();
                    operationResult = ManagementResult.Success("Cập nhật điểm số thành công.");
                    return;
                }

                var existing = _dbContext.ExamResults.FirstOrDefault(x => x.ExamId == exam.Id && x.EnrollmentId == input.EnrollmentId);
                if (existing is not null)
                {
                    existing.Score = input.Score;
                    existing.ResultStatus = Optional(resultStatus);
                    existing.Note = Optional(input.Note);
                    _dbContext.SaveChanges();
                    transaction.Commit();
                    operationResult = ManagementResult.Success("Cập nhật điểm số thành công.");
                    return;
                }

                _dbContext.ExamResults.Add(new ExamResultEntity
                {
                    ExamId = exam.Id,
                    EnrollmentId = input.EnrollmentId,
                    Score = input.Score,
                    ResultStatus = Optional(resultStatus),
                    Note = Optional(input.Note),
                    CreatedAt = DateTime.Now
                });

                _dbContext.SaveChanges();
                transaction.Commit();
                operationResult = ManagementResult.Success("Tạo điểm số thành công.");
            });

            return operationResult;
        }
        catch (InvalidOperationException ex)
        {
            return ManagementResult.Fail(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "save exam result");
        }
    }

    public ManagementResult DeleteExamResult(int id)
    {
        try
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var operationResult = ManagementResult.Fail("Không thể xóa điểm số.");

            strategy.Execute(() =>
            {
                using var transaction = _dbContext.Database.BeginTransaction();

                var result = _dbContext.ExamResults.FirstOrDefault(x => x.Id == id);
                if (result is null)
                {
                    operationResult = ManagementResult.Fail("Không tìm thấy điểm số cần xóa.");
                    transaction.Rollback();
                    return;
                }

                var examId = result.ExamId;
                _dbContext.ExamResults.Remove(result);
                _dbContext.SaveChanges();

                if (!_dbContext.ExamResults.Any(x => x.ExamId == examId))
                {
                    var exam = _dbContext.Exams.FirstOrDefault(x => x.Id == examId);
                    if (exam is not null)
                    {
                        _dbContext.Exams.Remove(exam);
                        _dbContext.SaveChanges();
                    }
                }

                transaction.Commit();
                operationResult = ManagementResult.Success("Xóa điểm số thành công.");
            });

            return operationResult;
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, "delete exam result");
        }
    }

    private ManagementResult SoftDelete(object? entity, string operation, string successMessage, string missingMessage)
    {
        if (entity is null)
        {
            return ManagementResult.Fail(missingMessage);
        }

        try
        {
            switch (entity)
            {
                case AccountEntity account:
                    account.IsDeleted = true;
                    account.UpdatedAt = DateTime.Now;
                    break;
                case StudentEntity student:
                    student.IsDeleted = true;
                    student.UpdatedAt = DateTime.Now;
                    break;
                case TeacherEntity teacher:
                    teacher.IsDeleted = true;
                    teacher.UpdatedAt = DateTime.Now;
                    break;
                case CourseEntity course:
                    course.IsDeleted = true;
                    course.UpdatedAt = DateTime.Now;
                    break;
                case ClassEntity courseClass:
                    courseClass.IsDeleted = true;
                    courseClass.UpdatedAt = DateTime.Now;
                    break;
                case EnrollmentEntity enrollment:
                    enrollment.IsDeleted = true;
                    enrollment.UpdatedAt = DateTime.Now;
                    break;
                default:
                    throw new InvalidOperationException("Kiểu dữ liệu không hỗ trợ soft delete.");
            }

            _dbContext.SaveChanges();
            return ManagementResult.Success(successMessage);
        }
        catch (InvalidOperationException ex)
        {
            return ManagementResult.Fail(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            return HandleUpdateException(ex, operation);
        }
    }

    private bool AttendanceBelongsToSessionClass(int enrollmentId, int classSessionId)
    {
        return _dbContext.Enrollments
            .Where(x => x.Id == enrollmentId && !x.IsDeleted)
            .Join(
                _dbContext.ClassSessions,
                enrollment => enrollment.ClassId,
                session => session.ClassId,
                (enrollment, session) => new { EnrollmentId = enrollment.Id, SessionId = session.Id })
            .Any(x => x.EnrollmentId == enrollmentId && x.SessionId == classSessionId);
    }

    private ManagementResult HandleUpdateException(DbUpdateException ex, string operation)
    {
        _logger.LogError(ex, "Could not complete management operation {Operation}.", operation);

        if (ex.InnerException is SqlException sqlEx)
        {
            if (sqlEx.Number is 2601 or 2627)
            {
                return ManagementResult.Fail("Dữ liệu bị trùng với bản ghi đã có trong hệ thống.");
            }

            if (sqlEx.Number == 547)
            {
                return ManagementResult.Fail("Không thể thực hiện thao tác vì dữ liệu đang được liên kết ở phân hệ khác.");
            }

            if (sqlEx.Number == 515)
            {
                return ManagementResult.Fail("Thiếu dữ liệu bắt buộc để lưu xuống cơ sở dữ liệu.");
            }
        }

        return ManagementResult.Fail("Không thể lưu dữ liệu xuống SQL Server. Vui lòng kiểm tra lại thông tin nhập.");
    }

    private static string Required(string? value, string message)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(message);
        }

        return normalized;
    }

    private static string? Optional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string GenerateSequentialCode(IEnumerable<string?> codes, string prefix)
    {
        var nextNumber = codes
            .Where(code => !string.IsNullOrWhiteSpace(code) && code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(code => code![prefix.Length..])
            .Select(suffix => int.TryParse(suffix, out var number) ? number : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"{prefix}{nextNumber:000}";
    }
}


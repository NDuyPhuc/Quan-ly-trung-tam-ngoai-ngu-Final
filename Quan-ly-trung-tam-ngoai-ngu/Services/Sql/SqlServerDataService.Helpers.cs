using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Sql;

public partial class SqlServerDataService
{
    private static string MapAccountStatus(bool isActive, byte status)
    {
        return isActive && status == 1 ? "Đang hoạt động" : "Tạm khóa";
    }

    private static string MapStudentStatus(string enrollmentStatus, DateTime? classStartDate, DateTime? classEndDate, byte studentStatus)
    {
        if (!string.IsNullOrWhiteSpace(enrollmentStatus))
        {
            return enrollmentStatus switch
            {
                "BaoLuu" => "Bảo lưu",
                "HoanThanh" => "Hoàn thành",
                "Huy" => "Đã hủy",
                "DangHoc" when classStartDate.HasValue && classStartDate.Value.Date > DateTime.Today => "Đã xếp lớp",
                "DangHoc" when classEndDate.HasValue && classEndDate.Value.Date < DateTime.Today => "Hoàn thành",
                "DangHoc" => "Đang học",
                _ => "Đang học"
            };
        }

        return studentStatus == 1 ? "Đang học" : "Tạm khóa";
    }

    private static string MapCourseStatus(byte status, DateTime? nextStartDate)
    {
        if (status != 1)
        {
            return "Tạm dừng";
        }

        if (!nextStartDate.HasValue)
        {
            return "Đang tuyển sinh";
        }

        var diffDays = (nextStartDate.Value.Date - DateTime.Today).Days;
        if (diffDays is >= 0 and <= 7)
        {
            return "Khai giảng sớm";
        }

        if (diffDays > 7)
        {
            return "Đang tuyển sinh";
        }

        return "Đang hoạt động";
    }

    private static string MapClassStatus(byte status, DateTime startDate, DateTime endDate, int enrolled, int capacity)
    {
        if (status != 1)
        {
            return "Tạm dừng";
        }

        if (enrolled >= capacity && capacity > 0)
        {
            return "Đã đủ chỗ";
        }

        if (DateTime.Today < startDate.Date)
        {
            return (startDate.Date - DateTime.Today).Days <= 10 ? "Sắp khai giảng" : "Mở đăng ký";
        }

        if (DateTime.Today <= endDate.Date)
        {
            return "Đang hoạt động";
        }

        return "Hoàn thành";
    }

    private static string MapEnrollmentStatus(string status, DateTime? classStartDate)
    {
        return status switch
        {
            "DangHoc" when classStartDate.HasValue && classStartDate.Value.Date > DateTime.Today => "Đã xếp lớp",
            "DangHoc" => "Đang học",
            "BaoLuu" => "Bảo lưu",
            "HoanThanh" => "Hoàn thành",
            "Huy" => "Đã hủy",
            _ => "Chờ xác nhận"
        };
    }

    private static string MapPaymentStatus(decimal totalFee, decimal paidAmount)
    {
        if (totalFee <= 0 || paidAmount >= totalFee)
        {
            return "Đã thanh toán";
        }

        if (paidAmount > 0)
        {
            return "Đóng một phần";
        }

        return "Còn nợ";
    }

    private static string MapPaymentMethod(string paymentMethod)
    {
        return paymentMethod switch
        {
            "Cash" => "Tiền mặt",
            "Transfer" => "Chuyển khoản",
            "Card" => "Thẻ",
            _ => paymentMethod
        };
    }

    private static DateTime CalculateDebtDueDate(DateTime enrollDate, DateTime? classStartDate)
    {
        if (classStartDate.HasValue && classStartDate.Value.Date > enrollDate.Date)
        {
            return classStartDate.Value.Date;
        }

        return enrollDate.Date.AddDays(14);
    }

    private static string MapDebtStatus(DateTime dueDate)
    {
        var diffDays = (dueDate.Date - DateTime.Today).Days;
        if (diffDays < 0)
        {
            return "Quá hạn";
        }

        if (diffDays <= 7)
        {
            return "Sắp đến hạn";
        }

        return "Đang theo dõi";
    }

    private static string MapSessionStatus(DateTime sessionDate)
    {
        if (sessionDate.Date == DateTime.Today)
        {
            return "Hôm nay";
        }

        return sessionDate.Date > DateTime.Today ? "Sắp diễn ra" : "Đã diễn ra";
    }

    private static string MapAttendanceStatus(string status)
    {
        return status switch
        {
            "Present" => "Có mặt",
            "Absent" => "Vắng",
            "Late" => "Muộn",
            _ => status
        };
    }

    private static string BuildExamLabel(string examType, string examName)
    {
        if (!string.IsNullOrWhiteSpace(examName))
        {
            return examName;
        }

        return examType switch
        {
            "Midterm" => "Giữa kỳ",
            "Final" => "Cuối kỳ",
            "Speaking" => "Đánh giá nói",
            "Test" => "Kiểm tra",
            _ => examType
        };
    }

    private static string MapExamResult(string resultStatus, decimal score, decimal averageScore)
    {
        return resultStatus switch
        {
            "Pass" => "Đạt",
            "Fail" => "Cần cải thiện",
            _ when score >= averageScore => "Đạt",
            _ => "Cần cải thiện"
        };
    }

    private static string InferCourseLevel(string courseName)
    {
        var normalized = RemoveDiacritics(courseName).ToLowerInvariant();

        if (normalized.Contains("foundation") || normalized.Contains("basic"))
        {
            return "Cơ bản";
        }

        if (normalized.Contains("ielts") && (normalized.Contains("6.5") || normalized.Contains("advanced") || normalized.Contains("intensive")))
        {
            return "Nâng cao";
        }

        if (normalized.Contains("toeic") || normalized.Contains("ielts"))
        {
            return "Trung cấp";
        }

        return "Tổng hợp";
    }

    private static string BuildShortDescription(string description, string courseName)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return $"Khóa học {courseName} đang được đồng bộ nội dung từ cơ sở dữ liệu.";
        }

        const int maxLength = 135;
        if (description.Length <= maxLength)
        {
            return description;
        }

        return $"{description[..maxLength].Trim()}...";
    }

    private static string BuildCourseTarget(string courseName, int durationHours)
    {
        if (durationHours <= 0)
        {
            return $"Hoàn thành chương trình {courseName} theo lộ trình đào tạo của trung tâm.";
        }

        return $"Hoàn thành {courseName} sau khoảng {durationHours} giờ học với tiến độ được theo dõi theo từng giai đoạn.";
    }

    private static List<string> BuildCourseObjectives(string courseName)
    {
        var normalized = RemoveDiacritics(courseName).ToLowerInvariant();
        if (normalized.Contains("ielts"))
        {
            return
            [
                "Củng cố nền tảng 4 kỹ năng theo chuẩn IELTS",
                "Rèn phản xạ nghe đọc với bộ đề có lộ trình",
                "Theo dõi tiến độ qua các bài kiểm tra định kỳ",
                "Chuẩn bị cho mục tiêu đầu ra rõ ràng"
            ];
        }

        if (normalized.Contains("toeic"))
        {
            return
            [
                "Củng cố ngữ pháp và từ vựng trọng tâm",
                "Rèn tốc độ xử lý đề nghe và đọc",
                "Luyện kỹ năng phân bổ thời gian khi làm bài",
                "Theo dõi kết quả qua từng mốc đánh giá"
            ];
        }

        return
        [
            "Củng cố nền tảng tiếng Anh thực hành",
            "Tăng phản xạ giao tiếp trong tình huống thực tế",
            "Theo dõi tiến độ học tập theo từng giai đoạn",
            "Chuẩn bị cho kiểm tra cuối khóa của trung tâm"
        ];
    }

    private static List<string> BuildCourseHighlights(string courseName)
    {
        var normalized = RemoveDiacritics(courseName).ToLowerInvariant();
        if (normalized.Contains("ielts"))
        {
            return
            [
                "Lộ trình rõ ràng theo band mục tiêu",
                "Bài tập được giao và theo dõi hàng tuần",
                "Chữa bài với phản hồi chi tiết",
                "Đánh giá định kỳ trong suốt khóa học"
            ];
        }

        if (normalized.Contains("toeic"))
        {
            return
            [
                "Bộ đề luyện tập theo từng phần thi",
                "Tổng hợp lỗi sai thường gặp",
                "Theo dõi tiến độ qua từng chặng",
                "Tài liệu tự ôn tập sau buổi học"
            ];
        }

        return
        [
            "Lớp học theo nhóm nhỏ dễ theo sát",
            "Giáo trình bám sát mục tiêu đầu ra",
            "Theo dõi tiến độ học viên định kỳ",
            "Hỗ trợ ôn tập và thực hành ngoài giờ"
        ];
    }

    private static string CreateSlug(string value)
    {
        var normalized = RemoveDiacritics(value).ToLowerInvariant();
        var builder = new StringBuilder();
        var previousWasDash = false;

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasDash = false;
                continue;
            }

            if (previousWasDash)
            {
                continue;
            }

            builder.Append('-');
            previousWasDash = true;
        }

        return builder.ToString().Trim('-');
    }

    private static string RemoveDiacritics(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalizedString = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalizedString.Length);

        foreach (var character in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character == 'đ' ? 'd' : character == 'Đ' ? 'D' : character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string GetString(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? string.Empty : Convert.ToString(reader[name]) ?? string.Empty;
    }

    private static int GetInt32(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? 0 : Convert.ToInt32(reader[name]);
    }

    private static decimal GetDecimal(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? 0m : Convert.ToDecimal(reader[name]);
    }

    private static DateTime GetDateTime(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(reader[name]);
    }

    private static DateTime? GetNullableDateTime(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? null : Convert.ToDateTime(reader[name]);
    }

    private static bool GetBoolean(SqlDataReader reader, string name)
    {
        return reader[name] != DBNull.Value && Convert.ToBoolean(reader[name]);
    }

    private static byte GetByte(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? (byte)0 : Convert.ToByte(reader[name]);
    }
}

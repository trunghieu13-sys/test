namespace SalonCare.Core.Entities;

/// <summary>
/// Thực thể Nhật ký kiểm toán hệ thống (Audit Log), lưu vết mọi tác vụ quan trọng.
/// </summary>
public class AuditLog
{
    public int AuditLogId { get; set; }

    /// <summary>
    /// Tên thực thể bị tác động (ví dụ: Appointment, Customer).
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// Hành động thực hiện (CREATE, UPDATE, DELETE, OVERLAP_ATTEMPT, NO_SHOW_FLAG).
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm phát sinh sự kiện.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Người thực hiện (Lễ tân, Quản trị viên hoặc Cron Worker).
    /// </summary>
    public string PerformedBy { get; set; } = "System";

    /// <summary>
    /// Chi tiết mô tả hoặc dữ liệu thay đổi dạng chuỗi JSON/Text.
    /// </summary>
    public string Details { get; set; } = string.Empty;
}

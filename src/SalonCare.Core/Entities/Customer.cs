using SalonCare.Core.Enums;

namespace SalonCare.Core.Entities;

/// <summary>
/// Thực thể Khách hàng trong hệ thống SalonCare CRM.
/// </summary>
public class Customer
{
    public int CustomerId { get; set; }

    /// <summary>
    /// Họ và tên khách hàng.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại liên hệ (Khóa định danh Zalo OA / ZNS).
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Email liên hệ (tùy chọn).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Địa chỉ khách hàng.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Tổng số tiền khách hàng đã chi tiêu tích lũy.
    /// </summary>
    public decimal TotalSpent { get; set; } = 0;

    /// <summary>
    /// Ngày ghé thăm salon lần gần nhất.
    /// </summary>
    public DateTime? LastVisitDate { get; set; }

    /// <summary>
    /// Ngày tạo tài khoản / hồ sơ khách hàng.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<RfmScore> RfmScores { get; set; } = new List<RfmScore>();
}

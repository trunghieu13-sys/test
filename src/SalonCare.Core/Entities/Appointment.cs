using SalonCare.Core.Enums;

namespace SalonCare.Core.Entities;

/// <summary>
/// Thực thể Lịch hẹn dịch vụ Spa/Salon (Trọng tâm quản trị lịch & chống trùng lặp).
/// </summary>
public class Appointment
{
    public int AppointmentId { get; set; }

    /// <summary>
    /// ID Khách hàng đặt hẹn.
    /// </summary>
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    /// <summary>
    /// ID Kỹ thuật viên phụ trách lịch hẹn.
    /// </summary>
    public int TechnicianId { get; set; }
    public Technician? Technician { get; set; }

    /// <summary>
    /// Ngày diễn ra lịch hẹn.
    /// </summary>
    public DateOnly AppointmentDate { get; set; }

    /// <summary>
    /// Giờ bắt đầu dịch vụ.
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// Giờ kết thúc dịch vụ.
    /// </summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Tên gói dịch vụ (ví dụ: Chăm sóc da chuyên sâu, Cắt uốn tạo kiểu).
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Đơn giá dịch vụ (VNĐ).
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Trạng thái lịch hẹn (Pending, Confirmed, Completed, NoShow, Canceled).
    /// </summary>
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    /// <summary>
    /// Ghi chú thêm từ khách hàng hoặc lễ tân.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Thời điểm khởi tạo lịch hẹn trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

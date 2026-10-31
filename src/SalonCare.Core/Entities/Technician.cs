namespace SalonCare.Core.Entities;

/// <summary>
/// Thực thể Kỹ thuật viên / Thợ lành nghề tại Salon/Spa.
/// </summary>
public class Technician
{
    public int TechnicianId { get; set; }

    /// <summary>
    /// Họ tên kỹ thuật viên.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại kỹ thuật viên.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Chuyên môn (ví dụ: Chuyên gia uốn nhuộm tóc, Kỹ thuật viên Skincare, Chăm sóc móng).
    /// </summary>
    public string Specialization { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái hoạt động (true: Đang làm việc, false: Nghỉ phép/Nghỉ việc).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Thời điểm tạo kỹ thuật viên.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

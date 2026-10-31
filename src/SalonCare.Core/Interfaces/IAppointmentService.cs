using SalonCare.Core.Entities;

namespace SalonCare.Core.Interfaces;

/// <summary>
/// Kết quả phản hồi từ nghiệp vụ xử lý Lịch hẹn.
/// </summary>
public record AppointmentResult(bool IsSuccess, string Message, Appointment? Appointment);

/// <summary>
/// Giao diện Service xử lý logic nghiệp vụ đặt lịch và kiểm tra ràng buộc.
/// </summary>
public interface IAppointmentService
{
    /// <summary>
    /// Xử lý tạo mới lịch hẹn: kiểm tra hợp lệ thời gian và chống trùng giờ KTV.
    /// </summary>
    Task<AppointmentResult> CreateAppointmentAsync(Appointment appointment);

    /// <summary>
    /// Lấy danh sách toàn bộ lịch hẹn.
    /// </summary>
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
}

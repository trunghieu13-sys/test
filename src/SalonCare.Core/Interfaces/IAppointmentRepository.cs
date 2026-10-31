using SalonCare.Core.Entities;

namespace SalonCare.Core.Interfaces;

/// <summary>
/// Giao diện Repository quản lý dữ liệu Lịch hẹn.
/// </summary>
public interface IAppointmentRepository
{
    /// <summary>
    /// Lấy toàn bộ danh sách lịch hẹn kèm thông tin khách hàng và kỹ thuật viên.
    /// </summary>
    Task<IEnumerable<Appointment>> GetAllAsync();

    /// <summary>
    /// Lấy danh sách lịch hẹn của một kỹ thuật viên trong một ngày cụ thể (loại trừ đã hủy).
    /// </summary>
    Task<IEnumerable<Appointment>> GetByTechnicianAndDateAsync(int technicianId, DateOnly appointmentDate);

    /// <summary>
    /// Lấy thông tin chi tiết một lịch hẹn theo Id.
    /// </summary>
    Task<Appointment?> GetByIdAsync(int id);

    /// <summary>
    /// Kiểm tra xem Kỹ thuật viên có bị trùng lịch hẹn trong khung giờ [startTime, endTime) vào ngày chỉ định hay không.
    /// Công thức trùng: (startTime < existing.EndTime) && (endTime > existing.StartTime).
    /// </summary>
    Task<bool> HasOverlapAsync(int technicianId, DateOnly appointmentDate, TimeOnly startTime, TimeOnly endTime, int? excludeAppointmentId = null);

    /// <summary>
    /// Tìm lịch hẹn đang bị trùng lặp của KTV (nếu có) để hiển thị chi tiết cho người dùng.
    /// </summary>
    Task<Appointment?> GetOverlappingAppointmentAsync(int technicianId, DateOnly appointmentDate, TimeOnly startTime, TimeOnly endTime, int? excludeAppointmentId = null);

    /// <summary>
    /// Thêm mới một lịch hẹn vào cơ sở dữ liệu.
    /// </summary>
    Task AddAsync(Appointment appointment);

    /// <summary>
    /// Cập nhật thông tin lịch hẹn.
    /// </summary>
    Task UpdateAsync(Appointment appointment);

    /// <summary>
    /// Xóa lịch hẹn theo Id.
    /// </summary>
    Task DeleteAsync(int id);
}

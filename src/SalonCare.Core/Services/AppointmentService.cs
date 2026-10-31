using SalonCare.Core.Entities;
using SalonCare.Core.Enums;
using SalonCare.Core.Interfaces;

namespace SalonCare.Core.Services;

/// <summary>
/// Hiện thực Service quản trị lịch hẹn, đảm bảo nghiệp vụ không chồng chéo thời gian của KTV.
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    /// <inheritdoc />
    public async Task<AppointmentResult> CreateAppointmentAsync(Appointment appointment)
    {
        // 1. Kiểm tra tính hợp lệ cơ bản của khung giờ
        if (appointment.StartTime >= appointment.EndTime)
        {
            return new AppointmentResult(false, "LỖI: Khung giờ không hợp lệ (Giờ kết thúc phải sau giờ bắt đầu)!", null);
        }

        // 2. Kiểm tra trùng lặp lịch hẹn của Kỹ thuật viên
        var isOverlapped = await _appointmentRepository.HasOverlapAsync(
            appointment.TechnicianId,
            appointment.AppointmentDate,
            appointment.StartTime,
            appointment.EndTime
        );

        if (isOverlapped)
        {
            return new AppointmentResult(
                false,
                "RẤT TIẾC: Kỹ thuật viên đã có lịch hẹn trùng trong khung giờ này!",
                null
            );
        }

        // 3. Thiết lập thông số mặc định nếu cần
        if (appointment.CreatedAt == default)
        {
            appointment.CreatedAt = DateTime.UtcNow;
        }

        // 4. Lưu vào CSDL
        await _appointmentRepository.AddAsync(appointment);

        return new AppointmentResult(true, "Đặt lịch hẹn thành công!", appointment);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return await _appointmentRepository.GetAllAsync();
    }
}

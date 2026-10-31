namespace SalonCare.Core.Enums;

/// <summary>
/// Trạng thái của lịch hẹn tại Salon/Spa.
/// </summary>
public enum AppointmentStatus
{
    /// <summary>
    /// Đang chờ xác nhận từ lễ tân hoặc khách hàng.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Đã xác nhận thành công (Khách đã nhận ZNS và xác nhận đến).
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Đã hoàn thành buổi dịch vụ và thanh toán.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Khách bỏ hẹn không báo trước (No-show) - Đối tượng trọng tâm giảm thiểu của SalonCare.
    /// </summary>
    NoShow = 3,

    /// <summary>
    /// Lịch hẹn đã bị hủy trước giờ hẹn.
    /// </summary>
    Canceled = 4
}

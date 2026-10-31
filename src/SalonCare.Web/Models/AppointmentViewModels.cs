using System.ComponentModel.DataAnnotations;
using SalonCare.Core.Entities;

namespace SalonCare.Web.Models;

/// <summary>
/// ViewModel chứa dữ liệu đầu vào cho Form tạo mới Lịch hẹn.
/// </summary>
public class CreateAppointmentViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn khách hàng.")]
    [Display(Name = "Khách hàng")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn kỹ thuật viên.")]
    [Display(Name = "Kỹ thuật viên")]
    public int TechnicianId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ.")]
    [StringLength(150, ErrorMessage = "Tên dịch vụ không vượt quá 150 ký tự.")]
    [Display(Name = "Tên dịch vụ")]
    public string ServiceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập giá dịch vụ.")]
    [Range(10000, 50000000, ErrorMessage = "Giá dịch vụ phải từ 10.000 đến 50.000.000 VNĐ.")]
    [Display(Name = "Đơn giá (VNĐ)")]
    public decimal Price { get; set; } = 250000;

    [Required(ErrorMessage = "Vui lòng chọn ngày hẹn.")]
    [Display(Name = "Ngày hẹn")]
    public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

    [Required(ErrorMessage = "Vui lòng chọn giờ bắt đầu.")]
    [Display(Name = "Giờ bắt đầu")]
    public TimeOnly StartTime { get; set; } = new(9, 0);

    [Required(ErrorMessage = "Vui lòng chọn giờ kết thúc.")]
    [Display(Name = "Giờ kết thúc")]
    public TimeOnly EndTime { get; set; } = new(10, 0);

    [Display(Name = "Ghi chú")]
    [StringLength(500, ErrorMessage = "Ghi chú không vượt quá 500 ký tự.")]
    public string? Notes { get; set; }
}

/// <summary>
/// ViewModel chính phục vụ hiển thị màn hình Lịch hẹn W02/W03.
/// </summary>
public class AppointmentIndexViewModel
{
    public IEnumerable<Appointment> Appointments { get; set; } = new List<Appointment>();
    public IEnumerable<Customer> Customers { get; set; } = new List<Customer>();
    public IEnumerable<Technician> Technicians { get; set; } = new List<Technician>();
    public CreateAppointmentViewModel FormModel { get; set; } = new();

    // Thống kê nhanh để hiển thị Dashboard
    public int TotalAppointments => Appointments.Count();
    public int ConfirmedCount => Appointments.Count(a => a.Status == SalonCare.Core.Enums.AppointmentStatus.Confirmed);
    public int PendingCount => Appointments.Count(a => a.Status == SalonCare.Core.Enums.AppointmentStatus.Pending);
    public int NoShowCount => Appointments.Count(a => a.Status == SalonCare.Core.Enums.AppointmentStatus.NoShow);
}

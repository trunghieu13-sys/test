using Microsoft.AspNetCore.Mvc;
using SalonCare.Core.Entities;
using SalonCare.Core.Enums;
using SalonCare.Core.Interfaces;
using SalonCare.Web.Models;

namespace SalonCare.Web.Controllers;

/// <summary>
/// Controller điều phối màn hình và luồng nghiệp vụ Lịch hẹn của SalonCare CRM.
/// Thực hiện kiểm tra xung đột trùng lặp lịch kỹ thuật viên theo W02/W03.
/// </summary>
public class AppointmentsController : Controller
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentRepository appointmentRepository,
        ICustomerRepository customerRepository,
        ITechnicianRepository technicianRepository,
        ILogger<AppointmentsController> logger)
    {
        _appointmentRepository = appointmentRepository;
        _customerRepository = customerRepository;
        _technicianRepository = technicianRepository;
        _logger = logger;
    }

    /// <summary>
    /// Hiển thị danh sách lịch hẹn và Form đặt lịch mới.
    /// GET: /Appointments hoặc /
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var appointments = await _appointmentRepository.GetAllAsync();
        var customers = await _customerRepository.GetAllAsync();
        var technicians = await _technicianRepository.GetAllAsync();

        var viewModel = new AppointmentIndexViewModel
        {
            Appointments = appointments,
            Customers = customers,
            Technicians = technicians,
            FormModel = new CreateAppointmentViewModel()
        };

        return View(viewModel);
    }

    /// <summary>
    /// Xử lý tạo mới lịch hẹn và kiểm tra xung đột lịch của KTV.
    /// POST: /Appointments/Create
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAppointmentViewModel model)
    {
        // 1. Kiểm tra tính hợp lệ của Model
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin nhập trên form!";
            return RedirectToAction(nameof(Index));
        }

        // 2. Kiểm tra tính logic của thời gian: Giờ kết thúc phải sau giờ bắt đầu
        if (model.StartTime >= model.EndTime)
        {
            TempData["ErrorMessage"] = "RẤT TIẾC: Giờ kết thúc phải sau giờ bắt đầu lịch hẹn!";
            return RedirectToAction(nameof(Index));
        }

        // 3. KIỂM TRA TRÙNG LỊCH KỸ THUẬT VIÊN (BẮT BUỘC THEO YÊU CẦU ĐỒ ÁN)
        var hasOverlap = await _appointmentRepository.HasOverlapAsync(
            model.TechnicianId,
            model.AppointmentDate,
            model.StartTime,
            model.EndTime
        );

        if (hasOverlap)
        {
            _logger.LogWarning(
                "Phát hiện trùng lịch KTV Id {TechId} vào ngày {Date} lúc {Start} - {End}",
                model.TechnicianId, model.AppointmentDate, model.StartTime, model.EndTime
            );

            // Thông báo lỗi bắt buộc theo yêu cầu đề bài
            TempData["ErrorMessage"] = "RẤT TIẾC: Kỹ thuật viên đã có lịch hẹn trùng trong khung giờ này!";
            return RedirectToAction(nameof(Index));
        }

        // 4. Nếu không trùng lịch, tiến hành tạo mới thực thể Appointment
        var appointment = new Appointment
        {
            CustomerId = model.CustomerId,
            TechnicianId = model.TechnicianId,
            AppointmentDate = model.AppointmentDate,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            ServiceName = model.ServiceName,
            Price = model.Price,
            Status = AppointmentStatus.Confirmed, // Mặc định Đã xác nhận khi xếp lịch
            Notes = model.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _appointmentRepository.AddAsync(appointment);

        _logger.LogInformation(
            "Đã tạo thành công lịch hẹn Id {AppointmentId} cho khách hàng {CustomerId}",
            appointment.AppointmentId, appointment.CustomerId
        );

        TempData["SuccessMessage"] = $"Đặt lịch hẹn thành công cho khách hàng! Thời gian: {model.StartTime} - {model.EndTime} ngày {model.AppointmentDate:dd/MM/yyyy}.";
        return RedirectToAction(nameof(Index));
    }
}

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
        // Xử lý tự động trích xuất nếu form submit có tiền tố FormModel.* từ Razor View
        if ((!model.CustomerId.HasValue || model.CustomerId == 0) && Request.Form.ContainsKey("FormModel.CustomerId"))
        {
            if (int.TryParse(Request.Form["FormModel.CustomerId"], out int cId)) model.CustomerId = cId;
            if (int.TryParse(Request.Form["FormModel.TechnicianId"], out int tId)) model.TechnicianId = tId;
            model.ServiceName = Request.Form["FormModel.ServiceName"].ToString();
            if (decimal.TryParse(Request.Form["FormModel.Price"], out decimal price)) model.Price = price;
            if (DateOnly.TryParse(Request.Form["FormModel.AppointmentDate"], out DateOnly date)) model.AppointmentDate = date;
            if (TimeOnly.TryParse(Request.Form["FormModel.StartTime"], out TimeOnly start)) model.StartTime = start;
            if (TimeOnly.TryParse(Request.Form["FormModel.EndTime"], out TimeOnly end)) model.EndTime = end;
            model.Notes = Request.Form["FormModel.Notes"].ToString();
        }

        // 1. Kiểm tra tính hợp lệ cơ bản
        if (!model.CustomerId.HasValue || model.CustomerId.Value <= 0)
        {
            TempData["ErrorMessage"] = "Vui lòng chọn khách hàng!";
            return RedirectToAction(nameof(Index));
        }

        if (!model.TechnicianId.HasValue || model.TechnicianId.Value <= 0)
        {
            TempData["ErrorMessage"] = "Vui lòng chọn kỹ thuật viên!";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(model.ServiceName))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập tên gói dịch vụ!";
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
            model.TechnicianId.Value,
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
            CustomerId = model.CustomerId.Value,
            TechnicianId = model.TechnicianId.Value,
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

        TempData["SuccessMessage"] = $"Đặt lịch hẹn thành công cho khách hàng! Thời gian: {model.StartTime:HH\\:mm} - {model.EndTime:HH\\:mm} ngày {model.AppointmentDate:dd/MM/yyyy}.";
        return RedirectToAction(nameof(Index));
    }
}

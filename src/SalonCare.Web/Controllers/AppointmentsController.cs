using Microsoft.AspNetCore.Mvc;
using SalonCare.Core.Entities;
using SalonCare.Core.Enums;
using SalonCare.Core.Interfaces;
using SalonCare.Web.Models;

namespace SalonCare.Web.Controllers;

/// <summary>
/// Controller điều phối màn hình và luồng nghiệp vụ Lịch hẹn của SalonCare CRM.
/// Khung giờ hoạt động chuẩn: 06:00 sáng đến 16:00 (4h chiều).
/// </summary>
public class AppointmentsController : Controller
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly ILogger<AppointmentsController> _logger;

    // Khung giờ làm việc cố định: 6h sáng đến 4h chiều (06:00 - 16:00)
    public static readonly TimeOnly WorkStart = new(6, 0);
    public static readonly TimeOnly WorkEnd = new(16, 0);

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
    /// API trả về danh sách khung giờ ĐÃ ĐẶT (nút đỏ) và khung giờ RẢNH (nút xanh)
    /// theo kỹ thuật viên và ngày làm việc (từ 06:00 đến 16:00).
    /// GET: /Appointments/GetSchedule?technicianId=1&date=2026-11-02
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSchedule(int technicianId, DateOnly date)
    {
        var existingAppointments = (await _appointmentRepository.GetByTechnicianAndDateAsync(technicianId, date)).ToList();

        // 1. Danh sách khung giờ ĐÃ ĐẶT (Bận)
        var busySlots = existingAppointments.Select(a => new
        {
            id = a.AppointmentId,
            start = a.StartTime.ToString("HH:mm"),
            end = a.EndTime.ToString("HH:mm"),
            label = $"{a.StartTime:HH\\:mm} - {a.EndTime:HH\\:mm}",
            service = a.ServiceName,
            customer = a.Customer?.FullName ?? "Khách hàng"
        }).ToList();

        // 2. Tính toán danh sách khung giờ RẢNH trong khoảng làm việc 06:00 -> 16:00 (mỗi slot 1 tiếng)
        var freeSlots = new List<object>();

        for (int hour = WorkStart.Hour; hour < WorkEnd.Hour; hour++)
        {
            var slotStart = new TimeOnly(hour, 0);
            var slotEnd = new TimeOnly(hour + 1, 0);

            // Kiểm tra slot này có bị giao thoa với bất kỳ lịch đã đặt nào không
            bool isBusy = existingAppointments.Any(a => slotStart < a.EndTime && slotEnd > a.StartTime);

            if (!isBusy)
            {
                freeSlots.Add(new
                {
                    start = slotStart.ToString("HH:mm"),
                    end = slotEnd.ToString("HH:mm"),
                    label = $"{slotStart:HH\\:mm} - {slotEnd:HH\\:mm}"
                });
            }
        }

        return Json(new
        {
            technicianId,
            date = date.ToString("yyyy-MM-dd"),
            workHours = $"{WorkStart:HH\\:mm} - {WorkEnd:HH\\:mm}",
            busySlots,
            freeSlots
        });
    }

    /// <summary>
    /// Xử lý tạo mới lịch hẹn và kiểm tra xung đột lịch của KTV.
    /// POST: /Appointments/Create
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAppointmentViewModel model)
    {
        // Tự động trích xuất nếu form submit có tiền tố FormModel.*
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

        // 2. Kiểm tra tính logic thời gian
        if (model.StartTime >= model.EndTime)
        {
            TempData["ErrorMessage"] = "RẤT TIẾC: Giờ kết thúc phải sau giờ bắt đầu lịch hẹn!";
            return RedirectToAction(nameof(Index));
        }

        // 3. KIỂM TRA KHUNG GIỜ LÀM VIỆC CỦA SALON: 06:00 - 16:00
        if (model.StartTime < WorkStart || model.EndTime > WorkEnd)
        {
            TempData["ErrorMessage"] = $"RẤT TIẾC: Khung giờ làm việc của Salon chỉ từ {WorkStart:HH\\:mm} sáng đến {WorkEnd:HH\\:mm} (4h chiều)!";
            return RedirectToAction(nameof(Index));
        }

        // 4. KIỂM TRA TRÙNG LỊCH KỸ THUẬT VIÊN (BẮT BUỘC THEO YÊU CẦU ĐỒ ÁN)
        var overlappingAppt = await _appointmentRepository.GetOverlappingAppointmentAsync(
            model.TechnicianId.Value,
            model.AppointmentDate,
            model.StartTime,
            model.EndTime
        );

        if (overlappingAppt != null)
        {
            _logger.LogWarning(
                "Phát hiện trùng lịch KTV Id {TechId} vào ngày {Date} lúc {Start} - {End} (Trùng với Lịch #{ApptId})",
                model.TechnicianId, model.AppointmentDate, model.StartTime, model.EndTime, overlappingAppt.AppointmentId
            );

            TempData["ErrorMessage"] = $"RẤT TIẾC: Kỹ thuật viên đã có lịch hẹn trùng trong khung giờ này! (Đang trùng với Lịch #{overlappingAppt.AppointmentId} lúc {overlappingAppt.StartTime:HH\\:mm} - {overlappingAppt.EndTime:HH\\:mm}: {overlappingAppt.ServiceName})";
            return RedirectToAction(nameof(Index));
        }

        // 5. Nếu không trùng lịch, tạo mới thực thể Appointment
        var appointment = new Appointment
        {
            CustomerId = model.CustomerId.Value,
            TechnicianId = model.TechnicianId.Value,
            AppointmentDate = model.AppointmentDate,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            ServiceName = model.ServiceName,
            Price = model.Price,
            Status = AppointmentStatus.Confirmed,
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

    /// <summary>
    /// Xóa lịch hẹn.
    /// POST: /Appointments/Delete
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _appointmentRepository.DeleteAsync(id);
        TempData["SuccessMessage"] = $"Đã xóa lịch hẹn #{id} thành công!";
        return RedirectToAction(nameof(Index));
    }
}

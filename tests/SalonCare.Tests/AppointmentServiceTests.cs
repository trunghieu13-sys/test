using FluentAssertions;
using Moq;
using SalonCare.Core.Entities;
using SalonCare.Core.Enums;
using SalonCare.Core.Interfaces;
using SalonCare.Core.Services;
using Xunit;

namespace SalonCare.Tests;

/// <summary>
/// Bộ kiểm thử tự động (Unit Tests) cho các nghiệp vụ cốt lõi của SalonCare CRM W02/W03.
/// Sử dụng xUnit, Moq và FluentAssertions theo chuẩn công nghiệp.
/// </summary>
public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepo;
    private readonly AppointmentService _appointmentService;
    private readonly RfmService _rfmService;

    public AppointmentServiceTests()
    {
        _mockAppointmentRepo = new Mock<IAppointmentRepository>();
        _appointmentService = new AppointmentService(_mockAppointmentRepo.Object);
        _rfmService = new RfmService();
    }

    /// <summary>
    /// TEST 1: Kiểm tra trường hợp kỹ thuật viên đã có lịch hẹn trùng trong khung giờ chỉ định.
    /// Kỳ vọng: Service phát hiện trùng lịch (HasOverlapAsync = true), từ chối tạo và không ghi vào DB.
    /// </summary>
    [Fact]
    public async Task CreateAppointment_ShouldFail_WhenTechnicianIsOverlapped()
    {
        // ARRANGE (Chuẩn bị dữ liệu)
        int technicianId = 1; // KTV Nguyễn Văn Hùng
        var appointmentDate = new DateOnly(2026, 11, 2);
        var startTime = new TimeOnly(9, 15);
        var endTime = new TimeOnly(10, 15);

        var newAppointment = new Appointment
        {
            CustomerId = 1,
            TechnicianId = technicianId,
            AppointmentDate = appointmentDate,
            StartTime = startTime,
            EndTime = endTime,
            ServiceName = "Cắt uốn tạo kiểu cao cấp",
            Price = 350000,
            Status = AppointmentStatus.Confirmed
        };

        // Giả lập Repository phát hiện khung giờ đã bị trùng với lịch hiện có
        _mockAppointmentRepo
            .Setup(repo => repo.HasOverlapAsync(technicianId, appointmentDate, startTime, endTime, null))
            .ReturnsAsync(true);

        // ACT (Thực thi hành động)
        var result = await _appointmentService.CreateAppointmentAsync(newAppointment);

        // ASSERT (Kiểm chứng kết quả)
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("bởi vì kỹ thuật viên đã bận trong khung giờ được chọn");
        result.Message.Should().Be("RẤT TIẾC: Kỹ thuật viên đã có lịch hẹn trùng trong khung giờ này!");
        result.Appointment.Should().BeNull();

        // Đảm bảo tuyệt đối không gọi lệnh AddAsync vào CSDL
        _mockAppointmentRepo.Verify(repo => repo.AddAsync(It.IsAny<Appointment>()), Times.Never);
    }

    /// <summary>
    /// TEST 2: Kiểm tra trường hợp khung giờ yêu cầu hoàn toàn trống lịch.
    /// Kỳ vọng: Service xác nhận khung giờ hợp lệ (HasOverlapAsync = false), ghi nhận lịch hẹn vào DB và trả về thành công.
    /// </summary>
    [Fact]
    public async Task CreateAppointment_ShouldSucceed_WhenTimeSlotIsAvailable()
    {
        // ARRANGE (Chuẩn bị dữ liệu)
        int technicianId = 2; // KTV Trần Thị Mai
        var appointmentDate = new DateOnly(2026, 11, 2);
        var startTime = new TimeOnly(14, 0);
        var endTime = new TimeOnly(15, 30);

        var newAppointment = new Appointment
        {
            CustomerId = 2,
            TechnicianId = technicianId,
            AppointmentDate = appointmentDate,
            StartTime = startTime,
            EndTime = endTime,
            ServiceName = "Chăm sóc da mặt chuyên sâu",
            Price = 500000,
            Status = AppointmentStatus.Confirmed
        };

        // Giả lập Repository xác nhận khung giờ hoàn toàn rảnh rỗi
        _mockAppointmentRepo
            .Setup(repo => repo.HasOverlapAsync(technicianId, appointmentDate, startTime, endTime, null))
            .ReturnsAsync(false);

        _mockAppointmentRepo
            .Setup(repo => repo.AddAsync(It.IsAny<Appointment>()))
            .Returns(Task.CompletedTask);

        // ACT (Thực thi hành động)
        var result = await _appointmentService.CreateAppointmentAsync(newAppointment);

        // ASSERT (Kiểm chứng kết quả)
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("bởi vì khung giờ rảnh và thông tin lịch hẹn hợp lệ");
        result.Message.Should().Be("Đặt lịch hẹn thành công!");
        result.Appointment.Should().NotBeNull();
        result.Appointment!.TechnicianId.Should().Be(technicianId);

        // Đảm bảo phương thức AddAsync đã được gọi đúng 1 lần
        _mockAppointmentRepo.Verify(repo => repo.AddAsync(It.IsAny<Appointment>()), Times.Once);
    }

    /// <summary>
    /// TEST 3: Kiểm tra thuật toán phân loại RFM đối với khách hàng ngưng hoạt động trên 60 ngày.
    /// Kỳ vọng: Trả về phân khúc AtRisk (Nguy cơ rời bỏ) để hệ thống kích hoạt kịch bản ZNS chăm sóc tự động.
    /// </summary>
    [Fact]
    public void CalculateRFM_ShouldReturnAtRisk_WhenCustomerInactiveOver60Days()
    {
        // ARRANGE (Chuẩn bị dữ liệu: khách đã 75 ngày chưa ghé salon)
        int customerId = 10;
        int recencyDays = 75; // > 60 ngày nhưng <= 120 ngày
        int frequency = 2;
        decimal monetary = 650000m;

        // ACT (Thực thi phân loại)
        var segment = _rfmService.ClassifyCustomer(recencyDays, frequency, monetary);
        var rfmScore = _rfmService.ComputeRfmScore(customerId, recencyDays, frequency, monetary);

        // ASSERT (Kiểm chứng kết quả)
        segment.Should().Be(RfmSegment.AtRisk, "vì khách hàng ngưng hoạt động trên 60 ngày cần được cảnh báo nguy cơ No-show/Chuyển salon");
        rfmScore.RfmSegment.Should().Be(RfmSegment.AtRisk);
        rfmScore.RecencyDays.Should().Be(75);
    }

    /// <summary>
    /// TEST BỔ SUNG 4: Kiểm tra tính hợp lệ cơ bản - Giờ kết thúc sớm hơn hoặc bằng giờ bắt đầu.
    /// Kỳ vọng: Service từ chối ngay lập tức mà không cần kiểm tra CSDL.
    /// </summary>
    [Fact]
    public async Task CreateAppointment_ShouldFail_WhenEndTimeIsBeforeOrEqualStartTime()
    {
        // ARRANGE: Giờ bắt đầu 10:00 nhưng giờ kết thúc 09:30 (vô lý)
        var invalidAppointment = new Appointment
        {
            CustomerId = 1,
            TechnicianId = 1,
            AppointmentDate = new DateOnly(2026, 11, 2),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(9, 30),
            ServiceName = "Gội đầu dưỡng sinh"
        };

        // ACT
        var result = await _appointmentService.CreateAppointmentAsync(invalidAppointment);

        // ASSERT
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Giờ kết thúc phải sau giờ bắt đầu");
        _mockAppointmentRepo.Verify(repo => repo.HasOverlapAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>()), Times.Never);
    }

    /// <summary>
    /// TEST BỔ SUNG 5: Kiểm tra phân khúc Champions đối với khách hàng VIP (Mới ghé, tần suất cao, chi tiêu khủng).
    /// </summary>
    [Fact]
    public void CalculateRFM_ShouldReturnChampions_WhenCustomerVisitsFrequentlyWithHighSpending()
    {
        // ARRANGE: Khách mới ghé 5 ngày trước, đã đi 6 lần, chi 3.500.000 VNĐ
        var segment = _rfmService.ClassifyCustomer(recencyDays: 5, frequency: 6, monetary: 3500000m);

        // ASSERT
        segment.Should().Be(RfmSegment.Champions);
    }
}

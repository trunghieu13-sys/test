using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SalonCare.Core.Entities;
using SalonCare.Core.Enums;
using SalonCare.Infrastructure.Data;
using SalonCare.Infrastructure.Repositories;
using Xunit;

namespace SalonCare.Tests;

/// <summary>
/// Kiểm thử tích hợp Repository với In-Memory Database để xác nhận thuật toán chồng lấn thời gian.
/// </summary>
public class AppointmentRepositoryTests
{
    private SalonCareDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<SalonCareDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new SalonCareDbContext(options);

        // Seed 1 KTV và 1 lịch hẹn từ 09:00 đến 10:00 ngày 2026-11-02
        context.Technicians.Add(new Technician
        {
            TechnicianId = 1,
            FullName = "Nguyễn Văn Hùng",
            PhoneNumber = "0908111222",
            Specialization = "Tạo mẫu tóc",
            IsActive = true
        });

        context.Customers.Add(new Customer
        {
            CustomerId = 1,
            FullName = "Khách Hàng Test",
            PhoneNumber = "0900000000"
        });

        context.Appointments.Add(new Appointment
        {
            AppointmentId = 1,
            CustomerId = 1,
            TechnicianId = 1,
            AppointmentDate = new DateOnly(2026, 11, 2),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            ServiceName = "Cắt tóc",
            Price = 200000,
            Status = AppointmentStatus.Confirmed
        });

        context.SaveChanges();
        return context;
    }

    [Theory]
    [InlineData(8, 30, 9, 30, true)]   // Chồng lấn phần đầu: 08:30 - 09:30
    [InlineData(9, 15, 9, 45, true)]   // Nằm trọn bên trong: 09:15 - 09:45
    [InlineData(9, 30, 10, 30, true)]  // Chồng lấn phần đuôi: 09:30 - 10:30
    [InlineData(8, 0, 11, 0, true)]    // Bao trùm toàn bộ: 08:00 - 11:00
    [InlineData(8, 0, 9, 0, false)]    // Vừa chạm mép trước (không trùng): 08:00 - 09:00
    [InlineData(10, 0, 11, 0, false)]  // Vừa chạm mép sau (không trùng): 10:00 - 11:00
    public async Task HasOverlapAsync_ShouldCorrectlyDetectOverlaps(
        int startH, int startM, int endH, int endM, bool expectedOverlap)
    {
        // ARRANGE
        using var context = CreateInMemoryContext();
        var repo = new AppointmentRepository(context);
        int techId = 1;
        var date = new DateOnly(2026, 11, 2);
        var startTime = new TimeOnly(startH, startM);
        var endTime = new TimeOnly(endH, endM);

        // ACT
        var isOverlap = await repo.HasOverlapAsync(techId, date, startTime, endTime);

        // ASSERT
        isOverlap.Should().Be(expectedOverlap);
    }
}

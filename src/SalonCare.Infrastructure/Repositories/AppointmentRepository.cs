using Microsoft.EntityFrameworkCore;
using SalonCare.Core.Entities;
using SalonCare.Core.Enums;
using SalonCare.Core.Interfaces;
using SalonCare.Infrastructure.Data;

namespace SalonCare.Infrastructure.Repositories;

/// <summary>
/// Hiện thực Repository quản lý lịch hẹn sử dụng Entity Framework Core 8.0 & PostgreSQL.
/// </summary>
public class AppointmentRepository : IAppointmentRepository
{
    private readonly SalonCareDbContext _context;

    public AppointmentRepository(SalonCareDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Technician)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Technician)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);
    }

    /// <inheritdoc />
    public async Task<bool> HasOverlapAsync(
        int technicianId,
        DateOnly appointmentDate,
        TimeOnly startTime,
        TimeOnly endTime,
        int? excludeAppointmentId = null)
    {
        // Công thức kiểm tra 2 khoảng thời gian [start1, end1) và [start2, end2) giao nhau:
        // Overlap khi: start1 < end2 && end1 > start2
        // Bỏ qua các lịch hẹn đã Hủy (Canceled)
        return await _context.Appointments
            .AsNoTracking()
            .AnyAsync(a =>
                a.TechnicianId == technicianId &&
                a.AppointmentDate == appointmentDate &&
                a.Status != AppointmentStatus.Canceled &&
                (!excludeAppointmentId.HasValue || a.AppointmentId != excludeAppointmentId.Value) &&
                startTime < a.EndTime &&
                endTime > a.StartTime
            );
    }

    /// <inheritdoc />
    public async Task AddAsync(Appointment appointment)
    {
        await _context.Appointments.AddAsync(appointment);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id)
    {
        var existing = await _context.Appointments.FindAsync(id);
        if (existing != null)
        {
            _context.Appointments.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}

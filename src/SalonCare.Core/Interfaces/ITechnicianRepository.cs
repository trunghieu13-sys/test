using SalonCare.Core.Entities;

namespace SalonCare.Core.Interfaces;

/// <summary>
/// Giao diện Repository quản lý dữ liệu Kỹ thuật viên.
/// </summary>
public interface ITechnicianRepository
{
    Task<IEnumerable<Technician>> GetAllAsync();
    Task<Technician?> GetByIdAsync(int id);
    Task AddAsync(Technician technician);
}

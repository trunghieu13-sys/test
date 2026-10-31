using Microsoft.EntityFrameworkCore;
using SalonCare.Core.Entities;
using SalonCare.Core.Interfaces;
using SalonCare.Infrastructure.Data;

namespace SalonCare.Infrastructure.Repositories;

public class TechnicianRepository : ITechnicianRepository
{
    private readonly SalonCareDbContext _context;

    public TechnicianRepository(SalonCareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Technician>> GetAllAsync()
    {
        return await _context.Technicians
            .Where(t => t.IsActive)
            .OrderBy(t => t.FullName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Technician?> GetByIdAsync(int id)
    {
        return await _context.Technicians
            .FirstOrDefaultAsync(t => t.TechnicianId == id);
    }

    public async Task AddAsync(Technician technician)
    {
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using SalonCare.Core.Entities;
using SalonCare.Core.Interfaces;
using SalonCare.Infrastructure.Data;

namespace SalonCare.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly SalonCareDbContext _context;

    public CustomerRepository(SalonCareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .OrderBy(c => c.FullName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .Include(c => c.RfmScores)
            .FirstOrDefaultAsync(c => c.CustomerId == id);
    }

    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }
}

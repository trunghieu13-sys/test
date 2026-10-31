using SalonCare.Core.Entities;

namespace SalonCare.Core.Interfaces;

/// <summary>
/// Giao diện Repository quản lý dữ liệu Khách hàng.
/// </summary>
public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
}

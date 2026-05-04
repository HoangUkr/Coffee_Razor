using Domain.Entities;

namespace Application.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer?> GetByIdAsync(Guid id);
        Task UpdateAsync(Customer customer);
        Task<IEnumerable<Customer>> GetAllAsync();
    }
}

using Application.DTOs.Common;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IReservationRepository
    {
        Task<Reservation> GetByIdAsync(int id);
        Task<Reservation> CreateAsync(Reservation reservation);
        Task<Reservation> UpdateAsync(Reservation reservation, int originalVersion);
        Task DeleteAsync(int id);
        Task<PaginatedResult<Reservation>> SearchAsync(SearchParameters parameters);
        Task<List<Reservation>> GetByDateAsync(DateTime date);
        Task<PaginatedResult<Reservation>> GetByDatePaginatedAsync(DateTime date, int pageNumber, int pageSize);
        Task<PaginatedResult<Reservation>> SearchWithFiltersAsync(DateTime? date, string? searchTerm, string? statusFilter, int pageNumber, int pageSize);
        Task<bool> IsTimeSlotAvailableAsync(DateTime date, string time, int? excludeReservationId = null);
    }
}

using Application.DTOs.Common;
using Application.DTOs.Reservation;

namespace Application.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationResponse> CreateAsync(CreateReservationRequest request);
        Task<ReservationResponse> GetByIdAsync(int id);
        Task<PaginatedResult<ReservationResponse>> SearchAsync(SearchParameters parameters);
        Task<List<ReservationResponse>> GetByDateAsync(DateTime date);
        Task<PaginatedResult<ReservationResponse>> GetByDatePaginatedAsync(DateTime date, int pageNumber, int pageSize);
        Task<PaginatedResult<ReservationResponse>> SearchWithFiltersAsync(DateTime? date, string? searchTerm, string? statusFilter, int pageNumber, int pageSize);
        Task<ReservationResponse> UpdateAsync(int id, UpdateReservationRequest request);
        Task<ReservationResponse> ConfirmAsync(int id);
        Task<ReservationResponse> CancelAsync(int id);
        Task<ReservationResponse> CompleteAsync(int id);
    }
}

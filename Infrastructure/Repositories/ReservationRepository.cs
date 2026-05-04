using Application.DTOs.Common;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly CoffeeDbContext _context;
        private const int MaxTablesPerTimeSlot = 5;

        public ReservationRepository(CoffeeDbContext context)
        {
            _context = context;
        }

        public async Task<Reservation> GetByIdAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                throw new KeyNotFoundException($"Reservation with ID {id} not found.");
            }
            return reservation;
        }

        public async Task<Reservation> CreateAsync(Reservation reservation)
        {
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task<Reservation> UpdateAsync(Reservation reservation, int originalVersion)
        {
            _context.Reservations.Attach(reservation);
            var entry = _context.Entry(reservation);
            entry.Property(r => r.Version).OriginalValue = originalVersion;
            entry.State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task DeleteAsync(int id)
        {
            var reservation = await GetByIdAsync(id);
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResult<Reservation>> SearchAsync(SearchParameters parameters)
        {
            var query = _context.Reservations.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.ToLower();
                query = query.Where(r =>
                    r.CustomerName.ToLower().Contains(searchTerm) ||
                    r.PhoneNumber.Contains(searchTerm) ||
                    (r.Email != null && r.Email.ToLower().Contains(searchTerm))
                );
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PaginatedResult<Reservation>(
                items,
                totalCount,
                parameters.PageNumber,
                parameters.PageSize
            );
        }

        public async Task<List<Reservation>> GetByDateAsync(DateTime date)
        {
            return await _context.Reservations
                .Where(r => r.ReservationDate.Date == date.Date)
                .OrderBy(r => r.ReservationTime)
                .ThenBy(r => r.CustomerName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PaginatedResult<Reservation>> GetByDatePaginatedAsync(DateTime date, int pageNumber, int pageSize)
        {
            var query = _context.Reservations
                .Where(r => r.ReservationDate.Date == date.Date);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .OrderBy(r => r.ReservationTime)
                .ThenBy(r => r.CustomerName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PaginatedResult<Reservation>(
                items,
                totalCount,
                pageNumber,
                pageSize
            );
        }

        public async Task<PaginatedResult<Reservation>> SearchWithFiltersAsync(DateTime? date, string? searchTerm, string? statusFilter, int pageNumber, int pageSize)
        {
            var query = _context.Reservations.AsQueryable();

            // Apply date filter if provided
            if (date.HasValue)
            {
                query = query.Where(r => r.ReservationDate.Date == date.Value.Date);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchTermLower = searchTerm.ToLower();
                query = query.Where(r =>
                    r.CustomerName.ToLower().Contains(searchTermLower) ||
                    r.PhoneNumber.Contains(searchTerm) ||
                    (r.Email != null && r.Email.ToLower().Contains(searchTermLower))
                );
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                query = query.Where(r => r.Status == statusFilter);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting and pagination
            var items = await query
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .ThenBy(r => r.CustomerName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PaginatedResult<Reservation>(
                items,
                totalCount,
                pageNumber,
                pageSize
            );
        }

        public async Task<bool> IsTimeSlotAvailableAsync(DateTime date, string time, int? excludeReservationId = null)
        {
            var query = _context.Reservations
                .Where(r => r.ReservationDate.Date == date.Date &&
                           r.ReservationTime == time &&
                           r.Status != "Cancelled" &&
                           r.Status != "Completed");

            if (excludeReservationId.HasValue)
            {
                query = query.Where(r => r.Id != excludeReservationId.Value);
            }

            var bookedTablesCount = await query.CountAsync();
            return bookedTablesCount < MaxTablesPerTimeSlot;
        }
    }
}

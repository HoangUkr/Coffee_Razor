using Application.DTOs.Common;
using Application.DTOs.Reservation;
using Application.Exceptions;
using Application.Interfaces;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _repository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IReservationRepository repository,
            IEmailService emailService,
            IMapper mapper,
            ILogger<ReservationService> logger)
        {
            _repository = repository;
            _emailService = emailService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ReservationResponse> CreateAsync(CreateReservationRequest request)
        {
            // Validate reservation date is not in the past
            if (request.ReservationDate.Date < DateTime.Today)
            {
                throw new InvalidOperationException("Cannot create reservation for a past date.");
            }

            // Check if time slot is available
            var isAvailable = await _repository.IsTimeSlotAvailableAsync(
                request.ReservationDate,
                request.ReservationTime
            );

            if (!isAvailable)
            {
                throw new InvalidOperationException(
                    $"The time slot {request.ReservationTime} on {request.ReservationDate:MMMM dd, yyyy} is fully booked. Please choose another time."
                );
            }

            // Create reservation entity
            var reservation = new Reservation(
                request.CustomerName,
                request.PhoneNumber,
                request.ReservationDate,
                request.ReservationTime,
                request.NumberOfGuests,
                request.Email,
                request.SpecialRequests
            );

            // Save to database
            var createdReservation = await _repository.CreateAsync(reservation);

            // Send confirmation email if email is provided
            if (!string.IsNullOrWhiteSpace(createdReservation.Email))
            {
                try
                {
                    await _emailService.SendReservationConfirmationAsync(
                        createdReservation.Email,
                        createdReservation.CustomerName,
                        createdReservation.ReservationDate,
                        createdReservation.ReservationTime,
                        createdReservation.NumberOfGuests
                    );
                }
                catch (Exception ex)
                {
                    // Log email error but don't fail the reservation
                    _logger.LogError(ex, "Failed to send confirmation email for reservation {ReservationId}", createdReservation.Id);
                }
            }

            return _mapper.Map<ReservationResponse>(createdReservation);
        }

        public async Task<ReservationResponse> GetByIdAsync(int id)
        {
            var reservation = await _repository.GetByIdAsync(id);
            return _mapper.Map<ReservationResponse>(reservation);
        }

        public async Task<PaginatedResult<ReservationResponse>> SearchAsync(SearchParameters parameters)
        {
            var result = await _repository.SearchAsync(parameters);
            var mappedItems = _mapper.Map<List<ReservationResponse>>(result.Items);

            return new PaginatedResult<ReservationResponse>(
                mappedItems,
                result.TotalCount,
                result.PageNumber,
                result.PageSize
            );
        }

        public async Task<List<ReservationResponse>> GetByDateAsync(DateTime date)
        {
            var reservations = await _repository.GetByDateAsync(date);
            return _mapper.Map<List<ReservationResponse>>(reservations);
        }

        public async Task<PaginatedResult<ReservationResponse>> GetByDatePaginatedAsync(DateTime date, int pageNumber, int pageSize)
        {
            var result = await _repository.GetByDatePaginatedAsync(date, pageNumber, pageSize);
            var mappedItems = _mapper.Map<List<ReservationResponse>>(result.Items);

            return new PaginatedResult<ReservationResponse>(
                mappedItems,
                result.TotalCount,
                result.PageNumber,
                result.PageSize
            );
        }

        public async Task<PaginatedResult<ReservationResponse>> SearchWithFiltersAsync(DateTime? date, string? searchTerm, string? statusFilter, int pageNumber, int pageSize)
        {
            var result = await _repository.SearchWithFiltersAsync(date, searchTerm, statusFilter, pageNumber, pageSize);
            var mappedItems = _mapper.Map<List<ReservationResponse>>(result.Items);

            return new PaginatedResult<ReservationResponse>(
                mappedItems,
                result.TotalCount,
                result.PageNumber,
                result.PageSize
            );
        }

        public async Task<ReservationResponse> UpdateAsync(int id, UpdateReservationRequest request)
        {
            var reservation = await _repository.GetByIdAsync(id);

            // Check if time slot is available (excluding current reservation)
            if (reservation.ReservationDate != request.ReservationDate || 
                reservation.ReservationTime != request.ReservationTime)
            {
                var isAvailable = await _repository.IsTimeSlotAvailableAsync(
                    request.ReservationDate,
                    request.ReservationTime,
                    id
                );

                if (!isAvailable)
                {
                    throw new InvalidOperationException(
                        $"The time slot {request.ReservationTime} on {request.ReservationDate:MMMM dd, yyyy} is fully booked. Please choose another time."
                    );
                }
            }

            // Update reservation details
            reservation.UpdateDetails(
                customerName: request.CustomerName,
                phoneNumber: request.PhoneNumber,
                email: request.Email,
                reservationDate: request.ReservationDate,
                reservationTime: request.ReservationTime,
                numberOfGuests: request.NumberOfGuests,
                specialRequests: request.SpecialRequests
            );

            reservation.IncrementVersion();

            try
            {
                var updated = await _repository.UpdateAsync(reservation, request.Version);
                return _mapper.Map<ReservationResponse>(updated);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyConflictException("This reservation was updated by another admin. Your changes were not saved. Please reload and try again.");
            }
        }

        public async Task<ReservationResponse> ConfirmAsync(int id)
        {
            var reservation = await _repository.GetByIdAsync(id);
            reservation.Confirm();
            reservation.IncrementVersion();
            var updated = await _repository.UpdateAsync(reservation, reservation.Version - 1);
            return _mapper.Map<ReservationResponse>(updated);
        }

        public async Task<ReservationResponse> CancelAsync(int id)
        {
            var reservation = await _repository.GetByIdAsync(id);
            reservation.Cancel();
            reservation.IncrementVersion();
            var updated = await _repository.UpdateAsync(reservation, reservation.Version - 1);

            // Send cancellation email if email is provided
            if (!string.IsNullOrWhiteSpace(reservation.Email))
            {
                try
                {
                    await _emailService.SendReservationCancellationAsync(
                        reservation.Email,
                        reservation.CustomerName,
                        reservation.ReservationDate,
                        reservation.ReservationTime
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send cancellation email for reservation {ReservationId}", reservation.Id);
                }
            }

            return _mapper.Map<ReservationResponse>(updated);
        }

        public async Task<ReservationResponse> CompleteAsync(int id)
        {
            var reservation = await _repository.GetByIdAsync(id);
            reservation.Complete();
            reservation.IncrementVersion();
            var updated = await _repository.UpdateAsync(reservation, reservation.Version - 1);
            return _mapper.Map<ReservationResponse>(updated);
        }
    }
}

using Application.DTOs.Reservation;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class ReservationMappingProfile : Profile
    {
        public ReservationMappingProfile()
        {
            CreateMap<Reservation, ReservationResponse>()
                .ConstructUsing(r => new ReservationResponse(
                    r.Id,
                    r.CustomerName,
                    r.Email,
                    r.PhoneNumber,
                    r.ReservationDate,
                    r.ReservationTime,
                    r.NumberOfGuests,
                    r.SpecialRequests,
                    r.Status,
                    r.Version,
                    r.CreatedDate,
                    r.ConfirmedDate,
                    r.CancelledDate
                ));
        }
    }
}

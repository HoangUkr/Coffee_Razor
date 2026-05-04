using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using Domain.Entities;
using Application.DTOs.User;

namespace Application
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Mapping Entity -> Response DTO
            CreateMap<User, UserResponse>();

            // DO NOT map RegisterRequest -> User directly!
            // Password hashing must be done manually in the service layer
            // Use: var user = new User(request.Username, hashedPassword, salt);
        }
    }
}

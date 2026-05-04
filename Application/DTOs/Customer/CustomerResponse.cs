using System;

namespace Application.DTOs.Customer
{
    public record CustomerResponse
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string? PhoneNumber { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
        public bool IsDataCleared { get; init; }
        public string FullName => $"{FirstName} {LastName}";
    }
}

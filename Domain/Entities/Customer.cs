using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Customer entity for temporary order data
    /// Personal data (email, phone, address) cleared after order completion
    /// </summary>
    public class Customer
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        [MaxLength(10)]
        public string CustomerCode { get; private set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; private set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; private set; }

        [MaxLength(200)]
        public string? Email { get; private set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; private set; }

        [MaxLength(500)]
        public string? Address { get; private set; }

        public DateTimeOffset CreatedDate { get; private set; }

        public bool IsDataCleared { get; private set; }

        // One customer can have many orders
        public virtual ICollection<Order> Orders { get; private set; } = new HashSet<Order>();

        public Customer(string firstName, string lastName, string? email, string? phoneNumber, string? address = null)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required", nameof(lastName));

            Id = Guid.NewGuid();
            CustomerCode = GenerateCustomerCode();
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email?.Trim();
            PhoneNumber = phoneNumber?.Trim();
            Address = address?.Trim();
            CreatedDate = DateTimeOffset.UtcNow;
            IsDataCleared = false;
        }

        private static string GenerateCustomerCode()
        {
            return "C" + DateTimeOffset.UtcNow.ToString("yyMMdd") + Random.Shared.Next(100, 999).ToString();
        }

        private Customer() { }

        /// <summary>
        /// Clear personal data after order is completed for privacy
        /// </summary>
        public void ClearPersonalData()
        {
            FirstName = "Customer";
            LastName = CustomerCode;
            Email = null;
            PhoneNumber = null;
            Address = null;
            IsDataCleared = true;
        }

        public void UpdateInfo(string firstName, string lastName, string? email, string? phoneNumber, string? address = null)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required", nameof(lastName));

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email?.Trim();
            PhoneNumber = phoneNumber?.Trim();
            Address = address?.Trim();
        }
    }
}

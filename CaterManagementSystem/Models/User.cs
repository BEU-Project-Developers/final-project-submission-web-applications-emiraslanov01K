using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using CateringService.Models; // This assumes your models are in this namespace

namespace CateringService.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Phone]
        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(20)]
        public string Role { get; set; } = "user";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Testimonial> Testimonials { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<UserAuthority> UserAuthorities { get; set; }
    }
}
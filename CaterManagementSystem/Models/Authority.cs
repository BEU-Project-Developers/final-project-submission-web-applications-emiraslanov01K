using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CateringService.Models
{
    public class Authority
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<UserAuthority> UserAuthorities { get; set; }
    }
}
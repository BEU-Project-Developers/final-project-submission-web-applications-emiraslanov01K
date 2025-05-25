using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringService.Models
{
    public class UserAuthority
    {
        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Authority")]
        public int AuthorityId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; }
        public Authority Authority { get; set; }
    }
}
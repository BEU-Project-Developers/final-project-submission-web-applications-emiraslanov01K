// Models/Role.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Rol adı tələb olunur.")]
        [StringLength(50)]
        public string Name { get; set; } // Məsələn: "Admin", "User"

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
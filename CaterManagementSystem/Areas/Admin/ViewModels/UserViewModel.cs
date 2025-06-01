// Areas/Admin/ViewModels/UserViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.Areas.Admin.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string? ProfilePicturePath { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
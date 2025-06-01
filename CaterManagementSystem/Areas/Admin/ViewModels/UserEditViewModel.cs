// Areas/Admin/ViewModels/UserEditViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.Areas.Admin.ViewModels
{
    public class UserEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "İstifadəçi adı tələb olunur.")]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "E-poçt tələb olunur.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Tam ad tələb olunur.")]
        [StringLength(150)]
        public string FullName { get; set; }

        public bool EmailConfirmed { get; set; }

        // Rol seçimi üçün
        public List<string> UserRoles { get; set; } = new List<string>();
        public SelectList? AllRoles { get; set; } // Bütün mövcud rollar
        public List<string>? SelectedRoles { get; set; } // Seçilmiş rollar (POST üçün)
    }
}
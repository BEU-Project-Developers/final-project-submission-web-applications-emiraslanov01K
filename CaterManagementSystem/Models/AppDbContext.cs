
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using SchoolSystem.Models;

namespace CaterManagementSystem.Models // Namespace-i düzəltdik
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<About> About { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<GalleryImage> GalleryImages { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }


    }
}
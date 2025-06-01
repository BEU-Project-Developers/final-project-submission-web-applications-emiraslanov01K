using CaterManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CaterManagementSystem.Data 
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
        public DbSet<GalleryImage> GalleryImages { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Profession> Professions { get; set; } 
        public DbSet<Home> Homes { get; set; }
        public DbSet<Service> Services { get; set; } 
        public DbSet<ServiceDescription> ServiceDescriptions { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Booking> Bookings { get; set; }

     
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ... (digər konfiqurasiyalar) ...

            // User - UserDetails (One-to-One) yaratma
            
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserDetails) 
                .WithOne(ud => ud.User)    
                .HasForeignKey<UserDetails>(ud => ud.UserId); 
        }

    }

}
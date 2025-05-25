// Models/AppDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace CateringService.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Testimonial> Testimonials { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<CustomBooking> CustomBookings { get; set; }
        public DbSet<Authority> Authorities { get; set; }
        public DbSet<UserAuthority> UserAuthorities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure composite primary key for UserAuthority
            modelBuilder.Entity<UserAuthority>()
                .HasKey(ua => new { ua.UserId, ua.AuthorityId });

            // Configure one-to-one relationship between Booking and CustomBooking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.CustomBooking)
                .WithOne(cb => cb.Booking)
                .HasForeignKey<CustomBooking>(cb => cb.BookingId);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using QLKS_API.Models;

namespace QLKS_API.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceService> InvoiceServices { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<InvoiceLog> InvoiceLogs { get; set; }
        public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite keys
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<InvoiceService>()
                .HasKey(invServ => new { invServ.InvoiceId, invServ.ServiceId });

            // Default values
            modelBuilder.Entity<Room>()
                .Property(r => r.Status)
                .HasDefaultValue("available");

            modelBuilder.Entity<Booking>()
                .Property(b => b.Status)
                .HasDefaultValue("pending");

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Status)
                .HasDefaultValue("pending");

            // Relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany()
                .HasForeignKey(b => b.CustomerId);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Booking)
                .WithOne(b => b.Invoice)
                .HasForeignKey<Invoice>(i => i.BookingId);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Customer)
                .WithMany()
                .HasForeignKey(i => i.CustomerId);
        }
    }
}
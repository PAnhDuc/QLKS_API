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
                .HasConversion<string>(); // Đảm bảo dòng này có

            modelBuilder.Entity<Booking>()
                .Property(b => b.Status)
                .HasDefaultValue(BookingStatus.Pending); // hoặc .HasDefaultValue(0)

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Status)
                .HasDefaultValue(InvoiceStatus.Pending); // hoặc .HasDefaultValue(0)

            // Relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<Booking>()
                .Property(b => b.CustomerId)
                .HasColumnName("customer_id");

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .HasConstraintName("FK_Booking_Customer");

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Booking)
                .WithOne(b => b.Invoice)
                .HasForeignKey<Invoice>(i => i.BookingId);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Customer)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .Property(c => c.CustomerId)
                .HasColumnName("customer_id");
        }
    }
}
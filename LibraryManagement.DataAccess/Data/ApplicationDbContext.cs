using LibraryManagement.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DataAccess.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

    public DbSet<Book> Books { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<EndPointPermission> EndPointPermissions { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerBook> CustomerBooks { get; set; }
    public DbSet<APIStackTrace> APIStackTraces { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerBook>()
            .HasKey(cb => new { cb.CustomerId, cb.BookId });
    }
}

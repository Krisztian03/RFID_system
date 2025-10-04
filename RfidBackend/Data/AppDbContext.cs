using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;  
    public DbSet<AccessLog> AccessLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        builder.Entity<Employee>(entity => 
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RfidUid).IsUnique();
        });

        builder.Entity<AccessLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TimestampUtc).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Employee)
                .WithMany(u => u.AccessLogs)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<User>()
            .HasOne(u => u.Employee)
            .WithMany()
            .HasForeignKey(u => u.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
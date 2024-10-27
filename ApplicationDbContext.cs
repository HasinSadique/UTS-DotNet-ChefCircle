using Microsoft.EntityFrameworkCore;
using myfirstapi.Model;

public class ApplicationDbContext : DbContext
{
    // DbSet representing the Users table in the database
    public DbSet<User> Users { get; set; }

    // Constructor for the ApplicationDbContext that accepts DbContextOptions
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    // Override OnConfiguring method if not using Dependency Injection for configuration
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Example connection string for PostgreSQL
            optionsBuilder.UseNpgsql("Host=chef-circle-dotnet-project-chefcircle.f.aivencloud.com;Port=15473;Username=avnadmin;Password=AVNS_O7_liIuxjEB0i2ZhTMf;Database=ChefCircle;SSL Mode=Require;Trust Server Certificate=true");
        }
    }

    // Additional configurations can be added here
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the User entity and its table name
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<User>().HasKey(u => u.UID);
        
        // // Example of seeding initial data
        // modelBuilder.Entity<User>().HasData(
        //     new User { UID = 1, Fullname = "John Doe", Email = "john.doe@example.com", Phone = "1234567890", Address = "123 Elm Street", Password = "securepassword", Role = "Admin" },
        //     new User { UID = 2, Fullname = "Jane Smith", Email = "jane.smith@example.com", Phone = "9876543210", Address = "456 Oak Avenue", Password = "anotherpassword", Role = "User" }
        // );
    }
}

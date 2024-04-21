using Api.Classes;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nure.NET.Types;

namespace Api.Contexts;

public class Context : IdentityDbContext<AuthUser>
{
    public DbSet<Group> Groups { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Auditory> Auditories { get; set; }
    public Context() => Database.EnsureCreated();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        optionsBuilder.UseNpgsql(
            $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
            "Database=api; Username=postgres; Password=fastgame; Port=5432; Pooling=true;");
    }

    public Context(DbContextOptions<Context> options) : base(options) { }
}
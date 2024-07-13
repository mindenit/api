using Api.Classes;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nure.NET.Types;

namespace Api.Contexts;

public class Context : IdentityDbContext<AuthUser>
{
    public DbSet<ScheduleGroup> Groups { get; set; }
    public DbSet<ScheduleTeacher> Teachers { get; set; }
    public DbSet<ScheduleAuditory> Auditories { get; set; }
    public Context() => Database.EnsureCreated();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        optionsBuilder.UseNpgsql(
            $"Host=db;" +
            $"Database=api;" +
            $"Username=postgres;" +
            $"Password=password;" +
            $"Port=5433;");
        
    }
    
    public Context(DbContextOptions<Context> options) : base(options) { }
}

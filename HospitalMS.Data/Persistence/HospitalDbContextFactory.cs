using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HospitalMS.Data.Persistence;

public sealed class HospitalDbContextFactory : IDesignTimeDbContextFactory<HospitalDbContext>
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=hospitalms;Username=postgres;Password=postgres";

    public HospitalDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__postgres")
            ?? Environment.GetEnvironmentVariable("HOSPITALMS_POSTGRES_CONNECTION")
            ?? DefaultConnectionString;

        var optionsBuilder = new DbContextOptionsBuilder<HospitalDbContext>();
        optionsBuilder.UseNpgsql(connectionString,
            x => x.MigrationsAssembly(typeof(HospitalDbContext).Assembly.GetName().Name));

        return new HospitalDbContext(optionsBuilder.Options);
    }
}

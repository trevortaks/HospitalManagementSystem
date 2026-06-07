using HospitalMS.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Tests;

public sealed class IntegrationTestFixture : IDisposable
{
    public IntegrationTestFixture()
    {
        DatabaseName = TestFixtures.CreateDatabaseName();
        Options = TestFixtures.CreateInMemoryOptions(DatabaseName);
    }

    public string DatabaseName { get; }
    public DbContextOptions<HospitalDbContext> Options { get; }

    public HospitalDbContext CreateContext() => new(Options);

    public void Dispose() { }
}

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private IntegrationTestFixture? _fixture;

    protected IntegrationTestFixture Fixture => _fixture ?? throw new InvalidOperationException("Integration fixture has not been initialized.");
    protected HospitalDbContext Context { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        _fixture = new IntegrationTestFixture();
        Context = Fixture.CreateContext();

        await Context.Database.EnsureDeletedAsync();
        await Context.Database.EnsureCreatedAsync();
        await SeedAsync(Context);
    }

    public virtual async Task DisposeAsync()
    {
        if (Context is not null)
        {
            await Context.Database.EnsureDeletedAsync();
            await Context.DisposeAsync();
        }

        _fixture?.Dispose();
    }

    protected HospitalDbContext CreateContext() => Fixture.CreateContext();

    protected virtual Task SeedAsync(HospitalDbContext context) => Task.CompletedTask;
}

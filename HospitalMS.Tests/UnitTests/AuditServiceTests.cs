using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class AuditServiceTests : IntegrationTestBase
{
    private AuditService _service = null!;
    private Guid _userId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var user = TestFixtures.CreateUser(role: "Administrator");
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        _userId = user.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new AuditService(Context);
    }

    [Fact]
    public async Task LogAsync_PersistsEntry()
    {
        var request = new CreateAuditEntryRequest("Patient", AuditAction.Created,
            EntityId: Guid.NewGuid(), Details: "New patient registered",
            PerformedByUserId: _userId, PerformedByUsername: "admin");

        var result = await _service.LogAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Patient", result.EntityType);
        Assert.Equal(AuditAction.Created, result.Action);
        Assert.Equal("admin", result.PerformedByUsername);
        Assert.Equal(_userId, result.PerformedByUserId);
    }

    [Fact]
    public async Task LogAsync_WithEntityId_StoresEntityId()
    {
        var entityId = Guid.NewGuid();
        var request = new CreateAuditEntryRequest("Invoice", AuditAction.StatusChanged, EntityId: entityId);
        var result = await _service.LogAsync(request);

        Assert.Equal(entityId, result.EntityId);
    }

    [Fact]
    public async Task LogAsync_NullOptionalFields_Succeeds()
    {
        var request = new CreateAuditEntryRequest("User", AuditAction.LoggedIn);
        var result = await _service.LogAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Null(result.EntityId);
        Assert.Null(result.Details);
        Assert.Null(result.PerformedByUserId);
        Assert.Null(result.IpAddress);
    }

    [Fact]
    public async Task GetEntriesAsync_FiltersByEntityType()
    {
        await _service.LogAsync(new CreateAuditEntryRequest("Patient", AuditAction.Created));
        await _service.LogAsync(new CreateAuditEntryRequest("Invoice", AuditAction.Created));

        var entries = await _service.GetEntriesAsync(new AuditEntryFilter(EntityType: "Patient"));

        Assert.All(entries, e => Assert.Equal("Patient", e.EntityType));
        Assert.DoesNotContain(entries, e => e.EntityType == "Invoice");
    }

    [Fact]
    public async Task GetEntriesAsync_FiltersByAction()
    {
        await _service.LogAsync(new CreateAuditEntryRequest("Patient", AuditAction.Created));
        await _service.LogAsync(new CreateAuditEntryRequest("Patient", AuditAction.Updated));

        var entries = await _service.GetEntriesAsync(new AuditEntryFilter(Action: AuditAction.Created));

        Assert.All(entries, e => Assert.Equal(AuditAction.Created, e.Action));
    }

    [Fact]
    public async Task GetEntriesAsync_FiltersByUserId()
    {
        var otherId = Guid.NewGuid();
        await _service.LogAsync(new CreateAuditEntryRequest("Patient", AuditAction.Viewed, PerformedByUserId: _userId));
        await _service.LogAsync(new CreateAuditEntryRequest("Patient", AuditAction.Viewed, PerformedByUserId: otherId));

        var entries = await _service.GetEntriesAsync(new AuditEntryFilter(PerformedByUserId: _userId));

        Assert.All(entries, e => Assert.Equal(_userId, e.PerformedByUserId));
    }

    [Fact]
    public async Task GetEntriesAsync_RespectsLimit()
    {
        for (var i = 0; i < 10; i++)
            await _service.LogAsync(new CreateAuditEntryRequest("Patient", AuditAction.Viewed));

        var entries = await _service.GetEntriesAsync(new AuditEntryFilter(), limit: 3);

        Assert.Equal(3, entries.Count);
    }

    [Fact]
    public async Task GetEntriesAsync_OrderedDescendingByDate()
    {
        await _service.LogAsync(new CreateAuditEntryRequest("Patient", AuditAction.Created));
        await _service.LogAsync(new CreateAuditEntryRequest("Patient", AuditAction.Updated));
        await _service.LogAsync(new CreateAuditEntryRequest("Patient", AuditAction.Deleted));

        var entries = await _service.GetEntriesAsync(new AuditEntryFilter());

        Assert.True(entries[0].PerformedAtUtc >= entries[^1].PerformedAtUtc);
    }

    [Fact]
    public async Task GetEntriesByEntityAsync_ReturnsEntityHistory()
    {
        var entityId = Guid.NewGuid();
        var otherId  = Guid.NewGuid();

        await _service.LogAsync(new CreateAuditEntryRequest("Invoice", AuditAction.Created, EntityId: entityId));
        await _service.LogAsync(new CreateAuditEntryRequest("Invoice", AuditAction.Updated, EntityId: entityId));
        await _service.LogAsync(new CreateAuditEntryRequest("Invoice", AuditAction.Created, EntityId: otherId));

        var history = await _service.GetEntriesByEntityAsync("Invoice", entityId);

        Assert.Equal(2, history.Count);
        Assert.All(history, e => Assert.Equal(entityId, e.EntityId));
    }

    [Fact]
    public async Task GetEntriesAsync_FiltersByDateRange()
    {
        await _service.LogAsync(new CreateAuditEntryRequest("User", AuditAction.LoggedIn));

        var now = DateTime.UtcNow;
        var entries = await _service.GetEntriesAsync(new AuditEntryFilter(
            From: now.AddMinutes(-5),
            To:   now.AddMinutes(5)));

        Assert.NotEmpty(entries);
    }
}

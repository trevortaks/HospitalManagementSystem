using HospitalMS.Business.Repositories;
using HospitalMS.Business.Services;
using Moq;

namespace HospitalMS.Tests;

public abstract class UnitTestBase : IAsyncLifetime
{
    private readonly List<Mock> _trackedMocks = [];

    protected MockRepository MockRepository { get; } = new(MockBehavior.Strict);

    public Task InitializeAsync() => OnInitializeAsync();

    public async Task DisposeAsync()
    {
        await OnDisposeAsync();
        _trackedMocks.Clear();
    }

    protected virtual Task OnInitializeAsync() => Task.CompletedTask;

    protected virtual Task OnDisposeAsync() => Task.CompletedTask;

    protected Mock<T> CreateMock<T>(MockBehavior behavior = MockBehavior.Strict)
        where T : class
    {
        var mock = behavior == MockBehavior.Strict
            ? MockRepository.Create<T>()
            : new Mock<T>(behavior);

        return Track(mock);
    }

    protected Mock<IRepository<TEntity>> CreateRepositoryMock<TEntity>()
        where TEntity : class => Track(MockFactories.CreateRepositoryMock<TEntity>());

    protected Mock<ISystemStatusService> CreateSystemStatusServiceMock(string? status = null)
        => Track(MockFactories.CreateSystemStatusServiceMock(status));

    protected Mock<INotificationService> CreateNotificationServiceMock()
        => Track(MockFactories.CreateNotificationServiceMock());

    protected void VerifyAllMocks()
    {
        foreach (var mock in _trackedMocks)
        {
            mock.Verify();
        }
    }

    private Mock<T> Track<T>(Mock<T> mock)
        where T : class
    {
        _trackedMocks.Add(mock);
        return mock;
    }
}

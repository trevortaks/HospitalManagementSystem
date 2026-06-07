using HospitalMS.Business.Repositories;
using HospitalMS.Business.Services;
using Moq;

namespace HospitalMS.Tests;

public interface INotificationService
{
    Task NotifyAsync(string message, CancellationToken cancellationToken = default);
}

public static class MockFactories
{
    public static Mock<IRepository<TEntity>> CreateRepositoryMock<TEntity>()
        where TEntity : class => new(MockBehavior.Strict);

    public static Mock<ISystemStatusService> CreateSystemStatusServiceMock(string? status = null)
    {
        var mock = new Mock<ISystemStatusService>(MockBehavior.Strict);

        if (status is not null)
        {
            mock.Setup(service => service.GetStatus()).Returns(status);
        }

        return mock;
    }

    public static Mock<INotificationService> CreateNotificationServiceMock()
    {
        var mock = new Mock<INotificationService>(MockBehavior.Strict);
        mock.Setup(service => service.NotifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }
}

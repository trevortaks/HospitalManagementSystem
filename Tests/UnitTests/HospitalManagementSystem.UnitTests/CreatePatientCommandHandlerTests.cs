using AutoMapper;
using FluentAssertions;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.Common.Mappings;
using HospitalManagementSystem.Application.Features.Patients.Commands.Create;
using HospitalManagementSystem.Domain.Entities;
using HospitalManagementSystem.Application.Common.Exceptions;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HospitalManagementSystem.UnitTests;

public class CreatePatientCommandHandlerTests
{
    private readonly IMapper _mapper;

    public CreatePatientCommandHandlerTests()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), new LoggerFactory());
        _mapper = configuration.CreateMapper();
    }

    private IApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(x => x.UserName).Returns("test-user-id");

        return new TestApplicationDbContext(options, mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreatePatient_WhenNewPatient()
    {
        var context = GetDbContext();
        var handler = new CreatePatientCommandHandler(context, _mapper);

        var command = new CreatePatientCommand
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M",
            ContactNumber = "1234567890",
            Address = "123 Main St"
        };

        var id = await handler.Handle(command, CancellationToken.None);

        var patient = await context.Patients.FindAsync(id);
        patient.Should().NotBeNull();
        patient!.PatientId.Should().Be(id);
    }

    [Fact]
    public async Task Handle_ShouldThrowDuplicateEntityException_WhenPatientExists()
    {
        var context = GetDbContext();
        context.Patients.Add(new Patient
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M",
            ContactNumber = "1234567890",
            Address = "123 Main St",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var handler = new CreatePatientCommandHandler(context, _mapper);

        var command = new CreatePatientCommand
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M",
            ContactNumber = "1234567890",
            Address = "123 Main St"
        };

        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    private class TestApplicationDbContext : ApplicationDbContext, IApplicationDbContext
    {
        public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService) : base(options, currentUserService) { }

        public void MarkAsModified<TEntity>(TEntity entity) where TEntity : class =>
            Entry(entity).State = EntityState.Modified;

        public void MarkAsDeleted<TEntity>(TEntity entity) where TEntity : class =>
            Entry(entity).State = EntityState.Deleted;
    }
}

using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class HRServiceTests : IntegrationTestBase
{
    private HRService _service = null!;
    private Guid _userId;
    private Guid _hrManagerId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var user      = TestFixtures.CreateUser(role: "Doctor");
        var hrManager = TestFixtures.CreateUser(role: "HRManager");
        await context.Users.AddRangeAsync(user, hrManager);
        await context.SaveChangesAsync();
        _userId      = user.Id;
        _hrManagerId = hrManager.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new HRService(Context);
    }

    // ── Departments ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateDepartmentAsync_PersistsDepartment()
    {
        var req    = new CreateDepartmentRequest("Cardiology", "Heart care unit");
        var result = await _service.CreateDepartmentAsync(req);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Cardiology", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateDepartmentAsync_Throws_WhenDuplicateName()
    {
        await _service.CreateDepartmentAsync(new CreateDepartmentRequest("Radiology"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateDepartmentAsync(new CreateDepartmentRequest("Radiology")));
    }

    [Fact]
    public async Task ToggleDepartmentActiveAsync_FlipsFlag()
    {
        var dept   = await _service.CreateDepartmentAsync(new CreateDepartmentRequest("Oncology"));
        var result = await _service.ToggleDepartmentActiveAsync(dept.Id);

        Assert.False(result.IsActive);
    }

    // ── Employees ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateEmployeeRecordAsync_PersistsRecord()
    {
        var req = new CreateEmployeeRecordRequest(
            _userId, "Senior Physician", EmploymentType.FullTime,
            DateTime.UtcNow.AddYears(-2), 80000m);

        var result = await _service.CreateEmployeeRecordAsync(req);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Senior Physician", result.JobTitle);
        Assert.Equal(EmployeeStatus.Active, result.Status);
        Assert.StartsWith("EMP-", result.EmployeeNumber);
    }

    [Fact]
    public async Task CreateEmployeeRecordAsync_Throws_WhenDuplicateUser()
    {
        var req = new CreateEmployeeRecordRequest(
            _userId, "Physician", EmploymentType.FullTime, DateTime.UtcNow, 50000m);
        await _service.CreateEmployeeRecordAsync(req);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateEmployeeRecordAsync(req));
    }

    [Fact]
    public async Task UpdateEmployeeStatusAsync_TerminatesEmployee()
    {
        var req = new CreateEmployeeRecordRequest(
            _userId, "Nurse", EmploymentType.PartTime, DateTime.UtcNow.AddMonths(-6), 40000m);
        var emp = await _service.CreateEmployeeRecordAsync(req);

        var result = await _service.UpdateEmployeeStatusAsync(
            emp.Id, new UpdateEmployeeStatusRequest(EmployeeStatus.Terminated));

        Assert.Equal(EmployeeStatus.Terminated, result.Status);
        Assert.NotNull(result.TerminatedAtUtc);
    }

    // ── Leave Requests ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateLeaveRequestAsync_PersistsRequest()
    {
        var emp = await _service.CreateEmployeeRecordAsync(new CreateEmployeeRecordRequest(
            _userId, "Physician", EmploymentType.FullTime, DateTime.UtcNow.AddYears(-1), 70000m));

        var req    = new CreateLeaveRequestRequest(emp.Id, LeaveType.Annual,
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(10), "Holiday");
        var result = await _service.CreateLeaveRequestAsync(req);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(LeaveRequestStatus.Pending, result.Status);
        Assert.Equal(6, result.DurationDays);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_Throws_WhenEndBeforeStart()
    {
        var emp = await _service.CreateEmployeeRecordAsync(new CreateEmployeeRecordRequest(
            _userId, "Physician", EmploymentType.FullTime, DateTime.UtcNow.AddYears(-1), 70000m));

        var req = new CreateLeaveRequestRequest(emp.Id, LeaveType.Sick,
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(2));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateLeaveRequestAsync(req));
    }

    [Fact]
    public async Task ReviewLeaveRequestAsync_ApprovesRequest()
    {
        var emp = await _service.CreateEmployeeRecordAsync(new CreateEmployeeRecordRequest(
            _userId, "Physician", EmploymentType.FullTime, DateTime.UtcNow.AddYears(-1), 70000m));
        var leave = await _service.CreateLeaveRequestAsync(new CreateLeaveRequestRequest(
            emp.Id, LeaveType.Annual, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(3)));

        var result = await _service.ReviewLeaveRequestAsync(
            leave.Id, _hrManagerId, new ReviewLeaveRequestRequest(true));

        Assert.Equal(LeaveRequestStatus.Approved, result.Status);
        Assert.NotNull(result.ReviewedAtUtc);
    }

    [Fact]
    public async Task ReviewLeaveRequestAsync_Throws_WhenAlreadyReviewed()
    {
        var emp = await _service.CreateEmployeeRecordAsync(new CreateEmployeeRecordRequest(
            _userId, "Physician", EmploymentType.FullTime, DateTime.UtcNow.AddYears(-1), 70000m));
        var leave = await _service.CreateLeaveRequestAsync(new CreateLeaveRequestRequest(
            emp.Id, LeaveType.Sick, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)));
        await _service.ReviewLeaveRequestAsync(leave.Id, _hrManagerId,
            new ReviewLeaveRequestRequest(false));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ReviewLeaveRequestAsync(leave.Id, _hrManagerId,
                new ReviewLeaveRequestRequest(true)));
    }

    [Fact]
    public async Task CancelLeaveRequestAsync_SetsStatusCancelled()
    {
        var emp = await _service.CreateEmployeeRecordAsync(new CreateEmployeeRecordRequest(
            _userId, "Physician", EmploymentType.FullTime, DateTime.UtcNow.AddYears(-1), 70000m));
        var leave = await _service.CreateLeaveRequestAsync(new CreateLeaveRequestRequest(
            emp.Id, LeaveType.Emergency, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1)));

        var result = await _service.CancelLeaveRequestAsync(leave.Id);
        Assert.Equal(LeaveRequestStatus.Cancelled, result.Status);
    }

    // ── Summary ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSummaryAsync_ReturnsAggregates()
    {
        await _service.CreateEmployeeRecordAsync(new CreateEmployeeRecordRequest(
            _userId, "Physician", EmploymentType.FullTime, DateTime.UtcNow.AddYears(-1), 70000m));

        var summary = await _service.GetSummaryAsync();
        Assert.Equal(1, summary.TotalEmployees);
        Assert.Equal(1, summary.ActiveEmployees);
    }
}

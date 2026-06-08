using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class HRService(HospitalDbContext dbContext) : IHRService
{
    // ── Departments ─────────────────────────────────────────────────────────

    public async Task<DepartmentResponse> CreateDepartmentAsync(
        CreateDepartmentRequest request, CancellationToken ct = default)
    {
        var exists = await dbContext.Departments
            .AnyAsync(d => d.Name == request.Name, ct);
        if (exists)
            throw new InvalidOperationException($"Department '{request.Name}' already exists.");

        var dept = new Department
        {
            Name        = request.Name,
            Description = request.Description,
            HeadUserId  = request.HeadUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Departments.Add(dept);
        await dbContext.SaveChangesAsync(ct);
        return await GetDeptResponse(dept.Id, ct);
    }

    public async Task<IReadOnlyList<DepartmentResponse>> GetDepartmentsAsync(CancellationToken ct = default)
    {
        var depts = await dbContext.Departments
            .Include(d => d.HeadUser)
            .Include(d => d.Employees)
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

        return depts.Select(ToDeptResponse).ToArray();
    }

    public async Task<DepartmentResponse> ToggleDepartmentActiveAsync(Guid id, CancellationToken ct = default)
    {
        var dept = await dbContext.Departments.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Department {id} not found.");
        dept.IsActive = !dept.IsActive;
        await dbContext.SaveChangesAsync(ct);
        return await GetDeptResponse(id, ct);
    }

    // ── Employee Records ────────────────────────────────────────────────────

    public async Task<EmployeeRecordResponse> CreateEmployeeRecordAsync(
        CreateEmployeeRecordRequest request, CancellationToken ct = default)
    {
        var exists = await dbContext.EmployeeRecords
            .AnyAsync(e => e.UserId == request.UserId, ct);
        if (exists)
            throw new InvalidOperationException("Employee record already exists for this user.");

        var empNum = request.EmployeeNumber ?? await GenerateEmployeeNumberAsync(ct);

        var record = new EmployeeRecord
        {
            UserId         = request.UserId,
            DepartmentId   = request.DepartmentId,
            EmployeeNumber = empNum,
            JobTitle       = request.JobTitle,
            EmploymentType = request.EmploymentType,
            Status         = EmployeeStatus.Active,
            HiredAtUtc     = request.HiredAtUtc.ToUniversalTime(),
            Salary         = request.Salary,
            Notes          = request.Notes,
            CreatedAtUtc   = DateTime.UtcNow
        };

        dbContext.EmployeeRecords.Add(record);
        await dbContext.SaveChangesAsync(ct);
        return await GetEmployeeByIdAsync(record.Id, ct);
    }

    public async Task<IReadOnlyList<EmployeeRecordResponse>> GetEmployeesAsync(
        string? status = null, Guid? departmentId = null, CancellationToken ct = default)
    {
        var query = dbContext.EmployeeRecords
            .Include(e => e.User)
            .Include(e => e.Department)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(e => e.Status == status);
        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        var list = await query.OrderBy(e => e.EmployeeNumber).ToListAsync(ct);
        return list.Select(ToEmpResponse).ToArray();
    }

    public async Task<EmployeeRecordResponse> GetEmployeeByIdAsync(Guid id, CancellationToken ct = default)
    {
        var record = await dbContext.EmployeeRecords
            .Include(e => e.User)
            .Include(e => e.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new KeyNotFoundException($"Employee record {id} not found.");
        return ToEmpResponse(record);
    }

    public async Task<EmployeeRecordResponse> UpdateEmployeeStatusAsync(
        Guid id, UpdateEmployeeStatusRequest request, CancellationToken ct = default)
    {
        var record = await dbContext.EmployeeRecords.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Employee record {id} not found.");

        record.Status = request.Status;
        if (request.Status == EmployeeStatus.Terminated)
            record.TerminatedAtUtc = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(request.Notes))
            record.Notes = request.Notes;

        await dbContext.SaveChangesAsync(ct);
        return await GetEmployeeByIdAsync(id, ct);
    }

    // ── Leave Requests ──────────────────────────────────────────────────────

    public async Task<LeaveRequestResponse> CreateLeaveRequestAsync(
        CreateLeaveRequestRequest request, CancellationToken ct = default)
    {
        if (request.EndDate < request.StartDate)
            throw new ArgumentException("EndDate must be on or after StartDate.");

        var leave = new LeaveRequest
        {
            EmployeeRecordId = request.EmployeeRecordId,
            LeaveType        = request.LeaveType,
            StartDate        = request.StartDate.ToUniversalTime(),
            EndDate          = request.EndDate.ToUniversalTime(),
            Reason           = request.Reason,
            Status           = LeaveRequestStatus.Pending,
            RequestedAtUtc   = DateTime.UtcNow
        };

        dbContext.LeaveRequests.Add(leave);
        await dbContext.SaveChangesAsync(ct);
        return await GetLeaveResponse(leave.Id, ct);
    }

    public async Task<IReadOnlyList<LeaveRequestResponse>> GetLeaveRequestsAsync(
        string? status = null, Guid? employeeRecordId = null, CancellationToken ct = default)
    {
        var query = dbContext.LeaveRequests
            .Include(l => l.EmployeeRecord).ThenInclude(e => e.User)
            .Include(l => l.ReviewedByUser)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(l => l.Status == status);
        if (employeeRecordId.HasValue)
            query = query.Where(l => l.EmployeeRecordId == employeeRecordId.Value);

        var list = await query.OrderByDescending(l => l.RequestedAtUtc).ToListAsync(ct);
        return list.Select(ToLeaveResponse).ToArray();
    }

    public async Task<LeaveRequestResponse> ReviewLeaveRequestAsync(
        Guid id, Guid reviewerUserId, ReviewLeaveRequestRequest request, CancellationToken ct = default)
    {
        var leave = await dbContext.LeaveRequests.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Leave request {id} not found.");

        if (leave.Status != LeaveRequestStatus.Pending)
            throw new InvalidOperationException($"Leave request is already {leave.Status}.");

        leave.Status             = request.Approved ? LeaveRequestStatus.Approved : LeaveRequestStatus.Rejected;
        leave.ReviewedByUserId   = reviewerUserId;
        leave.ReviewNotes        = request.ReviewNotes;
        leave.ReviewedAtUtc      = DateTime.UtcNow;

        if (request.Approved)
        {
            var emp = await dbContext.EmployeeRecords.FindAsync([leave.EmployeeRecordId], ct);
            if (emp is not null) emp.Status = EmployeeStatus.OnLeave;
        }

        await dbContext.SaveChangesAsync(ct);
        return await GetLeaveResponse(id, ct);
    }

    public async Task<LeaveRequestResponse> CancelLeaveRequestAsync(Guid id, CancellationToken ct = default)
    {
        var leave = await dbContext.LeaveRequests.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Leave request {id} not found.");

        if (leave.Status == LeaveRequestStatus.Cancelled)
            throw new InvalidOperationException("Leave request is already cancelled.");

        leave.Status = LeaveRequestStatus.Cancelled;
        await dbContext.SaveChangesAsync(ct);
        return await GetLeaveResponse(id, ct);
    }

    // ── Summary ──────────────────────────────────────────────────────────────

    public async Task<HRSummary> GetSummaryAsync(CancellationToken ct = default)
    {
        var employees = await dbContext.EmployeeRecords
            .Include(e => e.Department)
            .AsNoTracking()
            .ToListAsync(ct);

        var totalDepts    = await dbContext.Departments.CountAsync(d => d.IsActive, ct);
        var pendingLeaves = await dbContext.LeaveRequests
            .CountAsync(l => l.Status == LeaveRequestStatus.Pending, ct);

        var byDept = employees
            .GroupBy(e => e.Department?.Name ?? "Unassigned")
            .Select(g => new LabelCount(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToArray();

        var byType = employees
            .GroupBy(e => e.EmploymentType)
            .Select(g => new LabelCount(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToArray();

        return new HRSummary(
            employees.Count,
            employees.Count(e => e.Status == EmployeeStatus.Active),
            employees.Count(e => e.Status == EmployeeStatus.OnLeave),
            pendingLeaves, totalDepts, byDept, byType);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private async Task<string> GenerateEmployeeNumberAsync(CancellationToken ct)
    {
        var count = await dbContext.EmployeeRecords.CountAsync(ct);
        return $"EMP-{(count + 1):D5}";
    }

    private async Task<DepartmentResponse> GetDeptResponse(Guid id, CancellationToken ct)
    {
        var dept = await dbContext.Departments
            .Include(d => d.HeadUser)
            .Include(d => d.Employees)
            .AsNoTracking()
            .FirstAsync(d => d.Id == id, ct);
        return ToDeptResponse(dept);
    }

    private async Task<LeaveRequestResponse> GetLeaveResponse(Guid id, CancellationToken ct)
    {
        var leave = await dbContext.LeaveRequests
            .Include(l => l.EmployeeRecord).ThenInclude(e => e.User)
            .Include(l => l.ReviewedByUser)
            .AsNoTracking()
            .FirstAsync(l => l.Id == id, ct);
        return ToLeaveResponse(leave);
    }

    private static DepartmentResponse ToDeptResponse(Department d) => new(
        d.Id, d.Name, d.Description,
        d.HeadUserId, d.HeadUser?.Username,
        d.IsActive, d.Employees.Count, d.CreatedAtUtc);

    private static EmployeeRecordResponse ToEmpResponse(EmployeeRecord e) => new(
        e.Id, e.UserId, e.User?.Username ?? string.Empty,
        e.User is null ? string.Empty : $"{e.User.Username}",
        e.User?.Email ?? string.Empty,
        e.DepartmentId, e.Department?.Name,
        e.EmployeeNumber, e.JobTitle, e.EmploymentType, e.Status,
        e.HiredAtUtc, e.TerminatedAtUtc, e.Salary, e.Notes, e.CreatedAtUtc);

    private static LeaveRequestResponse ToLeaveResponse(LeaveRequest l)
    {
        var days = (int)(l.EndDate - l.StartDate).TotalDays + 1;
        var name = l.EmployeeRecord?.User is null
            ? string.Empty
            : l.EmployeeRecord.User.Username;
        return new(
            l.Id, l.EmployeeRecordId, name,
            l.LeaveType, l.StartDate, l.EndDate, days,
            l.Reason, l.Status,
            l.ReviewedByUserId, l.ReviewedByUser?.Username,
            l.ReviewNotes, l.RequestedAtUtc, l.ReviewedAtUtc);
    }
}

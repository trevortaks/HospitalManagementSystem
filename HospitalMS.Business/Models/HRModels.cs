namespace HospitalMS.Business.Models;

public record DepartmentResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? HeadUserId,
    string? HeadUsername,
    bool IsActive,
    int EmployeeCount,
    DateTime CreatedAtUtc);

public record CreateDepartmentRequest(string Name, string? Description = null, Guid? HeadUserId = null);

public record EmployeeRecordResponse(
    Guid Id,
    Guid UserId,
    string Username,
    string FullName,
    string Email,
    Guid? DepartmentId,
    string? DepartmentName,
    string EmployeeNumber,
    string JobTitle,
    string EmploymentType,
    string Status,
    DateTime HiredAtUtc,
    DateTime? TerminatedAtUtc,
    decimal Salary,
    string? Notes,
    DateTime CreatedAtUtc);

public record CreateEmployeeRecordRequest(
    Guid UserId,
    string JobTitle,
    string EmploymentType,
    DateTime HiredAtUtc,
    decimal Salary,
    Guid? DepartmentId = null,
    string? EmployeeNumber = null,
    string? Notes = null);

public record UpdateEmployeeStatusRequest(string Status, string? Notes = null);

public record LeaveRequestResponse(
    Guid Id,
    Guid EmployeeRecordId,
    string EmployeeName,
    string LeaveType,
    DateTime StartDate,
    DateTime EndDate,
    int DurationDays,
    string? Reason,
    string Status,
    Guid? ReviewedByUserId,
    string? ReviewedByUsername,
    string? ReviewNotes,
    DateTime RequestedAtUtc,
    DateTime? ReviewedAtUtc);

public record CreateLeaveRequestRequest(
    Guid EmployeeRecordId,
    string LeaveType,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason = null);

public record ReviewLeaveRequestRequest(bool Approved, string? ReviewNotes = null);

public record HRSummary(
    int TotalEmployees,
    int ActiveEmployees,
    int OnLeaveEmployees,
    int PendingLeaveRequests,
    int TotalDepartments,
    IReadOnlyList<LabelCount> EmployeesByDepartment,
    IReadOnlyList<LabelCount> EmployeesByType);

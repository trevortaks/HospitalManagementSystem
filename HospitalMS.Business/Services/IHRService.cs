using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IHRService
{
    Task<DepartmentResponse> CreateDepartmentAsync(CreateDepartmentRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<DepartmentResponse>> GetDepartmentsAsync(CancellationToken ct = default);
    Task<DepartmentResponse> ToggleDepartmentActiveAsync(Guid id, CancellationToken ct = default);

    Task<EmployeeRecordResponse> CreateEmployeeRecordAsync(CreateEmployeeRecordRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeRecordResponse>> GetEmployeesAsync(string? status = null, Guid? departmentId = null, Guid? userId = null, CancellationToken ct = default);
    Task<EmployeeRecordResponse> GetEmployeeByIdAsync(Guid id, CancellationToken ct = default);
    Task<EmployeeRecordResponse> UpdateEmployeeStatusAsync(Guid id, UpdateEmployeeStatusRequest request, CancellationToken ct = default);

    Task<LeaveRequestResponse> CreateLeaveRequestAsync(CreateLeaveRequestRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequestResponse>> GetLeaveRequestsAsync(string? status = null, Guid? employeeRecordId = null, CancellationToken ct = default);
    Task<LeaveRequestResponse> ReviewLeaveRequestAsync(Guid id, Guid reviewerUserId, ReviewLeaveRequestRequest request, CancellationToken ct = default);
    Task<LeaveRequestResponse> CancelLeaveRequestAsync(Guid id, CancellationToken ct = default);

    Task<HRSummary> GetSummaryAsync(CancellationToken ct = default);
}

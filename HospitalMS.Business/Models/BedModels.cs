namespace HospitalMS.Business.Models;

public record WardResponse(
    Guid Id,
    string Name,
    string WardType,
    int TotalBeds,
    int FloorNumber,
    bool IsActive,
    DateTime CreatedAtUtc,
    int OccupiedBeds,
    int AvailableBeds);

public record CreateWardRequest(
    string Name,
    string WardType,
    int TotalBeds,
    int FloorNumber = 1);

public record UpdateWardRequest(
    string Name,
    string WardType,
    int TotalBeds,
    int FloorNumber);

public record BedResponse(
    Guid Id,
    Guid WardId,
    string WardName,
    string BedNumber,
    string BedType,
    string Status,
    bool IsActive,
    DateTime CreatedAtUtc);

public record CreateBedRequest(
    Guid WardId,
    string BedNumber,
    string BedType = "Standard");

public record BedAllocationResponse(
    Guid Id,
    Guid BedId,
    string BedNumber,
    string WardName,
    Guid PatientId,
    string PatientName,
    string MedicalRecordNumber,
    Guid? EncounterId,
    string AdmittedByUsername,
    DateTime AdmittedAtUtc,
    DateTime? DischargedAtUtc,
    string? DischargeReason,
    string? DischargedByUsername,
    Guid? TransferredToBedId);

public record AllocateBedRequest(
    Guid BedId,
    Guid PatientId,
    Guid AdmittedByUserId,
    Guid? EncounterId = null);

public record DischargePatientRequest(
    Guid AllocationId,
    Guid DischargedByUserId,
    string? DischargeReason = null,
    Guid? TransferToBedId = null);

public record BedAvailabilityResponse(
    int TotalBeds,
    int OccupiedBeds,
    int AvailableBeds,
    int OutOfServiceBeds,
    double OccupancyRate);

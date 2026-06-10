using HospitalMS.Business.Models;

namespace HospitalMS.Web.Models;

public sealed class PatientDetailViewModel
{
    public PatientResponse Patient { get; init; } = null!;
    public IReadOnlyList<AppointmentResponse> Appointments { get; init; } = [];
    public IReadOnlyList<EncounterResponse> Encounters { get; init; } = [];
    public IReadOnlyList<PrescriptionResponse> Prescriptions { get; init; } = [];
    public IReadOnlyList<LabOrderResponse> LabOrders { get; init; } = [];
    public IReadOnlyList<ImagingRequestResponse> ImagingRequests { get; init; } = [];
    public IReadOnlyList<InvoiceResponse> Invoices { get; init; } = [];
}

using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class ImagingService(HospitalDbContext dbContext) : IImagingService
{
    public async Task<IReadOnlyList<ImagingRequestResponse>> GetAllRequestsAsync(
        Guid? patientId = null, Guid? encounterId = null, string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.ImagingRequests
            .AsNoTracking()
            .Include(r => r.Patient)
            .Include(r => r.RequestedByUser)
            .Include(r => r.Report).ThenInclude(rep => rep!.RadiologyUser)
            .AsQueryable();

        if (patientId.HasValue)   query = query.Where(r => r.PatientId == patientId.Value);
        if (encounterId.HasValue) query = query.Where(r => r.EncounterId == encounterId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(r => r.Status == status);

        var requests = await query.OrderByDescending(r => r.RequestedAtUtc).ToListAsync(cancellationToken);
        return requests.Select(ToResponse).ToArray();
    }

    public async Task<ImagingRequestResponse?> GetRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await LoadRequestAsync(id, cancellationToken);
        return request is null ? null : ToResponse(request);
    }

    public async Task<ImagingRequestResponse> CreateRequestAsync(CreateImagingRequestRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new ImagingRequest
        {
            EncounterId = request.EncounterId,
            PatientId = request.PatientId,
            RequestedByUserId = request.RequestedByUserId,
            Modality = request.Modality,
            BodyPart = request.BodyPart,
            ClinicalIndication = request.ClinicalIndication,
            Priority = request.Priority
        };
        dbContext.ImagingRequests.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(await LoadRequestAsync(entity.Id, cancellationToken)!);
    }

    public async Task<ImagingRequestResponse> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.ImagingRequests.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Imaging request '{id}' not found.");

        if (request.Status == ImagingRequestStatus.Cancelled)
            throw new InvalidOperationException("Cannot update status of a cancelled request.");

        request.Status = status;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(await LoadRequestAsync(id, cancellationToken)!);
    }

    public async Task<ImagingRequestResponse> CancelRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.ImagingRequests.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Imaging request '{id}' not found.");

        if (request.Status == ImagingRequestStatus.Reported)
            throw new InvalidOperationException("Cannot cancel a request that has already been reported.");

        request.Status = ImagingRequestStatus.Cancelled;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(await LoadRequestAsync(id, cancellationToken)!);
    }

    public async Task<ImagingRequestResponse> CreateReportAsync(Guid requestId, CreateImagingReportRequest request, CancellationToken cancellationToken = default)
    {
        var imagingRequest = await dbContext.ImagingRequests.FindAsync([requestId], cancellationToken)
            ?? throw new KeyNotFoundException($"Imaging request '{requestId}' not found.");

        if (imagingRequest.Status == ImagingRequestStatus.Cancelled)
            throw new InvalidOperationException("Cannot report on a cancelled imaging request.");

        var existingReport = await dbContext.ImagingReports
            .FirstOrDefaultAsync(r => r.RequestId == requestId, cancellationToken);
        if (existingReport is not null)
            throw new InvalidOperationException("A report already exists for this imaging request.");

        var report = new ImagingReport
        {
            RequestId = requestId,
            RadiologyUserId = request.RadiologyUserId,
            ReportText = request.ReportText,
            Impression = request.Impression,
            AttachmentPath = request.AttachmentPath
        };
        dbContext.ImagingReports.Add(report);

        imagingRequest.Status = ImagingRequestStatus.Reported;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(await LoadRequestAsync(requestId, cancellationToken)!);
    }

    private async Task<ImagingRequest?> LoadRequestAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.ImagingRequests
            .AsNoTracking()
            .Include(r => r.Patient)
            .Include(r => r.RequestedByUser)
            .Include(r => r.Report).ThenInclude(rep => rep!.RadiologyUser)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    private static ImagingRequestResponse ToResponse(ImagingRequest r) => new(
        r.Id, r.EncounterId,
        r.PatientId, $"{r.Patient.FirstName} {r.Patient.LastName}", r.Patient.MedicalRecordNumber,
        r.RequestedByUserId, r.RequestedByUser.Username,
        r.Modality, r.BodyPart, r.ClinicalIndication, r.Priority, r.Status,
        r.RequestedAtUtc,
        r.Report is null ? null : new ImagingReportResponse(
            r.Report.Id, r.Report.RequestId, r.Report.RadiologyUserId,
            r.Report.RadiologyUser.Username,
            r.Report.ReportText, r.Report.Impression,
            r.Report.ReportedAtUtc, r.Report.AttachmentPath));
}

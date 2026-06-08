using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class InsuranceService(HospitalDbContext dbContext) : IInsuranceService
{
    // ── Providers ────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<InsuranceProviderResponse>> GetAllProvidersAsync(bool? activeOnly = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.InsuranceProviders.AsNoTracking().AsQueryable();
        if (activeOnly == true) query = query.Where(p => p.IsActive);
        var providers = await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);
        return providers.Select(ToProviderResponse).ToArray();
    }

    public async Task<InsuranceProviderResponse?> GetProviderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await dbContext.InsuranceProviders.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return provider is null ? null : ToProviderResponse(provider);
    }

    public async Task<InsuranceProviderResponse> CreateProviderAsync(CreateInsuranceProviderRequest request, CancellationToken cancellationToken = default)
    {
        var provider = new InsuranceProvider
        {
            Name = request.Name,
            ContactPhone = request.ContactPhone,
            ContactEmail = request.ContactEmail,
            Address = request.Address
        };
        dbContext.InsuranceProviders.Add(provider);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToProviderResponse(provider);
    }

    public async Task<InsuranceProviderResponse> ToggleProviderActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await dbContext.InsuranceProviders.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Insurance provider '{id}' not found.");
        provider.IsActive = !provider.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToProviderResponse(provider);
    }

    // ── Patient Insurance ────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PatientInsuranceResponse>> GetPatientInsuranceAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.PatientInsurances
            .AsNoTracking()
            .Include(pi => pi.Provider)
            .Where(pi => pi.PatientId == patientId)
            .OrderByDescending(pi => pi.IsPrimary)
            .ThenBy(pi => pi.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        return records.Select(ToPatientInsuranceResponse).ToArray();
    }

    public async Task<PatientInsuranceResponse> AddPatientInsuranceAsync(AddPatientInsuranceRequest request, CancellationToken cancellationToken = default)
    {
        var record = new PatientInsurance
        {
            PatientId = request.PatientId,
            ProviderId = request.ProviderId,
            PolicyNumber = request.PolicyNumber,
            GroupNumber = request.GroupNumber,
            IsPrimary = request.IsPrimary,
            ExpiresAt = request.ExpiresAt
        };

        if (request.IsPrimary)
            await ClearPrimaryFlagAsync(request.PatientId, cancellationToken);

        dbContext.PatientInsurances.Add(record);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToPatientInsuranceResponse(await dbContext.PatientInsurances
            .AsNoTracking().Include(pi => pi.Provider)
            .FirstAsync(pi => pi.Id == record.Id, cancellationToken));
    }

    public async Task<PatientInsuranceResponse> SetPrimaryAsync(Guid patientInsuranceId, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.PatientInsurances.FindAsync([patientInsuranceId], cancellationToken)
            ?? throw new KeyNotFoundException($"Patient insurance '{patientInsuranceId}' not found.");

        await ClearPrimaryFlagAsync(record.PatientId, cancellationToken);
        record.IsPrimary = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToPatientInsuranceResponse(await dbContext.PatientInsurances
            .AsNoTracking().Include(pi => pi.Provider)
            .FirstAsync(pi => pi.Id == patientInsuranceId, cancellationToken));
    }

    public async Task DeletePatientInsuranceAsync(Guid patientInsuranceId, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.PatientInsurances.FindAsync([patientInsuranceId], cancellationToken)
            ?? throw new KeyNotFoundException($"Patient insurance '{patientInsuranceId}' not found.");
        dbContext.PatientInsurances.Remove(record);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // ── Claims ───────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<InsuranceClaimResponse>> GetClaimsForInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var claims = await dbContext.InsuranceClaims
            .AsNoTracking()
            .Include(c => c.PatientInsurance).ThenInclude(pi => pi.Provider)
            .Include(c => c.Invoice)
            .Where(c => c.InvoiceId == invoiceId)
            .OrderByDescending(c => c.SubmittedAtUtc)
            .ToListAsync(cancellationToken);
        return claims.Select(ToClaimResponse).ToArray();
    }

    public async Task<InsuranceClaimResponse> CreateClaimAsync(CreateInsuranceClaimRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await dbContext.Invoices.FindAsync([request.InvoiceId], cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice '{request.InvoiceId}' not found.");

        if (invoice.Status is InvoiceStatus.Draft or InvoiceStatus.Voided)
            throw new InvalidOperationException("Claims can only be submitted for issued invoices.");

        var claim = new InsuranceClaim
        {
            InvoiceId = request.InvoiceId,
            PatientInsuranceId = request.PatientInsuranceId,
            ClaimNumber = request.ClaimNumber
        };
        dbContext.InsuranceClaims.Add(claim);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToClaimResponse(await dbContext.InsuranceClaims
            .AsNoTracking()
            .Include(c => c.PatientInsurance).ThenInclude(pi => pi.Provider)
            .Include(c => c.Invoice)
            .FirstAsync(c => c.Id == claim.Id, cancellationToken));
    }

    public async Task<InsuranceClaimResponse> UpdateClaimStatusAsync(Guid claimId, UpdateClaimStatusRequest request, CancellationToken cancellationToken = default)
    {
        var claim = await dbContext.InsuranceClaims.FindAsync([claimId], cancellationToken)
            ?? throw new KeyNotFoundException($"Insurance claim '{claimId}' not found.");

        claim.Status = request.Status;
        claim.ApprovedAmount = request.ApprovedAmount;
        claim.RejectionReason = request.RejectionReason;

        if (request.Status is ClaimStatus.Approved or ClaimStatus.Rejected or ClaimStatus.Paid)
            claim.ResolvedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToClaimResponse(await dbContext.InsuranceClaims
            .AsNoTracking()
            .Include(c => c.PatientInsurance).ThenInclude(pi => pi.Provider)
            .Include(c => c.Invoice)
            .FirstAsync(c => c.Id == claimId, cancellationToken));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task ClearPrimaryFlagAsync(Guid patientId, CancellationToken cancellationToken)
    {
        var existing = await dbContext.PatientInsurances
            .Where(pi => pi.PatientId == patientId && pi.IsPrimary)
            .ToListAsync(cancellationToken);
        foreach (var e in existing) e.IsPrimary = false;
    }

    private static InsuranceProviderResponse ToProviderResponse(InsuranceProvider p) =>
        new(p.Id, p.Name, p.ContactPhone, p.ContactEmail, p.Address, p.IsActive, p.CreatedAtUtc);

    private static PatientInsuranceResponse ToPatientInsuranceResponse(PatientInsurance pi) =>
        new(pi.Id, pi.PatientId, pi.ProviderId, pi.Provider.Name,
            pi.PolicyNumber, pi.GroupNumber, pi.IsPrimary,
            pi.ExpiresAt, pi.IsActive, pi.CreatedAtUtc);

    private static InsuranceClaimResponse ToClaimResponse(InsuranceClaim c) =>
        new(c.Id, c.InvoiceId, c.Invoice.InvoiceNumber,
            c.PatientInsuranceId, c.PatientInsurance.Provider.Name,
            c.PatientInsurance.PolicyNumber,
            c.ClaimNumber, c.Status,
            c.ApprovedAmount, c.RejectionReason,
            c.SubmittedAtUtc, c.ResolvedAtUtc);
}

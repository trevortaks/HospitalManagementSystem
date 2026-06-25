using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("prescriptions")]
[RequireSession]
public sealed class PrescriptionsController(IHttpClientFactory f) : AppController(f)
{

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] Guid? patientId,
        [FromQuery] Guid? encounterId,
        CancellationToken cancellationToken)
    {
        var client = Api();
        var qs = BuildQueryString(("patientId", patientId?.ToString()), ("encounterId", encounterId?.ToString()));
        var prescriptions = await client.GetFromJsonAsync<IReadOnlyList<PrescriptionResponse>>(
            $"/api/prescriptions{qs}", cancellationToken) ?? [];

        if (patientId.HasValue)
        {
            var patient = await client.GetFromJsonAsync<PatientResponse>($"/api/patients/{patientId}", cancellationToken);
            ViewBag.Patient = patient;
        }

        ViewBag.PatientId = patientId;
        ViewBag.EncounterId = encounterId;
        return View(prescriptions);
    }

    [HttpGet("queue")]
    public async Task<IActionResult> Queue(CancellationToken cancellationToken)
    {
        var client = Api();
        var prescriptions = await client.GetFromJsonAsync<IReadOnlyList<PrescriptionResponse>>(
            "/api/prescriptions?status=Active", cancellationToken) ?? [];
        return View(prescriptions);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(
        [FromQuery] Guid? encounterId,
        [FromQuery] Guid? patientId,
        CancellationToken cancellationToken)
    {
        var client = Api();
        var meds = await client.GetFromJsonAsync<IReadOnlyList<MedicationResponse>>(
            "/api/medications?activeOnly=true", cancellationToken) ?? [];
        var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>(
            "/api/users", cancellationToken) ?? [];

        ViewBag.Medications = meds;
        ViewBag.Doctors = users.Where(u => u.Role is "Doctor" or "Administrator").ToList();
        ViewBag.PreselectedEncounterId = encounterId;
        ViewBag.PreselectedPatientId = patientId;

        return View(new CreatePrescriptionRequest(
            encounterId ?? Guid.Empty,
            patientId ?? Guid.Empty,
            Guid.Empty, Guid.Empty,
            string.Empty, string.Empty));
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreatePrescriptionRequest request, CancellationToken cancellationToken)
    {
        var client = Api();
        var response = await client.PostAsJsonAsync("/api/prescriptions", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to create prescription.");
            var meds = await client.GetFromJsonAsync<IReadOnlyList<MedicationResponse>>("/api/medications?activeOnly=true", cancellationToken) ?? [];
            var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>("/api/users", cancellationToken) ?? [];
            ViewBag.Medications = meds;
            ViewBag.Doctors = users.Where(u => u.Role is "Doctor" or "Administrator").ToList();
            return View(request);
        }

        TempData["SuccessMessage"] = "Prescription created.";
        return RedirectToAction(nameof(Index), new { patientId = request.PatientId });
    }

    [HttpPost("{id:guid}/dispense")]
    public async Task<IActionResult> Dispense(Guid id, [FromForm] int QuantityDispensed, [FromForm] Guid DispensedByUserId, CancellationToken cancellationToken)
    {
        var client = Api();
        var request = new DispensePrescriptionRequest(QuantityDispensed, DispensedByUserId);
        await client.PatchAsJsonAsync($"/api/prescriptions/{id}/dispense", request, cancellationToken);
        TempData["SuccessMessage"] = "Prescription dispensed.";
        return RedirectToAction(nameof(Queue));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, Guid? patientId, CancellationToken cancellationToken)
    {
        var client = Api();
        await client.PatchAsync($"/api/prescriptions/{id}/cancel", null, cancellationToken);
        return RedirectToAction(nameof(Index), new { patientId });
    }


}

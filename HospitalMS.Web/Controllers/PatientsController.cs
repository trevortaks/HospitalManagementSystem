using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using HospitalMS.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("patients")]
[RequireSession]
public sealed class PatientsController(IHttpClientFactory f) : AppController(f)
{

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var client = Api();
        var patients = await client.GetFromJsonAsync<IReadOnlyList<PatientResponse>>(
            "/api/patients", cancellationToken) ?? [];
        return View(patients);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var client = Api();

        var patientTask      = client.GetFromJsonAsync<PatientResponse>($"/api/patients/{id}", cancellationToken);
        var appointmentsTask = client.GetFromJsonAsync<IReadOnlyList<AppointmentResponse>>($"/api/appointments?patientId={id}", cancellationToken);
        var encountersTask   = client.GetFromJsonAsync<IReadOnlyList<EncounterResponse>>($"/api/encounters?patientId={id}", cancellationToken);
        var prescriptionsTask= client.GetFromJsonAsync<IReadOnlyList<PrescriptionResponse>>($"/api/prescriptions?patientId={id}", cancellationToken);
        var labOrdersTask    = client.GetFromJsonAsync<IReadOnlyList<LabOrderResponse>>($"/api/lab-orders?patientId={id}", cancellationToken);
        var imagingTask      = client.GetFromJsonAsync<IReadOnlyList<ImagingRequestResponse>>($"/api/imaging-requests?patientId={id}", cancellationToken);
        var invoicesTask     = client.GetFromJsonAsync<IReadOnlyList<InvoiceResponse>>($"/api/invoices?patientId={id}", cancellationToken);

        await Task.WhenAll(patientTask, appointmentsTask, encountersTask,
            prescriptionsTask, labOrdersTask, imagingTask, invoicesTask);

        var patient = await patientTask;
        if (patient is null) return NotFound();

        ViewData["ActivePage"] = "Patients";
        return View(new PatientDetailViewModel
        {
            Patient          = patient,
            Appointments     = await appointmentsTask ?? [],
            Encounters       = await encountersTask ?? [],
            Prescriptions    = await prescriptionsTask ?? [],
            LabOrders        = await labOrdersTask ?? [],
            ImagingRequests  = await imagingTask ?? [],
            Invoices         = await invoicesTask ?? []
        });
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new CreatePatientRequest(string.Empty, string.Empty, string.Empty, DateTime.Today, string.Empty));

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreatePatientRequest request, CancellationToken cancellationToken)
    {
        var client = Api();
        var response = await client.PostAsJsonAsync("/api/patients", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to create patient.");
            return View(request);
        }

        TempData["SuccessMessage"] = "Patient record created.";
        return RedirectToAction("Index");
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var client = Api();
        var patient = await client.GetFromJsonAsync<PatientResponse>(
            $"/api/patients/{id}", cancellationToken);

        if (patient is null)
            return NotFound();

        return View(new UpdatePatientRequest(
            patient.FirstName, patient.LastName, patient.DateOfBirth, patient.Email,
            patient.PhoneNumber, patient.Gender, patient.BloodGroup,
            patient.AddressLine1, patient.AddressLine2, patient.City, patient.State,
            patient.PostalCode, patient.Country,
            patient.EmergencyContactName, patient.EmergencyContactPhone,
            patient.EmergencyContactRelationship, patient.EmergencyContactEmail,
            patient.Allergies, patient.ChronicConditions, patient.Notes));
    }

    [HttpPost("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, UpdatePatientRequest request, CancellationToken cancellationToken)
    {
        var client = Api();
        var response = await client.PutAsJsonAsync($"/api/patients/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to update patient.");
            return View(request);
        }

        TempData["SuccessMessage"] = "Patient record updated.";
        return RedirectToAction("Index");
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var client = Api();
        await client.DeleteAsync($"/api/patients/{id}", cancellationToken);
        TempData["SuccessMessage"] = "Patient record deleted.";
        return RedirectToAction("Index");
    }

}

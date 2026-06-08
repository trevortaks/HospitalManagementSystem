using System.Collections.ObjectModel;

namespace HospitalMS.Common.Constants;

public static class Permissions
{
    public const string PermissionClaimType = "permission";

    public const string UsersManage = "users.manage";
    public const string PatientsRead = "patients.read";
    public const string PatientsWrite = "patients.write";
    public const string AppointmentsRead = "appointments.read";
    public const string AppointmentsManage = "appointments.manage";
    public const string EncountersRead = "encounters.read";
    public const string EncountersWrite = "encounters.write";
    public const string EncountersClose = "encounters.close";
    public const string PrescriptionsManage = "prescriptions.manage";
    public const string LabResultsManage = "labresults.manage";
    public const string BillingManage = "billing.manage";
    public const string InvoicesManage = "invoices.manage";
    public const string PaymentsRecord = "payments.record";
    public const string InsuranceManage = "insurance.manage";
    public const string InventoryManage    = "inventory.manage";
    public const string FacilitiesManage   = "facilities.manage";
    public const string QualityManage      = "quality.manage";
    public const string HRManage           = "hr.manage";
    public const string AuditLogsRead = "auditlogs.read";
    public const string SystemAdmin = "system.admin";
    public const string SelfService = "selfservice.access";

    public static readonly IReadOnlyCollection<string> All =
    [
        UsersManage,
        PatientsRead,
        PatientsWrite,
        AppointmentsRead,
        AppointmentsManage,
        EncountersRead,
        EncountersWrite,
        EncountersClose,
        PrescriptionsManage,
        LabResultsManage,
        BillingManage,
        InvoicesManage,
        PaymentsRecord,
        InsuranceManage,
        InventoryManage,
        FacilitiesManage,
        QualityManage,
        HRManage,
        AuditLogsRead,
        SystemAdmin,
        SelfService
    ];

    public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> RolePermissions =
        new ReadOnlyDictionary<string, IReadOnlyCollection<string>>(
            new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
            {
                [UserRoles.Admin] = All,
                [UserRoles.Doctor] =
                [
                    PatientsRead,
                    PatientsWrite,
                    AppointmentsRead,
                    EncountersRead,
                    EncountersWrite,
                    EncountersClose,
                    PrescriptionsManage,
                    LabResultsManage,
                    QualityManage,
                    SelfService
                ],
                [UserRoles.Nurse] =
                [
                    PatientsRead,
                    PatientsWrite,
                    AppointmentsRead,
                    EncountersRead,
                    EncountersWrite,
                    FacilitiesManage,
                    SelfService
                ],
                [UserRoles.Patient] =
                [
                    SelfService
                ],
                [UserRoles.Receptionist] =
                [
                    PatientsRead,
                    AppointmentsRead,
                    AppointmentsManage,
                    SelfService
                ],
                [UserRoles.Pharmacist] =
                [
                    PatientsRead,
                    PrescriptionsManage,
                    InventoryManage,
                    SelfService
                ],
                [UserRoles.LabTechnician] =
                [
                    PatientsRead,
                    LabResultsManage,
                    SelfService
                ],
                [UserRoles.AccountsManager] =
                [
                    PatientsRead,
                    BillingManage,
                    InvoicesManage,
                    PaymentsRecord,
                    InsuranceManage,
                    AuditLogsRead,
                    SelfService
                ],
                [UserRoles.HRManager] =
                [
                    UsersManage,
                    HRManage,
                    AuditLogsRead,
                    SelfService
                ],
                [UserRoles.Radiologist] =
                [
                    PatientsRead,
                    LabResultsManage,
                    SelfService
                ]
            });

    public static IReadOnlyCollection<string> GetPermissionsForRole(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        return RolePermissions.TryGetValue(role, out var permissions)
            ? permissions
            : [];
    }

    public static IReadOnlyCollection<string> GetPermissionsForRoles(IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(roles);

        return roles
            .SelectMany(GetPermissionsForRole)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static bool RoleHasPermission(string role, string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        return GetPermissionsForRole(role).Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}

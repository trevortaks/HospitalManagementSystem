using System.Collections.ObjectModel;

namespace HospitalMS.Common.Constants;

public static class UserRoles
{
    public const string PolicyPrefix = "Role:";

    public const string Admin = "Admin";
    public const string Doctor = "Doctor";
    public const string Nurse = "Nurse";
    public const string Patient = "Patient";
    public const string Receptionist = "Receptionist";
    public const string Pharmacist = "Pharmacist";
    public const string LabTechnician = "LabTechnician";
    public const string AccountsManager = "AccountsManager";
    public const string HRManager = "HRManager";
    public const string Radiologist = "Radiologist";

    public static readonly IReadOnlyCollection<string> All =
    [
        Admin,
        Doctor,
        Nurse,
        Patient,
        Receptionist,
        Pharmacist,
        LabTechnician,
        AccountsManager,
        HRManager,
        Radiologist
    ];

    public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> RolePermissions =>
        new ReadOnlyDictionary<string, IReadOnlyCollection<string>>(
            Permissions.RolePermissions.ToDictionary(
                static pair => pair.Key,
                static pair => pair.Value,
                StringComparer.OrdinalIgnoreCase));

    public static bool IsValid(string role)
    {
        return !string.IsNullOrWhiteSpace(role) && All.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public static bool HasPermission(string role, string permission)
    {
        return Permissions.RoleHasPermission(role, permission);
    }

    public static bool HasPermission(IEnumerable<string> roles, string permission)
    {
        ArgumentNullException.ThrowIfNull(roles);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        return roles.Any(role => Permissions.RoleHasPermission(role, permission));
    }
}

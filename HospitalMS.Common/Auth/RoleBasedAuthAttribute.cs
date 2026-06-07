using Microsoft.AspNetCore.Authorization;

namespace HospitalMS.Common.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RoleBasedAuthAttribute : AuthorizeAttribute
{
    public RoleBasedAuthAttribute(params string[] roles)
    {
        var normalizedRoles = roles
            .Where(static role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedRoles.Length == 0)
        {
            throw new ArgumentException("At least one role must be specified.", nameof(roles));
        }

        Roles = string.Join(',', normalizedRoles);
    }
}

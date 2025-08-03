using Microsoft.AspNetCore.Identity;

namespace HospitalManagementSystem.Infrastructure.Identity
{
    /// <summary>
    /// Custom application role extending the default IdentityRole.
    /// </summary>
    public class ApplicationRole : IdentityRole
    {
        /// <summary>
        /// Gets or sets a brief description of the role.
        /// </summary>
        public string? Description { get; set; }
    }
}

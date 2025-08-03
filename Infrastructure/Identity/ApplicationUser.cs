using Microsoft.AspNetCore.Identity;

namespace HospitalManagementSystem.Infrastructure.Identity
{
    /// <summary>
    /// Custom application user that extends the default IdentityUser.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the user's date of birth.
        /// </summary>
        public DateOnly? DateOfBirth { get; set; }
    }
}

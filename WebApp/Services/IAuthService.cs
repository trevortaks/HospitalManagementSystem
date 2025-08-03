namespace HospitalManagementSystem.WebApp.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
}

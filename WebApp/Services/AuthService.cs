using System.Net.Http.Headers;
using System.Net.Http.Json;
using HospitalManagementSystem.WebApp.Models;

namespace HospitalManagementSystem.WebApp.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    public string? Token { get; private set; }

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/authentication/login", new LoginModel { Username = username, Password = password });
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (result?.Token == null)
        {
            return false;
        }

        Token = result.Token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        return true;
    }

    public Task LogoutAsync()
    {
        Token = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        return Task.CompletedTask;
    }

    private class LoginResponse
    {
        public string? Token { get; set; }
    }
}

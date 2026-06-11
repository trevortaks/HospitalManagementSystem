using HospitalMS.Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("auth")]
public sealed class AuthController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet("login")]
    public IActionResult Login() => View(new LoginRequest(string.Empty, string.Empty));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("HospitalAPI");
        var response = await client.PostAsJsonAsync("/api/auth/login", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(request);
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
        HttpContext.Session.SetString(TokenSessionKey, auth!.Token);
        HttpContext.Session.SetString("username", auth.Username);
        HttpContext.Session.SetString("role", auth.Role);
        HttpContext.Session.SetString("userId", auth.UserId.ToString());

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet("register")]
    public IActionResult Register() => View(new RegisterRequest(string.Empty, string.Empty, string.Empty, string.Empty));

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("HospitalAPI");
        var response = await client.PostAsJsonAsync("/api/auth/register", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Registration failed. Username may already be taken.");
            return View(request);
        }

        return RedirectToAction("Login");
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove(TokenSessionKey);
        return RedirectToAction("Login");
    }
}

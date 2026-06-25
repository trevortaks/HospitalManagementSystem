using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

public abstract class AppController(IHttpClientFactory httpClientFactory) : Controller
{
    protected HttpClient Api()
    {
        var client = httpClientFactory.CreateClient("HospitalAPI");
        var token  = HttpContext.Session.GetString("jwt_token");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected static string BuildQueryString(params (string key, string? value)[] pairs)
    {
        var parts = pairs
            .Where(p => !string.IsNullOrEmpty(p.value))
            .Select(p => $"{p.key}={Uri.EscapeDataString(p.value!)}");
        var qs = string.Join("&", parts);
        return string.IsNullOrEmpty(qs) ? string.Empty : $"?{qs}";
    }
}

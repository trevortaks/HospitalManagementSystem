using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
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

    /// <summary>
    /// Turns a failed API response into a message fit for a toast: the API's own
    /// message when it sent one (either <c>{ "message": "..." }</c> or a plain string),
    /// otherwise <paramref name="fallback"/>.
    /// </summary>
    protected static async Task<string> ApiErrorAsync(HttpResponseMessage response, string fallback, CancellationToken cancellationToken = default)
    {
        if (response.StatusCode == HttpStatusCode.Forbidden)
            return "You don't have permission to do that.";

        var body = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
        if (body.Length == 0) return fallback;

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.String)
                return root.GetString() is { Length: > 0 } str ? str : fallback;
            if (root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("message", out var msg)
                && msg.ValueKind == JsonValueKind.String)
                return msg.GetString() is { Length: > 0 } m ? m : fallback;
            return fallback;
        }
        catch (JsonException)
        {
            // Plain-text body (e.g. BadRequest("...") with a text/plain content type).
            return body.Length <= 200 ? body : fallback;
        }
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

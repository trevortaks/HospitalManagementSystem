using System.Text.Json;
using System.Text.RegularExpressions;

namespace HospitalMS.Tests.Security;

/// <summary>
/// The web app's CSP allows scripts and stylesheets only from 'self'. These tests keep
/// the Razor views compatible with that policy and stop the policy itself being loosened.
/// </summary>
public partial class ContentSecurityPolicyTests
{
    [Fact]
    public void Views_ContainNoInlineScriptBlocks()
    {
        // Any <script> without a src attribute is inline code the CSP will block.
        var offenders = FindInViews(InlineScriptRegex());
        Assert.True(offenders.Count == 0,
            "Inline <script> blocks are blocked by the CSP; move them to wwwroot/js:\n" + string.Join("\n", offenders));
    }

    [Fact]
    public void Views_ContainNoInlineEventHandlerAttributes()
    {
        var offenders = FindInViews(EventHandlerAttributeRegex());
        Assert.True(offenders.Count == 0,
            "on* attributes are blocked by the CSP; use data-* attributes and wwwroot/js instead:\n" + string.Join("\n", offenders));
    }

    [Fact]
    public void Views_ContainNoStyleElements()
    {
        // style-src-elem falls back to style-src 'self', so <style> blocks are blocked.
        var offenders = FindInViews(StyleElementRegex());
        Assert.True(offenders.Count == 0,
            "<style> blocks are blocked by the CSP; move them to wwwroot/css:\n" + string.Join("\n", offenders));
    }

    [Fact]
    public void WebContentSecurityPolicy_KeepsScriptsAndStyleSheetsSameOriginOnly()
    {
        var directives = ReadWebPolicy();

        Assert.Equal("'self'", directives["script-src"]);
        Assert.Equal("'self'", directives["style-src"]);
        Assert.Equal("'self'", directives["default-src"]);
        Assert.Equal("'none'", directives["object-src"]);
        Assert.Equal("'none'", directives["frame-ancestors"]);
        Assert.Equal("'self'", directives["base-uri"]);
        Assert.Equal("'self'", directives["form-action"]);
        Assert.DoesNotContain(directives, d => d.Value.Contains("'unsafe-eval'"));
        Assert.DoesNotContain(directives, d => d.Value.Contains("'unsafe-inline'") && d.Key != "style-src-attr");
    }

    private static Dictionary<string, string> ReadWebPolicy()
    {
        var path = Path.Combine(FindRepositoryRoot(), "HospitalMS.Web", "appsettings.json");
        using var json = JsonDocument.Parse(File.ReadAllText(path));
        var policy = json.RootElement.GetProperty("Security").GetProperty("ContentSecurityPolicy").GetString()!;

        return policy
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(d => d.Split(' ', 2, StringSplitOptions.TrimEntries))
            .ToDictionary(p => p[0], p => p.Length > 1 ? p[1] : string.Empty);
    }

    private static List<string> FindInViews(Regex pattern)
    {
        var viewsDir = Path.Combine(FindRepositoryRoot(), "HospitalMS.Web", "Views");
        var offenders = new List<string>();

        foreach (var file in Directory.EnumerateFiles(viewsDir, "*.cshtml", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            foreach (Match match in pattern.Matches(text))
            {
                var line = text.AsSpan(0, match.Index).Count('\n') + 1;
                offenders.Add($"{Path.GetRelativePath(viewsDir, file)}:{line}: {match.Value.Trim()}");
            }
        }

        return offenders;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "HospitalManagementSystem.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    [GeneratedRegex(@"<script(?![^>]*\bsrc\s*=)[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex InlineScriptRegex();

    [GeneratedRegex(@"\son[a-z]+\s*=\s*[""']", RegexOptions.IgnoreCase)]
    private static partial Regex EventHandlerAttributeRegex();

    [GeneratedRegex(@"<style[\s>]", RegexOptions.IgnoreCase)]
    private static partial Regex StyleElementRegex();
}

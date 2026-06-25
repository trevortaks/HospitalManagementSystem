namespace HospitalMS.Web;

public static class ViewHelpers
{
    private static readonly string[] AvatarPalette =
        ["#6366f1", "#0ea5e9", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6"];

    public static string AvatarColor(string name)
    {
        int h = 0;
        foreach (char c in name) h = c + ((h << 5) - h);
        return AvatarPalette[Math.Abs(h) % AvatarPalette.Length];
    }
}

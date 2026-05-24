namespace AMatterOfTaste.Api.Services;

/// <summary>
/// Resolves the on-disk directory where recipe photos are stored. In production this is a
/// stable path outside the published app (e.g. F:\AMatterOfTasteData\photos) so redeploys
/// never wipe user uploads. When PhotoStorage:Path is unset (local dev), falls back to
/// wwwroot/photos so the app still works without F: present.
/// </summary>
public static class PhotoStorage
{
    public const string RequestPath = "/photos";

    public static string GetDirectory(IConfiguration config, IWebHostEnvironment env)
    {
        var configured = config["PhotoStorage:Path"];
        var dir = !string.IsNullOrWhiteSpace(configured)
            ? configured
            : Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "photos");

        Directory.CreateDirectory(dir);
        return dir;
    }
}

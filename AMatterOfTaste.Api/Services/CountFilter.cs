using System.Net;

namespace AMatterOfTaste.Api.Services;

/// <summary>
/// Decides whether a request should be excluded from view/visit analytics so the
/// site owner's own traffic doesn't inflate the counts. A request is excluded when
/// it comes from a logged-in admin, or when its client IP is listed in
/// Analytics:IgnoredIps (for browsing while logged out).
/// </summary>
public static class CountFilter
{
    public static bool IsExcluded(HttpContext http, IConfiguration config)
    {
        // Logged-in admin (the token is sent on these public endpoints too).
        if (http.User?.IsInRole("Admin") == true)
            return true;

        var remote = http.Connection.RemoteIpAddress;
        if (remote is null)
            return false;
        if (remote.IsIPv4MappedToIPv6)
            remote = remote.MapToIPv4();

        var ignored = config.GetSection("Analytics:IgnoredIps").Get<string[]>();
        if (ignored is null)
            return false;

        foreach (var entry in ignored)
        {
            if (IPAddress.TryParse(entry, out var ip))
            {
                if (ip.IsIPv4MappedToIPv6)
                    ip = ip.MapToIPv4();
                if (ip.Equals(remote))
                    return true;
            }
        }

        return false;
    }
}

using System.Web;
using ReleasesFileGenerator.Launchpad.Types.Enums;

namespace ReleasesFileGenerator.Launchpad.Types.Options.Archive;

public class GetPublishedBinariesOptions : RequestOptionsBase
{
    public bool ExactMatch { get; set; } = false;
    public bool Ordered { get; set; } = false;
    public bool OrderByDate { get; set; } = true;
    public bool OrderByDateAscending { get; set; } = false;

    public string? BinaryPackageName { get; set; } = null;
    public string? ComponentName { get; set; } = null;
    public DateTimeOffset? CreatedSince { get; set; } = null;
    public Uri? DistroArchSeries { get; set; } = null;
    public ArchivePocket? Pocket { get; set; } = null;
    public ArchivePublishingStatus? Status { get; set; } = null;
    public string? Version { get; set; } = null;

    public static GetPublishedBinariesOptions Empty => new();

    internal override string ToQueryString()
    {
        const string wsop = "getPublishedBinaries";
        var queryString = HttpUtility.ParseQueryString(string.Empty);

        queryString.Add("ws.op", wsop);
        queryString.Add("exact_match", ExactMatch.ToString().ToLowerInvariant());
        queryString.Add("ordered", Ordered.ToString().ToLowerInvariant());
        queryString.Add("order_by_date", OrderByDate.ToString().ToLowerInvariant());
        queryString.Add("order_by_date_ascending", OrderByDateAscending.ToString().ToLowerInvariant());

        if (BinaryPackageName is not null)
            queryString.Add("binary_name", BinaryPackageName);
        if (ComponentName is not null)
            queryString.Add("component_name", ComponentName);
        if (CreatedSince is not null)
            queryString.Add("created_since_date", CreatedSince.ToString());
        if (DistroArchSeries is not null)
            queryString.Add("distro_arch_series", DistroArchSeries.ToString());
        if (Pocket is not null)
            queryString.Add("pocket", Enum.GetName(Pocket.Value));
        if (Status is not null)
            queryString.Add("status", Enum.GetName(Status.Value));
        if (Version is not null)
            queryString.Add("version", Version);

        return queryString.ToString() ?? string.Empty;
    }
}

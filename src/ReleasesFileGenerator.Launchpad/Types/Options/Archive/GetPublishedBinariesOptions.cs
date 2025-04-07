using System.Web;
using ReleasesFileGenerator.Launchpad.Types.Enums;

namespace ReleasesFileGenerator.Launchpad.Types.Options.Archive;

public class GetPublishedBinariesOptions : RequestOptionsBase
{
    /// <summary>
    /// Whether to filter binary names by exact matching.
    /// </summary>
    public bool ExactMatch { get; set; } = false;

    /// <summary>
    /// Ordered.
    /// <br/>
    /// Return ordered results by default, but specifying <c>false</c> will return results more quickly.
    /// </summary>
    public bool Ordered { get; set; } = false;

    /// <summary>
    /// Order by creation date.
    /// <br/>
    /// Return newest results first. This is recommended for applications that need to catch up with publications
    /// since their last run.
    /// </summary>
    public bool OrderByDate { get; set; } = true;

    /// <summary>
    /// Order by ascending creation date.
    /// <br/>
    /// Return oldest results first.
    /// </summary>
    public bool OrderByDateAscending { get; set; } = false;

    /// <summary>
    /// Binary Package Name.
    /// </summary>
    public string? BinaryPackageName { get; set; } = null;

    /// <summary>
    /// Component name.
    /// </summary>
    public string? ComponentName { get; set; } = null;

    /// <summary>
    /// Created Since Date.
    /// <br/>
    /// Return entries whose <c>date_created</c> is greater than or equal to this date.
    /// </summary>
    public DateTimeOffset? CreatedSince { get; set; } = null;

    /// <summary>
    /// Distro Arch Series.
    /// </summary>
    public Uri? DistroArchSeriesLink { get; set; } = null;

    /// <summary>
    /// Pocket.
    /// <br/>
    /// The pocket into which this entry is published.
    /// </summary>
    public ArchivePocket? Pocket { get; set; } = null;

    /// <summary>
    /// Package Publishing Status.
    /// <br/>
    /// The status of this publishing record.
    /// </summary>
    public ArchivePublishingStatus? Status { get; set; } = null;

    /// <summary>
    /// Version.
    /// </summary>
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
        if (DistroArchSeriesLink is not null)
            queryString.Add("distro_arch_series", DistroArchSeriesLink.ToString());
        if (Pocket is not null)
            queryString.Add("pocket", Enum.GetName(Pocket.Value));
        if (Status is not null)
            queryString.Add("status", Enum.GetName(Status.Value));
        if (Version is not null)
            queryString.Add("version", Version);

        return queryString.ToString() ?? string.Empty;
    }
}

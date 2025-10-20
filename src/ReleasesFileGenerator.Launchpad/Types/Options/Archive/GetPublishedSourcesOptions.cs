using System.Web;
using ReleasesFileGenerator.Launchpad.Types.Enums;

namespace ReleasesFileGenerator.Launchpad.Types.Options.Archive;

public class GetPublishedSourcesOptions : RequestOptionsBase
{
    /// <summary>
    /// Whether to filter source names by exact matching.
    /// </summary>
    public bool ExactMatch { get; set; } = false;
    /// <summary>
    /// Return newest results first. This is recommended for applications that need to catch up with publications since their last run.
    /// If not specified, results are ordered by source package name (lexicographically), then by descending version and then descending ID.
    /// </summary>
    public bool OrderByDate { get; set; } = true;
    /// <summary>
    /// Return oldest results first.
    /// </summary>
    public bool OrderByDateAscending { get; set; } = false;

    /// <summary>
    /// Component name.
    /// </summary>
    public string? ComponentName { get; set; } = null;
    /// <summary>
    /// Return entries whose <c>date_created</c> is greater than or equal to this date.
    /// </summary>
    public DateTimeOffset? CreatedSince { get; set; } = null;
    /// <summary>
    /// Distro series name.
    /// </summary>
    public Uri? DistroSeriesLink { get; set; } = null;
    /// <summary>
    /// The pocket into which this entry is published.
    /// </summary>
    public ArchivePocket? Pocket { get; set; } = null;
    /// <summary>
    /// Source package name.
    /// </summary>
    public string? SourcePackageName { get; set; } = null;
    /// <summary>
    /// The status of this publishing record.
    /// </summary>
    public ArchivePublishingStatus? Status { get; set; } = null;
    /// <summary>
    /// Version of the source package.
    /// </summary>
    public string? Version { get; set; } = null;

    public static GetPublishedSourcesOptions Empty => new();

    internal override string ToQueryString()
    {
        const string wsop = "getPublishedSources";
        var queryString = HttpUtility.ParseQueryString(string.Empty);

        queryString.Add("ws.op", wsop);
        queryString.Add("exact_match", ExactMatch.ToString().ToLowerInvariant());
        queryString.Add("order_by_date", OrderByDate.ToString().ToLowerInvariant());
        queryString.Add("order_by_date_ascending", OrderByDateAscending.ToString().ToLowerInvariant());

        if (ComponentName is not null)
            queryString.Add("component_name", ComponentName);
        if (CreatedSince is not null)
            queryString.Add("created_since_date", CreatedSince.ToString());
        if (DistroSeriesLink is not null)
            queryString.Add("distro_series", DistroSeriesLink.ToString());
        if (Pocket is not null)
            queryString.Add("pocket", Enum.GetName(Pocket.Value));
        if (SourcePackageName is not null)
            queryString.Add("source_name", SourcePackageName);
        if (Status is not null)
            queryString.Add("status", Enum.GetName(Status.Value));
        if (Version is not null)
            queryString.Add("version", Version);

        return queryString.ToString() ?? string.Empty;
    }
}

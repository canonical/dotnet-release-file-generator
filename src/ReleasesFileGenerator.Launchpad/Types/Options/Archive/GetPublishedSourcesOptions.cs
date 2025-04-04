using System.Web;
using ReleasesFileGenerator.Launchpad.Types.Enums;

namespace ReleasesFileGenerator.Launchpad.Types.Options.Archive;

public class GetPublishedSourcesOptions : RequestOptionsBase
{
    public bool ExactMatch { get; set; } = false;
    public bool OrderByDate { get; set; } = true;
    public bool OrderByDateAscending { get; set; } = false;

    public string? ComponentName { get; set; } = null;
    public DateTimeOffset? CreatedSince { get; set; } = null;
    public Uri? DistroSeries { get; set; } = null;
    public ArchivePocket? Pocket { get; set; } = null;
    public string? SourcePackageName { get; set; } = null;
    public ArchivePublishingStatus? Status { get; set; } = null;
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
        if (DistroSeries is not null)
            queryString.Add("distro_series", DistroSeries.ToString());
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

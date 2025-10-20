using System.Web;

namespace ReleasesFileGenerator.Launchpad.Types.Options.Distribution;

public class GetSeriesOptions(string nameOrVersion) : RequestOptionsBase
{
    /// <summary>
    /// Name or version of the series to retrieve.
    /// This can be the series name (e.g., "focal") or the version (e.g., "20.04") of the series.
    /// </summary>
    public string NameOrVersion { get; set; } = nameOrVersion;

    internal override string ToQueryString()
    {
        const string wsop = "getSeries";
        var queryString = HttpUtility.ParseQueryString(string.Empty);

        queryString.Add("ws.op", wsop);
        queryString.Add("name_or_version", NameOrVersion.ToLowerInvariant());

        return queryString.ToString() ?? string.Empty;
    }
}

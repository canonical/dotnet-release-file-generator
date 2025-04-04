using System.Web;

namespace ReleasesFileGenerator.Launchpad.Types.Options.BinaryPackagePublishingHistory;

public class GetBinaryPackageFilesOptions : RequestOptionsBase
{
    public bool IncludeMetadata { get; set; }

    public static GetBinaryPackageFilesOptions Empty => new();

    internal override string ToQueryString()
    {
        const string wsop = "binaryFileUrls";
        var queryString = HttpUtility.ParseQueryString(string.Empty);

        queryString.Add("ws.op", wsop);
        queryString.Add("include_meta", IncludeMetadata.ToString().ToLowerInvariant());

        return queryString.ToString() ?? string.Empty;
    }
}

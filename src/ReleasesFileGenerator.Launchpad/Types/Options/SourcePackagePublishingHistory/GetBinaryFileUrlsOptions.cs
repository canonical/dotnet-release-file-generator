using System.Web;

namespace ReleasesFileGenerator.Launchpad.Types.Options.SourcePackagePublishingHistory;

public class GetBinaryFileUrlsOptions : RequestOptionsBase
{
    internal override string ToQueryString()
    {
        const string wsop = "binaryFileUrls";
        var queryString = HttpUtility.ParseQueryString(string.Empty);

        queryString.Add("ws.op", wsop);

        return queryString.ToString() ?? string.Empty;
    }
}

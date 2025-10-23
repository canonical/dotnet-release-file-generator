using System.Web;

namespace ReleasesFileGenerator.Launchpad.Collections.Options.Archives;

public class GetByReferenceOptions : RequestOptionsBase
{
    public required string Reference { get; set; }

    public static GetByReferenceOptions Empty => new()
    {
        Reference = string.Empty
    };

    public static GetByReferenceOptions Ubuntu => new()
    {
        Reference = "ubuntu"
    };

    public static GetByReferenceOptions BackportsPpa => new()
    {
        Reference = "~dotnet/ubuntu/backports"
    };

    internal override string ToQueryString()
    {
        const string wsop = "getByReference";
        var queryString = HttpUtility.ParseQueryString(string.Empty);

        queryString.Add("ws.op", wsop);
        queryString.Add("reference", Reference);

        return queryString.ToString() ?? string.Empty;
    }
}

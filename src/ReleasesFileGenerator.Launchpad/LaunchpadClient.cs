using ReleasesFileGenerator.Launchpad.Services.Requests;

namespace ReleasesFileGenerator.Launchpad;

internal static class LaunchpadClient
{
    private const string BaseUrl = "https://api.launchpad.net";
    private const string ApiVersion = "devel";

    internal static HttpClient HttpClient { get; set; }

    static LaunchpadClient()
    {
        HttpClient = new HttpClient();
    }

    internal static Task<HttpResponseMessage> GetAsync(
        string resourcePath, RequestOptionsBase? options = null, CancellationToken cancellationToken = default)
    {
        var requestUrl = BuildRequestUrl(resourcePath, options);
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        return HttpClient.SendAsync(request, cancellationToken);
    }

    private static string BuildRequestUrl(string resourcePath, RequestOptionsBase? options)
    {
        var requestUrl = $"{BaseUrl}/{ApiVersion}/{resourcePath}";
        if (options is not null)
        {
            requestUrl = $"{requestUrl}?{options.ToQueryString()}";
        }

        return requestUrl;
    }
}

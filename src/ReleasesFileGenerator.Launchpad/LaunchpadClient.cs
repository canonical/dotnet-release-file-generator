namespace ReleasesFileGenerator.Launchpad;

internal static class LaunchpadClient
{
    private const string BaseUrl = "https://api.launchpad.net";
    private const string ApiVersion = "devel";

    private const string UserAgent =
        "ReleasesFileGenerator/1.0 (+https://github.com/canonical/dotnet-release-file-generator)";

    internal static HttpClient HttpClient { get; set; }

    static LaunchpadClient()
    {
        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
    }

    internal static Task<HttpResponseMessage> GetAsync(
        string resourcePath, RequestOptionsBase? options = null, CancellationToken cancellationToken = default)
    {
        var requestUrl = BuildRequestUrl(resourcePath, options);
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        return HttpClient.SendAsync(request, cancellationToken);
    }

    internal static string GetResourcePath(Uri selfLink)
    {
        ArgumentNullException.ThrowIfNull(selfLink, nameof(selfLink));
        return selfLink.ToString().Replace($"{BaseUrl}/{ApiVersion}/", string.Empty);
    }

    /// <summary>
    /// Builds an error message for a failed Launchpad API request, including the HTTP status code,
    /// reason phrase and request URL.
    /// </summary>
    /// <param name="message">A description of the operation that failed.</param>
    /// <param name="response">The non-successful HTTP response.</param>
    internal static string FormatHttpErrorMessage(string message, HttpResponseMessage response)
    {
        return $"{message}: HTTP {(int)response.StatusCode} {response.ReasonPhrase} " +
               $"(GET {response.RequestMessage?.RequestUri})";
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

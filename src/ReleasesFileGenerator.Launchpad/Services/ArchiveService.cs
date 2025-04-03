using System.Net.Http.Json;
using ReleasesFileGenerator.Launchpad.Models;
using ReleasesFileGenerator.Launchpad.Services.Contracts;
using ReleasesFileGenerator.Launchpad.Services.Requests;
using ReleasesFileGenerator.Launchpad.Types;

namespace ReleasesFileGenerator.Launchpad.Services;

public class ArchiveService : IArchiveService
{
    private const string BaseUrl = "https://api.launchpad.net";
    private const string ApiVersion = "devel";
    private const string ResourcePath = "ubuntu/+archive/primary";

    public async Task<LaunchpadCollectionResponse<SourcePackagePublishingHistory>> GetPublishedSourcesAsync(
        GetPublishedSourcesOptions? options = null,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        options ??= GetPublishedSourcesOptions.Empty;
        httpClient ??= new HttpClient();

        httpClient.BaseAddress = new Uri(BaseUrl);
        var requestUri = $"{ApiVersion}/{ResourcePath}?{options.ToQueryString()}";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApplicationException("Failed to get publishing history");
        }

        var result = await response.Content
            .ReadFromJsonAsync<LaunchpadCollectionResponse<SourcePackagePublishingHistory>>(
                cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new ApplicationException("Failed to get publishing history");
        }

        return result;
    }
}

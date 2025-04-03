using System.Net.Http.Json;
using ReleasesFileGenerator.Launchpad.Models;
using ReleasesFileGenerator.Launchpad.Services.Contracts;
using ReleasesFileGenerator.Launchpad.Services.Requests.Archive;
using ReleasesFileGenerator.Launchpad.Types;

namespace ReleasesFileGenerator.Launchpad.Services;

public class ArchiveService : IArchiveService
{
    private const string ResourcePath = "ubuntu/+archive/primary";

    public async Task<LaunchpadCollectionResponse<SourcePackagePublishingHistory>> GetPublishedSourcesAsync(
        GetPublishedSourcesOptionsBase? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= GetPublishedSourcesOptionsBase.Empty;
        var response = await LaunchpadClient.GetAsync(ResourcePath, options, cancellationToken);

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

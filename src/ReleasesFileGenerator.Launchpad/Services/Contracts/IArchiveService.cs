using ReleasesFileGenerator.Launchpad.Models;
using ReleasesFileGenerator.Launchpad.Services.Requests;
using ReleasesFileGenerator.Launchpad.Types;

namespace ReleasesFileGenerator.Launchpad.Services.Contracts;

public interface IArchiveService
{
    Task<LaunchpadCollectionResponse<SourcePackagePublishingHistory>> GetPublishedSourcesAsync(
        GetPublishedSourcesOptions? options = null,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default);
}

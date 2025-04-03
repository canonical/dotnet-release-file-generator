using ReleasesFileGenerator.Launchpad.Models;
using ReleasesFileGenerator.Launchpad.Services.Requests.Archive;
using ReleasesFileGenerator.Launchpad.Types;

namespace ReleasesFileGenerator.Launchpad.Services.Contracts;

public interface IArchiveService
{
    Task<LaunchpadCollectionResponse<SourcePackagePublishingHistory>> GetPublishedSourcesAsync(
        GetPublishedSourcesOptionsBase? options = null,
        CancellationToken cancellationToken = default);
}

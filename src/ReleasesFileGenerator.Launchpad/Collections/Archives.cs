using System.Net.Http.Json;
using ReleasesFileGenerator.Launchpad.Collections.Options.Archives;
using ReleasesFileGenerator.Launchpad.Types;

namespace ReleasesFileGenerator.Launchpad.Collections;

public static class Archives
{
    public static async Task<Archive> GetByReference(GetByReferenceOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= GetByReferenceOptions.Empty;
        var response = await LaunchpadClient.GetAsync("archives", options, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApplicationException("Failed to get archive");
        }

        var result = await response.Content
            .ReadFromJsonAsync<Archive>(cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new ApplicationException("Failed to get archive");
        }

        return result;
    }
}

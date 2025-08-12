using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ReleasesFileGenerator.Launchpad.Models;
using ReleasesFileGenerator.Launchpad.Types.Options.Distribution;

namespace ReleasesFileGenerator.Launchpad.Types;

public class Distribution : LaunchpadEntryType
{
    /// <summary>
    /// The canonical human-addressable web link to this resource.
    /// </summary>
    [JsonPropertyName("web_link")]
    public required Uri WebLink { get; set; }

    /// <summary>
    /// Retrieve the series with the name or version given.
    /// </summary>
    /// <param name="options">The request parameters.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// A <see cref="DistroSeries"/> object representing the series for the distribution.
    /// </returns>
    /// <exception cref="ApplicationException">
    /// Thrown when the request to get the series fails or the response is null.
    /// </exception>
    public async Task<DistroSeries> GetSeriesAsync(GetSeriesOptions options,
        CancellationToken cancellationToken = default)
    {
        var response =
            await LaunchpadClient.GetAsync(LaunchpadClient.GetResourcePath(SelfLink), options, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApplicationException("Failed to get series for distribution");
        }

        var result = await response.Content.ReadFromJsonAsync<DistroSeries>(cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new ApplicationException("Failed to get series for distribution");
        }

        return result;
    }
}

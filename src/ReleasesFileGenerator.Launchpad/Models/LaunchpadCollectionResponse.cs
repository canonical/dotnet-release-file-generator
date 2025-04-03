using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ReleasesFileGenerator.Launchpad.Converters;

namespace ReleasesFileGenerator.Launchpad.Models;

public class LaunchpadCollectionResponse<T> where T : LaunchpadEntryType
{
    [JsonPropertyName("start")]
    public int Start { get; set; }

    [JsonPropertyName("total_size")]
    public int? TotalSize { get; set; }

    [JsonPropertyName("total_size_link")]
    [JsonConverter(typeof(UriJsonConverter))]
    public Uri? TotalSizeLink { get; set; }

    [JsonPropertyName("next_collection_link")]
    [JsonConverter(typeof(UriJsonConverter))]
    public Uri? NextCollectionLink { get; set; }

    [JsonIgnore]
    public bool HasNextPage => NextCollectionLink != null;

    [JsonPropertyName("previous_collection_link")]
    [JsonConverter(typeof(UriJsonConverter))]
    public Uri? PreviousCollectionLink { get; set; }

    [JsonIgnore]
    public bool HasPreviousPage => PreviousCollectionLink != null;

    [JsonPropertyName("entries")]
    public IEnumerable<T> Entries { get; set; } = [];

    public async Task<int> GetTotalSizeAsync(CancellationToken cancellationToken = default)
    {
        if (TotalSize.HasValue)
        {
            return TotalSize.Value;
        }

        if (TotalSizeLink is null)
        {
            throw new ApplicationException("No total size or total size link available.");
        }

        var response = await LaunchpadClient.HttpClient.GetAsync(TotalSizeLink, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var totalSize = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!int.TryParse(totalSize, out var size))
            {
                throw new ApplicationException("Failed to parse total size.");
            }

            TotalSize = size;
            return size;
        }

        throw new ApplicationException(
            $"Failed to retrieve total size from {TotalSizeLink}. Status code: {response.StatusCode}");
    }

    public async Task<LaunchpadCollectionResponse<T>?> GetNextPageAsync(HttpClient? httpClient = null)
    {
        if (!HasNextPage)
        {
            return null;
        }

        httpClient ??= new HttpClient();
        var httpResponse = await httpClient.GetAsync(NextCollectionLink);

        if (httpResponse.IsSuccessStatusCode)
        {
            var result = await httpResponse.Content.ReadFromJsonAsync<LaunchpadCollectionResponse<T>>();

            return result;
        }

        throw new ApplicationException(
            $"Failed to retrieve next page from {NextCollectionLink}. Status code: {httpResponse.StatusCode}");
    }

    public async Task<LaunchpadCollectionResponse<T>?> GetPreviousPageAsync(HttpClient? httpClient = null)
    {
        if (!HasPreviousPage)
        {
            return null;
        }

        httpClient ??= new HttpClient();
        var httpResponse = await httpClient.GetAsync(PreviousCollectionLink);

        if (httpResponse.IsSuccessStatusCode)
        {
            var result = await httpResponse.Content.ReadFromJsonAsync<LaunchpadCollectionResponse<T>>();

            return result;
        }

        throw new ApplicationException(
            $"Failed to retrieve previous page from {PreviousCollectionLink}. Status code: {httpResponse.StatusCode}");
    }
}

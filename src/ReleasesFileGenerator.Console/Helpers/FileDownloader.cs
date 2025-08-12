namespace ReleasesFileGenerator.Console.Helpers;

public static class FileDownloader
{
    public static async Task DownloadFileAsync(Uri fileUrl, string destinationPath, IProgress<double>? progress)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        var destinationFile = Path.Join(destinationPath, fileUrl.Segments[^1]);

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream =
            new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        var moreToRead = true;

        do
        {
            var read = await contentStream.ReadAsync(buffer);
            if (read == 0)
            {
                moreToRead = false;
                continue;
            }

            await fileStream.WriteAsync(buffer.AsMemory(0, read));

            progress?.Report(read);
        }
        while (moreToRead);
    }
}

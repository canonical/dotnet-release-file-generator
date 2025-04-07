namespace ReleasesFileGenerator.Console;

public static class FileDownloader
{
    public static async Task DownloadFileAsync(Uri fileUrl, string destinationPath, IProgress<double> progress)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var canReportProgress = totalBytes != -1 && progress != null;

        var destinationFile = Path.Join(destinationPath, fileUrl.Segments[^1]);

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream =
            new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var totalRead = 0L;
        var buffer = new byte[8192];
        var isMoreToRead = true;

        do
        {
            var read = await contentStream.ReadAsync(buffer);
            if (read == 0)
            {
                isMoreToRead = false;
                progress?.Report(100);
                continue;
            }

            await fileStream.WriteAsync(buffer.AsMemory(0, read));

            totalRead += read;
            if (canReportProgress)
            {
                var progressPercentage = (double)totalRead / totalBytes * 100;
                progress?.Report(progressPercentage);
            }
        }
        while (isMoreToRead);
    }
}

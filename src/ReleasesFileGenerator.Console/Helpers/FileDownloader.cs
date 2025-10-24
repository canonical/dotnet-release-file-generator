using Downloader;
using Spectre.Console;

namespace ReleasesFileGenerator.Console.Helpers;

public static class FileDownloader
{
    public static Task DownloadFileAsync(
        string destinationDirectory,
        bool overwrite = false,
        CancellationToken cancellationToken = default,
        params Uri[] fileUrls)
    {
        return AnsiConsole
            .Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn(),
                new DownloadedColumn(),
                new TransferSpeedColumn())
            .StartAsync(async ctx =>
            {
                var tasks = new List<Task>();

                foreach (var fileUrl in fileUrls)
                {
                    var fileName = fileUrl.Segments.Last();
                    var destination = Path.Join(destinationDirectory, fileName);

                    if (File.Exists(destination))
                    {
                        if (overwrite)
                        {
                            File.Delete(destination);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    var download = DownloadBuilder.New()
                        .WithUrl(fileUrl)
                        .WithFileLocation(destination)
                        .WithConfiguration(new DownloadConfiguration
                        {
                            ClearPackageOnCompletionWithFailure = true
                        })
                        .Build();

                    var progressTask = ctx.AddTask(fileName);

                    download.DownloadProgressChanged += (s, e) =>
                    {
                        progressTask.Increment(e.ProgressedByteSize);
                    };

                    download.DownloadStarted += (s, e) => progressTask.MaxValue(e.TotalBytesToReceive);

                    tasks.Add(download.StartAsync(cancellationToken));

                    progressTask.StartTask();
                }

                await Task.WhenAll(tasks);

                if (tasks.Any(t => t.IsFaulted))
                {
                    throw new ApplicationException("One or more downloads failed.");
                }
            });
    }
}

using System.Diagnostics;

namespace ReleasesFileGenerator.Console.Helpers;

public static class FileDownloader
{
    public static Task<bool> DownloadFileAsync(
        Uri fileUrl, string destinationPath, CancellationToken cancellationToken = default)
    {
        const string wgetExecutable = "/usr/bin/wget";

        if (!File.Exists(wgetExecutable))
        {
            throw new FileNotFoundException("wget executable not found.", wgetExecutable);
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = wgetExecutable,
            Arguments = $"--continue \"{fileUrl}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = destinationPath
        };

        return Task.Run(() =>
        {
            var process = Process.Start(processStartInfo);

            if (process is null)
            {
                throw new ApplicationException("Could not start the dpkg process.");
            }

            process.WaitForExit();

            return process.ExitCode == 0;
        }, cancellationToken);
    }
}

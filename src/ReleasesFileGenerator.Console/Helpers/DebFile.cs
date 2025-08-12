using System.Diagnostics;

namespace ReleasesFileGenerator.Console.Helpers;

public static class DebFile
{
    /// <summary>
    /// Extracts the contents of a .deb file to a specified directory.
    /// </summary>
    /// <param name="filePath">The path to the .deb file.</param>
    /// <param name="destinationDirectory">The directory where the contents will be extracted.</param>
    /// <exception cref="FileNotFoundException">Thrown when the specified .deb file does not exist.</exception>
    /// <exception cref="ApplicationException">Thrown when the dpkg command fails or cannot be started.</exception>
    /// <remarks>
    /// This method uses the `dpkg` command-line tool to extract the contents of a .deb file.
    /// Ensure that `dpkg` is installed and available in the system's PATH.
    /// The destination directory must exist; if it does not, an exception will be thrown.
    /// </remarks>
    /// <example>
    /// <code>
    /// DebFile.ExtractDebFile("path/to/package.deb", "path/to/destination");
    /// </code>
    /// </example>
    /// <returns>None. The contents of the .deb file are extracted to the specified directory.</returns>
    /// <seealso href="https://manpages.debian.org/bullseye/dpkg/dpkg.1.en.html">dpkg manual page</seealso>
    /// <seealso href="https://wiki.debian.org/Dpkg">Debian Dpkg Wiki</seealso>
    public static void ExtractDebFile(string filePath, string destinationDirectory)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified .deb file does not exist.", filePath);
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dpkg",
            Arguments = $"--extract {filePath} .",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = destinationDirectory
        };

        using var process = Process.Start(processStartInfo);

        if (process is null)
        {
            throw new ApplicationException("Could not start the dpkg process.");
        }

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var errorMessage = process.StandardError.ReadToEnd();
            throw new ApplicationException($"dpkg failed with exit code {process.ExitCode}: {errorMessage}");
        }
    }
}

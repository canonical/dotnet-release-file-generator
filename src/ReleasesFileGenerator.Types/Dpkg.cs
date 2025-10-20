using System.Diagnostics;

namespace ReleasesFileGenerator.Types;

public static class Dpkg
{
    /// <summary>
    /// Compares two version strings using the `dpkg` command-line tool.
    /// This method allows you to compare versions using various operations such as less than, equal,
    /// greater than, etc. The comparison is performed using the `dpkg --compare-versions` command.
    /// </summary>
    /// <param name="version1">The left-side version to compare.</param>
    /// <param name="version2">The right-side version to compare.</param>
    /// <param name="operation">The comparison operation</param>
    /// <returns>
    /// Returns <c>true</c> if the comparison is true based on the specified operation;
    /// otherwise, returns <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when either version string is null or empty.
    /// </exception>
    /// <exception cref="ApplicationException">
    /// Thrown when the `dpkg` command fails to start or returns an error.
    /// This can happen if `dpkg` is not installed or not available in the system's PATH.
    /// </exception>
    public static bool CompareVersions(string version1, string version2, CompareVersionsOperation operation)
    {
        if (string.IsNullOrWhiteSpace(version1))
        {
            throw new ArgumentException("Version 1 cannot be null or empty.", nameof(version1));
        }

        if (string.IsNullOrWhiteSpace(version2))
        {
            throw new ArgumentException("Version 2 cannot be null or empty.", nameof(version2));
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dpkg",
            Arguments = $"--compare-versions {version1} {operation} {version2}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(processStartInfo);

        if (process is null)
        {
            throw new ApplicationException("Could not start the dpkg process.");
        }

        process.WaitForExit();

        return process.ExitCode == 0;
    }

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

    public sealed record CompareVersionsOperation(string Value)
    {
        public static readonly CompareVersionsOperation LessThan = new("lt");
        public static readonly CompareVersionsOperation LessThanOrEqual = new("le");
        public static readonly CompareVersionsOperation Equal = new("eq");
        public static readonly CompareVersionsOperation NotEqual = new("ne");
        public static readonly CompareVersionsOperation GreaterThanOrEqual = new("ge");
        public static readonly CompareVersionsOperation GreaterThan = new("gt");

        public override string ToString() => Value;
    }
}

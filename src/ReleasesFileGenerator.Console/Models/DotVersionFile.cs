namespace ReleasesFileGenerator.Console.Models;

public class DotVersionFile
{
    public required string CommitSha { get; init; }
    public required string Version { get; init; }

    /// <summary>
    /// Parse a .version file from the specified file path.
    /// The file is expected to have two lines:<br/>
    /// 1. The first line contains the commit SHA.<br/>
    /// 2. The second line contains the version.
    /// </summary>
    /// <param name="filePath">Path to the .version file.</param>
    /// <returns>A <c>DotVersionFile</c> instance with the information from the .version file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the .version file does not exist at the specified path.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the .version file is not structured as expected.</exception>
    public static DotVersionFile Parse(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Could not locate .version file.", filePath);
        }

        var lines = File.ReadAllLines(filePath);
        if (lines.Length < 2)
        {
            throw new InvalidOperationException("The .version file is not in the expected format.");
        }

        var commitSha = lines[0].Trim();
        var version = lines[1].Trim();

        return new DotVersionFile
        {
            CommitSha = commitSha,
            Version = version
        };
    }

    /// <summary>
    /// Find a .version file in the specified directory or its subdirectories.
    /// </summary>
    /// <param name="directory">The path to the top-most directory to search.</param>
    /// <returns>The .version file path.</returns>
    /// <exception cref="FileNotFoundException"> When no .version file is found in the specified directory or
    /// its subdirectories. </exception>
    public static string FindDotVersionFile(string directory)
    {
        var files = Directory.GetFiles(directory, ".version", SearchOption.AllDirectories);

        return files.Length == 0
            ? throw new FileNotFoundException("No .version file found in the specified directory.")
            : files[0];
    }
}

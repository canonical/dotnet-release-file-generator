namespace ReleasesFileGenerator.Console;

public class DotVersionFile
{
    public required string CommitSha { get; init; }
    public required string Version { get; init; }

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
}

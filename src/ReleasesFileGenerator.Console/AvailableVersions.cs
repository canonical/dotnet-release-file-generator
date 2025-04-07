using ReleasesFileGenerator.Types.ReleasesFile.Enums;

namespace ReleasesFileGenerator.Console;

public static class AvailableVersions
{
    public static AvailableVersionEntry[] GetAvailableVersions()
    {
        return
        [
            new AvailableVersionEntry
            {
                ChannelVersion = "9.0",
                Product = ".NET",
                SupportPhase = ReleaseSupportPhase.Active,
                ReleaseType = ReleaseType.Sts,
                EolDate = new DateOnly(2026, 05, 12),
                SourcePackageName = "dotnet9",
                RuntimeBinaryPackageName = "dotnet-runtime-9.0",
                SdkBinaryPackageName = "dotnet-sdk-9.0"
            },
            new AvailableVersionEntry
            {
                ChannelVersion = "8.0",
                Product = ".NET",
                SupportPhase = ReleaseSupportPhase.Active,
                ReleaseType = ReleaseType.Lts,
                EolDate = new DateOnly(2026, 11, 10),
                SourcePackageName = "dotnet8",
                RuntimeBinaryPackageName = "dotnet-runtime-8.0",
                SdkBinaryPackageName = "dotnet-sdk-8.0"
            },
            new AvailableVersionEntry
            {
                ChannelVersion = "7.0",
                Product = ".NET",
                SupportPhase = ReleaseSupportPhase.Eol,
                ReleaseType = ReleaseType.Sts,
                EolDate = new DateOnly(2024, 05, 14),
                SourcePackageName = "dotnet7",
                RuntimeBinaryPackageName = "dotnet-runtime-7.0",
                SdkBinaryPackageName = "dotnet-sdk-7.0"
            },
            new AvailableVersionEntry
            {
                ChannelVersion = "6.0",
                Product = ".NET",
                SupportPhase = ReleaseSupportPhase.Eol,
                ReleaseType = ReleaseType.Lts,
                EolDate = new DateOnly(2024, 11, 12),
                SourcePackageName = "dotnet6",
                RuntimeBinaryPackageName = "dotnet-runtime-6.0",
                SdkBinaryPackageName = "dotnet-sdk-6.0"
            }
        ];
    }
}

public class AvailableVersionEntry
{
    public required string ChannelVersion { get; set; }
    public required string Product { get; set; }
    public ReleaseSupportPhase SupportPhase { get; set; }
    public ReleaseType ReleaseType { get; set; }
    public DateOnly EolDate { get; set; }
    public required string SourcePackageName { get; set; }
    public required string RuntimeBinaryPackageName { get; set; }
    public required string SdkBinaryPackageName { get; set; }
}

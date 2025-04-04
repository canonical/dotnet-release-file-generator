using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace ReleasesFileGenerator.Types;

public partial class DotnetVersion : IEquatable<DotnetVersion>, IComparable<DotnetVersion>
{
    private static readonly Regex DotnetSourcePackageVersionPattern = new(
        @"^(?<SDKVersion>\d+\.\d+\.\d+)-(?<RuntimeVersion>\d+\.\d+\.\d+)(?:~(?<PreviewStatus>rc\d+|preview\d+))?-(?<UbuntuVersion>\d+ubuntu\d+)(?:~(?<UbuntuPreRelease>[\w\d\.]+))?$");

    public DotnetVersion(int major, int minor, int patch, bool isPreview = false, bool isRc = false,
        int? previewIdentifier = null)
    {
        if (isPreview && isRc)
        {
            throw new ApplicationException("The .NET version can either be a preview, an RC, or none.");
        }

        if (!isPreview && !isRc && previewIdentifier is not null)
        {
            throw new ApplicationException(
                "You can't specify a Preview Identifier if the version is neither a preview or an RC.");
        }

        Major = major;
        Minor = minor;
        Patch = patch;

        IsPreview = isPreview;
        IsRc = isRc;

        PreviewIdentifier = previewIdentifier;
    }

    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }

    public bool IsPreview { get; private set; }
    public bool IsRc { get; private set; }
    public bool IsStable => !IsPreview && !IsRc;
    public int? PreviewIdentifier { get; private set; } = null;

    public bool IsRuntime => Patch < 100;
    public bool IsSdk => !IsRuntime;

    public int? FeatureBand => !IsSdk ? default(int?) : int.Parse($"{Patch.ToString()[..1]}00");

    public static DotnetVersion Parse(string version)
    {
        var previewSplit = version.Split('-');
        var versionSections = previewSplit[0].Split('.');
        var parsedVersion = new DotnetVersion
        (
            int.Parse(versionSections[0]),
            int.Parse(versionSections[1]),
            int.Parse(versionSections[2])
        );

        if (previewSplit.Length > 1)
        {
            var previewIdentifierRegex = Regex.Match(previewSplit[1], @"\d+");

            if (previewSplit[1].Contains("preview"))
            {
                parsedVersion.IsPreview = true;
            }
            else if (previewSplit[1].Contains("rc"))
            {
                parsedVersion.IsRc = true;
            }

            if (previewIdentifierRegex.Success)
            {
                parsedVersion.PreviewIdentifier = int.Parse(previewIdentifierRegex.Value);
            }
        }

        return parsedVersion;
    }

    public static bool TryParseFromSourcePackageVersion(
        string sourcePackageVersion,
        [NotNullWhen(returnValue: true)] out DotnetVersion? sdkVersion,
        [NotNullWhen(returnValue: true)] out DotnetVersion? runtimeVersion)
    {
        sdkVersion = null;
        runtimeVersion = null;
        if (string.IsNullOrWhiteSpace(sourcePackageVersion)) return false;

        var parsedVersion = DotnetSourcePackageVersionPattern.Match(sourcePackageVersion);

        if (!parsedVersion.Success) return false;

        // Check for pre-release version
        var isPreview = parsedVersion.Groups["PreviewStatus"].Success &&
                        parsedVersion.Groups["PreviewStatus"].Value.StartsWith("preview");
        var isRc = parsedVersion.Groups["PreviewStatus"].Success &&
                   parsedVersion.Groups["PreviewStatus"].Value.StartsWith("rc");

        int? previewIdentifier = null;
        if (isPreview || isRc)
        {
            var previewIdentifierRegex = Regex.Match(parsedVersion.Groups["PreviewStatus"].Value, @"\d+");
            if (previewIdentifierRegex.Success)
            {
                previewIdentifier = int.Parse(previewIdentifierRegex.Value);
            }
            else
            {
                return false;
            }
        }

        // Parse product versions
        var splitSdkVersion = parsedVersion.Groups["SDKVersion"].Value.Split('.');
        sdkVersion = new DotnetVersion
        (
            int.Parse(splitSdkVersion[0]),
            int.Parse(splitSdkVersion[1]),
            int.Parse(splitSdkVersion[2]),
            isPreview,
            isRc,
            previewIdentifier
        );

        var splitRuntimeVersion = parsedVersion.Groups["RuntimeVersion"].Value.Split('.');
        runtimeVersion = new DotnetVersion
        (
            int.Parse(splitRuntimeVersion[0]),
            int.Parse(splitRuntimeVersion[1]),
            int.Parse(splitRuntimeVersion[2]),
            isPreview,
            isRc,
            previewIdentifier
        );

        return true;
    }

    public override string ToString()
    {
        var versionBuilder = new StringBuilder();

        versionBuilder.Append(Major);
        versionBuilder.Append('.');
        versionBuilder.Append(Minor);
        versionBuilder.Append('.');
        versionBuilder.Append(Patch);

        if (IsPreview) versionBuilder.Append("-preview");
        if (IsRc) versionBuilder.Append("-rc");

        if (PreviewIdentifier is not null)
        {
            versionBuilder.Append('.');
            versionBuilder.Append(PreviewIdentifier.ToString());
        }

        return versionBuilder.ToString();
    }
}

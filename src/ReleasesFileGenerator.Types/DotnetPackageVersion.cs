using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace ReleasesFileGenerator.Types;

public class DotnetPackageVersion
{
    private static readonly Regex DotnetSourcePackageVersionPattern = new(
        @"^(?<SDKVersion>\d+\.\d+\.\d+)-(?<RuntimeVersion>\d+\.\d+\.\d+)(?:~(?<PreviewStatus>rc\d+|preview\d+))?-(?<UbuntuSuffix>\d+ubuntu\d+)(?:~(?<UbuntuPreRelease>[\w\d\.]+))?$");

    public required string SourcePackageName { get; set; }
    public required string SourcePackageVersionString { get; set; }
    public required DotnetVersion UpstreamRuntimeVersion { get; set; }
    public required DotnetVersion UpstreamSdkVersion { get; set; }
    public required string UbuntuSuffix { get; set; }
    public string? UbuntuPreRelease { get; set; }

    private DotnetPackageVersion()
    { }

    public static DotnetPackageVersion Create(string sourcePackageName, string sourcePackageVersion)
    {
        if (string.IsNullOrWhiteSpace(sourcePackageVersion))
        {
            throw new FormatException("The source package version is empty.");
        }

        var parsedVersion = DotnetSourcePackageVersionPattern.Match(sourcePackageVersion);

        if (!parsedVersion.Success)
        {
            throw new FormatException("The source package version format is invalid.");
        }

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
                throw new FormatException("The source package version format is invalid.");
            }
        }

        // Parse product versions
        var splitSdkVersion = parsedVersion.Groups["SDKVersion"].Value.Split('.');
        var upstreamSdkVersion = new DotnetVersion
        (
            int.Parse(splitSdkVersion[0]),
            int.Parse(splitSdkVersion[1]),
            int.Parse(splitSdkVersion[2]),
            isPreview,
            isRc,
            previewIdentifier
        );

        var splitRuntimeVersion = parsedVersion.Groups["RuntimeVersion"].Value.Split('.');
        var upstreamRuntimeVersion = new DotnetVersion
        (
            int.Parse(splitRuntimeVersion[0]),
            int.Parse(splitRuntimeVersion[1]),
            int.Parse(splitRuntimeVersion[2]),
            isPreview,
            isRc,
            previewIdentifier
        );

        var packageVersion = new DotnetPackageVersion
        {
            SourcePackageName = sourcePackageName,
            SourcePackageVersionString = sourcePackageVersion,
            UpstreamRuntimeVersion = upstreamRuntimeVersion,
            UpstreamSdkVersion = upstreamSdkVersion,
            UbuntuSuffix = parsedVersion.Groups["UbuntuSuffix"].Value
        };

        if (parsedVersion.Groups["UbuntuPreRelease"].Success)
        {
            packageVersion.UbuntuPreRelease = parsedVersion.Groups["UbuntuPreRelease"].Value;
        }

        return packageVersion;
    }

    public static bool TryCreate(string sourcePackageName, string sourcePackageVersion,
        [NotNullWhen(returnValue: true)] out DotnetPackageVersion? packageVersion)
    {
        try
        {
            packageVersion = Create(sourcePackageName, sourcePackageVersion);
            return true;
        }
        catch (FormatException)
        {
            packageVersion = null;
            return false;
        }
    }

    public string GetUbuntuRuntimePackageVersion()
    {
        var sb = new StringBuilder();

        if (UpstreamRuntimeVersion.IsStable)
        {
            sb.Append(UpstreamRuntimeVersion);
        }
        else
        {
            sb.Append(UpstreamRuntimeVersion.ToString().Split('-')[0]);
            sb.Append(UpstreamRuntimeVersion.IsPreview ? "~preview" : "~rc");
            sb.Append(UpstreamRuntimeVersion.PreviewIdentifier);
        }

        sb.Append('-');
        sb.Append(UbuntuSuffix);
        if (UbuntuPreRelease is not null)
        {
            sb.Append('~');
            sb.Append(UbuntuPreRelease);
        }

        return sb.ToString();
    }

    public string GetUbuntuSdkPackageVersion()
    {
        var sb = new StringBuilder();

        if (UpstreamSdkVersion.IsStable)
        {
            sb.Append(UpstreamSdkVersion);
        }
        else
        {
            sb.Append(UpstreamSdkVersion.ToString().Split('-')[0]);
            sb.Append(UpstreamSdkVersion.IsPreview ? "~preview" : "~rc");
            sb.Append(UpstreamSdkVersion.PreviewIdentifier);
        }

        sb.Append('-');
        sb.Append(UbuntuSuffix);
        if (UbuntuPreRelease is not null)
        {
            sb.Append('~');
            sb.Append(UbuntuPreRelease);
        }

        return sb.ToString();
    }
}

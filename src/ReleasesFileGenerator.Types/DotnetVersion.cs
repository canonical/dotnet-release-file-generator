using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace ReleasesFileGenerator.Types;

public partial class DotnetVersion : IEquatable<DotnetVersion>, IComparable<DotnetVersion>
{
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
    public int? PreviewIdentifier { get; private set; }

    public bool IsRuntime => Patch < 100;
    public bool IsSdk => !IsRuntime;

    public int? FeatureBand => !IsSdk ? null : int.Parse($"{Patch.ToString()[..1]}00");

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

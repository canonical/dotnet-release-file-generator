namespace ReleasesFileGenerator.Types;

public partial class DotnetPackageVersion
{
    public int CompareTo(DotnetPackageVersion? other)
    {
        if (other is null)
        {
            return 1; // This instance is greater than null
        }

        var thisVersion = SourcePackageVersion;
        var otherVersion = other.SourcePackageVersion;

        if (Dpkg.CompareVersions(thisVersion, otherVersion, Dpkg.CompareVersionsOperation.GreaterThan))
        {
            return 1;
        }

        if (Dpkg.CompareVersions(thisVersion, otherVersion, Dpkg.CompareVersionsOperation.LessThan))
        {
            return -1;
        }

        return 0;
    }
}

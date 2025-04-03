namespace ReleasesFileGenerator.Types.Tests;

public class DotnetVersionOperatorsTests
{
    [Theory]
    [InlineData(8, 0, 100, false, false, null)]
    [InlineData(8, 0, 101, true, false, 2)]
    [InlineData(8, 0, 101, false, true, 1)]
    [InlineData(8, 0, 100, false, true, 1)]
    [InlineData(8, 0, 101, false, true, 4)]
    public void CompareTo_WithLowerVersion_ShouldReturnGreaterThanZero(int major, int minor, int patch, bool isPreview,
        bool isRc, int? previewIdentifier)
    {
        // Arrange
        var higherVersion1 = new DotnetVersion(8, 0, 101);
        var higherVersion2 = new DotnetVersion(8, 0, 102);
        var lowerVersion = new DotnetVersion(major, minor, patch, isPreview, isRc, previewIdentifier);

        // Act
        var result1 = higherVersion1.CompareTo(lowerVersion);
        var result2 = higherVersion2.CompareTo(lowerVersion);

        // Assert
        Assert.True(result1 > 0);
        Assert.True(result2 > 0);
    }

    [Theory]
    [InlineData(8, 0, 102, false, false, null)]
    [InlineData(8, 0, 100, true, false, 3)]
    [InlineData(8, 0, 100, false, true, 1)]
    [InlineData(8, 0, 103, false, true, 2)]
    public void CompareTo_WithHigherVersion_ShouldReturnSmallerThanZero(int major, int minor, int patch, bool isPreview,
        bool isRc, int? previewIdentifier)
    {
        // Arrange
        var higherVersion1 = new DotnetVersion(8, 0, 103);
        var higherVersion2 = new DotnetVersion(8, 0, 104);
        var lowerVersion = new DotnetVersion(major, minor, patch, isPreview, isRc, previewIdentifier);

        // Act
        var result1 = lowerVersion.CompareTo(higherVersion1);
        var result2 = lowerVersion.CompareTo(higherVersion2);

        // Assert
        Assert.True(result1 < 0);
        Assert.True(result2 < 0);
        Assert.True(lowerVersion.CompareTo(null) < 0);
    }

    [Theory]
    [InlineData(8, 0, 102, false, false, null)]
    [InlineData(8, 0, 100, true, false, 3)]
    [InlineData(8, 0, 100, false, true, 1)]
    public void CompareTo_WithEqualVersion_ShouldReturnZero(int major, int minor, int patch, bool isPreview,
        bool isRc, int? previewIdentifier)
    {
        // Arrange
        var version1 = new DotnetVersion(major, minor, patch, isPreview, isRc, previewIdentifier);
        var version2 = new DotnetVersion(major, minor, patch, isPreview, isRc, previewIdentifier);

        // Act
        var result = version1.CompareTo(version2);

        // Assert
        Assert.True(result == 0);
    }

    [Fact]
    public void CompareTo_WithBothPreviews_ShouldCompareCorrectly()
    {
        // Arrange
        var version1 = new DotnetVersion(8, 0, 100, true, false, 2);
        var version2 = new DotnetVersion(8, 0, 100, true, false, 3);

        // Act
        var result = version1.CompareTo(version2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void Equals_WithEqualVersions_ShouldReturnTrue()
    {
        // Arrange
        var version1 = new DotnetVersion(8, 0, 100, true, false, 2);
        var version2 = new DotnetVersion(8, 0, 100, true, false, 2);

        var version3 = new DotnetVersion(8, 0, 102);
        var version4 = new DotnetVersion(8, 0, 102);

        // Act
        var result1 = version1.Equals(version2);
        var result2 = version3.Equals(version4);

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(version1 == version2);
        Assert.True(version3 == version4);
        Assert.True(version1.Equals(version1));
    }

    [Fact]
    public void Equals_WithDifferentVersions_ShouldReturnFalse()
    {
        // Arrange
        var version1 = new DotnetVersion(8, 0, 100, true, false, 2);
        var version2 = new DotnetVersion(8, 0, 100, false, true, 2);

        var version3 = new DotnetVersion(8, 0, 102);
        var version4 = new DotnetVersion(8, 0, 104);

        // Act
        var result1 = version1.Equals(version2);
        var result2 = version3.Equals(version4);

        // Assert
        Assert.False(result1);
        Assert.False(result2);

        Assert.False(version1 == version2);
        Assert.False(version3 == version4);

        Assert.False(version1.Equals(null));
    }

    [Fact]
    public void Equals_WithAnyOrBothVersionsNull_ShouldReturnTrue()
    {
        // Arrange
        var version1 = new DotnetVersion(8, 0, 100, false, false, null);
        var version2 = default(DotnetVersion);
        var version3 = default(DotnetVersion);

        // Act
        var result1 = version1 == version2;
        var result2 = version2 == version3;

        // Assert
        Assert.False(result1);
        Assert.True(result2);
    }

    [Fact]
    public void StaticEquals_WithOneVersionNull_ShouldCompareCorrectly()
    {
        // Arrange
        var version1 = new DotnetVersion(8, 0, 0);
        var version2 = default(DotnetVersion);

        var version3 = default(DotnetVersion);
        var version4 = new DotnetVersion(8, 0, 103);

        // Act
        var different1 = DotnetVersion.Equals(version1, version2);
        var different2 = DotnetVersion.Equals(version3, version4);

        // Assert
        Assert.False(different1);
        Assert.False(different2);
    }

    [Fact]
    public void StaticEquals_WithBothVersionsNull_ShouldCompareCorrectly()
    {
        // Arrange
        var version1 = default(DotnetVersion);
        var version2 = default(DotnetVersion);

        // Act
        var equal = DotnetVersion.Equals(version1, version2);

        // Assert
        Assert.True(equal);
    }

    [Fact]
    public void StaticEquals_WithDefaultComparisonType_ShouldCompareCorrectly()
    {
        // Arrange
        var version1 = new DotnetVersion(8, 0, 0);
        var version2 = new DotnetVersion(8, 0, 0);

        var version3 = new DotnetVersion(8, 0, 100);
        var version4 = new DotnetVersion(8, 0, 103);

        // Act
        var equal = DotnetVersion.Equals(version1, version2, DotnetVersionComparison.Default);
        var different = DotnetVersion.Equals(version3, version4, DotnetVersionComparison.Default);

        // Assert
        Assert.True(equal);
        Assert.False(different);
    }

    [Fact]
    public void StaticEquals_WithIgnoreRevisionComparisonType_ShouldCompareCorrectly()
    {
        // Arrange
        var version1 = new DotnetVersion(8, 0, 0);
        var version2 = new DotnetVersion(8, 0, 0);

        var version3 = new DotnetVersion(8, 0, 100);
        var version4 = new DotnetVersion(8, 0, 103);

        // Act
        var equal = DotnetVersion.Equals(version1, version2, DotnetVersionComparison.IgnoreRevision);
        var different = DotnetVersion.Equals(version3, version4, DotnetVersionComparison.IgnoreRevision);

        // Assert
        Assert.True(equal);
        Assert.False(different);
    }

    [Theory]
    [InlineData(8, 0, 0, false, true, 2)]
    public void GetHashCode_WithEqualObjects_ShouldReturnTheSame(int major, int minor, int patch,
        bool isPreview, bool isRc, int? previewIdentifier)
    {
        // Arrange
        var version1 = new DotnetVersion(major, minor, patch, isPreview, isRc, previewIdentifier);
        var version2 = new DotnetVersion(major, minor, patch, isPreview, isRc, previewIdentifier);

        // Act
        var hash1 = version1.GetHashCode();
        var hash2 = version2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.True(version1 == version2);
    }

    [Fact]
    public void GetHashCode_WithDifferentObjects_ShouldReturnDifferentHashes()
    {
        // Arrange
        var version1 = new DotnetVersion(8, 0, 1);
        var version2 = new DotnetVersion(8, 0, 0);

        var version3 = new DotnetVersion(8, 0, 100, true, false, 2);
        var version4 = new DotnetVersion(8, 0, 3);

        // Act
        var hash1 = version1.GetHashCode();
        var hash2 = version2.GetHashCode();

        var hash3 = version3.GetHashCode();
        var hash4 = version4.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(hash3, hash4);

        Assert.False(version1 == version2);
        Assert.False(version3 == version4);
    }
}

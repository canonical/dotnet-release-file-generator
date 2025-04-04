namespace ReleasesFileGenerator.Types.UnitTests;

public class DotnetPackageVersionTests
{
    [Theory]
    [InlineData("dotnet8", "8.0.100-8.0.0-0ubuntu2", 8, 0, 100, 8, 0, 0)]
    [InlineData("dotnet9", "9.0.104-9.0.3-0ubuntu2", 9, 0, 104, 9, 0, 3)]
    public void Create_WithStableVersionInput_ShouldParseCorrectly(string sourcePackageName,
        string versionString, int sdkMajor, int sdkMinor, int sdkPatch, int runtimeMajor, int runtimeMinor,
        int runtimePatch)
    {
        // Act
        var packageVersion = DotnetPackageVersion.Create(sourcePackageName, versionString);

        // Assert
        Assert.Equal(packageVersion.UpstreamSdkVersion.Major, sdkMajor);
        Assert.Equal(packageVersion.UpstreamSdkVersion.Minor, sdkMinor);
        Assert.Equal(packageVersion.UpstreamSdkVersion.Patch, sdkPatch);

        Assert.Equal(packageVersion.UpstreamRuntimeVersion.Major, runtimeMajor);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.Minor, runtimeMinor);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.Patch, runtimePatch);
    }

    [Theory]
    [InlineData("dotnet8", "8.0.100-8.0.0~rc2-0ubuntu2", 8, 0, 100, 8, 0, 0, false, true, 2)]
    [InlineData("dotnet9", "9.0.104-9.0.3~preview1-0ubuntu2", 9, 0, 104, 9, 0, 3, true, false, 1)]
    public void Create_WithPreviewVersionInput_ShouldParseCorrectly(string sourcePackageName, string versionString,
        int sdkMajor, int sdkMinor, int sdkPatch, int runtimeMajor, int runtimeMinor, int runtimePatch,
        bool isPreview, bool isRc, int previewIdentifier)
    {
        // Act
        var packageVersion = DotnetPackageVersion.Create(sourcePackageName, versionString);

        // Assert
        Assert.Equal(packageVersion.UpstreamSdkVersion.Major, sdkMajor);
        Assert.Equal(packageVersion.UpstreamSdkVersion.Minor, sdkMinor);
        Assert.Equal(packageVersion.UpstreamSdkVersion.Patch, sdkPatch);
        Assert.Equal(packageVersion.UpstreamSdkVersion.IsPreview, isPreview);
        Assert.Equal(packageVersion.UpstreamSdkVersion.IsRc, isRc);
        Assert.Equal(packageVersion.UpstreamSdkVersion.PreviewIdentifier, previewIdentifier);

        Assert.Equal(packageVersion.UpstreamRuntimeVersion.Major, runtimeMajor);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.Minor, runtimeMinor);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.Patch, runtimePatch);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.IsPreview, isPreview);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.IsRc, isRc);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.PreviewIdentifier, previewIdentifier);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("dotnet8", "8.0.100-blah-blah-8.0.0~rc2-0ubuntu2")]
    [InlineData("dotnet9", "9.0.104-403-9.0.3~preview1-0ubuntu2+123")]
    public void Create_WithInvalidVersionInput_ShouldReturnFalse(string? packageName, string? versionString)
    {
        Assert.Throws<FormatException>(() =>
            DotnetPackageVersion.Create(packageName!, versionString!));
    }

    [Theory]
    [InlineData("dotnet8", "8.0.100-8.0.0-0ubuntu2", 8, 0, 100, 8, 0, 0)]
    [InlineData("dotnet9", "9.0.104-9.0.3-0ubuntu2", 9, 0, 104, 9, 0, 3)]
    public void TryCreate_WithStableVersionInput_ShouldParseCorrectly(string sourcePackageName,
        string versionString, int sdkMajor, int sdkMinor, int sdkPatch, int runtimeMajor, int runtimeMinor,
        int runtimePatch)
    {
        // Act
        var success = DotnetPackageVersion.TryCreate(sourcePackageName, versionString, out var packageVersion);

        // Assert
        Assert.True(success);
        Assert.NotNull(packageVersion);

        Assert.Equal(packageVersion.UpstreamSdkVersion.Major, sdkMajor);
        Assert.Equal(packageVersion.UpstreamSdkVersion.Minor, sdkMinor);
        Assert.Equal(packageVersion.UpstreamSdkVersion.Patch, sdkPatch);

        Assert.Equal(packageVersion.UpstreamRuntimeVersion.Major, runtimeMajor);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.Minor, runtimeMinor);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.Patch, runtimePatch);
    }

    [Theory]
    [InlineData("dotnet8", "8.0.100-8.0.0~rc2-0ubuntu2", 8, 0, 100, 8, 0, 0, false, true, 2)]
    [InlineData("dotnet9", "9.0.104-9.0.3~preview1-0ubuntu2", 9, 0, 104, 9, 0, 3, true, false, 1)]
    public void TryCreate_WithPreviewVersionInput_ShouldParseCorrectly(string sourcePackageName, string versionString,
        int sdkMajor, int sdkMinor, int sdkPatch, int runtimeMajor, int runtimeMinor, int runtimePatch,
        bool isPreview, bool isRc, int previewIdentifier)
    {
        // Act
        var success = DotnetPackageVersion.TryCreate(sourcePackageName, versionString, out var packageVersion);

        // Assert
        Assert.True(success);
        Assert.NotNull(packageVersion);

        Assert.Equal(packageVersion.UpstreamSdkVersion.Major, sdkMajor);
        Assert.Equal(packageVersion.UpstreamSdkVersion.Minor, sdkMinor);
        Assert.Equal(packageVersion.UpstreamSdkVersion.Patch, sdkPatch);
        Assert.Equal(packageVersion.UpstreamSdkVersion.IsPreview, isPreview);
        Assert.Equal(packageVersion.UpstreamSdkVersion.IsRc, isRc);
        Assert.Equal(packageVersion.UpstreamSdkVersion.PreviewIdentifier, previewIdentifier);

        Assert.Equal(packageVersion.UpstreamRuntimeVersion.Major, runtimeMajor);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.Minor, runtimeMinor);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.Patch, runtimePatch);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.IsPreview, isPreview);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.IsRc, isRc);
        Assert.Equal(packageVersion.UpstreamRuntimeVersion.PreviewIdentifier, previewIdentifier);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("dotnet8", "8.0.100-blah-blah-8.0.0~rc2-0ubuntu2")]
    [InlineData("dotnet9", "9.0.104-403-9.0.3~preview1-0ubuntu2+123")]
    public void TryCreate_WithInvalidVersionInput_ShouldReturnFalse(string? packageName, string? versionString)
    {
        // Act
        var success = DotnetPackageVersion.TryCreate(packageName!, versionString!, out var packageVersion);

        // Assert
        Assert.False(success);
        Assert.Null(packageVersion);
    }

    [Theory]
    [InlineData("dotnet8", "8.0.100-8.0.0-0ubuntu2", "8.0.0-0ubuntu2")]
    [InlineData("dotnet9", "9.0.104-9.0.3~preview1-0ubuntu2", "9.0.3~preview1-0ubuntu2")]
    [InlineData("dotnet9", "9.0.104-9.0.3~preview1-0ubuntu2~23.04.1", "9.0.3~preview1-0ubuntu2~23.04.1")]
    public void GetUbuntuRuntimePackageVersion(string packageName, string versionString, string expectedUbuntuRuntimeVersion)
    {
        // Arrange
        var packageVersion = DotnetPackageVersion.Create(packageName, versionString);

        // Act
        var ubuntuRuntimeVersion = packageVersion.GetUbuntuRuntimePackageVersion();

        // Assert
        Assert.Equal(expectedUbuntuRuntimeVersion, ubuntuRuntimeVersion);
    }

    [Theory]
    [InlineData("dotnet8", "8.0.100-8.0.0-0ubuntu2", "8.0.100-0ubuntu2")]
    [InlineData("dotnet9", "9.0.104-9.0.3~preview1-0ubuntu2", "9.0.104~preview1-0ubuntu2")]
    [InlineData("dotnet9", "9.0.104-9.0.3~preview1-0ubuntu2~23.04.1", "9.0.104~preview1-0ubuntu2~23.04.1")]
    public void GetUbuntuSdkPackageVersion(string packageName, string versionString, string expectedUbuntuSdkVersion)
    {
        // Arrange
        var packageVersion = DotnetPackageVersion.Create(packageName, versionString);

        // Act
        var ubuntuSdkVersion = packageVersion.GetUbuntuSdkPackageVersion();

        // Assert
        Assert.Equal(expectedUbuntuSdkVersion, ubuntuSdkVersion);
    }
}

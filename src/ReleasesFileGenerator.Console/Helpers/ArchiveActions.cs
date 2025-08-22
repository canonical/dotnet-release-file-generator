using System.Collections.ObjectModel;
using ReleasesFileGenerator.Console.Models;
using ReleasesFileGenerator.Launchpad.Types;
using ReleasesFileGenerator.Launchpad.Types.Models;
using ReleasesFileGenerator.Launchpad.Types.Options.Archive;
using ReleasesFileGenerator.Launchpad.Types.Options.BinaryPackagePublishingHistory;
using ReleasesFileGenerator.Types;

namespace ReleasesFileGenerator.Console.Helpers;

public static class ArchiveActions
{
    /// <summary>
    /// Gets the published source package history for a specific version in a given Ubuntu distribution series.
    /// </summary>
    /// <param name="ubuntuArchive">Ubuntu archive to be queried.</param>
    /// <param name="distroSeries">Distro series to be queried.</param>
    /// <param name="version">Version channel to query.</param>
    /// <returns>
    /// A read-only collection of <see cref="SourcePackagePublishingHistory"/> entries representing the published
    /// source package history for the specified version in the given Ubuntu distribution series.
    /// </returns>
    public static async Task<ReadOnlyCollection<SourcePackagePublishingHistory>> GetPublishedSourcesForVersion(
        Archive ubuntuArchive,
        DistroSeries distroSeries,
        AvailableVersionEntry version)
    {
        var publishedSources = new List<SourcePackagePublishingHistory>();
        var currentPageOfPublishedSources =
            await ubuntuArchive.GetPublishedSourcesAsync(new GetPublishedSourcesOptions
            {
                SourcePackageName = version.SourcePackageName,
                DistroSeriesLink = distroSeries.SelfLink
            });

        do
        {
            publishedSources.AddRange(currentPageOfPublishedSources.Entries);
            currentPageOfPublishedSources = currentPageOfPublishedSources.GetNextPageAsync().Result;
        } while (currentPageOfPublishedSources is not null);

        return publishedSources.AsReadOnly();
    }

    /// <summary>
    /// Gets the binary package file information for a specific binary package name and version from the given Ubuntu
    /// archive. This method defaults to the <c>amd64</c> architecture when selecting the package file.
    /// </summary>
    /// <param name="ubuntuArchive">Ubuntu archive to be queried.</param>
    /// <param name="binaryPackageName">The binary package name.</param>
    /// <param name="binaryPackageVersion">The binary package version.</param>
    /// <returns></returns>
    public static async Task<BinaryPackageFile> GetPackageFile(
        Archive ubuntuArchive,
        string binaryPackageName,
        string binaryPackageVersion)
    {
        var publishedBinaries = await ubuntuArchive.GetPublishedBinariesAsync(
            new GetPublishedBinariesOptions
            {
                BinaryPackageName = binaryPackageName,
                Version = binaryPackageVersion,
                ExactMatch = true
            });

        var package = publishedBinaries.Entries
            .First(p => p.DisplayName.Contains("amd64"));

        var packageFiles = await package.GetBinaryPackageFiles(new GetBinaryPackageFilesOptions
        {
            IncludeMetadata = true
        });

        return packageFiles.First();
    }

    /// <summary>
    /// Reads the .NET runtime, ASP.NET Core runtime, and SDK versions from the provided .deb package files.
    /// The method downloads the specified .deb files, extracts them to a temporary working directory,
    /// and reads the version information from the extracted files. After reading the versions, it cleans up
    /// the temporary files and directories.
    /// </summary>
    /// <param name="runtimePackageFile">.NET Runtime package file.</param>
    /// <param name="aspnetCoreRuntimePackageFile">ASP.NET Core Runtime package file.</param>
    /// <param name="sdkPackageFile">.NET SDK package file.</param>
    /// <param name="workingDirectory">Working directory.</param>
    /// <param name="shouldDeleteFiles">Whether to delete the downloaded files after reading the versions.</param>
    /// <returns>
    /// A tuple containing the .NET runtime version, ASP.NET Core runtime version, and SDK version.
    /// </returns>
    public static Task<(DotnetVersion RuntimeVersion, DotnetVersion AspNetRuntimeVersion, DotnetVersion SdkVersion)>
        ReadVersionFromDotVersionFiles(
            BinaryPackageFile runtimePackageFile,
            BinaryPackageFile aspnetCoreRuntimePackageFile,
            BinaryPackageFile sdkPackageFile,
            DirectoryInfo workingDirectory,
            bool shouldDeleteFiles = true)
    {
        return ReadVersionFromDotVersionFiles(
            runtimePackageFile.Url,
            aspnetCoreRuntimePackageFile.Url,
            sdkPackageFile.Url,
            workingDirectory,
            shouldDeleteFiles);
    }

    /// <summary>
    /// Reads the .NET runtime, ASP.NET Core runtime, and SDK versions from the provided .deb package files.
    /// The method downloads the specified .deb files, extracts them to a temporary working directory,
    /// and reads the version information from the extracted files. After reading the versions, it cleans up
    /// the temporary files and directories.
    /// </summary>
    /// <param name="runtimePackageFileUrl">.NET Runtime package URL.</param>
    /// <param name="aspnetCoreRuntimePackageFileUrl">ASP.NET Core Runtime package URL.</param>
    /// <param name="sdkPackageFileUrl">.NET SDK package URL.</param>
    /// <param name="workingDirectory">Working directory.</param>
    /// <param name="shouldDeleteFiles">Whether to delete the downloaded files after reading the versions.</param>
    /// <returns>
    /// A tuple containing the .NET runtime version, ASP.NET Core runtime version, and SDK version.
    /// </returns>
    public static async
        Task<(DotnetVersion RuntimeVersion, DotnetVersion AspNetRuntimeVersion, DotnetVersion SdkVersion)>
        ReadVersionFromDotVersionFiles(
            Uri runtimePackageFileUrl,
            Uri aspnetCoreRuntimePackageFileUrl,
            Uri sdkPackageFileUrl,
            DirectoryInfo workingDirectory,
            bool shouldDeleteFiles = true)
    {
        var runtimePackagePath =
            $"{workingDirectory.FullName}/{runtimePackageFileUrl.Segments.Last()}";
        var aspNetCoreRuntimePackagePath =
            $"{workingDirectory.FullName}/{aspnetCoreRuntimePackageFileUrl.Segments.Last()}";
        var sdkPackagePath =
            $"{workingDirectory.FullName}/{sdkPackageFileUrl.Segments.Last()}";

        var runtimeDownloadTask = File.Exists(runtimePackagePath)
            ? Task.FromResult(true)
            : FileDownloader.DownloadFileAsync(runtimePackageFileUrl, workingDirectory.FullName);

        var aspNetCoreRuntimeDownloadTask = File.Exists(aspNetCoreRuntimePackagePath)
            ? Task.FromResult(true)
            : FileDownloader.DownloadFileAsync(aspnetCoreRuntimePackageFileUrl, workingDirectory.FullName);

        var sdkDownloadTask = File.Exists(sdkPackagePath)
            ? Task.FromResult(true)
            : FileDownloader.DownloadFileAsync(sdkPackageFileUrl, workingDirectory.FullName);

        await Task.WhenAll(runtimeDownloadTask, aspNetCoreRuntimeDownloadTask, sdkDownloadTask);

        return ReadVersionsFromDotVersionFiles(
            runtimePackagePath,
            aspNetCoreRuntimePackagePath,
            sdkPackagePath,
            workingDirectory,
            shouldDeleteFiles);
    }

    private static (DotnetVersion RuntimeVersion, DotnetVersion AspNetRuntimeVersion, DotnetVersion SdkVersion)
        ReadVersionsFromDotVersionFiles(
            string runtimePackagePath, string aspNetCoreRuntimePackagePath, string sdkPackagePath,
            DirectoryInfo workingDirectory,
            bool shouldDeleteFiles = true)
    {
        Dpkg.ExtractDebFile(runtimePackagePath, workingDirectory.FullName);
        Dpkg.ExtractDebFile(aspNetCoreRuntimePackagePath, workingDirectory.FullName);
        Dpkg.ExtractDebFile(sdkPackagePath, workingDirectory.FullName);

        var runtimeVersion =
            DotnetVersion.Parse(
                DotVersionFile.Parse(
                        DotVersionFile.FindDotVersionFile(
                            $"{workingDirectory.FullName}/usr/lib/dotnet/shared/Microsoft.NETCore.App"))
                    .Version);
        var aspNetRuntimeVersion =
            DotnetVersion.Parse(
                DotVersionFile.Parse(
                        DotVersionFile.FindDotVersionFile(
                            $"{workingDirectory.FullName}/usr/lib/dotnet/shared/Microsoft.AspNetCore.App"))
                    .Version);
        var sdkVersion =
            DotnetVersion.Parse(
                DotVersionFile.Parse(
                        DotVersionFile.FindDotVersionFile($"{workingDirectory.FullName}/usr/lib/dotnet/sdk"))
                    .Version);


        if (shouldDeleteFiles)
        {
            File.Delete(runtimePackagePath);
            File.Delete(aspNetCoreRuntimePackagePath);
            File.Delete(sdkPackagePath);
        }

        Directory.Delete(Path.Join(workingDirectory.FullName, "usr"), recursive: true);

        return (runtimeVersion, aspNetRuntimeVersion, sdkVersion);
    }
}

using System.Security.AccessControl;
using ReleasesFileGenerator.Launchpad.Models;

namespace ReleasesFileGenerator.Launchpad.Types;

public class DistroSeries : LaunchpadEntryType
{
    /// <summary>
    /// The name of this series.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// The series display name.
    /// </summary>
    public required string DisplayName { get; set; }
    /// <summary>
    /// The series full name, e.g. Ubuntu Warty.
    /// </summary>
    public required string FullSeriesName { get; set; }
    /// <summary>
    /// The title of this series. It should be distinctive and designed to look good at the top of a page.
    /// </summary>
    public required string Title { get; set; }
    /// <summary>
    /// A detailed description of this series, with information on the architectures covered, the availability of
    /// security updates and any other relevant information.
    /// </summary>
    public required string Description { get; set; }
}

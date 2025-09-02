using System;

namespace LogUtils.Helpers.Extensions
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Bumps provided version to a specified value
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Increment value is negative</exception>
        /// <exception cref="NotImplementedException">VersionCode is invalid</exception>
        /// <exception cref="NotSupportedException">The Version object doesn't provide the requested version value</exception>
        public static Version Bump(this Version version, VersionCode code, int increment = 1)
        {
            if (version.Revision >= 0)
            {
                return code switch
                {
                    VersionCode.Major => new Version(version.Major + increment, version.Minor, version.Build, version.Revision),
                    VersionCode.Minor => new Version(version.Major, version.Minor + increment, version.Build, version.Revision),
                    VersionCode.Build => new Version(version.Major, version.Minor, version.Build + increment, version.Revision),
                    VersionCode.Revision => new Version(version.Major, version.Minor, version.Build, version.Revision + increment),
                    _ => throw new NotImplementedException(),
                };
            }
            else if (version.Build >= 0)
            {
                return code switch
                {
                    VersionCode.Major => new Version(version.Major + increment, version.Minor, version.Build),
                    VersionCode.Minor => new Version(version.Major, version.Minor + increment, version.Build),
                    VersionCode.Build => new Version(version.Major, version.Minor, version.Build + increment),
                    VersionCode.Revision => throw new NotSupportedException(),
                    _ => throw new NotImplementedException(),
                };
            }
            else if (version.Minor >= 0)
            {
                return code switch
                {
                    VersionCode.Major => new Version(version.Major + increment, version.Minor),
                    VersionCode.Minor => new Version(version.Major, version.Minor + increment),
                    VersionCode.Build or VersionCode.Revision => throw new NotSupportedException(),
                    _ => throw new NotImplementedException(),
                };
            }
            throw new NotSupportedException();
        }   
    }
    public enum VersionCode
    {
        Major,
        Minor,
        Build,
        Revision
    }
}

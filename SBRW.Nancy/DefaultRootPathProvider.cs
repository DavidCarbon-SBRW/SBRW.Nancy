namespace SBRW.Nancy
{
    /// <summary>
    /// Default implementation of <see cref="IRootPathProvider"/>.
    /// </summary>
    public class DefaultRootPathProvider : IRootPathProvider
    {
        /// <summary>
        /// Returns the root folder path of the current Nancy application.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the path of the root folder.</returns>
        /// <remarks><see href="https://github.com/aspnet/Announcements/issues/237">A valid non-empty application name must be provided.</see></remarks>
        public string GetRootPath()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}


namespace BioCheck
{
    /// <summary>
    /// Static helper class for managing the versions
    /// </summary>
    public static class Version
    {
        /// <summary>
        /// Update the Major version when the XML Schema changes.
        /// This requries a new XMLFactory.
        /// </summary>
        public const string Major = "3";

        /// <summary>
        /// Update the Minor version for live updates to the cloud.
        /// </summary>
        public const string Minor = "3.04";

        public new static string ToString()
        {
            return string.Format("{0}.{1}", Major, Minor);
        }
    }
}

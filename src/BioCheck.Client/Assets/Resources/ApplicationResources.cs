namespace BioCheck
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Browser;

    /// <summary>
    /// Wraps access to the strongly-typed resource classes so that you can bind control properties to resource strings in XAML.
    /// </summary>
    public sealed class ApplicationResources
    {
        private static readonly ApplicationStrings applicationStrings = new ApplicationStrings();
        private static readonly ErrorResources errorResources = new ErrorResources();

        /// <summary>
        /// Gets the <see cref="ApplicationStrings"/>.
        /// </summary>
        public ApplicationStrings Strings
        {
            get { return applicationStrings; }
        }

        /// <summary>
        /// Gets the <see cref="ErrorResources"/>.
        /// </summary>
        public ErrorResources Errors
        {
            get { return errorResources; }
        }
    }
}

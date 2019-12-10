using System;

namespace Unchase.Dynamics365.Shared.Extensions
{
    /// <summary>
    /// Helper class for throwing argument exceptions.
    /// </summary>
    public static class CheckParam
    {
        /// <summary>
        /// Check parameter value for null.
        /// </summary>
        /// <param name="parameter">Parameter value.</param>
        /// <param name="name">Parameter name.</param>
        public static void CheckForNull(object parameter, string name)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Check string parameter value for null or white space.
        /// </summary>
        /// <param name="parameter">Parameter value.</param>
        /// <param name="name">Parameter name.</param>
        public static void CheckForNullOrWhiteSpace(string parameter, string name)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Check Guid parameter value for empty.
        /// </summary>
        /// <param name="parameter">Parameter value.</param>
        /// <param name="name">Parameter name.</param>
        public static void CheckForNotEmpty(Guid parameter, string name)
        {
            if (parameter == Guid.Empty)
            {
                throw new ArgumentException(name);
            }
        }
    }
}

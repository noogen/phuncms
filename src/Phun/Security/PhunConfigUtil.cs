namespace Phun.Security
{
    using System;
    using System.Configuration;

    using WebMatrix.WebData;

    /// <summary>
    /// Config util, copied from: https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/src/WebMatrix.WebData/ConfigUtil.cs
    /// Apache License copyright 2.0 blah blah...
    /// and just make it public dang it.
    /// </summary>
    public static class PhunConfigUtil
    {
        /// <summary>
        /// The _simple membership enabled
        /// </summary>
        private static bool _simpleMembershipEnabled = IsSimpleMembershipEnabled();

        /// <summary>
        /// Gets a value indicating whether [simple membership enabled].
        /// </summary>
        /// <value>
        /// <c>true</c> if [simple membership enabled]; otherwise, <c>false</c>.
        /// </value>
        public static bool SimpleMembershipEnabled
        {
            get { return _simpleMembershipEnabled; }
        }

        /// <summary>
        /// Determines whether [is simple membership enabled].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is simple membership enabled]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsSimpleMembershipEnabled()
        {
            string settingValue = ConfigurationManager.AppSettings[WebSecurity.EnableSimpleMembershipKey];
            bool enabled;
            if (!String.IsNullOrEmpty(settingValue) && Boolean.TryParse(settingValue, out enabled))
            {
                return enabled;
            }

            // Simple Membership is enabled by default, but attempts to delegate to the current provider if not initialized.
            return true;
        }

        /// <summary>
        /// Shoulds the preserve login URL.
        /// </summary>
        /// <returns></returns>
        public static bool ShouldPreserveLoginUrl()
        {
            string settingValue = ConfigurationManager.AppSettings[FormsAuthenticationSettings.PreserveLoginUrlKey];
            bool preserveLoginUrl;
            if (!String.IsNullOrEmpty(settingValue) && Boolean.TryParse(settingValue, out preserveLoginUrl))
            {
                return preserveLoginUrl;
            }

            // For backwards compatible with WebPages 1.0, we override the loginUrl value if 
            // the PreserveLoginUrl key is not present.
            return false;
        }
    }
}

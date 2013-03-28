namespace Phun.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;

    /// <summary>
    /// Host authorization config.
    /// </summary>
    public interface IHostAuthorizationConfiguration
    {
        /// <summary>
        /// Gets or sets the key or host name.
        /// </summary>
        /// <value>
        /// The key or host name.
        /// </value>
        [ConfigurationProperty("key", IsRequired = true)]
        string Key { get; set; }

        /// <summary>
        /// Gets or sets the allow roles.
        /// </summary>
        /// <value>
        /// The allow roles.
        /// </value>
        [ConfigurationProperty("value", IsRequired = false)]
        string Value { get; set; }
    }
}

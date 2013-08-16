namespace Phun.Configuration
{
    using System.Configuration;

    /// <summary>
    /// Host authorization configuration element.
    /// </summary>
    public class KeyValueConfiguration : ConfigurationElement, IKeyValueConfiguration
    {
        /// <summary>
        /// Gets or sets the key or host name.
        /// </summary>
        /// <value>
        /// The key or host name.
        /// </value>
        [ConfigurationProperty("key", IsRequired = true)]
        public virtual string Key { get; set; }

        /// <summary>
        /// Gets or sets the allow roles.
        /// </summary>
        /// <value>
        /// The allow roles.
        /// </value>
        [ConfigurationProperty("value", IsRequired = false)]
        public virtual string Value { get; set; }
    }
}

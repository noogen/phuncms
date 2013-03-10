namespace Phun.Configuration
{
    using System.Configuration;

    /// <summary>
    /// Allow for mapping multiple routes.
    /// </summary>
    public class MapRouteCollection : AConfigurationElementCollection<MapRouteConfiguration>
    {
        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        /// <value>
        /// The name of the element.
        /// </value>
        protected override string ElementName
        {
            get { return "add"; }
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement" /> to return the key for.</param>
        /// <returns>
        /// An <see cref="T:System.Object" /> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MapRouteConfiguration)element).Route;
        }
    }   
}

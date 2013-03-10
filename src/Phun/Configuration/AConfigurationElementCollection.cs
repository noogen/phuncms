namespace Phun.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;

    /// <summary>
    /// Allow for looping through configuration elements.
    /// </summary>
    /// <typeparam name="T">Type of configuration element.</typeparam>
    [ConfigurationCollection(typeof(ConfigurationElement))]
    public abstract class AConfigurationElementCollection<T> : ConfigurationElementCollection, IEnumerable<T>
        where T : ConfigurationElement, new()
    {
        /// <summary>
        /// Gets or sets a property, attribute, or child element of this configuration element.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return (T)BaseGet(index); }
        }

        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T GetElement(object key)
        {
            return (T)BaseGet(key);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public new IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < this.Count; i++)
            { 
                yield return (T)this[i];
            }
        }

        /// <summary>
        /// Adds the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        public void Add(T element)
        {
            this.BaseAdd(element);
        }

        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

    }
}

namespace Phun.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;

    /// <summary>
    /// Allow for looping through configuration elements.
    /// </summary>
    /// <typeparam name="T">Type of configuration element.</typeparam>
    [ConfigurationCollection(typeof(ConfigurationElement))]
    public abstract class AConfigurationElementCollection<T> : ConfigurationElementCollection, ICollection<T>
        where T : ConfigurationElement, new()
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
        public new bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets a property, attribute, or child element of this configuration element.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Item of type T.</returns>
        public T this[int index]
        {
            get { return (T)BaseGet(index); }
        }

        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The element of T.</returns>
        public T GetElement(object key)
        {
            return (T)BaseGet(key);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>An enumerable of T.</returns>
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
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            this.BaseClear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            return this.BaseIndexOf(item) > 0;
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        /// <exception cref="System.NotImplementedException">Method not implemented.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public bool Remove(T item)
        {
            var canRemove = this.BaseIndexOf(item) >= 0;
            if (canRemove)
            {
                this.BaseRemoveAt(this.BaseIndexOf(item));
            }

            return canRemove;
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

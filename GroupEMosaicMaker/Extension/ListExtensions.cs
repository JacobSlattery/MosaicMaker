using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GroupEMosaicMaker.Extension
{
    /// <summary>
    ///     A class that extends the list class to add functionality
    /// </summary>
    public static class ListExtensions
    {
        #region Methods

        /// <summary>
        ///     Converts a list to the observable collection.
        /// </summary>
        /// <typeparam name="T"> the object type being inputted</typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns>the collection converted</returns>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection)
        {
            return new ObservableCollection<T>(collection);
        }

        #endregion
    }
}
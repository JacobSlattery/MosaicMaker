using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupEMosaicMaker.Extension
{
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

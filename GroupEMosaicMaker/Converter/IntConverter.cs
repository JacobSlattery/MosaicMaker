using System;
using Windows.UI.Xaml.Data;

namespace GroupEMosaicMaker.Converter
{
    public class IntConverter : IValueConverter
    {
        #region Methods

        /// <summary>
        ///     Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>Returns string from int</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var temperature = (int) value;
            return temperature.ToString();
        }

        /// <summary>
        ///     Converts the back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>Returns int from string</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var tempString = (string) value;
            var valueToReturn = 0;

            if (!string.IsNullOrEmpty(tempString))
            {
                valueToReturn = int.Parse(tempString);
            }

            return valueToReturn;
        }

        #endregion
    }
}
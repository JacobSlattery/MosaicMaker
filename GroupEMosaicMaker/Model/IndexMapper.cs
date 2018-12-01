using System.Collections.ObjectModel;

namespace GroupEMosaicMaker.Model
{
    internal class IndexMapper
    {
        #region Methods

        /// <summary>
        ///     Finds the indexes for a box within a larger box. Uses bounds to ensure that indexes do not go outside the bounds of
        ///     the larger box.
        /// </summary>
        /// <param name="startIndex">The index starting location</param>
        /// <param name="boxWidth">The width of the index box</param>
        /// <param name="boxHeight">The height of the index box</param>
        /// <param name="maxWidth">The total number of pixels contained in a row</param>
        /// <param name="maxHeight">The total number of pixels contained in a column</param>
        /// <returns>
        ///     A collection of the predicted indexes.
        /// </returns>
        public static Collection<int> CalculateIndexBox(int startIndex, int boxWidth, int boxHeight, int maxWidth,
            int maxHeight)
        {
            var indexCollection = new Collection<int>();
            var currentIndex = startIndex;
            var startingHeight = startIndex / maxWidth;

            for (var row = 0; row < boxHeight; row++)
            {
                var actualRow = row + startingHeight;
                var rowOffset = maxWidth * (row + startingHeight);
                if (actualRow < maxHeight)
                {
                    for (var j = 1; j <= boxWidth; j++)
                    {
                        if (currentIndex - rowOffset < maxWidth)
                        {
                            indexCollection.Add(currentIndex);
                        }

                        currentIndex++;
                    }

                    currentIndex = currentIndex - boxWidth + maxWidth;
                }
            }

            return indexCollection;
        }

        public static void ConvertEachIndexToMatchOffset(Collection<int> indexes, int offset)
        {
            for (var i = 0; i < indexes.Count; i++)
            {
                indexes[i] = indexes[i] * offset;
            }
        }

        #endregion
    }
}
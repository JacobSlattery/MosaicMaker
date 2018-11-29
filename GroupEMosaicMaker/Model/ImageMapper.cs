using System.Collections.ObjectModel;


namespace GroupEMosaicMaker.Model
{
    class ImageMapper
    {
        /// <summary>
        ///     Finds the indexes for a box within a larger box. Uses bounds to ensure that indexes do not go outside the bounds of the larger box.
        /// </summary>
        /// <param name="startIndex">The index starting location</param>
        /// <param name="boxSize">The size of the output box</param>
        /// <param name="maxWidth">The total number of elements contained in a row</param>
        /// <param name="maxHeight">The total number of elements contained in a column</param>
        /// <returns>
        ///     A collection of indexes.
        /// </returns>
        public static Collection<int> calculateIndexBox(int startIndex, int boxSize, int maxWidth, int maxHeight)
        {
            var indexCollection = new Collection<int>();
            var currentIndex = startIndex;
            int widthOffset;
            int heightOffset;

            for (var i = 1; i <= boxSize; i++)
            {
                widthOffset = maxWidth * (i - 1);
                heightOffset = currentIndex / maxWidth;
                if (heightOffset < maxHeight)
                {
                    for (var j = 1; j <= boxSize; j++)
                    {
                        if ((currentIndex - widthOffset * heightOffset) < maxWidth)
                        {
                            indexCollection.Add(currentIndex);
                        }
                        currentIndex++;
                    }
                    currentIndex = currentIndex - boxSize + maxWidth;
                }

            }
            return indexCollection;
        }

    }
}

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
        /// <param name="imageWidth">The total number of pixels contained in a row</param>
        /// <param name="imageHeight">The total number of pixels contained in a column</param>
        /// <returns>
        ///     A collection of indexes accounting for an RGBA offset.
        /// </returns>
        public static Collection<int> calculateIndexBox(int startIndex, int boxSize, int imageWidth, int imageHeight)
        {
            var indexCollection = new Collection<int>();
            var currentIndex = startIndex;
            int widthOffset;
            int heightOffset;
            var maxWidth = imageWidth * 4;
            var maxHeight = imageHeight * 4;
            int startingHeight = startIndex / maxHeight;


            for (var i = 1; i <= boxSize; i++)
            {
                widthOffset = maxWidth * (i - 1 + startingHeight);
                heightOffset = currentIndex / maxWidth;
                if (heightOffset < imageHeight)
                {
                    for (var j = 1; j <= boxSize; j++)
                    {
                        if (currentIndex - widthOffset < maxWidth)
                        {
                            indexCollection.Add(currentIndex);
                        }
                        currentIndex += 4;
                    }
                    currentIndex = currentIndex - (boxSize * 4) + maxWidth;
                }

            }
            return indexCollection;
        }

    }
}

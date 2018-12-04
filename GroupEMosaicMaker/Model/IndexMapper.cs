using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GroupEMosaicMaker.Model
{
    internal class IndexMapper
    {



        public static Collection<int[]> Triangle(int startIndex, int boxSize, int maxWidth, int maxHeight)
        {
            var leftTriangle = new Collection<int>();
            var rightTriangle = new Collection<int>();
            var currentIndex = startIndex;
            var startingHeight = startIndex / maxWidth;
            for (var row = 0; row < boxSize; row++)
            {
                var actualRow = row + startingHeight;
                var rowOffset = maxWidth * actualRow;
                if (actualRow < maxHeight)
                {
                    for (var j = 0; j < boxSize; j++)
                    {
                        var relativeRowIndex = currentIndex - startIndex - (row*maxWidth);
                        if (currentIndex - rowOffset < maxWidth)
                        {
                            if (relativeRowIndex - row <= 0)
                            {
                                leftTriangle.Add(currentIndex);
                            }
                            else
                            {
                                rightTriangle.Add(currentIndex);
                            }
                        }
                        currentIndex++;
                    }
                }
                currentIndex = currentIndex - boxSize + maxWidth;
            }
            return new Collection<int[]>() {leftTriangle.ToArray(), rightTriangle.ToArray()};
        }


        public static int[] Grid(int startIndex, int boxSize, int maxWidth, int maxHeight, bool includeDiagonalLine=false)
        {
            {
                var indexes = new Collection<int>();
                var currentIndex = startIndex;
                var startingHeight = startIndex / maxWidth;

                for (var row = 0; row < boxSize; row++)
                {
                    var actualRow = row + startingHeight;
                    var rowOffset = maxWidth * actualRow;
                    if (actualRow < maxHeight)
                    {
                        for (var j = 0; j < boxSize; j++)
                        {
                            var relativeRowIndex = currentIndex - startIndex - (row * maxWidth);
                            var isInBoxRange = (currentIndex - rowOffset < maxWidth);

                            if (isInBoxRange && isValidGridIndex(boxSize, row, relativeRowIndex, includeDiagonalLine))
                            {
                                indexes.Add(currentIndex);
                            }

                            currentIndex++;
                        }
                    }

                    currentIndex = currentIndex - boxSize + maxWidth;
                }


                return indexes.Distinct().ToArray();
            }
        }

        private static bool isValidGridIndex(int boxSize, int row, int relativeRowIndex, bool includeDiagonalLine=false)
        {
            var topWall = (row == 0);
            var leftWall = (relativeRowIndex % boxSize == 0);
            var bottomWall = (row == boxSize - 1);
            var rightWall = ((relativeRowIndex + 1) % boxSize == 0);

            var isValid = topWall || leftWall || bottomWall || rightWall;

            if (includeDiagonalLine)
            {
                isValid = isValid || relativeRowIndex - row == 0;
            }

            return isValid;
        }

        #region Methods

        /// <summary>
        ///     Finds the indexes for a box within a larger box. Uses bounds to ensure that indexes do not go outside the bounds of
        ///     the larger box.
        /// </summary>
        /// <param name="startIndex">The index starting location</param>
        /// <param name="boxSize">The height and width of the index box</param>
        /// <param name="maxWidth">The total number of pixels contained in a row</param>
        /// <param name="maxHeight">The total number of pixels contained in a column</param>
        /// <returns>
        ///     A collection of the predicted indexes.
        /// </returns>
        public static int[] Box(int startIndex, int boxSize, int maxWidth, int maxHeight)
        {
            var indexCollection = new Collection<int>();
            var currentIndex = startIndex;
            var startingHeight = startIndex / maxWidth;

            for (var row = 0; row < boxSize; row++)
            {
                var actualRow = row + startingHeight;
                var rowOffset = maxWidth * (row + startingHeight);
                if (actualRow < maxHeight)
                {
                    for (var j = 1; j <= boxSize; j++)
                    {
                        if (currentIndex - rowOffset < maxWidth)
                        {
                            indexCollection.Add(currentIndex);
                        }

                        currentIndex++;
                    }

                    currentIndex = currentIndex - boxSize + maxWidth;
                }
            }

            return indexCollection.ToArray();



        }

        public static void ConvertEachIndexToMatchOffset(int[] indexes, int offset)
        {
            for (var i = 0; i < indexes.Length; i++)
            {
                indexes[i] = indexes[i] * offset;
            }
        }

        #endregion
    }
}
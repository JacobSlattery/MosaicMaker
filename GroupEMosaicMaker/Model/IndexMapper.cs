using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GroupEMosaicMaker.Model
{
    internal class IndexMapper
    {



        public static Collection<int> TriangleGrid(int startIndex, int boxSize, int maxWidth, int maxHeight)
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
                        var relativeRowIndex = currentIndex - startIndex - (row*maxWidth);
                        if (currentIndex - rowOffset < maxWidth)
                        {
                            if (row == 0) //Top Wall
                            {
                                indexes.Add(currentIndex);
                            }
                            else if (relativeRowIndex % boxSize == 0) //Left Wall
                            {
                                indexes.Add(currentIndex);
                            }
                            else if (row == boxSize - 1) //Bottom Wall
                            {
                                indexes.Add(currentIndex);
                            }
                            else if ((relativeRowIndex + 1) % boxSize == 0) // Right Wall
                            {
                                indexes.Add(currentIndex);
                            }
                            else if ((relativeRowIndex - row) == (0)) //Triangle Line
                            {
                                indexes.Add(currentIndex);
                            }
                            else if ((relativeRowIndex - row) == (startIndex)) //Triangle Line
                            {
                                indexes.Add(currentIndex);
                            }
                        }

                        currentIndex++;
                    }
                }

                currentIndex = currentIndex - boxSize + maxWidth;
            }


            return indexes;
        }


        public static Collection<int> Grid(int startIndex, int boxSize, int maxWidth, int maxHeight)
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
                            if (currentIndex - rowOffset < maxWidth)
                            {
                                if (row == 0) //Top Wall
                                {
                                    indexes.Add(currentIndex);
                                }
                                else if (relativeRowIndex % boxSize == 0) //Left Wall
                                {
                                    indexes.Add(currentIndex);
                                }
                                else if (row == boxSize - 1) //Bottom Wall
                                {
                                    indexes.Add(currentIndex);
                                }
                                else if ((relativeRowIndex + 1) % boxSize == 0) // Right Wall
                                {
                                    indexes.Add(currentIndex);
                                }
                            }

                            currentIndex++;
                        }
                    }

                    currentIndex = currentIndex - boxSize + maxWidth;
                }


                return indexes;
            }
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
        public static Collection<int> Box(int startIndex, int boxSize, int maxWidth, int maxHeight)
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
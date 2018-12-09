using System;

namespace GroupEMosaicMaker.Model
{
    internal class Randomizer
    {
        #region Methods

        public Image getRandomImage(Image[] images)
        {
            var randomIndex = new Random().Next();
            return images[randomIndex];
        }

        #endregion
    }
}
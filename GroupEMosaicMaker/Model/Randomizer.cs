using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupEMosaicMaker.Model
{
    class Randomizer
    {
        public Image getRandomImage(Image[] images)
        {

            var randomIndex = new Random().Next();
            return images[randomIndex];
        }
    }
}

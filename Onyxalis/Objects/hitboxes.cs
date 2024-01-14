using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects
{
     public class Hitbox
    {
        public (int x, int y) topRight;
        public (int x, int y) topLeft;
        public (int x, int y) bottomRight;
        public (int x, int y) bottomLeft;

        public Hitbox(int sizeX, int sizeY)
        {
            topRight = (sizeX, 0);
            topLeft = (0, 0);
            bottomRight = (sizeX, sizeY);
            bottomLeft = (0, sizeY);
        }
    }
}

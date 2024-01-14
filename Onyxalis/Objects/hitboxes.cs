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
        public (int x, int y) topRight = (10, 0);
        public (int x, int y) topLeft = (0, 0);
        public (int x, int y) bottomRight = (10, 10);
        public (int x, int y) bottomLeft = (0, 10);

        public Hitbox(int sizeX, int sizeY)
        {

        }
    }
}

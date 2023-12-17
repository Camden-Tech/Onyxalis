using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Entities
{
    public class Player : LivingCreature
    {
        public Player() 
        {
            network = 0;
            IpAddress = 0;
            horizontalAcceleration = 1;
            jumpingAcceleration = 2;
            groundDeceleration = 1;
            airDeceleration = 0.25;
            xp = 0;
            levelCap = 50;
            stamina = 0;
            staminaCap = 0;
            hunger = 0;
            hungerCap = 0;
        }

        //Inventory inventory

        //Hotbar hotbar

        public int network;

        public int IpAddress;

        public const double levelMultiplier = 1.2;

        //Friend friend

        public double groundDeceleration;

        public double airDeceleration;

        public double horizontalAcceleration;

        public double jumpingAcceleration;

        public const double gravityAcceleration = 5;

        public int xp;

        public double levelCap;

        public int stamina;

        public int staminaCap;

        public int hunger;

        public int hungerCap;

    }
}

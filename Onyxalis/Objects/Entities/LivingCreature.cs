using Lucene.Net.Support;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Onyxalis.Objects.Math;

namespace Onyxalis.Objects.Entities
{
    public class LivingCreature
    {
        public Vector2 Process_(float delta, Hitbox[] possibleCollide)
            /*
             This function is the main process function for the rigid body LivingCreature class. 
            It processes collisions and physics for rigid bodies.

            delta = 
             */
        {
            Velocity += Acceleration;  // Move fast as hell boi

            deltaX = (Velocity.X * delta) + (Acceleration.X / 2 * (MathF.Pow(delta,2)));  // Get the change in X & Y using the 3rd kinematic equation
            deltaY = (Velocity.Y * delta) + (Acceleration.Y / 2 * (MathF.Pow(delta, 2))); // https://www.khanacademy.org/science/physics/one-dimensional-motion/kinematic-formulas/a/what-are-the-kinematic-formulas?modal=1&referrer=upsell
            oldPos = position;
            position.X += deltaX;  // boilerplate
            position.Y += deltaY;  // please give me an internship Camden's dad

            foreach (Hitbox box in possibleCollide)  // Check collisions
            {
                if (hitbox.CollidesWith(box))
                {
                    position = oldPos;
                    Velocity = Vector2.Zero;
                    Acceleration = Vector2.Zero;
                }
            }
            return position;
        }
        float deltaX, deltaY;  // change in x and y

        private float currentDistance;

        public Vector2 position, oldPos;

        public static HashMap<UUID, LivingCreature> creatures;

        public double health;
        public double maxHealth;
        //EntityType Type;

        //BodyPart[] parts

        public int level;

        public UUID uuid;

        public Vector2 Velocity;

        public Vector2 Acceleration;
        //StatusEffects effects

        public Hitbox hitbox;

        public float gravity;


    }


}

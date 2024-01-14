using Lucene.Net.Support;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Entities
{
    public class LivingCreature
    {
        public void process_(float delta, Vector2 collisions)
        {
            Velocity += Acceleration;  // Move fast as hell boi

            deltaX = (Velocity.X * delta) + (Acceleration.X / 2 * (MathF.Pow(delta,2)));  // Get the change in X & Y using the 3rd kinematic equation
            deltaY = (Velocity.Y * delta) + (Acceleration.Y / 2 * (MathF.Pow(delta, 2))); // https://www.khanacademy.org/science/physics/one-dimensional-motion/kinematic-formulas/a/what-are-the-kinematic-formulas?modal=1&referrer=upsell

            if (collisions.X > 0)
            {
                //  bouncy boi

            }

            position.X += deltaX;  // boilerplate
            position.Y += deltaY;  // please give me an internship Camden's dad
        }
        float deltaX, deltaY;  // change in x and y

        public Vector2 position;

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

        public Vector2[] hitbox;

        public int gravity;


    }


}

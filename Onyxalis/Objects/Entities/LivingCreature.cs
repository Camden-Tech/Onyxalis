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

        public Vector2 position;

        public static HashMap<LivingCreature, UUID> creatures;

        public double health;
        public double maxHealth;
        //EntityType Type;

        //BodyPart[] parts

        public int level;

        public UUID uuid;

        public Vector2 Velocity;

        public Vector2 Acceleration;
        //StatusEffects effects

    }
}

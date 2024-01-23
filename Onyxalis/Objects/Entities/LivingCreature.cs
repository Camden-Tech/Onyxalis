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
using MiNET.Items;
using System.Reflection.Metadata.Ecma335;
using Onyxalis.Objects.Worlds;
using Onyxalis.Objects.Tiles;

namespace Onyxalis.Objects.Entities
{
    public class LivingCreature
    {
        public Vector2 Process_(float delta)
            
            /*
             This function is the main process function for the rigid body LivingCreature class. 
            It processes collisions and physics for rigid bodies.

            delta = difference in time
            possibleCollide = objects that the object could collide with
             */
        {
            
            Velocity += Acceleration;  // Move fast as hell boi

            deltaX = (Velocity.X * delta) + (Acceleration.X / 2 * (MathF.Pow(delta, 2)));  // Get the change in X & Y using the 3rd kinematic equation
            deltaY = (Velocity.Y * delta) + (Acceleration.Y / 2 * (MathF.Pow(delta, 2))); // https://www.khanacademy.org/science/physics/one-dimensional-motion/kinematic-formulas/a/what-are-the-kinematic-formulas?modal=1&referrer=upsell
            oldPos.X = position.X;
            oldPos.Y = position.Y;
            position.X += deltaX;  // boilerplate
            position.Y += deltaY;  // please give me an internship Camden's dad
            hitbox.Update(position, 0);
            Hitbox[] possibleCollide = getTileHitboxesNearCreature();
            foreach (Hitbox box in possibleCollide)  // Check collisions
            {
                if (hitbox.CollidesWith(box))
                {
                  // hitbox.Update(oldPos, 0);
                 //  position = oldPos;
                 //   Velocity = Vector2.Zero;
                //   Acceleration = Vector2.Zero;
                    /*
                    // Step 1: Get the closest vertex on the this object to draw a line from later
                    float lastDistance = -1;
                    Vector2 closestVertex = Vector2.Zero;
                    foreach (Vector2 vertex in hitbox.GetWorldSpaceVertices())
                    {
                        float closest = MathF.Sqrt(MathF.Pow(vertex.X - box.Position.X, 2) + MathF.Pow(vertex.Y - box.Position.Y, 2));
                        if (lastDistance == -1 || closest < lastDistance)
                        {
                            closestVertex = vertex;
                            lastDistance = closest;
                        }
                    }

                    // Step 1.1: Get the 2 closest verticies from the colliding object's position

                    Vector2 closestCollidingVertexOne, closestCollidingVertexTwo;
                    closestCollidingVertexOne = Vector2.Zero;
                    closestCollidingVertexTwo = Vector2.Zero;

                    lastDistance = -1;
                    float secondLastDistance = -1;
                    foreach (Vector2 vertexSelf in hitbox.GetWorldSpaceVertices())
                    {
                        foreach (Vector2 vertexColliding in box.GetWorldSpaceVertices())
                        {
                            if (lastDistance == -1 || MathF.Sqrt(MathF.Pow(vertexColliding.X - vertexSelf.X, 2) + MathF.Pow(vertexColliding.Y - vertexSelf.Y, 2)) > lastDistance)
                            {
                                secondLastDistance = lastDistance;
                                closestCollidingVertexOne = vertexColliding;
                                closestCollidingVertexTwo = closestCollidingVertexOne;
                            }
                        }
                    }

                    /* Step 2: Get the intercept between a line of the two points in 1.1 and 
                       the point in step 1 and the center of the colliding object
                    */
                    /*

                    // Get the points 
                    // Points from closest point to colliding object and the center of the colliding object
                    CalculateIntersection.Point thisLinePointOne = new CalculateIntersection.Point(closestVertex.X, closestVertex.Y);
                    CalculateIntersection.Point thisLinePointTwo = new CalculateIntersection.Point(box.Position.X, box.Position.Y);
                    // Points from the two closest points from the colliding object to the this object
                    CalculateIntersection.Point collidingLinePointOne = new CalculateIntersection.Point(closestCollidingVertexOne.X, closestCollidingVertexOne.Y);
                    CalculateIntersection.Point collidingLinePointTwo = new CalculateIntersection.Point(closestCollidingVertexTwo.X, closestCollidingVertexTwo.Y);

                    CalculateIntersection.Point collidingPoint = CalculateIntersection.lineLineIntersection(collidingLinePointTwo, collidingLinePointTwo, thisLinePointOne, thisLinePointTwo);
                    Vector2 CollidingPoint = new Vector2(collidingPoint.x, collidingPoint.y);

                    // Step 3
                    float moveDistanceX = collidingPoint.x - position.X;  // If movement bugs happpen, it might be cause of this line
                    if (moveDistanceX > 0)
                    {
                        moveDistanceX = moveDistanceX - (hitbox.Vertices[0].X/2);
                    }
                    else
                    {
                        moveDistanceX = moveDistanceX + (hitbox.Vertices[0].X / 2);
                    }
                    float moveDistanceY = collidingPoint.y - position.Y;  // If movement bugs happpen, it might be cause of this line
                    if (moveDistanceY > 0)
                    {
                        moveDistanceY = moveDistanceY - (hitbox.Vertices[0].Y / 2);
                    }
                    else
                    {
                        moveDistanceY = moveDistanceY + (hitbox.Vertices[0].Y / 2);
                    }

                    position.X += moveDistanceX;
                    position.Y += moveDistanceY;
                     */
                }

            }

            return position;
        
        }



        public float deltaX, deltaY;  // change in x and y
        public float scale;
        private float currentDistance;

        public Hitbox[] getTileHitboxesNearCreature()
        {
            List<Hitbox> hitboxes = new List<Hitbox>();
            (int X, int Y) pos = World.findTilePosition((int)position.X, (int)position.Y);
            float textureSizeX = (hitbox.farthestVertice.X / Tile.tilesize) + 1;
            float textureSizeY = (hitbox.farthestVertice.Y / Tile.tilesize);
            for (int X = 0; X < textureSizeX; X++)
            {
                for (int Y = 0; Y < -textureSizeY; Y++)
                {
                    Tile tile = world.tiles[pos.X + X, pos.Y - Y];
                    
                    if (tile != null)
                    {
                        hitboxes.Add(tile.hitbox);
                    }
                }
            }
            return hitboxes.ToArray();
        }



        public Vector2 position, oldPos;

        public Hitbox feet;

        public static HashMap<UUID, LivingCreature> creatures;


        public World world;

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

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
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2[] Vertices { get; set; }

        public Hitbox(Vector2[] vertices, Vector2 position)
        {
            Position = position;
            Vertices = vertices;
        }

        public void Update(Vector2 position, float rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public bool CollidesWith(Hitbox other)
        {
            Vector2[] vertices1 = GetWorldSpaceVertices();
            Vector2[] vertices2 = other.GetWorldSpaceVertices();

            for (int i = 0; i < vertices1.Length; i++)
            {
                Vector2 axis = GetEdgeNormal(vertices1, i);

                float min1, max1, min2, max2;
                ProjectOntoAxis(vertices1, axis, out min1, out max1);
                ProjectOntoAxis(vertices2, axis, out min2, out max2);

                if (max1 < min2 || max2 < min1)
                {
                    return false;
                }
            }

            return true;
        }

        private Vector2[] GetWorldSpaceVertices()
        {
            Vector2[] transformedVertices = new Vector2[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                transformedVertices[i] = Vector2.Transform(Vertices[i], Matrix.CreateRotationZ(Rotation)) + Position;
            }
            return transformedVertices;
        }

        private Vector2 GetEdgeNormal(Vector2[] vertices, int i)
        {
            Vector2 v1 = vertices[i];
            Vector2 v2 = vertices[(i + 1) % vertices.Length];
            Vector2 edge = v2 - v1;
            return new Vector2(-edge.Y, edge.X);
        }

        private void ProjectOntoAxis(Vector2[] vertices, Vector2 axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                float projection = Vector2.Dot(vertices[i], axis);
                if (projection < min)
                {
                    min = projection;
                }
                if (projection > max)
                {
                    max = projection;
                }
            }
        }
    }
}

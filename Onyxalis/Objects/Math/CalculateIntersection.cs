using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Math
{
    /* 
     This code was from https://www.geeksforgeeks.org/program-for-point-of-intersection-of-two-lines/
    Very useful
     */
    public class CalculateIntersection
    {
        public class Point
        {
            public float x, y;

            public Point(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            // Method used to display X and Y coordinates 
            // of a point 
            public static void displayPoint(Point p)
            {
                Console.WriteLine("(" + p.x + ", " + p.y + ")");
            }
        }

        public static Point lineLineIntersection(Point A, Point B, Point C, Point D)
        {
            // Line AB represented as a1x + b1y = c1 
            float a1 = B.y - A.y;
            float b1 = A.x - B.x;
            float c1 = a1 * (A.x) + b1 * (A.y);

            // Line CD represented as a2x + b2y = c2 
            float a2 = D.y - C.y;
            float b2 = C.x - D.x;
            float c2 = a2 * (C.x) + b2 * (C.y);

            float determinant = a1 * b2 - a2 * b1;

            if (determinant == 0)
            {
                // The lines are parallel. This is simplified 
                // by returning a pair of FLT_MAX 
                return new Point(float.MaxValue, float.MaxValue);
            }
            else
            {
                float x = (b2 * c1 - b1 * c2) / determinant;
                float y = (a1 * c2 - a2 * c1) / determinant;
                return new Point(x, y);
            }
        }
    }
}

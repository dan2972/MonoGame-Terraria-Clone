using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Calculations
{
    class CollisionDetector
    {

        public static Boolean intersects(float x1, float y1, float width1, float height1, float x2, float y2, float width2, float height2)
        {
            return x1 + width1 > x2 && x1 < x2 + width2 && y1 + height1 > y2 && y1 < y2 + height2;
        }

        public static bool lineIntersects(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float denominator = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));
            float numerator1 = ((a.Y - c.Y) * (d.X - c.X)) - ((a.X - c.X) * (d.Y - c.Y));
            float numerator2 = ((a.Y - c.Y) * (b.X - a.X)) - ((a.X - c.X) * (b.Y - a.Y));

            if (denominator == 0) return numerator1 == 0 && numerator2 == 0;

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
        }

        public static Vector2 lineIntersectionPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {

            // calculate the distance to intersection point
            float uA = ((d.X - c.X) * (a.Y - c.Y) - (d.Y - c.Y) * (a.X - c.X)) / ((d.Y - c.Y) * (b.X - a.X) - (d.X - c.X) * (b.Y - a.Y));
            float uB = ((b.X - a.X) * (a.Y - c.Y) - (b.Y - a.Y) * (a.X - c.X)) / ((d.Y - c.Y) * (b.X - a.X) - (d.X - c.X) * (b.Y - a.Y));

            // if uA and uB are between 0-1, lines are colliding
            if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
            {
                return new Vector2(a.X + (uA * (b.X - a.X)), a.Y + (uA * (b.Y - a.Y)));
            }
            return new Vector2 (float.MaxValue, float.MaxValue);
        }

    }
}

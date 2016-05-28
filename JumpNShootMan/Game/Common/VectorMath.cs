using System;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace Pong
{
    class VectorMath
    {
        public float VectorToAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.X, -vector.Y);
        }

        public static Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Sin(angle * Math.PI / 180), -(float)Math.Cos(angle * Math.PI / 180));
        }

        // Determines if the lines AB and CD intersect.
        static bool LinesIntersect(PointF A, PointF B, PointF C, PointF D)
        {
            PointF CmP = new PointF(C.X - A.X, C.Y - A.Y);
            PointF r = new PointF(B.X - A.X, B.Y - A.Y);
            PointF s = new PointF(D.X - C.X, D.Y - C.Y);

            float CmPxr = CmP.X * r.Y - CmP.Y * r.X;
            float CmPxs = CmP.X * s.Y - CmP.Y * s.X;
            float rxs = r.X * s.Y - r.Y * s.X;

            if (CmPxr == 0f)
            {
                // Lines are collinear, and so intersect if they have any overlap

                return ((C.X - A.X < 0f) != (C.X - B.X < 0f))
                    || ((C.Y - A.Y < 0f) != (C.Y - B.Y < 0f));
            }

            if (rxs == 0f)
                return false; // Lines are parallel.

            float rxsr = 1f / rxs;
            float t = CmPxs * rxsr;
            float u = CmPxr * rxsr;

            return (t >= 0f) && (t <= 1f) && (u >= 0f) && (u <= 1f);
        }
    }
}

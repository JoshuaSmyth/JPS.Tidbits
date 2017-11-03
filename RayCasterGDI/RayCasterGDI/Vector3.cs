using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayCasterGDI
{
    struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z) {
            X = x; Y=y; Z=z;
        }

        public static float DotProduct(Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Vector3 CrossProduct(Vector3 a, Vector3 b)
        {
            var rv = new Vector3();
            var x = a.Y * b.Z - b.Y * a.Z;
            var y = -(a.X * b.Z - b.X * a.Z);
            var z = a.X * b.Y - b.X * a.Y;
            rv.X = x;
            rv.Y = y;
            rv.Z = z;
            return rv;
        }

        public static float Distance(Vector3 value1, Vector3 value2)
        {
            var d = DistanceSquared(value1, value2);
            return (float) Math.Sqrt(d);
        }

        public static float DistanceSquared(Vector3 a, Vector3 b)
        {
            return  (a.X - b.X) * (a.X - b.X) +
                    (a.Y - b.Y) * (a.Y - b.Y) +
                    (a.Z - b.Z) * (a.Z - b.Z);
        }

        public void Normalize()
        {
            var factor = (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
            factor = 1f / factor;
            X *= factor;
            Y *= factor;
            Z *= factor;
        }

        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return a.X == b.X
                && a.Y == b.Y
                && a.Z == b.Z;
        }

        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return !(a == b);
        }

        
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            a.X -= b.X;
            a.Y -= b.Y;
            a.Z -= b.Z;
            return a;
        }


        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            a.X += b.X;
            a.Y += b.Y;
            a.Z += b.Z;
            return a;
        }

        public static Vector3 operator *(Vector3 value, float scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        public static Vector3 operator *(float scaleFactor, Vector3 value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }
    }
}

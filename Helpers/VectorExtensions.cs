using System;
using System.Numerics;
using HECSFramework.Core;

namespace Helpers
{
    public static class VectorExtensions
    {
        private static Random random;
        private const double DegToRad = Math.PI / 180f;
        private const float RadToDeg = 57.29578f;

        public static Vector3 AsV3(this Vector2 vector2)
            => new Vector3(vector2.X, 0, vector2.Y);

        public static Vector2 AsV2(this Vector3 vector3)
            => new Vector2(vector3.X, vector3.Z);

        public static Vector3 WithRandomHorizontalOffset(this Vector3 vector3, float offset)
        {
            random ??= new Random();
            var angle = random.NextDouble() * 360;
            angle *= DegToRad;
            var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)angle);
            var randomOffset = random.NextDouble() * offset;
            var offsetVector = new Vector3(0, 0, (float)randomOffset);
            return vector3 + offsetVector.Rotate(rotation);
        }

        public static double Angle(this Vector3 from, Vector3 to)
        {
            var num = Math.Sqrt(from.LengthSquared() * (double)to.LengthSquared());
            return num < 1.00000000362749E-15 ? 0.0f : Math.Acos(Math.Clamp(Vector3.Dot(from, to) / num, -1f, 1f)) * RadToDeg;
        }

        public static Vector3 ClampMagnitude(this Vector3 from, float max)
            => @from.LengthSquared() > max * max ? @from.Normalized() * max : @from;

        public static Quaternion LookAt(this Vector3 sourcePoint, Vector3 destPoint)
        {
            Vector3 forwardVector = (destPoint - sourcePoint).Normalized();

            float dot = Vector3.Dot(Vector3.UnitZ, forwardVector);

            if (Math.Abs(dot - (-1.0f)) < 0.000001f)
            {
                return new Quaternion(Vector3.UnitY.X, Vector3.UnitY.Y, Vector3.UnitY.Z, (float)Math.PI);
            }
            if (Math.Abs(dot - (1.0f)) < 0.000001f)
            {
                return new Quaternion();
            }

            float rotAngle = (float)Math.Acos(dot);
            Vector3 rotAxis = Vector3.Cross(Vector3.UnitZ, forwardVector);
            rotAxis = rotAxis.Normalized();
            return CreateFromAxisAngle(rotAxis, rotAngle);
        }

        // just in case you need that function also
        public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
        {
            float halfAngle = angle * .5f;
            float s = (float)System.Math.Sin(halfAngle);
            var q = new Quaternion();
            q.X = axis.X * s;
            q.Y = axis.Y * s;
            q.Z = axis.Z * s;
            q.W = (float)System.Math.Cos(halfAngle);
            return q;
        }


        public static Vector3 Normalized(this Vector3 source)
        {
            float num = source.Length();
            if (num > 9.99999974737875E-06)
                return source / num;
            else
                return Vector3.Zero;
        }

        public static Vector3 MoveTowards(
            this Vector3 current,
            Vector3 target,
            float maxDistanceDelta)
        {
            float num1 = target.X - current.X;
            float num2 = target.Y - current.Y;
            float num3 = target.Z - current.Z;
            float num4 = (float)(num1 * (double)num1 + num2 * (double)num2 + num3 * (double)num3);
            if (num4 == 0.0 || maxDistanceDelta >= 0.0 && num4 <= maxDistanceDelta * (double)maxDistanceDelta)
                return target;
            float num5 = (float)Math.Sqrt(num4);
            return new Vector3(current.X + num1 / num5 * maxDistanceDelta, current.Y + num2 / num5 * maxDistanceDelta, current.Z + num3 / num5 * maxDistanceDelta);
        }

    }
}
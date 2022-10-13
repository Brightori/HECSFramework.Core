using System;
using System.Numerics;

namespace Helpers
{
    public static class QuaternionExtensions
    {
        private const float HalfPi = 1.57079632679489661923f; //pi/2
        private const float Rad2Deg = 57.29578F;
        private const float Pi = 3.14159265358979f;
        private const float DoublePi = Pi * 2;

        public static Vector3 Rotate(this Vector3 point, Quaternion rotation)
        {
            float num1 = rotation.X * 2f;
            float num2 = rotation.Y * 2f;
            float num3 = rotation.Z * 2f;
            float num4 = rotation.X * num1;
            float num5 = rotation.Y * num2;
            float num6 = rotation.Z * num3;
            float num7 = rotation.X * num2;
            float num8 = rotation.X * num3;
            float num9 = rotation.Y * num3;
            float num10 = rotation.W * num1;
            float num11 = rotation.W * num2;
            float num12 = rotation.W * num3;
            Vector3 vector3 = Vector3.Zero;
            vector3.X = (float)((1.0 - (num5 + (double)num6)) * point.X + (num7 - (double)num12) * point.Y + (num8 + (double)num11) * point.Z);
            vector3.Y = (float)((num7 + (double)num12) * point.X + (1.0 - (num4 + (double)num6)) * point.Y + (num9 - (double)num10) * point.Z);
            vector3.Z = (float)((num8 - (double)num11) * point.X + (num9 + (double)num10) * point.Y + (1.0 - (num4 + (double)num5)) * point.Z);
            return vector3;
        }

        public static Quaternion Quaternion(this Vector3 vector)
        {
            vector = new Vector3(vector.Y, vector.X, vector.Z) * (Pi / 180f);
            return System.Numerics.Quaternion.CreateFromYawPitchRoll(vector.X, vector.Y, vector.Z);
        }

        public static Vector3 ToEulerRad(this Quaternion rotation)
        {
            float sqw = rotation.W * rotation.W;
            float sqx = rotation.X * rotation.X;
            float sqy = rotation.Y * rotation.Y;
            float sqz = rotation.Z * rotation.Z;
            float unit = sqx + sqy + sqz + sqw;
            float test = rotation.X * rotation.W - rotation.Y * rotation.Z;
            Vector3 v;

            if (test > 0.4995f * unit)
            {
                v.Y = 2f * (float)Math.Atan2(rotation.Y, rotation.X);
                v.X = HalfPi;
                v.Z = 0;
            }
            else
            if (test < -0.4995f * unit)
            {
                v.Y = -2f * (float)Math.Atan2(rotation.Y, rotation.X);
                v.X = -HalfPi;
                v.Z = 0;
            }
            else
            {
                Quaternion q = new Quaternion(rotation.W, rotation.Z, rotation.X, rotation.Y);
                v.Y = (float)Math.Atan2(2f * q.X * q.W + 2f * q.Y * q.Z, 1 - 2f * (q.Z * q.Z + q.W * q.W));
                v.X = (float)Math.Asin(2f * (q.X * q.Z - q.W * q.Y));
                v.Z = (float)Math.Atan2(2f * q.X * q.Y + 2f * q.Z * q.W, 1 - 2f * (q.Y * q.Y + q.Z * q.Z));
            }
            return NormalizeAnglesRad(v);
        }

        private static Vector3 NormalizeAnglesRad(Vector3 angles)
        {
            angles.X = NormalizeAngleRad(angles.X);
            angles.Y = NormalizeAngleRad(angles.Y);
            angles.Z = NormalizeAngleRad(angles.Z);
            return angles;
        }

        public static Vector3 ToEuler(this Quaternion rotation)
        {
            return ToEulerRad(rotation) * Rad2Deg;
        }

        private static float NormalizeAngleRad(float angle) => Repeat(angle, DoublePi);
        private static float Repeat(float t, float length) => Clamp(t - (float)Math.Floor(t / length) * length, 0.0f, length);
        private static float Clamp(float value, float min, float max) => value < min ? min : (value > max ? max : value);
    }
}
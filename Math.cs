using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    public class Maths
    {
        public static readonly double Deg2Rad = Math.PI / 180.0;
        public static readonly double Rad2Deg = 180.0 / Math.PI;

        public static readonly float Deg2RadF = MathF.PI / 180.0f;
        public static readonly float Rad2DegF = 180.0f / MathF.PI;

        public static readonly double PercentOfCircle = 0.002777777777777778;
        public static readonly float PercentOfCircleF = 0.002777777777777778f;

        public static double ToDegrees(double radians) => radians * Rad2Deg;
        public static double ToRadians(double angle) => angle * Deg2Rad;

        public static float ToDegrees(float radians) => radians * Rad2DegF;
        public static float ToRadians(float angle) => angle * Deg2RadF;

        public static double Atan2(double y, double x)
        {
            if (Math.Abs(y) < 0.0000000001 && x >= 0.0)
                return 0.0;

            var ax = Math.Abs(x);
            var ay = Math.Abs(y);

            if (ax < ay)
            {
                var a = ax / ay;
                var s = a * a;
                var r = 0.25 - (((-0.0464964749 * s + 0.15931422) * s - 0.327622764) * s * a + a) * 0.15915494309189535;
                return x < 0.0 ? y < 0.0 ? 0.5 + r : 0.5 - r : y < 0.0 ? 1.0 - r : r;
            }
            else
            {
                var a = ay / ax;
                var s = a * a;
                var r = (((-0.0464964749 * s + 0.15931422) * s - 0.327622764) * s * a + a) * 0.15915494309189535;
                return x < 0.0 ? y < 0.0 ? 0.5 + r : 0.5 - r : y < 0.0 ? 1.0 - r : r;
            }
        }

        public static float Atan2(float y, float x)
        {
            if (MathF.Abs(y) < 0.0000000001f && x >= 0.0f)
                return 0.0f;

            var ax = MathF.Abs(x);
            var ay = MathF.Abs(y);

            if (ax < ay)
            {
                var a = ax / ay;
                var s = a * a;
                var r = 0.25f - (((-0.0464964749f * s + 0.15931422f) * s - 0.327622764f) * s * a + a) * 0.15915494309189535f;
                return x < 0.0f ? y < 0.0f ? 0.5f + r : 0.5f - r : y < 0.0f ? 1.0f - r : r;
            }
            else
            {
                var a = ay / ax;
                var s = a * a;
                var r = (((-0.0464964749f * s + 0.15931422f) * s - 0.327622764f) * s * a + a) * 0.15915494309189535f;
                return x < 0.0f ? y < 0.0f ? 0.5f + r : 0.5f - r : y < 0.0f ? 1.0f - r : r;
            }
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        public static float Lerp(float value1, float value2, float amount) => value1 + (value2 - value1) * amount;

        public static double Lerp(double value1, double value2, double amount) => value1 + (value2 - value1) * amount;

        public static double Wrap(double value, double min, double max)
        {
            if (value < min)
                return max - ((min - value) % (max - min));
            else return min + ((value - min) % (max - min));
        }

        public static float Wrap(float value, float min, float max)
        {
            if (value < min)
                return max - ((min - value) % (max - min));
            else return min + ((value - min) % (max - min));
        }

        public static int WrapAround(int num, int wrapTo) => (num % wrapTo + wrapTo) % wrapTo;

        public static double WrapAround(double num, double wrapTo)
        {
            while (num < 0)
                num += wrapTo;
            while (num >= wrapTo)
                num -= wrapTo;

            return num;
        }

        public static float WrapAround(float num, float wrapTo)
        {
            while (num < 0)
                num += wrapTo;
            while (num >= wrapTo)
                num -= wrapTo;

            return num;
        }

        public static double Normalize(double x, double y) => Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

        public static float Normalize(float x, float y) => MathF.Sqrt(MathF.Pow(x, 2) + MathF.Pow(y, 2));
    }
}

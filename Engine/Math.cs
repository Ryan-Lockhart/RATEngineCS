namespace rat
{
    public static class Math
    {
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        public static void Clamp(this ref int value, int min, int max)
        {
            if (value < min)
                value = min;

            if (value > max)
                value = max;
        }

        public static int WrapAround(int num, int wrapTo) => (num % wrapTo + wrapTo) % wrapTo;
        public static void WrapAround(this ref int num, int wrapTo) => num = (num % wrapTo + wrapTo) % wrapTo;

        public const double Deg2Rad = System.Math.PI / 180.0;
        public const double Rad2Deg = 180.0 / System.Math.PI;

        public const double PercentOfCircle = 0.002777777777777778;

        public static double ToDegrees(double radians) => radians * Rad2Deg;
        public static void ToDegrees(this ref double radians) => radians *= Rad2Deg;
        public static double ToRadians(double angle) => angle * Deg2Rad;
        public static void ToRadians(this ref double angle) => angle *= Deg2Rad;

        public static double Atan2(double y, double x)
        {
            if (System.Math.Abs(y) < 0.0000000001 && x >= 0.0)
                return 0.0;

            var ax = System.Math.Abs(x);
            var ay = System.Math.Abs(y);

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
            if (System.MathF.Abs(y) < 0.0000000001 && x >= 0.0)
                return 0.0f;

            var ax = System.MathF.Abs(x);
            var ay = System.MathF.Abs(y);

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

        public static float Atan2(in System.Numerics.Vector2 vector)
        {
            var ay = MathF.Abs(vector.Y);

            if (ay < float.Epsilon && vector.X >= 0.0f)
                return 0.0f;

            var ax = MathF.Abs(vector.X);

            var a = ay / ay;
            var s = a * a;
            float r;

            if (ax < ay)
                r = 0.25f - (((-0.0464964749f * s + 0.15931422f) * s - 0.327622764f) * s * a + a) * 0.15915494309189535f;
            else r = (((-0.0464964749f * s + 0.15931422f) * s - 0.327622764f) * s * a + a) * 0.15915494309189535f;

            return vector.X < 0.0f ? vector.Y < 0.0f ? 0.5f + r : 0.5f - r : vector.Y < 0.0f ? 1.0f - r : r;
        }

        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        public static void Clamp(this ref double value, double min, double max)
        {
            if (value < min)
                value = min;

            if (value > max)
                value = max;
        }

        public static double Lerp(double value1, double value2, double amount) => value1 + (value2 - value1) * amount;

        public static double Wrap(double value, double min, double max)
        {
            if (value < min)
                return max - ((min - value) % (max - min));
            else return min + ((value - min) % (max - min));
        }
        public static void Wrap(this ref double value, double min, double max)
        {
            if (value < min)
                value = max - ((min - value) % (max - min));
            else value = min + ((value - min) % (max - min));
        }

        public static double WrapAround(double num, double wrapTo)
        {
            while (num < 0)
                num += wrapTo;
            while (num >= wrapTo)
                num -= wrapTo;

            return num;
        }
        public static void WrapAround(this ref double num, double wrapTo)
        {
            while (num < 0)
                num += wrapTo;
            while (num >= wrapTo)
                num -= wrapTo;
        }

        public static double Normalize(double x, double y) => System.Math.Sqrt(System.Math.Pow(x, 2) + System.Math.Pow(y, 2));
        public static void Normalize(this ref double value, double x, double y) => value = System.Math.Sqrt(System.Math.Pow(x, 2) + System.Math.Pow(y, 2));

        public const float Deg2RadF = MathF.PI / 180.0f;
        public const float Rad2DegF = 180.0f / MathF.PI;

        public const float PercentOfCircleF = 0.002777777777777778f;

        public static float ToRadians(float angle) => angle * Deg2RadF;
        public static void ToRadians(this ref float angle) => angle *= Deg2RadF;

        public static float ToDegrees(float radians) => radians * Rad2DegF;
        public static void ToDegrees(this ref float radians) => radians *= Rad2DegF;

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        public static void Clamp(this ref float value, float min, float max)
        {
            if (value < min)
                value = min;

            if (value > max)
                value = max;
        }

        public static float Lerp(float value1, float value2, float amount) => value1 + (value2 - value1) * amount;

        public static float Wrap(float value, float min, float max)
        {
            if (value < min)
                return max - ((min - value) % (max - min));
            else return min + ((value - min) % (max - min));
        }
        public static void Wrap(this ref float value, float min, float max)
        {
            if (value < min)
                value = max - ((min - value) % (max - min));
            else value = min + ((value - min) % (max - min));
        }

        public static float WrapAround(float num, float wrapTo)
        {
            while (num < 0)
                num += wrapTo;
            while (num >= wrapTo)
                num -= wrapTo;

            return num;
        }
        public static void WrapAround(this ref float num, float wrapTo)
        {
            while (num < 0)
                num += wrapTo;
            while (num >= wrapTo)
                num -= wrapTo;
        }

        public static float Normalize(float x, float y) => MathF.Sqrt(MathF.Pow(x, 2) + MathF.Pow(y, 2));
        public static void Normalize(this ref float value, float x, float y) => value = MathF.Sqrt(MathF.Pow(x, 2) + MathF.Pow(y, 2));
    }
}

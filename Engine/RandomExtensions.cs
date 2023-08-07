using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    public static class RandomExtensions
    {
        public const int HalfMax = int.MaxValue / 2;

        public const double FullRange = (double.MaxValue - double.MinValue) + double.MinValue;
        public const float FullRangeF = (float.MaxValue - float.MinValue) + float.MinValue;

        private static double GetRange(double minValue, double maxValue) => (maxValue - minValue) + minValue;
        private static float GetRange(float minValue, float maxValue) => (maxValue - minValue) + minValue;

        /// <summary>
        /// Generate a double between <see cref="double.MinValue"/> and <see cref="double.MaxValue"/>
        /// </summary>
        /// <returns>A randomly generated double</returns>
        public static double NextRangeDouble(this Random random) => random.NextDouble() * FullRange;

        /// <summary>
        /// Generate a double between variable minimum and maximum
        /// </summary>
        /// <param name="min">The minimum value of the double</param>
        /// <param name="max">The maximum value of the double</param>
        /// <returns>A randomly generated double</returns>
        public static double NextRangeDouble(this Random random, double min, double max) => random.NextDouble() * GetRange(min, max);

        /// <summary>
        /// Generate a collection of doubles between <see cref="double.MinValue"/> and <see cref="double.MaxValue"/>
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of randomly generated doubles</returns>
        public static List<double> GenerateDoubles(this Random random, int count)
        {
            List<double> result = new List<double>(count);

            for (int i = 0; i < count; i++)
                result.Add(random.NextRangeDouble());

            return result;
        }

        /// <summary>
        /// Generate a collection of doubles between variable minimum and maximum
        /// </summary>
        /// <param name="min">The minimum value of the double</param>
        /// <param name="max">The maximum value of the double</param>
        /// <returns>A <see cref="List{T}"/> of randomly generated doubles</returns>
        public static List<double> GenerateDoubles(this Random random, int count, double min, double max)
        {
            List<double> result = new List<double>(count);

            for (int i = 0; i < count; i++)
                result.Add(random.NextRangeDouble(min, max));

            return result;
        }

        /// <summary>
        /// Generate a float between <see cref="float.MinValue"/> and <see cref="float.MaxValue"/>
        /// </summary>
        /// <returns>A randomly generated double</returns>
        public static float NextRangeSingle(this Random random) => random.NextSingle() * FullRangeF;

        /// <summary>
        /// Generate a float between variable minimum and maximum
        /// </summary>
        /// <param name="min">The minimum value of the double</param>
        /// <param name="max">The maximum value of the double</param>
        /// <returns>A randomly generated double</returns>
        public static float NextRangeSingle(this Random random, float min, float max) => random.NextSingle() * GetRange(min, max);

        /// <summary>
        /// Generate a collection of floats between <see cref="float.MinValue"/> and <see cref="float.MaxValue"/>
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of randomly generated floats</returns>
        public static List<float> GenerateSingles(this Random random, int count)
        {
            List<float> result = new List<float>(count);

            for (int i = 0; i < count; i++)
                result.Add(random.NextRangeSingle());

            return result;
        }

        /// <summary>
        /// Generate a collection of floats between variable minimum and maximum
        /// </summary>
        /// <param name="min">The minimum value of the float</param>
        /// <param name="max">The maximum value of the float</param>
        /// <returns>A <see cref="List{T}"/> of randomly generated floats</returns>
        public static List<float> GenerateSingles(this Random random, int count, float min, float max)
        {
            List<float> result = new List<float>(count);

            for (int i = 0; i < count; i++)
                result.Add(random.NextRangeSingle(min, max));

            return result;
        }

        /// <summary>
        /// Generate a bool with an equal probability
        /// </summary>
        /// <returns>A randomly generated bool</returns>
        public static bool NextBool(this Random random) => random.Next() < HalfMax;

        /// <summary>
        /// Generate a bool with a variable probability
        /// </summary>
        /// <param name="probabilty">A percentile for the probability of true</param>
        /// <returns>A randomly generated bool</returns>
        public static bool NextBool(this Random random, double probabilty) => random.Next() < int.MaxValue * probabilty;

        /// <summary>
        /// Generate a collection of bools with an equal probability
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of randomly generated bools</returns>
        public static List<bool> GenerateBools(this Random random, int count)
        {
            List<bool> result = new List<bool>(count);

            for (int i = 0; i < count; i++)
                result.Add(random.NextBool());

            return result;
        }

        /// <summary>
        /// Generate a collection of bools with a variable probability
        /// </summary>
        /// <param name="probability">A percentile for the probability of true</param>
        /// <returns>A <see cref="List{T}"/> of randomly generated bools</returns>
        public static List<bool> GenerateBools(this Random random, int count, double probability)
        {
            List<bool> result = new List<bool>(count);

            for (int i = 0; i < count; i++)
                result.Add(random.NextBool(probability));

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    namespace Random
    {
        public class RandomEngine : IDisposable
        {
            private readonly ulong seed;

            public RandomEngine()
            {
                seed = (ulong)System.Random.Shared.NextInt64();
                set_seed(seed);

                initialize();
            }

            public RandomEngine(ulong seed)
            {
                this.seed = seed;
                set_seed(this.seed);

                initialize();
            }

            public ulong Seed => seed;

            public int NextInt() => next_int();

            public int NextInt(int min, int max) => next_int();

            public float NextFloat() => next_float();

            public float NextFloat(float min, float max) => next_float_min_max(min, max);

            public double NextDouble() => next_double();

            public double NextDouble(double min, double max) => next_double_min_max(min, max);

            public bool NextBool() => next_bool();

            public bool NextBool(double probability) => next_bool_prob(probability);

            public void Dispose() => close();

            [DllImport("RandomEngine.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern void initialize();

            [DllImport("RandomEngine.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern void close();

            [DllImport("RandomEngine.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern void set_seed(ulong seed);

            [DllImport("RandomEngine.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern void unset_seed();

            [DllImport("RandomEngine.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern int next_int();
            [DllImport("RandomEngine.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern int next_int_min_max(int min, int max);

            [DllImport("RandomEngine.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern float next_float();
            [DllImport("RandomEngine.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern float next_float_min_max(float min, float max);

            [DllImport("RandomEngine.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern double next_double();
            [DllImport("RandomEngine.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern double next_double_min_max(double min, double max);

            [DllImport("RandomEngine.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern bool next_bool();
            [DllImport("RandomEngine.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern bool next_bool_prob(double probability);
        }
    }
}

using rat.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    namespace NameGenerator
    {
        public static class Globals
        {
            public static readonly int MinLength = 3;
            public static readonly int MaxLength = 10;

            public static readonly Alphabet Alphabet = new Alphabet();

            public static NameGenerator Basic = new NameGenerator("Basic", "", "Assets\\Names", true);
            //public static NameGenerator Humanoid = new NameGenerator("Humanoid", "", "Assets\\Names", true);
            //public static NameGenerator Greenskin = new NameGenerator("Greenskin", "", "Assets\\Names", true);
            //public static NameGenerator Reptilian = new NameGenerator("Reptilian", "", "Assets\\Names", true);
            //public static NameGenerator Undead = new NameGenerator("Undead", "", "Assets\\Names", true);
        }
    }

    public static class Globals
    {
        public static Random Generator = new Random();
        public static void Reseed(int seed) => Generator = new Random(seed);

        public static Engine Engine;

        public static ulong CurrentID;

        public static int CurrentTurn;
    }
}

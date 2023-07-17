using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    public class Globals
    {
        public static void Reseed(int seed) => Generator = new Random(seed);

        public static void ClearLog() => MessageLog.Clear();

        public static Random Generator = new Random();
        public static Queue<string> MessageLog = new Queue<string>();
    }
}

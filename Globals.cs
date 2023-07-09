using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    public class Globals
    {
        public static Random.RandomEngine Generator = new Random.RandomEngine();
        public static Queue<string> MessageLog = new Queue<string>();
    }
}

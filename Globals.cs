﻿using rat.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    public static class Globals
    {

        public static Random Generator = new Random();
        public static void Reseed(int seed) => Generator = new Random(seed);

        public static Engine Engine;

        public static ulong CurrentID;

        public static int CurrentTurn;
    }
}

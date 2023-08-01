using rat.Constants;
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

        public static Queue<string> MessageLog = new Queue<string>();
        public static int LogSize => MessageLog.Count;

        public static void AppendMessage(in string message) => MessageLog.Enqueue(message);

        public static void TrimLog()
        {
            while (MessageLog.Count > Settings.MaxMessages)
                MessageLog.Dequeue();
        }

        public static void ClearLog() => MessageLog.Clear();

        public static GlyphSet? GameSet;
        public static bool GameSetExists => GameSet != null;

        public static GlyphSet? UISet;
        public static bool UISetExists => UISet != null;

        public static Map? Map;
        public static bool MapExists => Map != null;

        public static Cursor? Cursor;
        public static bool CursorExists => Cursor != null;

        public static Actor? Player;
        public static bool PlayerExists => Player != null;

        private static ulong m_CurrentID;

        public static ulong CurrentID => m_CurrentID++;

        public static List<Actor> Actors = new List<Actor>();
        public static int TotalActors => Actors.Count;

        public static List<Actor> Living = new List<Actor>();
        public static int TotalAlive => Living.Count;
        public static bool AliveMajority => TotalAlive > TotalDead;


        public static List<Actor> Dead = new List<Actor>();
        public static int TotalDead => Dead.Count;
        public static bool DeadMajority => TotalDead > TotalAlive;

        public static bool PopulationSynced => TotalActors == TotalAlive + TotalDead;
        public static bool PopulationDesynced => !PopulationSynced;

        public static RelationMatrix Relations;

        public static int CurrentTurn;
    }
}

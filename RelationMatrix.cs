using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    /// <summary>
    /// An approximation of one actor's <see cref="Relation"/> to another actor)
    /// </summary>
    public enum Opinion
    {
        /// <summary>
        /// The <see cref="Relation"/>'s origin actor is indifferent to the <see cref="Relation"/>'s target actor
        /// </summary>
        Neutral,

        /// <summary>
        /// The <see cref="Relation"/>'s origin actor will fight alongside the <see cref="Relation"/>'s target actor
        /// </summary>
        Ally,

        /// <summary>
        /// The <see cref="Relation"/>'s origin actor is fond of the <see cref="Relation"/>'s target actor
        /// </summary>
        Friend,

        /// <summary>
        /// The <see cref="Relation"/>'s origin actor dislikes the <see cref="Relation"/>'s target actor
        /// </summary>
        Rival,

        /// <summary>
        /// The <see cref="Relation"/>'s origin actor will fight against the <see cref="Relation"/>'s target actor
        /// </summary>
        Foe
    }

    public class Relation
    {
        private readonly Actor m_Origin;
        private readonly Actor m_Target;

        private readonly int m_Maximum;
        private int m_Current;
        private readonly int m_Minimum;

        public Relation(in Actor origin, in Actor target, int current = 0, int maximum = 100, int minimum = -100)
        {
            m_Origin = origin;
            m_Target = target;

            m_Minimum = minimum;
            m_Maximum = maximum;
            Current = current;
        }

        public Actor Origin { get => m_Origin; }
        public Actor Target { get => m_Target; }

        public int Maximum { get => m_Maximum; }
        public int Current { get => m_Current; set => m_Current = System.Math.Clamp(value, Minimum, Maximum); }
        public int Minimum { get => m_Minimum; }

        private int FriendThreshold => Maximum / 3;
        private int AllyThreshold => FriendThreshold * 2;

        private int RivalThreshold => Minimum / 3;
        private int FoeThreshold => RivalThreshold * 2;

        public Opinion Opinion
        {
            get
            {
                if (Current >= AllyThreshold)return Opinion.Ally;
                else if (Current >= FriendThreshold) return Opinion.Friend;
                else if (Current <= FoeThreshold) return Opinion.Foe;
                else if (Current <= RivalThreshold) return Opinion.Rival;
                else return Opinion.Neutral;
            }
        }

        public override string ToString()
            => $"{m_Origin.Name} views {m_Target.Name} as a {Opinion}";
    }

    public class RelationMatrix
    {
        private Dictionary<ulong, Dictionary<ulong, Relation>> m_Relations;

        public RelationMatrix()
        {
            m_Relations = new Dictionary<ulong, Dictionary<ulong, Relation>>();
        }

        public RelationMatrix(in List<Actor> actors)
        {
            m_Relations = new Dictionary<ulong, Dictionary<ulong, Relation>>(actors.Count);

            AddActors(actors);

            foreach (var origin in actors)
                foreach (var target in actors)
                    if (target != origin)
                        AddRelation(origin, target);
        }

        public Relation this[in Actor origin, in Actor target]
            => m_Relations[origin.ID][target.ID];

        public void AddActor(in Actor actor)
            => m_Relations.Add(actor.ID, new Dictionary<ulong, Relation>());

        public void AddActors(in List<Actor> actors)
        {
            foreach (Actor actor in actors)
                AddActor(actor);
        }

        public void AddRelation(in Actor origin, in Actor target, bool generate = true)
        {
            if (!Acquainted(origin, target) && origin != target)
                m_Relations[origin.ID].Add(target.ID, new Relation(origin, target, generate ? Globals.Generator.Next(-100, 101) + GetBiases(origin, target) : 0));
        }

        public void AddRelation(in Actor origin, in Actor target, int current, int maximum = 100, int minimum = -100)
        {
            if (!Acquainted(origin, target) && origin != target)
                m_Relations[origin.ID].Add(target.ID, new Relation(origin, target, current, maximum, minimum));
        }

        /// <summary>
        /// Generate relational biases based on name of actors and whether they are AI
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <returns>A specialized bias to be applied to the generated relation</returns>
        private int GetBiases(in Actor origin, in Actor target)
        {
            int currentBias = 0;

            // NPCs should be hostile to the player
            if (origin.IsAI != target.IsAI)
                currentBias -= 75;

            // Creatures with the same name should be friendlier
            if (origin.Name == target.Name)
                currentBias += 50;
            else currentBias -= 50;

            bool originIsWraith = origin.Name == "Wraith";
            bool targetIsWraith = target.Name == "Wraith";

            bool originIsDraugr = origin.Name == "Draugr";
            bool targetIsDraugr = target.Name == "Draugr";

            bool originIsUndead = originIsWraith || originIsDraugr;
            bool targetIsUndead = targetIsWraith || targetIsDraugr;

            // Wraiths and draugr are only friendly to each other
            if (originIsUndead && targetIsUndead)
                return 200;
            else if ((originIsUndead && !targetIsUndead) || (!originIsUndead && targetIsUndead))
                return -200;

            bool originIsBasilisk = origin.Name == "Basilisk";
            bool targetIsBasilisk = target.Name == "Basilisk";

            bool originIsSerpentman = origin.Name == "Serpentman";
            bool targetIsSerpentman = target.Name == "Serpentman";

            bool originIsReptilian = originIsSerpentman || originIsBasilisk;
            bool targetIsReptilian = targetIsSerpentman || targetIsBasilisk;

            // Serpentmen and basilisks are only friendly to each other
            if (originIsReptilian && targetIsReptilian)
                return 200;
            else if ((originIsReptilian && !targetIsReptilian) || (!originIsReptilian && targetIsReptilian))
                return -200;

            bool originIsGremlin = origin.Name == "Gremlin";
            bool targetIsGremlin = target.Name == "Gremlin";

            bool originIsGoblin = origin.Name == "Goblin";
            bool targetIsGoblin = target.Name == "Goblin";

            bool originIsOrk = origin.Name == "Ork";
            bool targetIsOrk = target.Name == "Ork";

            bool originIsTroll = origin.Name == "Troll";
            bool targetIsTroll = target.Name == "Troll";

            bool originIsGreenskin = originIsGremlin || originIsGoblin || originIsOrk || originIsTroll;
            bool targetIsGreenskin = targetIsGremlin || targetIsGoblin || targetIsOrk || targetIsTroll;

            // Gremlins, goblins, orks and troll are only friendly to each other
            if (originIsGreenskin && targetIsGreenskin)
                return 200;
            else if ((originIsGreenskin && !targetIsGreenskin) || (!originIsGreenskin && targetIsGreenskin))
                return -200;

            return currentBias;
        }

        public void SetRelation(in Actor origin, in Actor target, int value)
        {
            if (Acquainted(origin, target) && origin != target)
                m_Relations[origin.ID][target.ID].Current = value;
            else AddRelation(origin, target, value);
        }

        public bool Acquainted(in Actor origin, in Actor target)
            => m_Relations.ContainsKey(origin.ID) ? m_Relations[origin.ID].ContainsKey(target.ID) : false;
    }
}

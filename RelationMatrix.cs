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

            m_Maximum = maximum;
            m_Current = current;
            m_Minimum = minimum;
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
                        AddRelation(origin, target, -100);
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
            if (!Acquainted(origin, target))
                m_Relations[origin.ID].Add(target.ID, new Relation(origin, target, generate ? Globals.Generator.Next(-100, 101) : 0));
        }

        public void AddRelation(in Actor origin, in Actor target, int current, int maximum = 100, int minimum = -100)
        {
            if (!Acquainted(origin, target))
                m_Relations[origin.ID].Add(target.ID, new Relation(origin, target, current, maximum, minimum));
        }

        public void SetRelation(in Actor origin, in Actor target, int value)
        {
            if (Acquainted(origin, target))
                m_Relations[origin.ID][target.ID].Current = value;
            else AddRelation(origin, target, value);
        }

        public bool Acquainted(in Actor origin, in Actor target)
            => m_Relations.ContainsKey(origin.ID) ? m_Relations[origin.ID].ContainsKey(target.ID) : false;
    }
}

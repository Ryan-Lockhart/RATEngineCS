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
        private ulong m_Origin;
        private ulong m_Target;

        private int m_Maximum;
        private int m_Current;
        private int m_Minimum;

        public Relation(ulong origin, ulong target, int current = 0, int maximum = 100, int minimum = -100)
        {
            m_Origin = origin;
            m_Target = target;

            m_Maximum = maximum;
            m_Current = current;
            m_Minimum = minimum;
        }

        public ulong Origin { get => m_Origin; set => m_Origin = value; }
        public ulong Target { get => m_Target; set => m_Target = value; }

        public int Maximum { get => m_Maximum; set => m_Maximum = value; }
        public int Current { get => m_Current; set => m_Current = value; }
        public int Minimum { get => m_Minimum; set => m_Minimum = value; }

        private int FriendThreshold => Maximum / 3;
        private int AllyThreshold => FriendThreshold * 2;

        private int RivalThreshold => Minimum / 3;
        private int FoeThreshold => RivalThreshold * 2;

        public Opinion Opinion
        {
            get
            {
                if (Current >= AllyThreshold) return Opinion.Ally;
                else if (Current >= FriendThreshold) return Opinion.Friend;
                else if (Current <= FoeThreshold) return Opinion.Foe;
                else if (Current <= RivalThreshold) return Opinion.Rival;
                else return Opinion.Neutral;
            }
        }
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
        }

        public void AddActor(in Actor actor)
            => m_Relations.Add(actor.ID, new Dictionary<ulong, Relation>());

        public void AddActors(in List<Actor> actors)
        {
            foreach (Actor actor in actors)
                AddActor(actor);
        }

        public void AddRelation(in Actor origin, in Actor target)
            => m_Relations[origin.ID].Add(target.ID, new Relation(origin.ID, target.ID));

        public void AddRelation(in Actor origin, in Actor target, int current, int maximum, int minimum)
            => m_Relations[origin.ID].Add(target.ID, new Relation(origin.ID, target.ID, current, maximum, minimum));

        public void SetRelation(in Actor origin, in Actor target, int value)
            => m_Relations[origin.ID][target.ID].Current = value;
    }
}

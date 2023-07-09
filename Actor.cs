using rat.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    public enum Action
    {
        /// <summary>
		/// Default state of action enum
		/// </summary>
        None,
		/// <summary>
		/// Move to specified cell (no bump attack)
		/// </summary>
		MoveTo,
		/// <summary>
		/// Look at specified cell
		/// </summary>
		LookAt,
		/// <summary>
		/// Attack occupant of specifed cell
		/// </summary>
		Attack,
		/// <summary>
		/// Push occupant out of specified cell
		/// </summary>
		Push,
		/// <summary>
		/// Destroy terrain at specified cell
		/// </summary>
		Mine,
	};

    public enum Stance
    {
        Erect,
		Crouch,
		Prone
    };

    public class Actor
    {
        protected ulong m_ID;

        protected bool m_AI;

        protected string m_Name;
        protected string m_Description;

        protected Map? m_Parent;
        protected Cell? m_Residency;

        protected Actor? m_Target;
        protected Stack<Coord>? m_Path;

        protected Glyph m_Glyph;

        protected Coord m_Position;
        protected double m_Angle;
        protected Stance m_Stance;

        protected int m_Reach;

        protected bool m_Dead;
        protected bool m_Bleeding;

        protected double m_MaxHealth;
        protected double m_CurrentHealth;

        protected double m_Damage;
        protected double m_Armor;

        protected double m_Accuracy;
        protected double m_Dodge;

        public ulong ID { get => m_ID; set => m_ID = value; }

        public bool IsAI { get => m_AI; set => m_AI = value; }

        public string Name { get => m_Name; set => m_Name = value; }
        public string Description { get => m_Description; set => m_Description = value; }

        public Map? Parent { get => m_Parent; set => m_Parent = value; }
        public bool HasParent => m_Parent != null;

        public Cell? Residency { get => m_Residency; set => m_Residency = value; }
        public bool HasResidency => m_Residency != null;

        public Actor? Target { get => m_Target; set => m_Target = value; }
        public bool HasTarget => m_Target != null;

        public Stack<Coord>? Path { get => m_Path; set => m_Path = value; }
        public bool HasPath => m_Path != null;

        public Glyph Glyph { get => m_Glyph; set => m_Glyph = value; }

        public Coord Position { get => m_Position; set => m_Position = value; }

        public double Angle { get => m_Angle; set => m_Angle = value; }

        public Stance Stance { get => m_Stance; set => m_Stance = value; }

        public int Reach { get => m_Reach; set => m_Reach = value; }

        public bool Alive => !Dead;
        public bool Dead { get => m_Dead; set => m_Dead = value; }

        public bool Bleeding { get => m_Bleeding; set => m_Bleeding = value; }

        public double MaxHealth { get => m_MaxHealth; set => m_MaxHealth = value; }
        public double CurrentHealth { get => m_CurrentHealth; set => m_CurrentHealth = value; }

        public double Damage { get => m_Damage; set => m_Damage = value; }
        public double Armor { get => m_Armor; set => m_Armor = value; }

        public double Accuracy { get => m_Accuracy; set => m_Accuracy = value; }
        public double Dodge { get => m_Dodge; set => m_Dodge = value; }

        public Actor(ulong id, Cell? startingCell, string name, string description, in Glyph glyph, int reach, float health, float damage, float armor, float accuracy, float dodge, bool randomize, bool isAI = true)
        {
            if (startingCell == null) throw new ArgumentNullException(nameof(startingCell));
            else if (startingCell.Occupied) throw new ArgumentException(nameof(startingCell));

            m_Parent = startingCell.Parent;
            m_Residency = startingCell;
            m_Residency.Occupant = this;

            m_ID = id;

            m_AI = isAI;

            m_Name = name;
            m_Description = description;

            m_Glyph = glyph;

            m_Reach = reach;

            m_Dead = false;
            m_Bleeding = false;

            m_MaxHealth = health + (randomize ? 0.0 : 0.0);
            m_CurrentHealth = MaxHealth;

            m_Damage = damage + (randomize ? 0.0 : 0.0);
            m_Armor = armor + (randomize ? 0.0 : 0.0);

            m_Accuracy = accuracy + (randomize ? 0.0 : 0.0);
            m_Dodge = dodge + (randomize ? 0.0 : 0.0);
        }

        public Actor(ulong id, Map? map, string name, string description, in Glyph glyph, int reach, float health, float damage, float armor, float accuracy, float dodge, bool randomize, bool isAI = true)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));

            m_Parent = map;
            m_Residency = map.FindOpen();

            if (m_Residency == null) throw new NullReferenceException(nameof(m_Residency));

            m_Residency.Occupant = this;

            m_ID = id;

            m_AI = isAI;

            m_Name = name;
            m_Description = description;

            m_Glyph = glyph;

            m_Reach = reach;

            m_Dead = false;
            m_Bleeding = false;

            m_MaxHealth = health + (randomize ? 0.0 : 0.0);
            m_CurrentHealth = MaxHealth;

            m_Damage = damage + (randomize ? 0.0 : 0.0);
            m_Armor = armor + (randomize ? 0.0 : 0.0);

            m_Accuracy = accuracy + (randomize ? 0.0 : 0.0);
            m_Dodge = dodge + (randomize ? 0.0 : 0.0);
        }

        public virtual void Update()
        {

        }

        public virtual void Act(in Coord position, bool offset = true)
        {

        }

        public virtual void Act(in Coord position, Action action, bool offset = true)
        {

        }

        public virtual void Draw(in GlyphSet glyphSet, in Point offset)
        {

        }

		public bool WithinReach(in Coord position)
        {
            Coord deltaPosition = position - Position;

            return Math.Abs(deltaPosition.x) <= Reach && (Math.Abs(deltaPosition.y) <= Reach);
        }

		public bool WithinReach(in Cell cell)
        {
            Coord deltaPosition = cell.Position - Position;

            return Math.Abs(deltaPosition.x) <= Reach && (Math.Abs(deltaPosition.y) <= Reach);
        }

        protected virtual void Move(in Coord where)
        {

        }
		protected virtual void Move(Cell? to)
        {

        }

        protected virtual void LookAt(in Coord where)
        {

        }
		protected virtual void LookAt(Cell? what)
        {

        }

        protected virtual void Attack(in Coord where)
        {

        }
		protected virtual void Attack(Actor? what)
        {

        }

        protected virtual void Defend(Actor? attacker, in Coord direction, float accuracy, float damage)
        {

        }

        protected virtual void Push(in Coord where)
        {

        }
		protected virtual void Push(Actor? what)
        {

        }

        protected virtual void Displace(Actor? displacer, in Coord to, bool offset = true)
        {

        }

        protected virtual void Mine(in Coord where)
        {

        }

        protected virtual void Mine(Cell? what)
        {

        }
    }
}

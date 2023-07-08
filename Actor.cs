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
        protected ulong iD;

        protected bool isAI;

        protected string name;
        protected string description;

        protected Map? parent;
        protected Cell? residency;

        protected Actor? target;
        protected Stack<Coord> path;

        protected Glyph glyph;

        protected Coord position;
        protected double angle;
        protected Stance stance;

        protected int reach;

        protected bool dead;
        protected bool bleeding;

        protected double maxHealth;
        protected double currentHealth;

        protected double damage;
        protected double armor;

        protected double accuracy;
        protected double dodge;

        public ulong ID { get => iD; set => iD = value; }

        public bool IsAI { get => isAI; set => isAI = value; }

        public string Name { get => name; set => name = value; }
        public string Description { get => description; set => description = value; }

        public Map? Parent { get => parent; set => parent = value; }

        public Cell? Residency { get => residency; set => residency = value; }

        public Actor? Target { get => target; set => target = value; }

        public Stack<Coord> Path { get => path; set => path = value; }

        public Glyph Glyph { get => glyph; set => glyph = value; }

        public Coord Position { get => position; set => position = value; }

        public double Angle { get => angle; set => angle = value; }

        public Stance Stance { get => stance; set => stance = value; }

        public int Reach { get => reach; set => reach = value; }

        public bool Dead { get => dead; set => dead = value; }

        public bool Bleeding { get => bleeding; set => bleeding = value; }

        public double MaxHealth { get => maxHealth; set => maxHealth = value; }
        public double CurrentHealth { get => currentHealth; set => currentHealth = value; }

        public double Damage { get => damage; set => damage = value; }
        public double Armor { get => armor; set => armor = value; }

        public double Accuracy { get => accuracy; set => accuracy = value; }
        public double Dodge { get => dodge; set => dodge = value; }

        public Actor(ulong id, string name, string description, in Glyph glyph, int reach, float health, float damage, float armor, float accuracy, float dodge, bool randomize, Cell? startingCell, bool isAI = true)
        {

        }
        public Actor(ulong id, string name, string description, in Glyph glyph, int reach, float health, float damage, float armor, float accuracy, float dodge, bool randomize, Map? map, bool isAI = true)
        {

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

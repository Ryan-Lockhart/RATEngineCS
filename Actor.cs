using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using rat.Primitives;

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
        protected Stack<Coord> m_Path;

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

        public Actor(ref ulong id, Cell? startingCell, string name, string description, in Glyph glyph, int reach, float health, float damage, float armor, float accuracy, float dodge, bool randomize, bool isAI = true)
        {
            if (startingCell == null) throw new ArgumentNullException(nameof(startingCell));
            else if (startingCell.Occupied) throw new ArgumentException(nameof(startingCell));

            m_Parent = startingCell.Parent;

            m_Residency = startingCell;
            m_Residency.Occupant = this;

            m_Position = m_Residency.Position;

            m_ID = id;
            id++;

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

            m_Path = new Stack<Coord>();
        }

        public Actor(ref ulong id, Map? map, string name, string description, in Glyph glyph, int reach, float health, float damage, float armor, float accuracy, float dodge, bool randomize, bool isAI = true)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));

            m_Parent = map;
            m_Residency = map.FindOpen();

            if (m_Residency == null) throw new NullReferenceException(nameof(m_Residency));

            m_Residency.Occupant = this;
            m_Position = m_Residency.Position;

            m_ID = id;
            id++;

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

            m_Path = new Stack<Coord>();
        }

        public virtual void Update()
        {
            if (Parent == null) return;

            if (Alive)
			    m_Dead = m_CurrentHealth <= 0;

            if (Dead || !IsAI) return;

            if (m_Path.Count > 0)
                Act(m_Path.Pop(), false);
            else
            {
                if (Target == null)
                {
                    List<Coord> fov = Parent.CalculateFOV(m_Position, 32.0, m_Angle, 135.0);

                    Actor? closestActor = null;
                    double closestDistance = double.MaxValue;

                    foreach (var position in fov)
                    {
                        Cell? cell = Parent[position];

                        if (cell == null) continue;

                        Actor? actor = cell.Occupant;

                        if (actor == null) continue;

                        if (actor == this || actor.Dead) continue;

                        Coord delta = actor.Position - m_Position;
                        double distance = Math.Normalize(delta.x, delta.y);

                        if (closestActor == null)
                        {
                            closestActor = actor;
                            closestDistance = distance;
                        }
                        else
                        {
                            if (closestDistance > distance)
                            {
                                closestActor = actor;
                                closestDistance = distance;
                            }
                        }
                    }

                    Target = closestActor;
                }

                if (Target != null)
                {
                    if (WithinReach(Target.Position)) Act(Target.Position, false);
                    else
                    {
                        m_Path = Parent.CalculatePath(Position, Target.Position);

                        if (m_Path.Count > 0)
                            Act(m_Path.Pop(), false);
                    }
                }
                else
                {
                    Coord wanderTarget = Coord.Zero;

                    bool is_valid = false;

                    while (!is_valid)
                    {
                        wanderTarget = new Coord(Globals.Generator.Next(-10, 11), Globals.Generator.Next(-10, 11), 0) + m_Position;
                        is_valid = Parent[wanderTarget].Open;
                    }

                    m_Path = Parent.CalculatePath(m_Position, wanderTarget);

                    if (m_Path.Count > 0)
                        Act(m_Path.Pop(), false);
                }
            }
        }

        public virtual void Act(in Coord position, bool offset = true)
        {
            if (Dead) return;

            if (Parent == null || Residency == null) throw new Exception("Orphaned actors cannot act!");

            if (m_Bleeding)
            {
                m_CurrentHealth -= 0.25f;
                Residency.Bloody = true;

                if (m_CurrentHealth <= 0)
                {
                    Dead = true;
                    Residency.AddCorpse(this);
                    Residency.Vacate();

                    if (Name == "Jenkins" || (Target != null && Target.Name == "Jenkins"))
                        Globals.MessageLog.Enqueue((Name == "Jenkins" ? "\n" : "\nThe ") + m_Name + " bled out!\nIt writhes in a pool of its own blood...\n");

                    return;
                }
            }

            Coord actPosition = offset ? m_Position + position : position;

            if (!WithinReach(actPosition)) return;

            Coord delta = actPosition - m_Position;

            Angle = Math.ToDegrees(System.Math.Atan2(delta.y, delta.x));

            if (Parent.WithinBounds(actPosition))
            {
                Cell? currentCell = Parent[actPosition];

                if (currentCell != null)
                {
                    if (!currentCell.Solid && currentCell.Vacant)
                        Move(currentCell);
                    else if (currentCell.Occupied && m_Stance == Stance.Erect)
                        Attack(currentCell.Occupant);
                    else if (currentCell.Solid && m_Stance == Stance.Erect)
                        Mine(currentCell);
                }
            }
        }

        public virtual void Act(in Coord position, Action action, bool offset = true)
        {
            if (Dead) return;

            if (Parent == null || Residency == null) throw new Exception("Orphaned actors cannot act!");

            if (m_Bleeding)
            {
                m_CurrentHealth -= 0.25f;
                Residency.Bloody = true;

                if (m_CurrentHealth <= 0)
                {
                    Dead = true;
                    Residency.AddCorpse(this);

                    if (Name == "Jenkins" || (Target != null && Target.Name == "Jenkins"))
                        Globals.MessageLog.Enqueue((Name == "Jenkins" ? "\n" : "\nThe ") + m_Name + " bled out!\nIt writhes in a pool of its own blood...\n");

                    return;
                }
            }

            Coord actPosition = offset ? m_Position + position : position;

            Coord delta = actPosition - m_Position;

            Angle = Math.ToDegrees(System.Math.Atan2(delta.y, delta.x));

            if (Parent.WithinBounds(actPosition) && action != Action.LookAt)
            {
                Cell? currentCell = Parent[actPosition];

                if (currentCell != null)
                {
                    switch (action)
                    {
                        case Action.Attack:
                            Attack(currentCell.Position);
                            break;
                        case Action.Push:
                            Push(currentCell.Position);
                            break;
                        case Action.Mine:
                            Mine(currentCell.Position);
                            break;
                    }
                }                
            }
            else if (action != Action.LookAt) LookAt(actPosition);
        }

        public virtual void Draw(in GlyphSet glyphSet, in Point offset) => glyphSet.DrawGlyph(m_Glyph, m_Position - offset);

        public bool WithinReach(in Coord position)
        {
            Coord deltaPosition = position - Position;

            return System.Math.Abs(deltaPosition.x) <= Reach && (System.Math.Abs(deltaPosition.y) <= Reach);
        }

		public bool WithinReach(in Cell cell)
        {
            Coord deltaPosition = cell.Position - Position;

            return System.Math.Abs(deltaPosition.x) <= Reach && (System.Math.Abs(deltaPosition.y) <= Reach);
        }

        protected virtual void Move(in Coord where)
        {
            if (Parent == null) return;

            if (!WithinReach(where)) return;

            Cell? cell = Parent[where];
            if (cell == null) return;

            if (cell == m_Residency) return;

            m_Residency.Vacate();
            m_Residency = cell;

            m_Residency.Occupant = this;
            m_Position = where;
        }

		protected virtual void Move(Cell? to)
        {
            if (to == null) return;
            if (m_Residency == null) return;

            if (!WithinReach(to)) return;

            if (to == m_Residency) return;

            m_Residency.Vacate();
            m_Residency = to;

            m_Residency.Occupant = this;
            m_Position = to.Position;
        }

        protected virtual void LookAt(in Coord where)
        {
            Coord delta = where - m_Position;

            m_Angle = Math.ToDegrees(System.Math.Atan2(delta.y, delta.x));
        }

		protected virtual void LookAt(Cell? what)
        {
            if (what == null) return;

            LookAt(what.Position);
        }

        protected virtual void LookAt(Actor? what)
        {
            if (what == null) return;

            LookAt(what.Position);
        }

        protected virtual void Attack(in Coord where)
        {
            if (Parent == null) return;

            if (!WithinReach(where)) return;

            Cell? cell = Parent[where];
            if (cell == null) return;
        }

		protected virtual void Attack(Actor? what)
        {
            if (what == null) return;

            double randomizedAccuracy = Math.Clamp(m_Accuracy * Globals.Generator.NextRangeDouble(0.5, 1.5), 0.0, 3.0);
            double randomizedDamage = Math.Clamp(m_Accuracy * Globals.Generator.NextRangeDouble(0.75, 1.25), 0.0, double.MaxValue);

            if (Name == "Jenkins" || what.Name == "Jenkins")
            {
                if (randomizedAccuracy <= 0.0)
                    Globals.MessageLog.Enqueue(Name == "Jenkins" ? "\n" : "\nThe " + Name + " misses!");
                else if (randomizedAccuracy > 0.0f && randomizedAccuracy <= 0.5f)
                    Globals.MessageLog.Enqueue(Name == "Jenkins" ? "\n" : "\nThe " + Name + " swings with reckless abandon!");
                else if (randomizedAccuracy > 0.5f && randomizedAccuracy <= 1.0f)
                    Globals.MessageLog.Enqueue(Name == "Jenkins" ? "\n" : "\nThe " + Name + " swings wildly!");
                else if (randomizedAccuracy > 1.0f && randomizedAccuracy <= 1.75f)
                    Globals.MessageLog.Enqueue(Name == "Jenkins" ? "\n" : "\nThe " + Name + " swings with skill!");
                else if (randomizedAccuracy > 1.75f && randomizedAccuracy <= 2.5f)
                    Globals.MessageLog.Enqueue(Name == "Jenkins" ? "\n" : "\nThe " + Name + " executes an exquisite swing!");
                else if (randomizedAccuracy > 2.5f)
                    Globals.MessageLog.Enqueue(Name == "Jenkins" ? "\n" : "\nThe " + Name + " unleashes a masterful swing!");
            }

            what.Defend(this, what.Position - Position, randomizedAccuracy, randomizedDamage);
        }

        protected virtual void Defend(Actor? attacker, in Coord direction, double accuracy, double damage)
        {
            if (attacker == null) return;
            if (m_Residency == null) return;
            if (m_Parent == null) return;

            double randomizedDodge = Math.Clamp(m_Dodge * Globals.Generator.NextRangeDouble(0.15, 1.15), 0.0, 1.0);

            bool crit = accuracy - randomizedDodge > 0.5f;

            if (IsAI) LookAt(attacker);

            if (accuracy > randomizedDodge)
            {
                double modifiedDamage = Math.Clamp(damage - (crit ? m_Armor * 0.25f : m_Armor), 0.0f, double.MaxValue);

                if (crit) m_Bleeding = true;

                if (modifiedDamage > 0.0f)
                {
                    m_CurrentHealth = Math.Clamp(m_CurrentHealth - modifiedDamage, 0.0f, double.MaxValue);

                    m_Residency.Bloody = true;
                    int bloodParticles = crit ? 4 : 2;

                    if (m_CurrentHealth <= 0.0f)
                    {
                        m_Dead = true;
                        m_Residency.AddCorpse(this);
                        m_Residency.Vacate();

                        bloodParticles *= 2;

                        if (Name == "Jenkins" || attacker.Name == "Jenkins")
                            Globals.MessageLog.Enqueue(crit ? (Name == "Jenkins" ? "\n" : "\nThe ") + m_Name + " was decapitated!\nIts head rolls onto the bloodied ground...\n" : Name == "Jenkins" ? "\n" : "\nThe " + m_Name + " was slain!\nIts blood stains the ground...\n");
                    }
                    else
                    {
                        if (Name == "Jenkins" || attacker.Name == "Jenkins")
                            Globals.MessageLog.Enqueue(crit ? (Name == "Jenkins" ? "\n" + m_Name + "'s mortality has been shaken...\nAnother blow may be fatal!\n" : "\nThe " + m_Name + " suffered a terrible blow...\nIt quivers with haggard anticipation!\n") : (Name == "Jenkins" ? "\n" : "\nThe ") + m_Name + " was merely wounded...\nThe gods demand more bloodshed!\n");
                    }

                    for (int i = 0; i < bloodParticles; i++)
                    {
                        double spatterDistance = Globals.Generator.NextRangeDouble(0.1, 1.0);

                        Coord spatterPos = new Coord(
                            (long)(spatterDistance * Globals.Generator.NextRangeDouble(direction.x, direction.x * (crit ? 4 : 2))) + m_Position.x,
                            (long)(spatterDistance * Globals.Generator.NextRangeDouble(direction.y, direction.y * (crit ? 4 : 2))) + m_Position.y,
                            m_Position.z
                        );

                        Cell? spatterCell = m_Parent[spatterPos];

                        if (spatterCell != null)
                            spatterCell.Bloody = true;
                    }
                }
                else
                {
                    if (Name == "Jenkins" || attacker.Name == "Jenkins")
                        Globals.MessageLog.Enqueue(
                            Name == "Jenkins" ? "\n" + Name + "' armor absorbed the blow...\nIts burnished surface remains stalwart!\n" : "\nThe " + Name + "'s armor absorbed the blow...\nStrike its weakpoints!\n");
                }
            }
            else
            {
                if (Name == "Jenkins" || attacker.Name == "Jenkins")
                    Globals.MessageLog.Enqueue(
                        (Name == "Jenkins" ? "\n" + Name + " evaded its attack..." : "\nThe " + Name + " evaded your attack...") + (Name == "Jenkins" ? "\nJenkins' confidence is bolstered!\n" : "\nIt goads you to strike again!\n"));
            }
        }

        protected virtual void Push(in Coord where)
        {
            if (Parent == null) return;

            if (!WithinReach(where)) return;

            Cell? cell = Parent[where];
            if (cell == null) return;

            if (cell.Occupied) Push(cell.Occupant);
        }

		protected virtual void Push(Actor? what)
        {
            if (what == null) return;

            Coord deltaPos = what.Position - Position;

            what.Displace(this, deltaPos, true);
        }

        protected virtual void Displace(Actor? displacer, in Coord to, bool offset = true)
        {
            if (displacer == null) return;
            if (Parent == null) return;
            if (m_Residency == null) return;

            Coord displacePos = offset ? m_Position + to : to;

            Cell? where = Parent[displacePos];
            if (where == null) return;

            if (where.Vacant)
            {
                m_Residency.Vacate();
                m_Residency = where;

                m_Residency.Occupant = this;
                m_Position = m_Residency.Position;
            }
            else
            {
                m_CurrentHealth -= 0.5f * displacer.Damage;
                Globals.MessageLog.Enqueue(m_Name == "Jenkins" ? "\n" : "\nThe" + m_Name + " was shoved into a wall!\n" + m_Name == "Jenkins" ? "He suffered blunt force trauma!" : "It suffered blunt force trauma!");
            }
        }

        protected virtual void Mine(in Coord where)
        {
            if (Parent == null) return;

            if (!WithinReach(where)) return;

            Cell? cell = Parent[where];
            if (cell == null) return;

            cell.Solid = false;
            cell.Opaque = false;
        }

        protected virtual void Mine(Cell? what)
        {
            if (what == null) return;

            what.Solid = false;
            what.Opaque = false;
        }
    }
}

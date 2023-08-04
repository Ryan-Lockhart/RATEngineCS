using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// Do nothing on turn
        /// </summary>
        Wait,
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

        protected Map m_Parent;
        protected Cell m_Residency;

        protected Actor? m_Target;
        protected Stack<Point>? m_Path;

        protected Glyph m_Glyph;

        protected Point m_Position;
        protected Angle m_Heading;
        protected Stance m_Stance;

        protected int m_Reach;

        protected bool m_Dead;
        protected bool m_Bleeding;

        protected float m_MaxHealth;
        protected float m_CurrentHealth;

        protected float m_Damage;
        protected float m_Armor;

        protected float m_Accuracy;
        protected float m_Dodge;

        public ulong ID { get => m_ID; set => m_ID = value; }

        public bool IsAI { get => m_AI; set => m_AI = value; }

        public string Name { get => m_Name; set => m_Name = value; }
        public string Description { get => m_Description; set => m_Description = value; }

        public Map Parent { get => m_Parent; set => m_Parent = value; }
        public Cell  Residency { get => m_Residency; set => m_Residency = value; }

        public Actor? Target { get => m_Target; set => m_Target = value; }
        public bool HasTarget => m_Target != null;

        public Stack<Point>? Path { get => m_Path; set => m_Path = value; }
        public bool HasPath => m_Path != null;

        public Glyph Glyph { get => m_Glyph; set => m_Glyph = value; }

        public Point Position { get => m_Position; set => m_Position = value; }

        public Angle Angle { get => m_Heading; set => m_Heading = value; }

        public Stance Stance { get => m_Stance; set => m_Stance = value; }

        public int Reach { get => m_Reach; set => m_Reach = value; }

        public bool Alive => !Dead;
        public bool Dead { get => m_Dead; set => m_Dead = value; }

        public bool Bleeding { get => m_Bleeding; set => m_Bleeding = value; }

        public float MaxHealth { get => m_MaxHealth; set => m_MaxHealth = value; }
        public float CurrentHealth
        {
            get => m_CurrentHealth;
            set => m_CurrentHealth = System.Math.Min(value, MaxHealth);
        }

        public float Damage { get => m_Damage; set => m_Damage = value; }
        public float Armor { get => m_Armor; set => m_Armor = value; }

        public float Accuracy { get => m_Accuracy; set => m_Accuracy = value; }
        public float Dodge { get => m_Dodge; set => m_Dodge = value; }

        public Actor(Cell? startingCell, string name, string description, in Glyph glyph, int reach, float health, float damage, float armor, float accuracy, float dodge, bool randomize, bool isAI = true)
        {
            if (startingCell == null) throw new ArgumentNullException(nameof(startingCell));
            else if (startingCell.Occupied) throw new ArgumentException(nameof(startingCell));

            m_Parent = startingCell.Parent;

            m_Residency = startingCell;
            m_Residency.Occupant = this;

            m_Position = m_Residency.Position;

            m_ID = Globals.CurrentID++;

            m_AI = isAI;

            m_Name = name;
            m_Description = description;

            m_Glyph = glyph;

            m_Reach = reach;

            m_Dead = false;
            m_Bleeding = false;

            m_MaxHealth = health * (randomize ? Globals.Generator.NextRangeSingle(0.75f, 1.25f) : 1.0f);
            m_CurrentHealth = MaxHealth;

            m_Damage = damage * (randomize ? Globals.Generator.NextRangeSingle(0.75f, 1.25f) : 1.0f);
            m_Armor = armor * (randomize ? Globals.Generator.NextRangeSingle(0.75f, 1.25f) : 1.0f);

            m_Accuracy = accuracy * (randomize ? Globals.Generator.NextRangeSingle(0.75f, 1.25f) : 1.0f);
            m_Dodge = dodge * (randomize ? Globals.Generator.NextRangeSingle(0.75f, 1.25f) : 1.0f);

            m_Path = new Stack<Point>();
        }

        public Actor(in Map map, string name, string description, in Glyph glyph, int reach, float health, float damage, float armor, float accuracy, float dodge, bool randomize, bool isAI = true)
        {
            m_Parent = map;

            var randomCell = m_Parent!.FindOpen();
            if (randomCell == null) throw new Exception($"{nameof(map)} has no open cells!");
            else if (randomCell.Occupied) throw new Exception($"{nameof(randomCell)} is not vacant!");

            m_Residency = randomCell;

            m_Residency.Occupant = this;
            m_Position = m_Residency.Position;

            m_ID = Globals.CurrentID++;

            m_AI = isAI;

            m_Name = name;
            m_Description = description;

            m_Glyph = glyph;

            m_Reach = reach;

            m_Dead = false;
            m_Bleeding = false;

            m_MaxHealth = health * (randomize ? Globals.Generator.NextRangeSingle(0.75f, 1.25f) : 1.0f);
            m_CurrentHealth = MaxHealth;

            m_Damage = damage * (randomize ? Globals.Generator.NextRangeSingle(0.75f, 1.25f) : 1.0f);
            m_Armor = armor * (randomize ? Globals.Generator.NextRangeSingle(0.75f, 1.25f) : 1.0f);

            m_Accuracy = accuracy * (randomize ? Globals.Generator.NextRangeSingle(0.75f, 1.25f) : 1.0f);
            m_Dodge = dodge * (randomize ? Globals.Generator.NextRangeSingle(0.75f, 1.25f) : 1.0f);

            m_Path = new Stack<Point>();
        }

        public virtual void Update(in Engine engine)
        {
            if (Parent == null) return;

            if (Alive)
			    m_Dead = m_CurrentHealth <= 0;

            if (Dead || !IsAI) return;

            List<Point> fov = Parent.CalculateFOV(m_Position, 32.0, m_Heading.Degrees, 135.0);

            List<Actor> allies = new List<Actor>();
            List<Actor> foes = new List<Actor>();

            Actor? closestAlly = null;
            Actor? closestFoe = null;

            double closestAllyDistance = double.MaxValue;
            double closestFoeDistance = double.MaxValue;

            foreach (var position in fov)
            {
                Cell? cell = Parent[position];

                if (cell == null) continue;

                var actor = cell.Occupant;

                if (actor == null) continue;
                if (actor == this) continue;
                if (actor.Dead) continue;

                var relation = engine.Population.Relations[this, actor];

                if (relation == null) continue;

                double distance = (actor.Position - m_Position).Magnitude;

                switch (relation.Opinion)
                {
                    case Opinion.Neutral:
                        // Ignore
                        break;
                    case Opinion.Ally:
                        allies.Add(actor);

                        if (closestAlly == null)
                        {
                            closestAlly = actor;
                            closestAllyDistance = distance;
                        }
                        else
                        {
                            if (closestAllyDistance > distance)
                            {
                                closestAlly = actor;
                                closestAllyDistance = distance;
                            }
                        }
                        break;
                    case Opinion.Friend:
                        // Ignore
                        break;
                    case Opinion.Rival:
                        // Ignore
                        break;
                    case Opinion.Foe:
                        foes.Add(actor);

                        if (closestFoe == null)
                        {
                            closestFoe = actor;
                            closestFoeDistance = distance;
                        }
                        else
                        {
                            if (closestFoeDistance > distance)
                            {
                                closestFoe = actor;
                                closestFoeDistance = distance;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            if (closestAlly != null && closestAlly.HasTarget)
                Target = closestAlly.Target;
            else if (closestFoe != null)
                Target = closestFoe;
            else Target = null;

            if (Target != null)
            {
                if (WithinReach(Target.Position)) Act(Target.Position, false);
                else
                {
                    //Act(Point.Direction(Position, Target.Position), true);

                    m_Path = Parent.CalculatePath(Position, Target.Position);

                    if (HasPath && m_Path!.Count > 0)
                        Act(m_Path.Pop(), false);
                }
            }
            else if (m_Path == null || m_Path.Count <= 0)
            {
                Point wanderTarget = Point.Zero;

                bool is_valid = false;

                while (!is_valid)
                {
                    wanderTarget = new Point(Globals.Generator.Next(-10, 11), Globals.Generator.Next(-10, 11)) + m_Position;

                    var cell = Parent[wanderTarget];
                    if (cell == null) continue;

                    is_valid = cell.Open;
                }

                m_Path = Parent.CalculatePath(m_Position, wanderTarget);
            }

            if (!HasTarget && HasPath && m_Path!.Count > 0)
                Act(m_Path.Pop(), false);
        }

        public virtual void Act(in Point position, bool offset = true)
        {
            if (Parent == null || Residency == null) throw new Exception("Orphaned actors cannot act!");

            if (Bleeding) Bleed(0.025f);

            if (Dead) return;

            Point actPosition = offset ? m_Position + position : position;

            if (!WithinReach(actPosition)) return;

            Point delta = actPosition - m_Position;

            Angle = new Angle(Math.ToDegrees(System.Math.Atan2(delta.y, delta.x)));

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

        public virtual void Act(in Point position, Action action, bool offset = true)
        {
            if (Parent == null || Residency == null) throw new Exception("Orphaned actors cannot act!");

            if (Bleeding) Bleed(0.025f);

            if (Dead) return;

            if (action == Action.Wait) return;

            Point actPosition = offset ? m_Position + position : position;

            Point delta = actPosition - m_Position;

            Angle = new Angle(Math.ToDegrees(System.Math.Atan2(delta.y, delta.x)));

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

        public virtual void Draw(in GlyphSet glyphSet, in Point offset) => glyphSet.DrawGlyph(Constants.Characters.Entity[m_Heading.Direction], m_Glyph.color, m_Position - offset);

        public bool WithinReach(in Point position)
            => Point.Distance(Position, position) <= Reach;

		public bool WithinReach(in Cell cell)
            => Point.Distance(Position, cell.Position) <= Reach;

        /// <summary>
        /// Cause the actor to lose health equal to their max health multiplied by blood loss percentage
        /// </summary>
        /// <param name="bloodLoss">The percentage of blood lost</param>
        protected virtual void Bleed(float bloodLoss)
        {
            m_CurrentHealth -= m_MaxHealth * bloodLoss;
            Residency.Bloody = true;

            if (m_CurrentHealth <= 0)
            {
                Die(null, CauseOfDeath.Exsanguination);

                return;
            }
        }

        protected virtual void Die(Actor? killer, CauseOfDeath cause)
        {
            Dead = true;
            Bleeding = false;
            Residency.AddCorpse(this, cause);
            Residency.Vacate();

            if (Name == "Jenkins" || (killer != null && killer.Name == "Jenkins"))
            {
                switch (cause)
                {
                    case CauseOfDeath.Exsanguination:
                        Globals.Engine.MessageScreen.MessageLog.AppendMessage((Name == "Jenkins" ? "" : "\nThe ") + m_Name + " bled out! It writhes in a pool of its own blood...\n");
                        return;
                    case CauseOfDeath.Decapitation:
                        Globals.Engine.MessageScreen.MessageLog.AppendMessage((Name == "Jenkins" ? "" : "\nThe ") + m_Name + " was decapitated! Its head rolls onto the bloodied ground...\n");
                        return;
                    default:
                        Globals.Engine.MessageScreen.MessageLog.AppendMessage((Name == "Jenkins" ? "" : "\nThe ") + m_Name + " was slain! Its blood stains the ground...\n");
                        return;
                }
            }
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

        protected virtual void LookAt(in Point where)
        {
            var delta = where - m_Position;

            m_Heading = new Angle(Math.ToDegrees(System.Math.Atan2(delta.y, delta.x)));
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

        protected virtual void Attack(in Point where)
        {
            if (Parent == null) return;

            if (!WithinReach(where)) return;

            var cell = Parent[where];
            if (cell == null) return;

            var actor = cell.Occupant;

            if (actor == null) return;
            Attack(actor);
        }

		protected virtual void Attack(Actor? what)
        {
            if (what == null) return;
            if (what == this)
            {
                Target = null;
                return;
            }

            Target = what;

            float randomizedAccuracy = Math.Clamp(m_Accuracy * Globals.Generator.NextRangeSingle(0.5f, 1.5f), 0.0f, 3.0f);
            float randomizedDamage = Math.Clamp(m_Damage * Globals.Generator.NextRangeSingle(0.75f, 1.25f), 0.0f, float.MaxValue);

            if (Name == "Jenkins" || what.Name == "Jenkins")
            {
                if (randomizedAccuracy <= 0.0f)
                    Globals.Engine.MessageScreen.MessageLog.AppendMessage((Name == "Jenkins" ? "" : "The ") + Name + " misses!");
                else if (randomizedAccuracy > 0.0f && randomizedAccuracy <= 0.5f)
                    Globals.Engine.MessageScreen.MessageLog.AppendMessage((Name == "Jenkins" ? "" : "The ") + Name + " swings with reckless abandon!");
                else if (randomizedAccuracy > 0.5f && randomizedAccuracy <= 1.0f)
                    Globals.Engine.MessageScreen.MessageLog.AppendMessage((Name == "Jenkins" ? "" : "The ") + Name + " swings wildly!");
                else if (randomizedAccuracy > 1.0f && randomizedAccuracy <= 1.75f)
                    Globals.Engine.MessageScreen.MessageLog.AppendMessage((Name == "Jenkins" ? "" : "The ") + Name + " swings with skill!");
                else if (randomizedAccuracy > 1.75f && randomizedAccuracy <= 2.5f)
                    Globals.Engine.MessageScreen.MessageLog.AppendMessage((Name == "Jenkins" ? "" : "The ") + Name + " executes an exquisite swing!");
                else if (randomizedAccuracy > 2.5f)
                    Globals.Engine.MessageScreen.MessageLog.AppendMessage((Name == "Jenkins" ? "" : "The ") + Name + " unleashes a masterful swing!");
            }

            what.Defend(this, what.Position - Position, randomizedAccuracy, randomizedDamage);
        }

        protected virtual void Defend(Actor? attacker, in Point direction, float accuracy, float damage)
        {
            if (attacker == null) return;

            Target = attacker;

            Globals.Engine.Population.Relations[this, attacker].Current -= 25;

            float randomizedDodge = Math.Clamp(m_Dodge * Globals.Generator.NextRangeSingle(0.15f, 1.15f), 0.0f, 1.0f);

            bool crit = accuracy - randomizedDodge > 0.5;

            if (IsAI) LookAt(attacker);

            if (accuracy > randomizedDodge)
            {
                float modifiedDamage = Math.Clamp(damage - (crit ? m_Armor * 0.25f : m_Armor), 0.0f, float.MaxValue);

                if (crit) Bleeding = true;

                if (modifiedDamage > 0.0)
                {
                    m_CurrentHealth = Math.Clamp(m_CurrentHealth - modifiedDamage, 0.0f, float.MaxValue);

                    Residency.Bloody = true;
                    int bloodParticles = crit ? 4 : 2;

                    if (m_CurrentHealth <= 0.0)
                    {
                        Die(attacker, crit ? CauseOfDeath.Decapitation : CauseOfDeath.Slashed);

                        bloodParticles *= 2;
                    }
                    else
                    {
                        if (Name == "Jenkins" || attacker.Name == "Jenkins")
                            Globals.Engine.MessageScreen.MessageLog.AppendMessage(crit ? (Name == "Jenkins" ? "" + m_Name + "'s mortality has been shaken... Another blow may be fatal!" : "The " + m_Name + " suffered a terrible blow... It quivers with haggard anticipation!") : (Name == "Jenkins" ? "" : "The ") + m_Name + " was merely wounded... The gods demand more bloodshed!");
                    }

                    for (int i = 0; i < bloodParticles; i++)
                    {
                        double spatterDistance = Globals.Generator.NextRangeDouble(0.1, 1.0);

                        Point spatterPos = new Point(
                            (int)(spatterDistance * Globals.Generator.NextRangeDouble(direction.x, direction.x * (crit ? 4 : 2))) + m_Position.x,
                            (int)(spatterDistance * Globals.Generator.NextRangeDouble(direction.y, direction.y * (crit ? 4 : 2))) + m_Position.y
                        );

                        Cell? spatterCell = m_Parent[spatterPos];

                        if (spatterCell != null)
                            spatterCell.Bloody = true;
                    }
                }
                else
                {
                    if (Name == "Jenkins" || attacker.Name == "Jenkins")
                        Globals.Engine.MessageScreen.MessageLog.AppendMessage(
                            Name == "Jenkins" ? "" + Name + "' armor absorbed the blow... Its burnished surface remains stalwart!" : "The " + Name + "'s armor absorbed the blow... Strike its weakpoints!");
                }
            }
            else
            {
                if (Name == "Jenkins" || attacker.Name == "Jenkins")
                    Globals.Engine.MessageScreen.MessageLog.AppendMessage(
                        (Name == "Jenkins" ? "" + Name + " evaded its attack..." : "The " + Name + " evaded your attack...") + (Name == "Jenkins" ? " Jenkins' confidence is bolstered!" : " It goads you to strike again!"));
            }
        }

        protected virtual void Push(in Point where)
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

            Point deltaPos = what.Position - Position;

            what.Displace(this, deltaPos, true);
        }

        protected virtual void Displace(Actor? displacer, in Point to, bool offset = true)
        {
            if (displacer == null) return;
            if (Parent == null) return;
            if (m_Residency == null) return;

            Point displacePos = offset ? m_Position + to : to;

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
                Globals.Engine.MessageScreen.MessageLog.AppendMessage(m_Name == "Jenkins" ? "" : "The" + m_Name + " was shoved into a wall!" + m_Name == "Jenkins" ? "He suffered blunt force trauma!" : "It suffered blunt force trauma!");
            }
        }

        protected virtual void Mine(in Point where)
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

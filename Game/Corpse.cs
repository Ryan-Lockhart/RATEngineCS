using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    public enum CauseOfDeath
    {
        /// <summary>
        /// The loss of one's head
        /// </summary>
        Decapitation,

        /// <summary>
        /// Bleeding to death
        /// </summary>
        Exsanguination,

        /// <summary>
        /// Death by falling
        /// </summary>
        Defenestration,

        /// <summary>
        /// Blunt force death
        /// </summary>
        Bludgeoned,

        /// <summary>
        /// Death by overwhelming force
        /// </summary>
        Crushed,

        /// <summary>
        /// Stabbed to death
        /// </summary>
        Skewered,

        /// <summary>
        /// Death by cuts
        /// </summary>
        Slashed,
    }

    public class Corpse
    {
        /// <summary>
        /// The actor that this corpse represents
        /// </summary>
        private Actor? m_Actor;

        /// <summary>
        /// The place where this corpse was created
        /// </summary>
        private Cell? m_PlaceOfDeath;
        /// <summary>
        /// The time at which this corpse was created
        /// </summary>
        private int m_TimeOfDeath;
        /// <summary>
        /// The time at which this corpse will be decayed
        /// </summary>
        private int m_TimeOfDecay;

        /// <summary>
        /// Has the controlled actor seen this corpse after it died?
        /// </summary>
        private bool m_Observed;
        /// <summary>
        /// Is this corpse eligible for resurrection?
        /// </summary>
        private bool m_Resurrectable;

        public Actor? Actor { get => m_Actor; }
        public Cell? PlaceOfDeath { get => m_PlaceOfDeath; }
        public int TimeOfDeath { get => m_TimeOfDeath; }
        public bool Observed { get => m_Observed; set => m_Observed = value; }
        public bool Resurrectable { get => m_Resurrectable && Globals.CurrentTurn < m_TimeOfDecay; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actor">The actor which has died</param>
        /// <param name="ressurectable">Is it possible to raise this corpse from the dead?</param>
        /// <param name="decayTime">The amount of time in turns it takes for this corpse to fully decay</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Corpse(Actor? actor, bool ressurectable = false, int decayTime = 10)
        {
            if (actor == null) throw new ArgumentNullException(nameof(actor));

            m_Actor = actor;
            m_PlaceOfDeath = actor.Residency;
            m_TimeOfDeath = Globals.CurrentTurn;
            m_Observed = false;
            m_Resurrectable = ressurectable;
            m_TimeOfDecay = m_TimeOfDeath + decayTime;
        }

        /// <summary>
        /// Resurrect this corpse at the place of its death
        /// </summary>
        /// <returns>Whether or not the resurrection was successful</returns>
        public bool Resurrect(bool force = false)
            => Resurrect(PlaceOfDeath, false, force);

        /// <summary>
        /// Resurrect this corpse in a specified cell
        /// </summary>
        /// <param name="cell">The cell at which the corpse will be resurrected</param>
        /// <param name="allowFallback">If the specified resurrection site is occupied, allow the corpse to be resurrected normally?</param>
        /// <returns>The outcome of the resurrection</returns>
        public bool Resurrect(Cell? cell, bool allowFallback = false, bool force = false)
        {
            if (Actor == null) return false;
            if (cell == null) return false;

            if (!Resurrectable && !force) return false;

            if (cell.Occupied)
            {
                if (!allowFallback || PlaceOfDeath == null || PlaceOfDeath.Occupied)
                    return false;
            }
            else Actor.Residency = cell;
            
            Actor.Residency.Occupant = Actor;
            Actor.CurrentHealth = Actor.MaxHealth;
            Actor.Name = $"Undead {Actor.Name}";
            Actor.Dead = false;

            Actor.Residency.Corpses.Remove(this);

            return true;
        }
    }
}

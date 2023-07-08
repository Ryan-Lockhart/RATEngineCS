using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
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
        private DateTime m_TimeOfDeath;

        /// <summary>
        /// Has the controlled actor seen this corpse after it died?
        /// </summary>
        private bool m_Observed;
        /// <summary>
        /// Is this corpse eligible for resurrection?
        /// </summary>
        private bool m_Resurrectable;

        /// <summary>
        /// The amount of decay this corpse has built up
        /// </summary>
        private double m_Decay;

        public Actor? Actor { get => m_Actor; set => m_Actor = value; }
        public Cell? PlaceOfDeath { get => m_PlaceOfDeath; set => m_PlaceOfDeath = value; }
        public DateTime TimeOfDeath { get => m_TimeOfDeath; set => m_TimeOfDeath = value; }
        public bool Observed { get => m_Observed; set => m_Observed = value; }
        public bool Resurrectable { get => m_Resurrectable; set => m_Resurrectable = value; }
        public double Decay { get => m_Decay; set => m_Decay = value; }

        public Corpse(Actor? actor, bool ressurectable = false)
        {
            if (actor == null) throw new ArgumentNullException(nameof(actor));

            m_Actor = actor;
            m_PlaceOfDeath = actor.Residency;
            m_TimeOfDeath = DateTime.Now;
            m_Observed = false;
            m_Resurrectable = ressurectable;
            m_Decay = 1.0;
        }

        /// <summary>
        /// Resurrect this corpse at the place of its death
        /// </summary>
        /// <returns>Whether or not the resurrection was successful</returns>
        public bool Resurrect()
        {
            if (Actor == null) return false;
            if (PlaceOfDeath == null) return false;

            if (!Resurrectable) return false;

            if (PlaceOfDeath.Occupied) return false;

            Actor.CurrentHealth = Actor.MaxHealth * Decay;
            Actor.Dead = false;

            return true;
        }

        /// <summary>
        /// Resurrect this corpse in a specified cell
        /// </summary>
        /// <param name="cell">The cell at which the corpse will be resurrected</param>
        /// <param name="allowFallback">If the specified resurrection site is occupied, allow the corpse to be resurrected normally?</param>
        /// <returns>The outcome of the resurrection</returns>
        public bool Resurrect(Cell? cell, bool allowFallback = false)
        {
            if (Actor == null) return false;
            if (cell == null) return false;

            if (!Resurrectable) return false;

            if (cell.Occupied)
            {
                if (!allowFallback || PlaceOfDeath == null || PlaceOfDeath.Occupied)
                    return false;
            }
            else Actor.Residency = cell;

            Actor.CurrentHealth = Actor.MaxHealth * Decay;
            Actor.Dead = false;

            return true;
        }
    }
}

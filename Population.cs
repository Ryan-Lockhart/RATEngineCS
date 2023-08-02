using rat.Constants;
using rat.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    public class Population
    {
        private List<Actor> m_Actors;
        public List<Actor> Actors { get => m_Actors; set => m_Actors = value; }
        public int TotalActors => m_Actors.Count;

        private List<Actor> m_Living;
        public List<Actor> Living { get => m_Living; set => m_Living = value; }
        public int TotalAlive => m_Living.Count;
        public bool AliveMajority => TotalAlive > TotalDead;

        private List<Actor> m_Dead;
        public List<Actor> Dead { get => m_Dead; set => m_Dead = value; }
        public int TotalDead => m_Dead.Count;
        public bool DeadMajority => TotalDead > TotalAlive;

        public bool PopulationSynced => TotalActors == TotalAlive + TotalDead;
        public bool PopulationDesynced => !PopulationSynced;

        private RelationMatrix m_Relations;
        public RelationMatrix Relations => m_Relations;

        public Population()
        {
            m_Actors = new List<Actor>();
            m_Living = new List<Actor>();
            m_Dead = new List<Actor>();

            m_Relations = new RelationMatrix();
        }

        public Population(Actor? player)
        {
            m_Actors = new List<Actor>();
            m_Living = new List<Actor>();

            if (player != null)
            {
                m_Actors.Add(player);
                m_Living.Add(player);
            }

            m_Dead = new List<Actor>();

            m_Relations = new RelationMatrix(m_Actors);
        }

        public Population(Actor? player, int summonCount)
        {
            m_Actors = new List<Actor>();
            m_Living = new List<Actor>();

            if (player != null)
            {
                m_Actors.Add(player);
                m_Living.Add(player);
            }

            m_Dead = new List<Actor>();

            m_Relations = new RelationMatrix(m_Actors);

            SummonEnemies(summonCount);
        }

        /// <summary>
        /// BRING OUT YER DEAD!
        /// </summary>
        public void CollectDead()
        {
            List<Actor> the_living = new List<Actor>();
            List<Actor> the_dead = new List<Actor>();

            foreach (var maybe_living in Living)
            {
                if (maybe_living == null) continue;

                if (maybe_living.Alive) the_living.Add(maybe_living);
                else the_dead.Add(maybe_living);
            }

            if (Settings.AllowResurrection)
            {
                // Check for resurrection!
                foreach (var maybe_dead in Dead)
                {
                    if (maybe_dead == null) continue;

                    if (maybe_dead.Dead) the_dead.Add(maybe_dead);
                    else the_living.Add(maybe_dead);
                }
            }

            Living = the_living;
            Dead = the_dead;
        }

        /// <summary>
        /// Fetch their fetid souls from the warp!
        /// </summary>
        public void SummonEnemies(int amount)
        {
            var enemies = new List<Actor>();

            for (int i = 0; i < amount; i++)
            {
                long next = Globals.Generator.NextBool(0.00666) ? 7 : Globals.Generator.NextBool(0.75) ? Globals.Generator.Next(0, Settings.Population.MaximumEnemyTypes / 2) : Globals.Generator.Next(Settings.Population.MaximumEnemyTypes / 2, Settings.Population.MaximumEnemyTypes - 1);

                Actor? newlySpawned = null;

                switch (next)
                {
                    case 0:
                        newlySpawned = new Actor(Globals.Engine.Map, "Gremlin", "A dimunitive creature with a cunning disposition", new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightYellow), 1, 1.5f, 0.65f, 0.0f, 0.266f, 0.475f, true);
                        break;
                    case 1:
                        newlySpawned = new Actor(Globals.Engine.Map, "Goblin", "A dexterous and selfish humanoid", new Glyph(Characters.Entity[Cardinal.Central], Colors.LightGreen), 1, 3.5f, 1.25f, 0.5f, 0.375f, 0.675f, true);
                        break;
                    case 2:
                        newlySpawned = new Actor(Globals.Engine.Map, "Ork", "A brutal and violent humanoid", new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightOrange), 1, 12.5f, 3.5f, 1.25f, 0.666f, 0.275f, true);
                        break;
                    case 3:
                        newlySpawned = new Actor(Globals.Engine.Map, "Troll", "A giant humaniod of great strength", new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightRed), 1, 25.0f, 12.5f, 2.5f, 0.125f, 0.114f, true);
                        break;
                    case 4:
                        newlySpawned = new Actor(Globals.Engine.Map, "Draugr", "An undead servant of a wraith", new Glyph(Characters.Entity[Cardinal.Central], Colors.DarkMarble), 1, 7.5f, 2.5f, 5.0f, 0.675f, 0.221f, true);
                        break;
                    case 5:
                        newlySpawned = new Actor(Globals.Engine.Map, "Basilisk", "A large hexapedal reptile of terrible power", new Glyph(Characters.Entity[Cardinal.Central], Colors.Intrite), 1, 17.5f, 7.5f, 3.75f, 0.425f, 0.321f, true);
                        break;
                    case 6:
                        newlySpawned = new Actor(Globals.Engine.Map, "Serpentman", "A slithering humanoid with superior agility", new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightBlue), 1, 17.5f, 7.5f, 3.75f, 0.425f, 0.321f, true);
                        break;
                    case 7:
                        newlySpawned = new Actor(Globals.Engine.Map, "Wraith", "An eldritch abomination! Woe upon thee...", new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightMagenta), 2, 125.0f, 75.0f, 30.0f, 0.75f, 0.975f, true);
                        break;
                }

                if (newlySpawned == null) continue;

                enemies.Add(newlySpawned);

                Actors.Add(newlySpawned);
                Living.Add(newlySpawned);
            }

            m_Relations.AddActors(enemies);

            foreach (var actor in Actors)
                foreach (var other_actor in Actors)
                    m_Relations.AddRelation(actor, other_actor);

            Globals.Engine.SetLastSummon();
        }
    }
}

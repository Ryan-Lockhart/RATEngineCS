﻿using rat.Constants;
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
            var names = new List<string>(NameGenerator.Globals.Basic.GenerateNames(amount, true));

            for (int i = 0; i < amount; i++)
            {
                string name = names[i];

                // 0.666% chance to generate a wraith
                // 75% chance to generate a Gremlin, Goblin, or Ork
                // 25% chance to generate a Troll, Draugr, Basilisk, or Serpentman
                long next =
                    Globals.Generator.NextBool(0.00666) ? 7 :
                    Globals.Generator.NextBool(0.75) ? Globals.Generator.Next(0, Settings.Population.MaximumEnemyTypes / 2) :
                    Globals.Generator.Next(Settings.Population.MaximumEnemyTypes / 2, Settings.Population.MaximumEnemyTypes - 1);

                var newlySpawned = next switch
                {
                    0 => new Actor
                    (
                        Globals.Engine.Map, name, Clades.Greenskin["Gremlin"],
                        new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightYellow),
                        1, 1.5f, 0.65f, 0.0f, 0.266f, 0.475f, true
                    ),

                    1 => new Actor
                    (
                        Globals.Engine.Map, name, Clades.Greenskin["Goblin"],
                        new Glyph(Characters.Entity[Cardinal.Central], Colors.LightGreen),
                        1, 3.5f, 1.25f, 0.5f, 0.375f, 0.675f, true
                    ),

                    2 => new Actor
                    (
                        Globals.Engine.Map, name, Clades.Greenskin["Ork"],
                        new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightOrange),
                        1, 12.5f, 3.5f, 1.25f, 0.666f, 0.275f, true
                    ),

                    3 => new Actor
                    (
                        Globals.Engine.Map, name, Clades.Greenskin["Troll"],
                        new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightRed),
                        1, 25.0f, 12.5f, 2.5f, 0.125f, 0.114f, true
                    ),

                    4 => new Actor
                    (
                        Globals.Engine.Map, name, Clades.Undead["Draugr"],
                        new Glyph(Characters.Entity[Cardinal.Central], Colors.DarkMarble),
                        1, 7.5f, 2.5f, 5.0f, 0.675f, 0.221f, true
                    ),

                    5 => new Actor
                    (
                        Globals.Engine.Map, name, Clades.Reptilian["Basilisk"],
                        new Glyph(Characters.Entity[Cardinal.Central], Colors.Intrite),
                        1, 35.0f, 12.75f, 11.25f, 0.325f, 0.257f, true
                    ),

                    6 => new Actor
                    (
                        Globals.Engine.Map, name, Clades.Reptilian["Serpentman"],
                        new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightBlue),
                        1, 17.5f, 7.5f, 3.75f, 0.525f, 0.621f, true
                    ),

                    7 => new Actor
                    (
                        Globals.Engine.Map, name, Clades.Undead["Wraith"],
                        new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightMagenta),
                        2, 125.0f, 75.0f, 30.0f, 0.75f, 0.975f, true
                    ),

                    _ => null
                };

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

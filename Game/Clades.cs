using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    public class Clade
    {
        private string m_Name;
        private string m_Description;

        private Dictionary<string, Species> m_Species;

        public Clade(string name, string description)
        {
            m_Name = name;
            m_Description = description;

            m_Species = new Dictionary<string, Species>();
        }

        public Clade(string name, string description, List<(string, string)> speciesDescriptors)
        {
            m_Name = name;
            m_Description = description;

            m_Species = new Dictionary<string, Species>();

            foreach (var species in speciesDescriptors)
                Add(new Species(this, species.Item1, species.Item2));
        }

        public string Name { get => m_Name; set => m_Name = value; }
        public string Description { get => m_Description; set => m_Description = value; }

        public Species this[string key] => m_Species[key];

        public void Add(Species species)
        {
            if (!m_Species.ContainsKey(species.Name))
                m_Species.Add(species.Name, species);
        }

        public void Remove(Species species)
        {
            if (m_Species.ContainsKey(species.Name))
                m_Species.Remove(species.Name);
        }

        public bool Contains(Species species)
            => m_Species.ContainsKey(species.Name);
    }

    public static class Clades
    {
        public static readonly Clade Humanoid =
            new Clade("Humanoid", "The civilized creatures of the world",
                new List<(string, string)>()
                {
                    ("Human", "A creature fond of civilization, law, and order"),
                }
            );

        public static readonly Clade Greenskin =
            new Clade("Greenskin", "Savage creatures with a tribal cohesion",
                new List<(string, string)>()
                {
                    ("Gremlin", "A dimunitive creature with a cunning disposition"),
                    ("Goblin", "A dexterous and selfish humanoid"),
                    ("Ork", "A brutal and violent humanoid"),
                    ("Troll", "A giant humaniod of great strength"),
                }
            );

        public static readonly Clade Reptilian =
            new Clade("Reptilian", "Cold-blooded creatures",
                new List<(string, string)>()
                {
                    ("Basilisk", "A large hexapedal reptile of terrible power"),
                    ("Serpentman", "A slithering humanoid with superior agility"),
                }
            );

        public static readonly Clade Undead =
            new Clade("Undead", "Creatures without a heartbeat",
                new List<(string, string)>()
                {
                    ("Draugr", "An undead servant of a wraith"),
                    ("Wraith", "An eldritch abomination! Woe upon thee..."),
                }
            );
    }

    public class Species
    {
        private Clade m_Parent;

        private string m_Name;
        private string m_Description;

        public Species(Clade parent, string name, string description)
        {
            m_Parent = parent;
            m_Name = name;
            m_Description = description;
        }

        public Clade Parent { get => m_Parent; set => m_Parent = value; }
        public string Name { get => m_Name; set => m_Name = value; }
        public string Description { get => m_Description; set => m_Description = value; }
    }
}

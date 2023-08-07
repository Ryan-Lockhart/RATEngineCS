namespace rat
{
    namespace NameGenerator
    {
        public class NameList
        {
            private List<string> _names;

            public NameList() => _names = new List<string>();

            public NameList(string path)
            {
                _names = new List<string>();

                Load(path);
            }

            public NameList(ref string[] names)
            {
                _names = new List<string>();

                foreach (string name in names)
                    if (name != "")
                        _names.Add(name);
            }

            public void Load(string path, bool overwrite = true)
            {
                if (overwrite) _names.Clear();

                using (var reader = new StreamReader(path))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(';', StringSplitOptions.RemoveEmptyEntries);

                        foreach (var value in values)
                            Add(value);
                    }

                    Sort();
                }
            }

            public List<string> Expose() => _names;

            public void Save(string path)
            {
                using (var writer = new StreamWriter(path, false))
                {
                    Sort();

                    char previousLetter = '\0';

                    foreach (string name in _names)
                    {
                        if (name == "") break;

                        if (char.ToLower(name[0]) != char.ToLower(previousLetter) && previousLetter != '\0')
                            writer.WriteLine();

                        writer.Write(name + ";");

                        previousLetter = name[0];
                    }
                }
            }

            /// <summary>
            /// Add a name to the name list
            /// </summary>
            /// <param name="name">The name to be added</param>
            /// <returns>Returns <see langword="true"/> if the name was added, <see langword="false"/> if it already contained it</returns>
            public bool Add(string name)
            {
                if (name == "" || _names.Contains(name)) return false;
                else _names.Add(name);

                return true;
            }

            /// <summary>
            /// Remove a name from the name list
            /// </summary>
            /// <param name="name">The name to be removed</param>
            /// <returns>Returns <see langword="true"/> if the name was removed, <see langword="false"/> if it didn't contain it</returns>
            public bool Remove(string name)
            {
                if (name == "" || !_names.Contains(name)) return false;
                else _names.Remove(name);

                return true;
            }

            public ProbabilityMatrix CreateMatrix()
            {
                double[,] weights = new double[Globals.Alphabet.Size, Globals.Alphabet.Size];

                foreach (string name in _names)
                {
                    for (int i = 0; i < name.Length - 1; i++)
                    {
                        Letter letter_one = Globals.Alphabet[name[i]];
                        Letter letter_two = Globals.Alphabet[name[i + 1]];

                        weights[letter_one.Index, letter_two.Index]++;
                    }
                }

                return new ProbabilityMatrix(ref weights);
            }

            public void Sort() => _names.Sort();
        }
    }
}

namespace rat
{
    namespace NameGenerator
    {
        public class ProbabilityMatrix
        {
            private double[] _weights;

            public ProbabilityMatrix(string path)
            {
                _weights = new double[Globals.Alphabet.Size * Globals.Alphabet.Size];

                Reset();
                Load(path);
            }

            public ProbabilityMatrix(ref double[,] weights)
            {
                _weights = new double[Globals.Alphabet.Size * Globals.Alphabet.Size];

                for (int i = 0; i < Globals.Alphabet.Size; i++)
                    for (int j = 0; j < Globals.Alphabet.Size; j++)
                        this[i, j] = weights[i, j];

                Normalize();
            }

            public ProbabilityMatrix(NameList names)
            {
                _weights = new double[Globals.Alphabet.Size * Globals.Alphabet.Size];

                Reset();
                Generate(names);
            }

            public void Reset()
            {
                for (int i = 0; i < Globals.Alphabet.Size; i++)
                    for (int j = 0; j < Globals.Alphabet.Size; j++)
                        this[i, j] = 0.0;

                Normalize();
            }

            public void Load(ref string[] names)
            {
                foreach (string name in names)
                {
                    if (name.Length < 1) return;

                    for (int i = 0; i < name.Length - 1; i++)
                    {
                        Letter letter_one = Globals.Alphabet[name[i]];
                        Letter letter_two = Globals.Alphabet[name[i + 1]];

                        this[letter_one.Index, letter_two.Index]++;
                    }
                }

                Normalize();
            }

            public void Load(string path)
            {
                using (var reader = new StreamReader(path))
                {
                    List<string> weights = new List<string>();

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        if (line == null) continue;

                        var values = line.Split(';', StringSplitOptions.RemoveEmptyEntries);

                        foreach (var value in values)
                            weights.Add(value);
                    }

                    int count = 0;

                    for (int i = 0; i < Globals.Alphabet.Size; i++)
                        for (int j = 0; j < Globals.Alphabet.Size; j++)
                        {
                            this[i, j] = double.Parse(weights[count]);

                            count++;
                        }
                }

                Normalize();
            }

            public void Save(string path)
            {
                Normalize();

                using (var writer = new StreamWriter(path))
                {
                    for (int i = 0; i < Globals.Alphabet.Size; i++)
                    {
                        string line = "";

                        for (int j = 0; j < Globals.Alphabet.Size; j++)
                        {
                            if (j != 0)
                                line += ";";

                            line += this[i, j].ToString();
                        }

                        writer.WriteLine(line);
                    }
                }
            }

            public double this[int i, int j]
            {
                get { return _weights[j * Globals.Alphabet.Size + i]; }
                set { _weights[j * Globals.Alphabet.Size + i] = value; }
            }

            public double this[char i, char j]
            {
                get { return _weights[Globals.Alphabet[j].Index * Globals.Alphabet.Size + Globals.Alphabet[i].Index]; }
                set { _weights[Globals.Alphabet[j].Index * Globals.Alphabet.Size + Globals.Alphabet[i].Index] = value; }
            }

            private void Normalize()
            {
                for (int i = 0; i < Globals.Alphabet.Size; i++)
                    NormalizeIndex(i);
            }

            private void NormalizeIndex(int i)
            {
                double count = 0.0;

                for (int j = 0; j < Globals.Alphabet.Size; j++)
                    count += this[i, j];

                if (count > 0.0)
                    for (int j = 0; j < Globals.Alphabet.Size; j++)
                        this[i, j] /= count;
                else
                    for (int j = 0; j < Globals.Alphabet.Size; j++)
                        this[i, j] = System.Math.Pow(Globals.Alphabet.Size, -1);
            }

            public void Generate(NameList names)
            {
                foreach (string name in names.Expose())
                    for (int i = 0; i < name.Length - 1; i++)
                        this[name[i], name[i + 1]]++;

                Normalize();
            }

            public void Modulate(string name, double value)
            {
                for (int i = 0; i < name.Length - 1; i++)
                    this[name[i], name[i + 1]] *= Math.Clamp(value, 0.0, 2.0);

                Normalize();
            }
        }
    }
}

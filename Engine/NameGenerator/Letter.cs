namespace rat
{
    namespace NameGenerator
    {
        public struct Letter : IEquatable<Letter>
        {
            public int Index;

            public char UpperCase;
            public char LowerCase;

            public bool Vowel;
            public bool Consonant;

            /// <summary>
            /// The empty letter
            /// </summary>
            public Letter()
            {
                Index = -1;
                UpperCase = '\0';
                LowerCase = '\0';
                Vowel = false;
                Consonant = false;
            }

            /// <summary>
            /// An empty letter (invalid in most cases)
            /// </summary>
            public static readonly Letter None = new Letter();

            public Letter(int index, char upperCase, char lowerCase, bool vowel, bool consonant)
            {
                Index = index;

                UpperCase = upperCase;
                LowerCase = lowerCase;

                Vowel = vowel;
                Consonant = consonant;
            }

            public bool IsValid()
            {
                if (Index < 0)
                    return false;
                else if (UpperCase == '\0')
                    return false;
                else if (LowerCase == '\0')
                    return false;
                else if (Vowel == false && Consonant == false)
                    return false;
                else return true;
            }

            public static Letter GetRandom(bool needConsonant, bool needVowel)
            {
                Letter ret = new Letter();
                bool done = false;

                while (!done)
                {
                    ret = Globals.Alphabet[rat.Globals.Generator.Next(0, 26)];

                    if ((needConsonant && ret.Vowel) || (needVowel && ret.Consonant))
                        done = false;
                    else done = true;
                }

                return ret;
            }

            public static Letter GetRandom(int prevNum, bool needConsonant, bool needVowel, ProbabilityMatrix matrix)
            {
                Letter ret = new Letter();
                bool done = false;

                while (!done)
                {
                    ret = GetWeighted(prevNum, matrix);

                    if ((needConsonant && ret.Vowel) || (needVowel && ret.Consonant))
                        done = false;
                    else done = true;
                }

                return ret;
            }

            public static Letter GetWeighted(int i, ProbabilityMatrix matrix)
            {
                double rand = rat.Globals.Generator.NextDouble();
                double total = 0.0;

                for (int j = 0; j < Globals.Alphabet.Size; j++)
                {
                    total += matrix[i, j];

                    if (rand <= total || j == Globals.Alphabet.Size)
                        return Globals.Alphabet[j];
                }

                Console.Error.WriteLine("Failed to find weighted letter! Returning empty letter...");
                return new Letter();
            }

            public override bool Equals(object? obj)
            {
                return obj is Letter letter && Equals(letter);
            }

            public bool Equals(Letter other)
            {
                return Index == other.Index &&
                       UpperCase == other.UpperCase &&
                       LowerCase == other.LowerCase &&
                       Vowel == other.Vowel &&
                       Consonant == other.Consonant;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Index, UpperCase, LowerCase, Vowel, Consonant);
            }

            public static bool operator ==(Letter left, Letter right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Letter left, Letter right)
            {
                return !(left == right);
            }
        }
    }
}
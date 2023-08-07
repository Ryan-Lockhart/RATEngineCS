namespace rat
{
    namespace NameGenerator
    {
        public class Alphabet
        {
            private Dictionary<int, Letter> _letters;

            /// <summary>
            /// The default alphabet (A-Z)
            /// </summary>
            public Alphabet()
            {
                _letters = new Dictionary<int, Letter>()
                {
                    {  0, new Letter( 0, 'A', 'a', true, false) },
                    {  1, new Letter( 1, 'B', 'b', false, true) },
                    {  2, new Letter( 2, 'C', 'c', false, true) },
                    {  3, new Letter( 3, 'D', 'd', false, true) },
                    {  4, new Letter( 4, 'E', 'e', true, false) },
                    {  5, new Letter( 5, 'F', 'f', false, true) },
                    {  6, new Letter( 6, 'G', 'g', false, true) },
                    {  7, new Letter( 7, 'H', 'h', false, true) },
                    {  8, new Letter( 8, 'I', 'i', true, false) },
                    {  9, new Letter( 9, 'J', 'j', false, true) },
                    { 10, new Letter(10, 'K', 'k', false, true) },
                    { 11, new Letter(11, 'L', 'l', false, true) },
                    { 12, new Letter(12, 'M', 'm', false, true) },
                    { 13, new Letter(13, 'N', 'n', false, true) },
                    { 14, new Letter(14, 'O', 'o', true, false) },
                    { 15, new Letter(15, 'P', 'p', false, true) },
                    { 16, new Letter(16, 'Q', 'q', false, true) },
                    { 17, new Letter(17, 'R', 'r', false, true) },
                    { 18, new Letter(18, 'S', 's', false, true) },
                    { 19, new Letter(19, 'T', 't', false, true) },
                    { 20, new Letter(20, 'U', 'u', true, false) },
                    { 21, new Letter(21, 'V', 'v', false, true) },
                    { 22, new Letter(22, 'W', 'w', false, true) },
                    { 23, new Letter(23, 'X', 'x', false, true) },
                    { 24, new Letter(24, 'Y', 'y',  true, true) },
                    { 25, new Letter(25, 'Z', 'z', false, true) },
                };
            }

            public Alphabet(ref Dictionary<int, Letter> letters) => _letters = letters;

            public Letter this[int index]
            {
                get
                {
                    try { return _letters[index]; }
                    catch { return Letter.None; }
                }
            }

            public Letter this[char ch]
            {
                get
                {
                    foreach (Letter letter in _letters.Values)
                    {
                        if (letter.UpperCase == ch || letter.LowerCase == ch)
                            return letter;
                    }

                    return Letter.None;
                }
            }

            public bool Contains(char ch)
            {
                foreach (Letter letter in _letters.Values)
                {
                    if (letter.UpperCase == ch || letter.LowerCase == ch)
                        return true;
                }

                return false;
            }

            public int Size { get { return _letters.Count; } }
        }
    }
}
using System.Xml.Linq;

namespace rat
{
    namespace NameGenerator
    {
        public class NameGenerator
        {
            public enum NameStatus
            {
                Pending,
                Approved,
                Rejected
            }

            public class NameEntry
            {
                private int index;
                private string name;
                private NameStatus status;

                public NameEntry(int index, string name, NameStatus status)
                {
                    Index = index;
                    Name = name;
                    Status = status;
                }

                public int Index { get => index; set => index = value; }
                public string Name { get => name; set => name = value; }
                public NameStatus Status { get => status; set => status = value; }
            }

            private string _name;
            private string _description;
            private string _path;

            private NameList _names;
            private ProbabilityMatrix _weights;

            private Stack<string> _approvalQueue;

            public int QueueSize => _approvalQueue.Count;

            public string Name { get => _name; set => _name = value; }
            public string Description { get => _description; set => _description = value; }

            /// <summary>
            /// Manually process the approval queue one name at a time
            /// </summary>
            public void PopSingle()
            {
                while (_approvalQueue.Count > 0)
                {
                    bool nameProcessed = false;
                    string currentName = _approvalQueue.Pop();

                    while (!nameProcessed)
                    {
                        Console.Clear();
                        Console.WriteLine($"Name: {currentName}\nApprove? (y/n): ");
                        var key = Console.ReadKey(true);

                        switch (key.Key)
                        {
                            case ConsoleKey.Y:
                                _names.Add(currentName);
                                _weights.Modulate(currentName, 1.01);
                                nameProcessed = true;
                                break;
                            case ConsoleKey.N:
                                _weights.Modulate(currentName, 0.99);
                                nameProcessed = true;
                                break;
                            default:
                                Console.Clear();
                                Console.WriteLine("Please enter a valid input...");
                                Console.ReadKey(true);
                                break;
                        }
                    }
                }
            }

            /// <summary>
            /// Manually process the approval queue in a list UI
            /// </summary>
            public void PopMulti()
            {
                NameEntry[] nameEntries = new NameEntry[_approvalQueue.Count];

                var names = _approvalQueue.ToList();
                names.Sort();
                _approvalQueue.Clear();

                for (int i = 0; i < nameEntries.Length; i++)
                    nameEntries[i] = new NameEntry(i, names[i], NameStatus.Pending);

                bool finished = false;
                int currentPosition = 0;

                int longestName = 0;

                foreach (string name in names)
                    if (name.Length > longestName) longestName = name.Length;

                while (!finished)
                {
                    Console.Clear();

                    if (currentPosition > nameEntries.Length - 1)
                        currentPosition = 0;
                    else if (currentPosition < 0)
                        currentPosition = nameEntries.Length - 1;

                    for (int i = 0; i < nameEntries.Length; i++)
                    {
                        string lengthModifier = new string(' ', longestName - nameEntries[i].Name.Length);

                        switch (nameEntries[i].Status)
                        {
                            case NameStatus.Approved:
                                Console.WriteLine(i == currentPosition ? $"[ Name: {nameEntries[i].Name}{lengthModifier}  Rejected  |  Pending  | [Approved] ]" :
                                                                         $"  Name: {nameEntries[i].Name}{lengthModifier}  Rejected  |  Pending  | [Approved]  ");
                                break;
                            case NameStatus.Pending:
                                Console.WriteLine(i == currentPosition ? $"[ Name: {nameEntries[i].Name}{lengthModifier}  Rejected  | [Pending] |  Approved  ]" :
                                                                         $"  Name: {nameEntries[i].Name}{lengthModifier}  Rejected  | [Pending] |  Approved   ");
                                break;
                            case NameStatus.Rejected:
                                Console.WriteLine(i == currentPosition ? $"[ Name: {nameEntries[i].Name}{lengthModifier} [Rejected] |  Pending  |  Approved  ]" :
                                                                         $"  Name: {nameEntries[i].Name}{lengthModifier} [Rejected] |  Pending  |  Approved   ");
                                break;
                        }
                    }

                    var key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.LeftArrow:
                            nameEntries[currentPosition].Status = NameStatus.Rejected;
                            break;
                        case ConsoleKey.RightArrow:
                            nameEntries[currentPosition].Status = NameStatus.Approved;
                            break;
                        case ConsoleKey.UpArrow:
                            currentPosition--;
                            break;
                        case ConsoleKey.DownArrow:
                            currentPosition++;
                            break;
                        case ConsoleKey.Enter:
                            finished = true;
                            break;
                        default:
                            Console.Clear();
                            Console.WriteLine("Please enter a valid input...");
                            Console.ReadKey(true);
                            break;
                    }
                }

                for (int i = 0; i < nameEntries.Length; i++)
                {
                    switch (nameEntries[i].Status)
                    {
                        case NameStatus.Pending:
                            _approvalQueue.Push(nameEntries[i].Name);
                            break;
                        case NameStatus.Approved:
                            Approve(nameEntries[i].Name);
                            break;
                        case NameStatus.Rejected:
                            Reject(nameEntries[i].Name);
                            break;
                    }
                }
            }

            public void Approve(string name, double scale = 1.01)
            {
                if (_names.Add(Name))
                    _weights.Modulate(name, scale);
            }

            public void Reject(string name, double scale = 0.99)
            {
                if (_names.Remove(Name))
                    _weights.Modulate(name, scale);
            }

            public NameGenerator(string name, string description, string path, bool regenerate = false)
            {
                _name = name;
                _description = description;
                _path = path;

                _names = new NameList($"{_path}\\{Name}\\Names.csv");
                _weights = regenerate ? new ProbabilityMatrix(_names) : new ProbabilityMatrix($"{_path}\\{Name}\\Weights.csv");

                _approvalQueue = new Stack<string>();
            }

            public NameGenerator(string name, string description, string path, NameList names, ProbabilityMatrix weights)
            {
                _name = name;
                _description = description;
                _path = path;
                _names = names;
                _weights = weights;

                _approvalQueue = new Stack<string>();
            }

            public void Save()
            {
                _names.Save($"{_path}\\{Name}\\Names.csv");
                _weights.Save($"{_path}\\{Name}\\Weights.csv");
            }

            public void Load(string path)
            {
                _path = path;

                _names = new NameList($"{_path}\\{Name}\\Names.csv");
                _weights = new ProbabilityMatrix($"{_path}\\{Name}\\Weights.csv");
            }

            public string GenerateName(bool skipApproval = false)
            {
                int length = Random.Shared.Next(Globals.MinLength, Globals.MaxLength + 1);

                string name = "";

                bool prev_vowel = false;
                bool prev_consonant = false;

                bool prev2_vowel = false;
                bool prev2_consonant = false;

                int prev_num = 0;

                for (int i = 0; i < length; i++)
                {
                    Letter letter = Letter.GetRandom(prev_num, prev2_vowel, prev2_consonant, _weights);

                    if (i == 0) name += letter.UpperCase;
                    else name += letter.LowerCase;

                    prev2_vowel = letter.Vowel && prev_vowel;
                    prev2_consonant = letter.Consonant && prev_consonant;

                    prev_vowel = letter.Vowel;
                    prev_consonant = letter.Consonant;

                    prev_num = letter.Index;
                }

                if (skipApproval)
                    _names.Add(name);
                else _approvalQueue.Push(name);

                return name;
            }

            public List<string> GenerateNames(int amount, bool skipApproval = false)
            {
                List<string> names = new List<string>();

                for (int i = 0; i < amount; i++)
                    names.Add(GenerateName());

                if (skipApproval)
                {
                    foreach (string name in names)
                        _names.Add(name);
                }
                else
                {
                    foreach (string name in names)
                        _approvalQueue.Push(name);
                }

                return names;
            }
        }
    }
}

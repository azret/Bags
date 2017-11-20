using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bags
{
    class App
    {           
        static int Compare(string value, string comparand)
        {
            if (value.Length > comparand.Length)
            {
                return +1;
            }
            else if (value.Length < comparand.Length)
            {
                return -1;
            }

            int L = value.Length;

            for (int i = 0; i < L; i++)
            {
                if (value[i] > comparand[i])
                {
                    return +1;
                }
                else if (value[i] < comparand[i])
                {
                    return -1;
                }
            }

            return 0;
        }

        static string Read(string s)
        {
            StringBuilder r = new StringBuilder();

            Func<char, string> Convert = (char c) =>
            {
                switch (c)
                {
                    /*
                     */

                    case 'Æ': return "AE";
                    case 'æ': return "ae";
                    case 'Œ': return "OE";
                    case 'œ': return "oe";

                    /*
                    */

                    case 'a': return "a";
                    case 'e': return "e";
                    case 'i': return "i";
                    case 'o': return "o";
                    case 'u': return "u";
                    case 'y': return "y";
                    case 'A': return "A";
                    case 'E': return "E";
                    case 'I': return "I";
                    case 'O': return "O";
                    case 'U': return "V";
                    case 'Y': return "Y";

                    /*
                     */

                    case 'ă': return "a";
                    case 'ĕ': return "e";
                    case 'ĭ': return "i";
                    case 'ŏ': return "o";
                    case 'ŭ': return "u";
                    case 'ў': return "y";
                    case 'Ă': return "A";
                    case 'Ĕ': return "E";
                    case 'Ĭ': return "I";
                    case 'Ŏ': return "O";
                    case 'Ŭ': return "U";
                    case 'Ў': return "Y";

                    /*
                     */

                    case 'ā': return "a";
                    case 'ē': return "e";
                    case 'ī': return "i";
                    case 'ō': return "o";
                    case 'ū': return "u";
                    case 'ȳ': return "y";
                    case 'Ā': return "A";
                    case 'Ē': return "E";
                    case 'Ī': return "I";
                    case 'Ō': return "O";
                    case 'Ū': return "U";
                    case 'Ȳ': return "Y";

                    /*
                     */

                    case 'j': return "i";
                    case 'J': return "I";
                    case 'v': return "u";
                    case 'V': return "V";
                }

                return c.ToString();
            };

            for (int i = 0; i < s.Length; i++)
            {
                r.Append(Convert(s[i]));
            }
            
            return r.ToString();
        }

        static bool IsLegible(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                if (!char.IsLetter(c))
                {
                    return false;
                }                 
            }
             
            return true;
        }

        static object SCREEN = new object(); 

        static void Log(string msg)
        {
            lock (SCREEN)
            {
                Console.WriteLine(msg);
            }
        }

        static void Main(string[] args)
        {
            ISet<string> PARTICLES = new HashSet<string>(Tokens.Map("..\\data\\LA.ignore", 
                
                (TOKEN, EMIT) =>
                {
                    String s = Read(TOKEN);

                    if (EMIT != null)
                    {
                        EMIT(s);
                    }
                }), 
                
                StringComparer.InvariantCultureIgnoreCase

            );

            Dictionary<String, Bag> M = new Dictionary<String, Bag>();

            Tokens.Map(
                
                new string[] 
                {
                    "..\\DATA\\LA\\"
                }, 
                
                @"*.la",
                
                (TOKEN, EMIT) =>
                {
                    string s = Read(TOKEN);

                    if (!IsLegible(s))
                    {
                        return;
                    }
                    
                    if (s.EndsWith("que") && s.Length > "que".Length)
                    {
                        s = s.Substring(0, s.Length - "que".Length);
                    }

                    if (s.EndsWith("QVE") && s.Length > "QVE".Length)
                    {
                        s = s.Substring(0, s.Length - "QVE".Length);
                    }

                    if (PARTICLES.Contains(s))
                    {
                        return;
                    }

                    if (EMIT != null)
                    {
                        EMIT(s);
                    }
                },

                (FILE, DOC, LEX) =>
                {
                    StringBuilder DEBUG = new StringBuilder();

                    Log(Path.GetFullPath(FILE));

                    var BagOfWords = System.Text.Bags.Compute(DOC, 5, (FOCUS, NEIGHBOR, Δ) =>
                    {

                        if (FOCUS[0] == char.ToUpperInvariant(FOCUS[0]))
                        {
                            if (NEIGHBOR[0] != char.ToUpperInvariant(NEIGHBOR[0]))
                            {
                                return false;
                            }
                        }
                        else if (NEIGHBOR[0] == char.ToUpperInvariant(NEIGHBOR[0]))
                        {
                            if (FOCUS[0] != char.ToUpperInvariant(FOCUS[0]))
                            {
                                return false;
                            }
                        }

                        return true;

                    });

                    foreach (var bag in BagOfWords)
                    {
                        if (DEBUG.Length > 0)
                        {
                            DEBUG.Append("\r\n");
                        }

                        DEBUG.Append(String.Format("{0} <{1}>", bag.Key, bag.Total));

                        if (LEX != null)
                        {
                            lock (LEX)
                            {
                                Bag lex; string key = bag.Key;

                                if (!LEX.TryGetValue(key, out lex))
                                {
                                    LEX[key] = lex = new Bag(key);
                                }

                                lex.Add(bag);
                            }
                        }
                    }

                }, M);
            
            var Axis = new List<Bag>();

            foreach (var i in M)
            {
                Axis.Add(i.Value);
            }

            Axis.Sort((a, b) => {

                int c = 0;

                if (a.Total > b.Total)
                {
                    c = -1;
                }
                else if (a.Total < b.Total)
                {
                    c = +1;
                }

                if (c == 0)
                {
                    c = Compare(a.Key, b.Key);
                }

                return c;

            });            

            var OUTPUT = new StringBuilder();

            Parallel.ForEach(Axis, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, (bag)=>
            {
                List<Tuple<String, Int32, Int32>> SORT = new List<Tuple<String, Int32, Int32>>();

                bag.ForEach((key, count) =>
                {
                    Bag lex;

                    if (M.TryGetValue(key, out lex))
                    {
                        SORT.Add(new Tuple<String, Int32, Int32>(key, count, lex.Total));
                    }

                });

                SORT.Sort((a, b) => {

                    int c = 0;

                    if (a.Item2 > b.Item2)
                    {
                        c = -1;
                    }
                    else if (a.Item2 < b.Item2)
                    {
                        c = +1;
                    }

                    if (c == 0)
                    {
                        c = Compare(a.Item1, b.Item1);
                    }

                    return c;

                });

                List<Tuple<String, Int32, Int32>> TAKE = new List<Tuple<String, Int32, Int32>>();

                for (int i = 0; i < SORT.Count; i++)
                {
                    if (TAKE.Count >= 17)
                    {
                        break;
                    }

                    TAKE.Add(SORT[i]);
                }

                StringBuilder LINE = new StringBuilder(); int COUNT = 0;

                for (int i = 0; i < TAKE.Count; i++)
                {
                    var item = TAKE[i];

                    if (LINE.Length > 0)
                    {
                        LINE.Append(", ");
                    }

                    LINE.Append(String.Format("*{0}* ({1}|{2})", item.Item1, item.Item3, item.Item2));

                    COUNT++;
                }

                int MIN = 0;

                if (bag.Total > MIN)
                {
                    lock (OUTPUT)
                    {
                        if (OUTPUT.Length > 0)
                        {
                            OUTPUT.Append("\r\n");
                        }

                        OUTPUT.Append(String.Format("~ **{0}** ({1})",
                            bag.Key, bag.Total, COUNT));

                        if (LINE.Length > 0)
                        {
                            OUTPUT.AppendFormat(" - {0}", LINE.ToString());
                        }
                    }
                }

            });            
              
            File.WriteAllText("..\\DATA\\LA.md", OUTPUT.ToString());

            Console.WriteLine("Done.");
        }
    }
}
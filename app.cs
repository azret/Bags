using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

static class App
{
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
        var lang = System.Language.Orthography.Create();

        /**
         *  
         **/

        ISet<string> exclude = new HashSet<string>(Tokens.Parse("..\\data\\LA.ignore",

            (s, f) =>
            {
                s = lang.Convert(s);

                if (lang.IsLegible(s))
                {
                    if (f != null)
                    {
                        f(s);
                    }
                }
            }),

            StringComparer.InvariantCultureIgnoreCase

        );

        /**
         * 
         **/

        IDictionary<String, Bag> lexicon = Build(lang, exclude,

            new string[]
            {
                    "..\\DATA\\LA\\cato\\"
            }

        );

        int MIN = 3;

        /**
         * 
         **/

        lexicon.Reduce(lang,

            /** 
             */

            weight: MIN,

            /** 
             */

            limit: 17

        );

        var graph = Graph.Create(lexicon);
        
        var OUTPUT = new StringBuilder();

        foreach (var node in graph)
        {
            StringBuilder LINE = new StringBuilder(); int COUNT = 0;

            for (int j = 0; node.Links != null && j < node.Links.Length; j++)
            {
                var link = node.Links[j];

                if (LINE.Length > 0)
                {
                    LINE.Append(", ");
                }

                string label = graph[link.No].Label;

                LINE.Append(String.Format("*{0}* ({1}|{2})", label, link.Weight, link.No));

                COUNT++;
            }

            if (OUTPUT.Length > 0)
            {
                OUTPUT.Append("\r\n");
            }

            OUTPUT.Append(String.Format("~ **{0}** ({1})",
                node.Label, node.Weight, COUNT));

            if (LINE.Length > 0)
            {
                OUTPUT.AppendFormat(" - {0}", LINE.ToString());
            }

        }

        File.WriteAllText("..\\DATA\\LA.md", OUTPUT.ToString());


        graph.Save(@"D:\Bags\graph\graph.json");

        Console.WriteLine("Done.");
    }
    
    static IDictionary<string, Bag> Build(System.Language.IOrthography lang, ISet<string> ignore, string[] paths, string search = "*.*")
    {
        Dictionary<String, Bag> lexicon = new Dictionary<String, Bag>();

        Tokens.Parse(paths, search,

            (TOKEN, EMIT) =>
            {
                string s = lang.Convert(TOKEN);

                if (!lang.IsLegible(s))
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

                if (ignore.Contains(s))
                {
                    return;
                }

                if (EMIT != null)
                {
                    EMIT(s);
                }
            },

            (FILE, DOC) =>
            {
                Log(Path.GetFullPath(FILE));

                var bags = Bags.Compute(DOC, 5, (FOCUS, NEIGHBOR, Δ) =>
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

                foreach (var bag in bags)
                {
                    if (lexicon != null)
                    {
                        lock (lexicon)
                        {
                            Bag lex; string key = bag.Key;

                            if (!lexicon.TryGetValue(key, out lex))
                            {
                                lexicon[key] = lex = new Bag(key, lexicon.Count);
                            }

                            lex.Add(bag, bag.Weight);
                        }
                    }
                }

            });

        return lexicon;
    }

    /// <summary>
    /// Reduces the size of the specified lexicon.
    /// </summary>
    /// <param name="weight">Minimum weight of the entry to be included.</param>
    /// <param name="limit">Maximum number of entries in the window.</param>
    static void Reduce(this IDictionary<string, Bag> lexicon, System.Language.IOrthography lang, int weight, int limit)
    {
        var reduce = new HashSet<string>(); var depends = new HashSet<string>();

        Parallel.ForEach(lexicon, (bag) =>
        {
            List<Tuple<String, Int32, Int32>> SORT = new List<Tuple<String, Int32, Int32>>();

            // A single bag should never be worked on concurrently

            bag.Value.ForEach((key, count) =>
            {
                Bag lex = null;

                if (!lexicon.TryGetValue(key, out lex))
                {
                    lex = null;
                }

                if (lex != null && lex.Weight >= weight)
                {
                    SORT.Add(new Tuple<String, Int32, Int32>(key, count, lex.Weight));
                }

            });

            SORT.Sort((a, b) =>
            {
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
                    c = lang.Compare(a.Item1, b.Item1);
                }

                return c;

            });

            List<Tuple<String, Int32, Int32>> TAKE = new List<Tuple<String, Int32, Int32>>();

            for (int i = 0; i < SORT.Count; i++)
            {
                if (TAKE.Count >= limit)
                {
                    break;
                }

                TAKE.Add(SORT[i]);
            }

            bag.Value.Clear();

            for (int i = 0; i < TAKE.Count; i++)
            {
                string key = TAKE[i].Item1;

                bag.Value.Add(key, TAKE[i].Item2);

                lock (depends)
                {
                    depends.Add(key);
                }
            }

            if (bag.Value.Weight < weight)
            {
                lock (reduce)
                {
                    reduce.Add(bag.Value.Key);
                }
            }

        });

        foreach (var key in reduce)
        {
            if (!depends.Contains(key))
            {
                lexicon.Remove(key);
            }
        }

    }
}

namespace System.Language
{
    public interface IOrthography
    {
        int Compare(string s, string comparand);
        string Convert(string s);
        bool IsLegible(string s);
    }

    public static class Orthography
    {
        public class Latin : IOrthography
        {
            public string Convert(string s)
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
                        case 'U': return "U";
                        case 'Y': return "Y";


                        /*
                         */

                        case 'â': return "a";
                        case 'ê': return "e";
                        case 'î': return "i";
                        case 'ô': return "o";
                        case 'û': return "u";
                        case 'Â': return "A";
                        case 'Ê': return "E";
                        case 'Î': return "I";
                        case 'Ô': return "O";
                        case 'Û': return "U";

                        /*
                         */

                        case 'à': return "a";
                        case 'è': return "e";
                        case 'ì': return "i";
                        case 'ò': return "o";
                        case 'ù': return "u";
                        case 'À': return "A";
                        case 'È': return "E";
                        case 'Ì': return "I";
                        case 'Ò': return "O";
                        case 'Ù': return "U";

                        /*
                         */

                        case 'á': return "a";
                        case 'é': return "e";
                        case 'í': return "i";
                        case 'ó': return "o";
                        case 'ú': return "v";
                        case 'ý': return "y";
                        case 'Á': return "A";
                        case 'É': return "E";
                        case 'Í': return "I";
                        case 'Ó': return "O";
                        case 'Ú': return "U";
                        case 'Ý': return "Y";

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

                    }

                    return c.ToString();
                };

                Func<string, string> Change = (glyph) =>
                {
                    switch (glyph)
                    {
                        case "j": return "i";
                        case "J": return "I";
                        case "v": return "u";
                        case "U": return "V";
                    }

                    return glyph;
                };

                for (int i = 0; i < s.Length; i++)
                {
                    var glyph = Convert(s[i]);

                    glyph = Change(glyph);

                    r.Append(glyph);
                }

                return r.ToString();
            }

            public bool IsLegible(string s)
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

            public int Compare(string s, string comparand)
            {
                if (s.Length > comparand.Length)
                {
                    return +1;
                }
                else if (s.Length < comparand.Length)
                {
                    return -1;
                }

                int L = s.Length;

                for (int i = 0; i < L; i++)
                {
                    if (s[i] > comparand[i])
                    {
                        return +1;
                    }
                    else if (s[i] < comparand[i])
                    {
                        return -1;
                    }
                }

                return 0;
            }
        }

        public static IOrthography Create()
        {
            return new Latin();
        }
    }
}
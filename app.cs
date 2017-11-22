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
       
        Grammar LA = new Grammar();

        LA.DictionaryPath = "..\\src\\whitakar";

        LA.LOAD();
         

        int OCCURS = 3; int DIMENSIONS = 17; int WINDOW = 7;

        /**
         * 
         **/

        IDictionary<String, Bag> lex = Build(WINDOW, lang, 
            
            (s) => 
            { 

                return s;

            },
            
            exclude,

            new string[]
            {
                    "..\\DATA\\LA\\"
            }

        );

        /**
         * 
         **/

        lex.Reduce(lang,

            /** 
             */

            weight: OCCURS,

            /** 
             */

            limit: DIMENSIONS

        );         

        var g = Graph.Create(lex);

        g.MD("..\\DATA\\LA.md");
        // g.JSON("..\\DATA\\LA.json");
        g.JSON("D:\\Latin\\17-7.json");

        Console.WriteLine("Done.");
    }

    static Dictionary<char, int> Numerals = new Dictionary<char, int>() { {'i', 1}, {'u', 5}, {'x', 10}, {'l', 50}, {'c', 100}, {'d', 500}, {'m', 1000} };
    static int? RomanToInteger(string s)
    {
        int number = 0;
        for (int i = 0; i < s.Length; i++) {
            if (!Numerals.ContainsKey(s[i])) {
                return null;
            }
            if (i + 1 < s.Length && !Numerals.ContainsKey(s[i + 1])) {
                return null;
            }
            if (i + 1 < s.Length && Numerals[s[i]] < Numerals[s[i + 1]]) {
                number -= Numerals[s[i]];
            } else {
                number += Numerals[s[i]];
            }
        }
        return number;
    }
    static string IntegerToRoman(int number)
    {
        if ((number < 0) || (number > 3999)) return null;
        if (number < 1) return string.Empty;
        if (number >= 1000) return "m" + IntegerToRoman(number - 1000);
        if (number >= 900) return "cm" + IntegerToRoman(number - 900);
        if (number >= 500) return "d" + IntegerToRoman(number - 500);
        if (number >= 400) return "cd" + IntegerToRoman(number - 400);
        if (number >= 100) return "c" + IntegerToRoman(number - 100);
        if (number >= 90) return "xc" + IntegerToRoman(number - 90);
        if (number >= 50) return "l" + IntegerToRoman(number - 50);
        if (number >= 40) return "xl" + IntegerToRoman(number - 40);
        if (number >= 10) return "x" + IntegerToRoman(number - 10);
        if (number >= 9) return "ix" + IntegerToRoman(number - 9);
        if (number >= 5) return "u" + IntegerToRoman(number - 5);
        if (number >= 4) return "iu" + IntegerToRoman(number - 4);
        if (number >= 1) return "i" + IntegerToRoman(number - 1);
        return null;
    }

    static IDictionary<string, Bag> Build(int WINDOW, System.Language.IOrthography lang, Func<string, string> support, ISet<string> ignore, string[] paths, string search = "*.*")
    {
        Dictionary<String, Bag> lexicon = new Dictionary<String, Bag>();

        Tokens.Parse(paths, search,

            (TOKEN, EMIT) =>
            {
                if (TOKEN.Length > 1)
                {
                    if (char.ToUpperInvariant(TOKEN[1]) != TOKEN[1])
                    {
                        TOKEN = TOKEN.ToLowerInvariant();
                    }
                }

                /*
                 */

                string s = lang.Convert(TOKEN);

                if (!lang.IsLegible(s))
                {
                    return;
                }

                /* Do not take single letter entries unless they start with upper case */

                if (s.Length == 1)
                {
                    if (char.ToUpperInvariant(s[0]) != s[0]) {
                        return;
                    } 
                }
                 
                /* Do not take roman numerals  */

                if (s.Length > 1 && char.ToUpperInvariant(s[0]) != s[0])
                {
                    bool same = true;
                    for (int i = 1; i < s.Length; i++)
                    {
                        if (char.ToUpperInvariant(s[i]) != char.ToUpperInvariant(s[i - 1])) {
                            same = false;
                            break;
                        }
                    }
                    if (s != "ui" && s != "uim" && s != "uix" && s != "uii"
                           && s != "lux" && s != "lum"
                           && s != "cum"
                           && s != "cui"
                           && s != "mum"
                           && s != "mi"
                           && s != "id"
                           && s != "mix"
                           && s != "diu"
                           && s != "dix"
                           && s != "di"
                           && s != "dii"
                           && s != "dux" && s != "dum")
                    {
                        var n = RomanToInteger(s);
                        if (n.HasValue) {
                            var c = IntegerToRoman(n.Value);
                            if (c == s) {
                                return;
                            }
                        }
                    }
                    if (same) {
                        return;
                    }                    
                }

                /* Must have at least one vowel
                 
                    - Abbreviates should be capitalized .
                    - Foreign words might be ignored which is a good side effect.
                                     
                 */

                if (s.Length > 1 && char.ToUpperInvariant(s[0]) != s[0])
                {
                    int vowels = 0;
                    for (int i = 0; i < s.Length; i++)
                    {
                        switch (s[i])
                        {
                            case 'a':
                            case 'e':
                            case 'i':
                            case 'o':
                            case 'u':
                                vowels++;
                                break;
                        } 
                    }
                    if (vowels <= 0)
                    {
                        return;
                    }
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

                if (support != null)
                {
                    s = lang.Convert(support(s));

                    if (!lang.IsLegible(s))
                    {
                        return;
                    }
                }

                if (EMIT != null)
                {
                    EMIT(s);
                }
            },

            (FILE, DOC) =>
            {
                Log(Path.GetFullPath(FILE));

                var bags = Bags.Compute(DOC, WINDOW, (FOCUS, NEIGHBOR, Δ) =>
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
        int Compare(string s, string c);
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

                if (s != null)
                {
                    for (int i = 0; i < s.Length; i++)
                    {
                        var glyph = Convert(s[i]);

                        glyph = Change(glyph);

                        r.Append(glyph);
                    }
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

            public int Compare(string s, string c)
            {
                int l = Math.Min(s.Length, c.Length);

                for (int i = 0; i < l; i++)
                {
                    if (s[i] > c[i])
                    {
                        return +1;
                    }
                    else if (s[i] < c[i])
                    {
                        return -1;
                    }
                }

                if (s.Length > c.Length)
                {
                    return +1;
                }
                else if (s.Length < c.Length)
                {
                    return -1;
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
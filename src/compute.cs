﻿namespace System.Text
{
    using System.Collections.Generic;

    public class Bag
    {
        public String Key
        {
            get;
        }

        public Int32 No
        {
            get;
        }

        public Int32 Weight
        {
            get;
            set;
        }
             
        public Bag(String key, Int32 no)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Key = key;

            if (no < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(no));
            }

            No = no;
        }

        Dictionary<String, Int32> _Items = null;

        public void Add(Bag src, Int32 weight)
        {
            if (src == null || src.Key != Key)
            {
                throw new ArgumentException();
            }

            Weight += weight;

            if (src._Items != null)
            {
                foreach (var i in src._Items)
                {
                    Add(i.Key, i.Value);
                }
            }
        }

        public void Add(String key, Int32 count)
        {
            if (Key == key)
            {
                return;
            }

            if (_Items == null)
            {
                _Items = new Dictionary<String, Int32>();
            }

            Int32 total;

            if (_Items.TryGetValue(key, out total))
            {
                _Items[key] = total + count;
            }
            else
            {
                _Items[key] = count;
            }
        }

        public void Remove(String key)
        {
            if (_Items != null)
            {
                if (_Items.ContainsKey(key))
                {
                    _Items.Remove(key);
                }
            }
        }

        public void Clear()
        {
            if (_Items != null)
            {
                _Items.Clear();
            }

            _Items = null;
        }

        public bool Has(String key)
        {
            if (_Items != null)
            {
                if (_Items.ContainsKey(key))
                {
                    return true;
                }
            }

            return false;
        }

        public void ForEach(Action<String, Int32> take)
        {
            if (_Items != null)
            {
                foreach (var i in _Items)
                {
                    if (take != null)
                    {
                        take(i.Key, i.Value);
                    } 
                }
            }
        }
    }

    public static class Bags
    {
        public static IList<Bag> Compute(this IList<String> doc, int WINDOW = 3, Func<String, String, int, bool> take = null)
        {
            if (WINDOW < 0 || WINDOW > 13)
            {
                throw new ArgumentOutOfRangeException();
            }

            /*
             */

            IDictionary<String, Token> lex = Unique(doc);

            var Axis = new Token[lex.Count];

            foreach (var i in lex)
            {
                Axis[i.Value.Axis] = i.Value;
            }

            /*
             */

            int[][] MATRIX = null;

            if (WINDOW > 0)
            {
                MATRIX = new int[Axis.Length][];

                for (int i = 0; i < Axis.Length; i++)
                {
                    MATRIX[i] = new int[Axis.Length];
                }

                for (int focus = 0; focus < doc.Count; focus++)
                {
                    for (int neighbor = focus - WINDOW; neighbor < focus + WINDOW + 1; neighbor++)
                    {
                        String FOCUS = doc[focus];

                        if (neighbor >= 0 && neighbor < doc.Count && neighbor != focus)
                        {
                            int distance = 0;

                            if (neighbor > focus)
                            {
                                distance = neighbor - focus;
                            }
                            else if (neighbor < focus)
                            {
                                distance = focus - neighbor;
                            }

                            String NEIGHBOR = doc[neighbor];

                            if (take != null)
                            {
                                if (!take(FOCUS, NEIGHBOR, distance))
                                {
                                    distance = 0;
                                }
                            }

                            if (distance > 0)
                            {
                                MATRIX[lex[FOCUS].Axis][lex[NEIGHBOR].Axis] += (WINDOW - distance);
                            }
                        }
                    }
                }
            }

            var list = new List<Bag>();

            for (int i = 0; i < Axis.Length; i++)
            {
                Bag bag = new Bag(Axis[i].Key, i)
                {
                    Weight = Axis[i].Count
                };

                if (MATRIX != null && MATRIX[i] != null)
                {
                    ISet<int> vector = new HashSet<int>();

                    for (int j = 0; j < Axis.Length; j++)
                    {
                        if (MATRIX[i][j] > 0)
                        {
                            vector.Add(j);
                        }
                    }

                    if (vector.Count > 0)
                    {
                        foreach (var j in vector)
                        {
                            bag.Add(Axis[j].Key, MATRIX[i][j]);
                        }
                    }
                }

                list.Add(bag);
            }

            return list;
        }

        class Token
        {
            public String Key
            {
                get;
            }

            public Int32 Axis
            {
                get;
            }

            public Int32 Count
            {
                get;
                set;
            }

            public Token(String key, Int32 axis)
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                Key = key;

                if (axis < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(axis));
                }

                Axis = axis;
            }
        }

        static IDictionary<String, Token> Unique(IList<String> doc)
        {
            IDictionary<String, Token> hash = new Dictionary<String, Token>();

            Int32 axis = 0;

            for (int i = 0; i < doc.Count; i++)
            {
                var key = doc[i];

                Token t;

                if (!hash.TryGetValue(key, out t))
                {
                    hash[key] = t = new Token(key, axis++);
                }

                t.Count++;

                if (hash.Count > int.MaxValue)
                {
                    break;
                }
            }

            return hash;
        }
    }
}
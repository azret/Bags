namespace System.Text
{
    using System.Collections.Generic;

    public class Bag
    {
        public String Key
        {
            get;
        }

        public Int32 Total
        {
            get;
            set;
        }

        public Bag(String key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Key = key;
        }

        Dictionary<String, Int32> _Items = null;

        public void Add(Bag src)
        {
            if (src == null || src.Key != Key)
            {
                throw new ArgumentException();
            }

            Total += src.Total;

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
        public static IList<Bag> Compute(IList<String> doc, int WINDOW = 3, Func<String, String, int, bool> take = null)
        {
            if (WINDOW < 0 || WINDOW > 13)
            {
                throw new ArgumentOutOfRangeException();
            }

            /*
             */

            IDictionary<String, Token> M = Unique(doc);

            var Axis = new Token[M.Count];

            foreach (var i in M)
            {
                Axis[i.Value.Axis] = i.Value;
            }

            /*
             */

            int[][] CoOccurrences = null;

            if (WINDOW > 0)
            {
                CoOccurrences = new int[Axis.Length][];

                for (int i = 0; i < Axis.Length; i++)
                {
                    CoOccurrences[i] = new int[Axis.Length];
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
                                CoOccurrences[M[FOCUS].Axis][M[NEIGHBOR].Axis] += (WINDOW - distance);
                            }
                        }
                    }
                }
            }

            var list = new List<Bag>();

            for (int i = 0; i < Axis.Length; i++)
            {
                Bag bag = new Bag(Axis[i].Key)
                {
                    Total = Axis[i].Count
                };

                if (CoOccurrences != null && CoOccurrences[i] != null)
                {
                    ISet<int> vector = new HashSet<int>();

                    for (int j = 0; j < Axis.Length; j++)
                    {
                        if (CoOccurrences[i][j] > 0)
                        {
                            vector.Add(j);
                        }
                    }

                    if (vector.Count > 0)
                    {
                        foreach (var j in vector)
                        {
                            bag.Add(Axis[j].Key, CoOccurrences[i][j]);
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
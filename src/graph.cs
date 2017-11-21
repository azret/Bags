namespace System.Text
{
    using System.Collections.Generic;

    public static partial class Graph
    {
        public class Link
        {
            public int No
            {
                get;
                set;
            }

            public int Weight
            {
                get;
                set;
            }
        }

        public class Node
        {
            public int No
            {
                get;
                set;
            }

            public int Weight
            {
                get;
                set;
            }

            public string Label
            {
                get;
                set;
            }

            public Link[] Links
            {
                get;
                set;
            }
        }

        public static int Compare(string s, string comparand)
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

        public static Node Find(this IList<Node> graph, string label)
        {
            Node found = null;

            int low = 0, high = graph.Count - 1;

            while (low <= high)
            {
                int i = (int)(((uint)low + (uint)high) >> 1);

                Node p = graph[i];

                int c = Compare(p.Label, label);

                if (c < 0)
                {
                    low = i + 1;
                }
                else
                {
                    high = i - 1;

                    if (c == 0)
                    {
                        found = p;                        
                    }
                }
            }

            return found;
        }        
    }
}

namespace System.Text
{
    using System.Collections.Generic;

    public static partial class Graph
    { 
        public static IList<Node> Create(this IDictionary<string, Bag> lexicon)
        {
            var g = new List<Node>();

            foreach (var i in lexicon)
            {
                List<Link> links = new List<Link>();

                i.Value.ForEach((key, count) =>
                {
                    links.Add(new Link()
                    {
                        No = lexicon[key].No,
                        Weight = count
                    });
                });

                Node n = n = new Node()
                {
                    No = i.Value.No,
                    Weight = i.Value.Weight,
                    Label = i.Value.Key,
                };

                g.Add(n);

                if (links.Count > 0)
                {
                    n.Links = links.ToArray();
                }
            }

            g.Sort(

                (s, comparand) => {

                    return Compare(s.Label, comparand.Label);
                }

            );

            IDictionary<int, Node> swap = new Dictionary<int, Node>();

            for (int i = 0; i < g.Count; i++)
            {
                swap[g[i].No] = g[i]; g[i].No = i;
            }

            for (int i = 0; i < g.Count; i++)
            {
                for (int j = 0; g[i].Links != null && j < g[i].Links.Length; j++)
                {
                    g[i].Links[j].No = swap[g[i].Links[j].No].No;
                }
            }

            return g;
        }
    }
}

namespace System.Text
{
    using System.Collections.Generic;
    using System.IO;

    public static partial class Graph
    {
        public static void Save(this IList<Node> graph, string file)
        {
            Action<Stream, string> Emit = (stream, s) =>
            {
                byte[] bytes = Encoding.UTF8.GetBytes(s);

                if (bytes != null && bytes.Length > 0)
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            };

            Func<string, string> Escape = (s) =>
            {
                StringBuilder b = new StringBuilder();

                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];

                    if (c == '\"')
                    {
                        b.Append("\\");
                    }

                    b.Append(c);
                }

                return b.ToString();
            };
            
            using (var w = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Emit(w, "{\n\"graph\":[");

                int nodes = 0;

                for (int i = 0; i < graph.Count; i++)
                {
                    var node = graph[i]; var links = node.Links;

                    Emit(w, (nodes > 0) ? ",{" : "\n{");

                    nodes++;

                    Emit(w, $"\"i\":{node.No},\"l\":\"{Escape(node.Label)}\",\"w\":{node.Weight}");

                    if (links != null && links.Length > 0)
                    {
                        Emit(w, $",\"n\":[");

                        int c = 0;

                        for (int j = 0; j < links.Length; j++)
                        {
                            var link = links[j];

                            if (c > 0)
                            {
                                Emit(w, (j % 7 != 0) ? ",{" : ",\n{");
                            }
                            else
                            {
                                Emit(w, "{");
                            }

                            c++;

                            Emit(w, $"\"i\":{link.No},\"w\":{link.Weight}}}");
                        }

                        Emit(w, $"\n]}}");
                    }
                    else
                    {
                        Emit(w, $"\n}}");
                    }
                }

                Emit(w, "]}");
            }
        }
    }
}
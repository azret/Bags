namespace System.Text
{
    using System.Collections.Generic;

    public static partial class Graph
    {
        public class Link
        {
            public Link()
            {
            }

            public Link(int no, double weight)
            {
                No = no;
                Weight = weight;
            }

            public int No
            {
                get;
                set;
            }

            public double Weight
            {
                get;
                set;
            }
        }

        public class Node
        {
            public Node()
            {
            }

            public Node(int no, string label, double weight)
            {
                No = no;
                Label = label;
                Weight = weight;
            }

            public int No
            {
                get;
                set;
            }

            public string Label
            {
                get;
                set;
            }

            public double Weight
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

        public static int Compare(Node n, Node c) { return Compare(n.Label, c.Label); }
        public static int Compare(string s, string c)
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

        public static Node Find(this IList<Node> g, string s)
        {
            Node found = null;

            int low = 0, high = g.Count - 1;

            while (low <= high)
            {
                int i = (int)(((uint)low + (uint)high) >> 1);

                Node p = g[i];

                int c = Compare(p.Label, s);

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
        public static IList<Node> Create(this IDictionary<string, Bag> src)
        {
            var g = new List<Node>();

            foreach (var i in src)
            {
                List<Link> links = new List<Link>();

                i.Value.ForEach((key, count) => links.Add(new Link(src[key].No, count)));

                Node n;

                g.Add(n = new Node(i.Value.No, i.Value.Key, i.Value.Weight));

                if (links.Count > 0)
                {
                    n.Links = links.ToArray();
                }
            }

            g.Sort(Compare);

            Dictionary<int, Node> swap = new Dictionary<int, Node>();

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
        public static double sigmoid(double value)
        {
            return 1 / (1 + Math.Exp(-value));
        }

        public static void MD(this IList<Node> g, string file)
        {
            Action<Stream, string> Emit = (stream, s) =>
            {
                byte[] b = Encoding.UTF8.GetBytes(s);

                if (b != null && b.Length > 0)
                {
                    stream.Write(b, 0, b.Length);
                }
            }; 

            Func<string, string> Escape = (s) =>
            {
                StringBuilder b = new StringBuilder();

                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];

                    switch (c)
                    {
                        case '`':
                            b.Append("\\`");
                            break;
                        case '*':
                            b.Append("\\*");
                            break;
                        case '#':
                            b.Append("\\#");
                            break;
                        case '+':
                            b.Append("\\+");
                            break;
                        case '-':
                            b.Append("\\-");
                            break;
                        case '.':
                            b.Append("\\.");
                            break;
                        case '!':
                            b.Append("\\!");
                            break;
                        case '_':
                            b.Append("\\_");
                            break;
                        case '[':
                            b.Append("\\[");
                            break;
                        case ']':
                            b.Append("\\]");
                            break;
                        case '{':
                            b.Append("\\{");
                            break;
                        case '}':
                            b.Append("\\}");
                            break;
                        case '\\':
                            b.Append("\\\\");
                            break;
                        default:
                            b.Append(c);
                            break;
                    }
                }

                return b.ToString();
            };

            using (var w = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                int n = 0;

                for (int i = 0; i < g.Count; i++)
                {
                    var node = g[i]; var links = node.Links;

                    if (n++ > 0)
                    {
                        Emit(w, "\n");
                    }

                    Emit(w, $"**{Escape(node.Label)}** ({node.Weight})");

                    if (links != null && links.Length > 0)
                    {
                        double Σ = 0.0;

                        for (int j = 0; j < links.Length; j++)
                        {
                            Σ += Math.Pow(Math.PI / Math.E, links[j].Weight);
                        }

                        int c = 0;

                        for (int j = 0; j < links.Length; j++)
                        {
                            if (c++ > 0)
                            {
                                Emit(w, ",");
                            }
                             
                            Emit(w, $" {Escape(g[links[j].No].Label)} ({links[j].Weight}|{Math.Pow(Math.PI / Math.E, links[j].Weight) / Σ})");
                        }
                    }
                }
            }
        }

        public static void JSON(this IList<Node> g, string file)
        {
            Action<Stream, string> Emit = (stream, s) =>
            {
                byte[] b = Encoding.UTF8.GetBytes(s);

                if (b != null && b.Length > 0)
                {
                    stream.Write(b, 0, b.Length);
                }
            };

            Func<string, string> Escape = (s) =>
            {
                StringBuilder b = new StringBuilder();

                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];

                    switch (c)
                    {
                        case '\x08':
                            b.Append("\\b");
                            break;
                        case '\x0C':
                            b.Append("\\f");
                            break;
                        case '\n':
                            b.Append("\\n");
                            break;
                        case '\r':
                            b.Append("\\r");
                            break;
                        case '\t':
                            b.Append("\\t");
                            break;
                        case '\"':
                            b.Append("\\\"");
                            break;
                        case '\\':
                            b.Append("\\\\");
                            break;
                        default:
                            b.Append(c);
                            break;
                    }
                }

                return b.ToString();
            };
            
            using (var w = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Emit(w, "{\n\"graph\":[");

                int n = 0;

                for (int i = 0; i < g.Count; i++)
                {
                    var node = g[i]; var links = node.Links;

                    Emit(w, (n > 0) ? ",{" : "\n{");

                    n++;

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
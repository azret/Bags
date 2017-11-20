namespace System.Text
{
    using System.Collections.Generic;
    using System.IO;

    public static class Graph
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

            public string Label
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

        public static IList<Node> Create(this IDictionary<string, Bag> lexicon)
        {
            var graph = new List<Node>();

            // Re-shape the bag of words dictionary into a graph...

            foreach (var entry in lexicon)
            {
                Node node = null;

                graph.Add(node = new Node()
                {
                    No = entry.Value.No,
                    Label = entry.Value.Key,
                    Weight = entry.Value.Weight
                });

                var links = new List<Link>();

                entry.Value.ForEach((key, count) =>
                {
                    links.Add(new Link()
                    {
                        No = lexicon[key].No,
                        Label = key,
                        Weight = count
                    });

                });
                
                node.Links = links.ToArray();                
            }

            // Sort the nodes ordinally. It is important that the
            //      compare function is simple at portable...
             
            graph.Sort((i, comparand) => Compare(i.Label, comparand.Label));
            
            var swap = new Dictionary<int, Node>();

            for (int i = 0; i < graph.Count; i++)
            {
                var no = graph[i].No;

                Node node;

                if (swap.TryGetValue(no, out node))
                {
                    throw new InvalidOperationException();
                }

                node = graph[i]; swap[no] = node;

                // Create a zero-based index based on the new sort order...

                node.No = i;
            }

            for (int i = 0; i < graph.Count; i++)
            {
                for (int j = 0; graph[i].Links != null && j < graph[i].Links.Length; j++)
                {
                    // Re-reference into new sort order...

                    graph[i].Links[j].No = swap[graph[i].Links[j].No].No;
                }
            }            

            return graph;
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

        public static void Save(this IList<Node> graph, string file)
        {
            using (var writer = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                writer.Write("{\r\n \"graph\": [");

                int written = 0;

                for (int i = 0; i < graph.Count; i++)
                {
                    var node = graph[i];

                    if (written > 0)
                    {
                        writer.Write(", { ");
                    }
                    else
                    {
                        writer.Write("\r\n  { ");
                    }

                    written++;

                    writer.Write($"\"id\": {node.No}, ");
                    writer.Write($"\"l\": \"{node.Label}\", ");
                    writer.Write($"\"w\": {node.Weight}");

                    if (node.Links != null && node.Links.Length > 0)
                    {
                        writer.Write($", \"n\": [");

                        int links = 0;

                        for (int j = 0; node.Links != null && j < node.Links.Length; j++)
                        {
                            var link = node.Links[j];

                            if (links > 0)
                            {
                                writer.Write(",\r\n     { ");
                            }
                            else
                            {
                                writer.Write("\r\n     { ");
                            }

                            links++;

                            writer.Write($"\"id\": {link.No}, ");
                            writer.Write($"\"w\": {link.Weight}}}");
                        }

                        writer.Write($"\r\n  ]\r\n");
                    }
                    else
                    {
                        writer.Write($"\r\n");
                    }

                    writer.Write(" }");

                }

                writer.Write("\r\n ]\r\n}");
            }
        }

        static void Write(this Stream stream, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);

            if (bytes != null && bytes.Length > 0)
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
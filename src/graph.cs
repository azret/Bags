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

            /**
            */

            foreach (var entry in lexicon)
            {
                Node node = null;

                /**
                */

                var links = new List<Link>();

                entry.Value.ForEach((key, count) =>
                {

                    links.Add(new Link()
                    {
                        /**
                         */

                        No = lexicon[key].No,

                        /**
                        */

                        Weight = count

                    });

                });

                /**
                */

                graph.Add(node = new Node()
                {
                    /**
                     */

                    No = entry.Value.No,

                    /**
                     */

                    Label = entry.Value.Key,

                    /**
                     */

                    Weight = entry.Value.Weight,

                    /**
                    */

                    Links = links.ToArray()

                });
            }

            /**
            */

            graph.Sort((a, b) => string.CompareOrdinal(a.Label, b.Label));

            /**
            */

            return graph;
        }

        public static void Save(this IList<Node> graph, string file)
        {
            using (var writer = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                writer.Write("{\r\n\t\"graph\": [");

                int written = 0;

                for (int i = 0; i < graph.Count; i++)
                {
                    var node = graph[i];

                    if (written > 0)
                    {
                        writer.Write(", {\r\n");
                    }
                    else
                    {
                        writer.Write("\r\n\t{\r\n");
                    }

                    written++;

                    writer.Write($"\t\t\"id\": {node.No}, ");
                    writer.Write($"\"l\": \"{node.Label}\", ");
                    writer.Write($"\"w\": {node.Weight},\r\n");
                    writer.Write($"\t\t\"n\": [");

                    int links = 0;

                    for (int j = 0; node.Links != null && j < node.Links.Length; j++)
                    {
                        var link = node.Links[j];

                        if (links > 0)
                        {
                            writer.Write(",\r\n\t\t\t{");
                        }
                        else
                        {
                            writer.Write("\r\n\t\t\t{");
                        }

                        links++;

                        writer.Write($"\"id\": {link.No}, ");
                        writer.Write($"\"w\": {link.Weight}}}");
                    }

                    writer.Write($"\r\n\t\t]\r\n");

                    writer.Write("\t}");

                }

                writer.Write("\r\n\t]\r\n}");
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
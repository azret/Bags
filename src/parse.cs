namespace System.IO
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Tokens
    {
        public static IList<string> Parse(string file) { return Parse(file, null); }
        public static IList<string> Parse(string file, Action<string, Action<string>> read)
        {
            int i = 0; string plain = File.ReadAllText(file);

            var DOC = new List<string>();
             
            while (i < plain.Length)
            {
                if (char.IsWhiteSpace(plain[i]))
                {
                    i++;

                    while (i < plain.Length
                                                && char.IsWhiteSpace(plain[i]))
                    {
                        i++;
                    }
                }
                else
                {
                    int start = i;

                    while (i < plain.Length
                                                && !char.IsWhiteSpace(plain[i]))
                    {
                        i++;
                    }

                    int end = i;

                    // Trim Left

                    while (start < end
                                                && (char.IsPunctuation(plain[start]) || !char.IsLetter(plain[start])))
                    {
                        start++;
                    }

                    // Trim Right

                    while (end > start
                                                && (char.IsPunctuation(plain[end - 1]) || !char.IsLetter(plain[end - 1])))
                    {
                        end--;
                    }

                    int len = end - start;

                    if (len > 0)
                    {
                        String s = plain.Substring(start, len);

                        if (read != null)
                        {
                            read(s, (w) =>
                            {
                                if (!String.IsNullOrWhiteSpace(w))
                                {
                                    DOC.Add(w);
                                }
                            });
                        }                        
                    }
                }

            }

            return DOC;
        }
        public static void Parse(string[] paths, string search, Action<string, Action<string>> read, Action<string, IList<string>> process)
        {
            ISet<string> files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in paths)
            {
                foreach (var file in Directory.EnumerateFiles(path, search, SearchOption.AllDirectories))
                {
                    if (files.Contains(file))
                    {
                        continue;
                    }

                    files.Add(file);
                }
            }

            if (process != null)
            {
                Parallel.ForEach(files, (file) =>
                {

                    process(file, Parse(file, read));

                });
            }
        }
    }
}
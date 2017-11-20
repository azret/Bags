namespace System.IO
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class Tokens
    {
        public static IList<string> Map(string file) { return Map(file, null); }
        public static IList<string> Map(string file, Action<string, Action<string>> read)
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

        public static void Save(string file, IEnumerable<object> data)
        {
            using (var FILE = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                int SIZE = 0;

                foreach (var i in data)
                {
                    if (SIZE > 0)
                    {
                        FILE.WriteByte((byte)'\n');
                    }

                    byte[] bytes = Encoding.UTF8.GetBytes(i.ToString());

                    if (bytes != null && bytes.Length > 0)
                    {
                        FILE.Write(bytes, 0, bytes.Length);

                        SIZE += bytes.Length;
                    }
                }
            }
        }

        public static void Map<T>(string[] paths, string search, Action<string, Action<string>> read, Action<string, IList<string>, T> process, T state)
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

                    process(file, Map(file, read), state);

                });
            }
        }
        
    }
}
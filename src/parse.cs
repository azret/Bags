namespace System.IO
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class Tokens
    {
        public static IList<string> Map(string file) { return Map(file, null); }
        public static IList<string> Map(string file, Func<string, string> take)
        {
            int i = 0; string plain = File.ReadAllText(file);

            var DOC = new List<string>();

            Func<char, bool> IsPunctuation = (c) =>
           {
               switch (c)
               {
                   case '!': return true;
                   case '"': return true;
                   case '#': return true;
                   case '$': return true;
                   case '%': return true;
                   case '&': return true;
                   case '\'': return true;
                   case '(': return true;
                   case ')': return true;
                   case '*': return true;
                   case '+': return true;
                   case ',': return true;
                   case '.': return true;
                   case '/': return true;
                   case ':': return true;
                   case ';': return true;
                   case '<': return true;
                   case '=': return true;
                   case '>': return true;
                   case '?': return true;
                   case '@': return true;
                   case '\\': return true;
                   case '^': return true;
                   case '`': return true;
                   case '{': return true;
                   case '|': return true;
                   case '}': return true;
                   case '~': return true;
                   case '-': return true;
               }

               return false;
           };

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
                                                && (IsPunctuation(plain[start]) || !char.IsLetter(plain[start])))
                    {
                        start++;
                    }

                    // Trim Right

                    while (end > start
                                                && (IsPunctuation(plain[end - 1]) || !char.IsLetter(plain[end - 1])))
                    {
                        end--;
                    }

                    int len = end - start;

                    if (len > 0)
                    {
                        String s = plain.Substring(start, len);

                        if (take != null)
                        {
                            s = take(s);
                        }

                        if (!String.IsNullOrWhiteSpace(s))
                        {
                            DOC.Add(s);
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

        public static void Map<T>(string[] paths, string search, Func<string, string> read, Action<string, IList<string>, T> process, T state)
        {
            ISet<string> processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in paths)
            {
                foreach (var file in Directory.EnumerateFiles(path, search, SearchOption.AllDirectories))
                {
                    if (processed.Contains(file))
                    {
                        continue;
                    }

                    processed.Add(file);

                    if (process != null)
                    {
                        process(file, Map(file, read), state);
                    }
                }
            }
        }

        public static void Map<T>(string path, string search, Func<string, string> read, Action<string, IList<string>, T> process, T state)
        {
            foreach (var file in Directory.EnumerateFiles(path, search, SearchOption.AllDirectories))
            {
                if (process != null)
                {
                    process(file, Map(file, read), state);
                }
            }
        }

        public static void Map(string path, string search, Func<string, string> read, Action<string, IList<string>> process)
        {
            foreach (var file in Directory.EnumerateFiles(path, search, SearchOption.AllDirectories))
            {
                if (process != null)
                {
                    process(file, Map(file, read));
                }
            }
        }
    }
}
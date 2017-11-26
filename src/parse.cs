namespace System.IO {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Tokens {
        public static IList<string> Parse(string plain) { return Parse(null, null, plain); }
        public static IList<string> Parse(string file, Action<string, Action<string>> read) { return Parse(null, file, read); }
        public static IList<string> Parse(IList<string> doc, string file, Action<string, Action<string>> read) { return Parse(doc, read, File.ReadAllText(file)); }
        public static IList<string> Parse(IList<string> doc, Action<string, Action<string>> read, string plain) {
            if (doc == null) {
                doc = new List<string>();
            }
            for (int i = 0; i < plain.Length;) {
                if (char.IsWhiteSpace(plain[i]) || char.IsPunctuation(plain[i])) {

                    i++;
                    while (i < plain.Length
                            && (char.IsWhiteSpace(plain[i]))) {
                        i++;
                    }

                } else {

                    int start = i;

                    while (i < plain.Length
                            && !(char.IsWhiteSpace(plain[i]))) {
                        i++;
                    }

                    int end = i;

                    while (start < end
                            && (char.IsPunctuation(plain[start]) || !char.IsLetter(plain[start]))) {
                        start++;
                    }

                    while (end > start
                            && (char.IsPunctuation(plain[end - 1]) || !char.IsLetter(plain[end - 1]))) {
                        end--;
                    }

                    int len = end - start;

                    if (len > 0) {
                        string s = plain.Substring(start, len);
                        if (read != null) {
                            read(s, (w) => {
                                if (!String.IsNullOrWhiteSpace(w)) {
                                    doc.Add(w);
                                }
                            });
                        } else {
                            doc.Add(s);
                        }
                    }
                }
            }

            return doc;
        }
        public static void Parse(string[] paths, string search, Action<string, Action<string>> read, Action<string, IList<string>> process) {
            ISet<string> files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in paths) {
                foreach (var file in Directory.EnumerateFiles(path, search, SearchOption.AllDirectories)) {
                    if (files.Contains(file)) {
                        continue;
                    }

                    files.Add(file);
                }
            }
            if (process != null) {
                Parallel.ForEach(files, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, (file) => {
                    process(file, Parse(file, read));
                });
            }
        }
    }
}
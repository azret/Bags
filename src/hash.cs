namespace System {
    public class Hash {
        public const int MAX = 15485863;

        public class Gram : IComparable<Gram> {
            public static int min(int a, int b) {
                if (a > b) {
                    return b;
                } else {
                    return a;
                }
            }

            public static int compare(string a, string b) {
                int l = min(a.Length, b.Length);
                for (int i = 0; i < l; i++) {
                    if (a[i] > b[i]) {
                        return +1;
                    } else if (a[i] < b[i]) {
                        return -1;
                    }
                }
                if (a.Length > b.Length) {
                    return +1;
                } else if (a.Length < b.Length) {
                    return -1;
                }
                return 0;
            }

            public static int hash(string s) {
                int h = 0x1505;
                for (int i = 0; i < s.Length; i++) {
                    h = (h << 0x5) + h + s[i];
                }
                return h & 0x7fffffff;
            }

            public Gram(int h, string s) {
                this.h = h;
                this.s = s;
            }

            int h;
            public override int GetHashCode() {
                return h;
            }

            string s;
            public override string ToString() {
                return s;
            }

            int count;
            public int Count {
                get {
                    return count;
                }
            }

            public void SetCount(int value) {
                count = value;
            }

            public int CompareTo(string s) {
                return compare(this.s, s);
            }

            int IComparable<Gram>.CompareTo(Gram other) {
                return compare(this.s, other.s);
            }
        }

        int count;
        public int Count {
            get {
                return count;
            }
        }

        int version;
        public int Version {
            get {
                return version;
            }
        }

        int collisions;
        public int Collisions {
            get {
                return collisions;
            }
        }

        class Node {
            public Gram g;
            public Node n;
        }

        Node[] l;

        public Hash(int size = MAX) {
            if (size > MAX) {
                throw new ArgumentOutOfRangeException();
            }
            l = new Node[size];
        }        

        Node find(int h, string s) {
            Node n = l[h % l.Length];
            while (n != null) {
                if (n.g.GetHashCode() == h && n.g.CompareTo(s) == 0) {
                    return n;
                }
                n = n.n;
            }
            return null;
        }

        public Gram find(string s) {
            Node n = find(Gram.hash(s), s);
            if (n != null) {
                return n.g;
            }
            return null;
        }

        public void forEach(Action<Gram> take) {
            for (int i = 0; i < l.Length; i++) {
                Node n = l[i];
                while (n != null) {
                    take(n.g);
                    n = n.n;
                }
            }
        }

        public Gram inc(string s) { return inc(Gram.hash(s), s); }
        Gram inc(int h, string s) {
            Node n = find(h, s);
            if (n == null) {
                n = new Node() {
                    g = new Gram(h, s)
                };
                int i = h % l.Length;                
                n.n = l[i];
                if (n.n != null) {
                    collisions++;
                }
                l[i] = n;
                count++;
            }
            version++;
            n.g.SetCount(n.g.Count + 1);
            return n.g;
        }         
    }
}
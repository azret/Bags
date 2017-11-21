; (function () {

    "use strict";

    function Compare(s, c) {
        var l = Math.min(s.length, c.length);
        for (var i = 0; i < l; i++) {
            if (s[i] > c[i]) {
                return +1;
            }
            else if (s[i] < c[i]) {
                return -1;
            }
        }
        if (s.length > c.length) {
            return +1;
        }
        else if (s.length < c.length) {
            return -1;
        }
        return 0;
    }

    function Find(g, s) {
        var found = null;
        let low = 0, high = g.length - 1;
        while (low <= high) {
            let i = (low + high) >> 1, p = g[i];
            const c = Compare(p.l, s);
            if (c < 0) {
                low = i + 1;
            }
            else {
                high = i - 1;

                if (c == 0) {
                    found = p;
                }
            }
        }
        return found;
    }

}());
function Compare(s, comparand) {
    if (s.length > comparand.length) {
        return +1;
    }
    else if (s.length < comparand.length) {
        return -1;
    }
    var l = s.length;
    for (var i = 0; i < l; i++) {
        if (s[i] > comparand[i]) {
            return +1;
        }
        else if (s[i] < comparand[i]) {
            return -1;
        }
    }
    return 0;
}

function Find(graph, label) {
    var found = null;
    let low = 0, high = graph.length - 1;
    while (low <= high) {
        let i = (low + high) >> 1, p = graph[i];
        const c = Compare(p.l, label);
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
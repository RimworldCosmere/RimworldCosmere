#!/usr/bin/env bash
# Fails if any C# file has an inline // comment running past one line.
# The ceiling: 1 line inline, 2 prose lines for a /// docblock.
set -uo pipefail

root="${1:-.}"
found=0

# git decides what to check, not a path list: it already knows which files are ours, so
# every ignored tree - build output, dependencies, scratch checkouts - drops out on its own.
list_sources() {
    git -C "$root" ls-files --cached --others --exclude-standard -z -- '*.cs' \
        | tr '\0' '\n' \
        | grep -v '\.generated\.cs$' \
        | sed "s|^|$root/|"
}

if ! git -C "$root" rev-parse --git-dir >/dev/null 2>&1; then
    echo "check-comment-length: $root is not a git repository" >&2
    exit 2
fi

while IFS= read -r file; do
    awk -v f="$file" '
        /^[[:space:]]*\/\/[^\/]/ { if (c == 0) start = FNR; c++; next }
        { if (c > 1) printf "%s(%d): error CC0001: inline comment runs %d lines, ceiling is 1. use a /// docblock (max 2 prose lines) or cut it.\n", f, start, c; c = 0 }
        END { if (c > 1) printf "%s(%d): error CC0001: inline comment runs %d lines, ceiling is 1. use a /// docblock (max 2 prose lines) or cut it.\n", f, start, c }
    ' "$file"
    awk -v f="$file" '
        /^[[:space:]]*\/\/[^\/]/ && length($0) > 120 { printf "%s(%d): error CC0002: comment line is %d chars, ceiling is 120. cut it to one fact.\n", f, FNR, length($0) }
    ' "$file"
done < <(list_sources) > /tmp/cc0001.txt

if [ -s /tmp/cc0001.txt ]; then
    cat /tmp/cc0001.txt
    found=$(wc -l < /tmp/cc0001.txt)
    echo "CC000x: $found comment(s) over the one-line ceiling" >&2
    exit 1
fi
exit 0

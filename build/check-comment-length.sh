#!/usr/bin/env bash
# Fails if any C# file has an inline // comment running past one line.
# The ceiling lives in ~/.claude/skills/code-comments: 1 line inline, 2 for a /// docblock.
set -uo pipefail

root="${1:-.}"
found=0

while IFS= read -r file; do
    awk -v f="$file" '
        /^[[:space:]]*\/\/[^\/]/ { if (c == 0) start = FNR; c++; next }
        { if (c > 1) printf "%s(%d): error CC0001: inline comment runs %d lines, ceiling is 1. use a /// docblock (max 2 prose lines) or cut it.\n", f, start, c; c = 0 }
        END { if (c > 1) printf "%s(%d): error CC0001: inline comment runs %d lines, ceiling is 1. use a /// docblock (max 2 prose lines) or cut it.\n", f, start, c }
    ' "$file"
    awk -v f="$file" '
        /^[[:space:]]*\/\/[^\/]/ && length($0) > 120 { printf "%s(%d): error CC0002: comment line is %d chars, ceiling is 120. cut it to one fact.\n", f, FNR, length($0) }
    ' "$file"
done < <(find "$root" -name '*.cs' \
    -not -name '*.generated.cs' \
    -not -path '*/obj/*' -not -path '*/bin/*' \
    -not -path '*/.worktrees/*' -not -path '*/.claude/*') > /tmp/cc0001.txt

if [ -s /tmp/cc0001.txt ]; then
    cat /tmp/cc0001.txt
    found=$(wc -l < /tmp/cc0001.txt)
    echo "CC000x: $found comment(s) over the one-line ceiling" >&2
    exit 1
fi
exit 0

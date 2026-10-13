#!/usr/bin/env python3
"""Extract .NET 4 registry sections from Wine system.reg into standalone .reg file."""
import re
import sys
from pathlib import Path

V4_MARKERS = (
    "v4.0.30319",
    "4.0.30319",
    "NET Framework Setup\\\\NDP\\\\v4",
    "NET Framework Setup\\NDP\\v4",
    "NET Framework Setup\\\\v4",
    "NET Framework Setup\\v4",
    "Framework\\v4.0",
    "Framework\\\\v4.0",
    "Policy\\\\v4.0",
    "Policy\\v4.0",
    "dotnet40",
    "Client Profile v4",
    "Full Profile v4",
)


def parse_key(line: str) -> str:
    m = re.match(r"^\[(.+?)\]", line)
    return m.group(1) if m else ""


def section_matches(header: str) -> bool:
    return any(m in header for m in V4_MARKERS)


def extract_sections(reg_text: str) -> list[str]:
    sections: list[str] = []
    current: list[str] = []
    header = ""

    for line in reg_text.splitlines():
        if line.startswith("["):
            if current and section_matches(header):
                sections.append("\n".join(current) + "\n")
            header = parse_key(line)
            current = [line]
        elif current:
            current.append(line)

    if current and section_matches(header):
        sections.append("\n".join(current) + "\n")

    return sections


def main() -> int:
    if len(sys.argv) != 3:
        print(f"Usage: {sys.argv[0]} <system.reg> <out.reg>", file=sys.stderr)
        return 2

    src = Path(sys.argv[1]).read_text(encoding="utf-8", errors="replace")
    sections = extract_sections(src)
    if not sections:
        print("ERROR: no .NET 4 sections found", file=sys.stderr)
        return 1

    out = "".join(sections)
    Path(sys.argv[2]).write_text(out, encoding="utf-8")
    print(f"Extracted {len(sections)} .NET 4 registry sections → {sys.argv[2]}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

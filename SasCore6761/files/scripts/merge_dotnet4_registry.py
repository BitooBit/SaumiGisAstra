#!/usr/bin/env python3
"""Merge .NET 4 registry sections from dotnet4_registry.reg into Wine system.reg (offline, no wine)."""
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
    "Policy\\\\v4.0",
    "Policy\\v4.0",
    "Framework\\v4.0",
    "Framework\\\\v4.0",
    "dotnet40",
)


def parse_key(line: str) -> str:
    m = re.match(r"^\[(.+?)\]", line)
    return m.group(1) if m else ""


def section_matches(header: str) -> bool:
    return any(m in header for m in V4_MARKERS)


def remove_v4_sections(reg_text: str) -> str:
    out: list[str] = []
    current: list[str] = []
    header = ""
    keep = True
    preamble: list[str] = []
    seen_key = False

    for line in reg_text.splitlines(keepends=True):
        if line.startswith("["):
            if not seen_key:
                out.extend(preamble)
                seen_key = True
            if current and keep:
                out.extend(current)
            header = parse_key(line.rstrip("\n"))
            current = [line]
            keep = not section_matches(header)
        elif not seen_key:
            preamble.append(line)
        elif current:
            current.append(line)

    if not seen_key:
        return reg_text
    if current and keep:
        out.extend(current)
    text = "".join(out)
    if not text.lstrip("\ufeff").startswith("WINE REGISTRY"):
        raise SystemExit("ERROR: merge would drop WINE REGISTRY header — abort")
    return text


def read_sections(reg_text: str) -> list[str]:
    sections: list[str] = []
    current: list[str] = []
    header = ""

    for line in reg_text.splitlines():
        if line.startswith("["):
            if current:
                sections.append("\n".join(current) + "\n")
            header = parse_key(line)
            current = [line]
        elif current:
            current.append(line)

    if current:
        sections.append("\n".join(current) + "\n")

    # drop Windows Registry Editor header if present
    if sections and sections[0].startswith("Windows Registry Editor"):
        sections = sections[1:]
    return [s for s in sections if section_matches(parse_key(s.splitlines()[0]))]


def main() -> int:
    if len(sys.argv) != 4:
        print(
            f"Usage: {sys.argv[0]} <dotnet4_registry.reg> <target_system.reg> <backup.reg>",
            file=sys.stderr,
        )
        return 2

    addon = Path(sys.argv[1]).read_text(encoding="utf-8", errors="replace")
    target = Path(sys.argv[2])
    backup = Path(sys.argv[3])

    sections = read_sections(addon)
    if not sections:
        print("ERROR: no .NET 4 sections in addon registry", file=sys.stderr)
        return 1

    original = target.read_text(encoding="utf-8", errors="replace")
    backup.write_text(original, encoding="utf-8")

    cleaned = remove_v4_sections(original)
    merged = cleaned.rstrip() + "\n\n" + "".join(sections)
    target.write_text(merged, encoding="utf-8")
    print(f"Merged {len(sections)} .NET 4 registry sections into {target}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

#!/usr/bin/env python3
"""Merge .NET-related registry keys from old Wine prefix into new prefix."""
import re
import sys
from pathlib import Path

KEY_PREFIXES = (
    "NETFramework",
    "NET Framework Setup",
    "Fusion",
    "StrongName",
    "ASP.NET",
)


def parse_key(line: str) -> str:
    m = re.match(r"^\[(.+?)\]", line)
    return m.group(1) if m else ""


def key_matches(key: str) -> bool:
    if "Software\\Microsoft\\" not in key and "Software\\\\Microsoft\\\\" not in key:
        return False
    return any(p in key for p in KEY_PREFIXES)


def extract_sections(reg_text: str) -> list[str]:
    sections: list[str] = []
    current: list[str] = []
    header = ""

    for line in reg_text.splitlines():
        if line.startswith("["):
            if current and key_matches(header):
                sections.append("\n".join(current) + "\n")
            header = parse_key(line)
            current = [line]
        elif current:
            current.append(line)

    if current and key_matches(header):
        sections.append("\n".join(current) + "\n")

    return sections


def remove_existing_sections(reg_text: str) -> str:
    """Drop matching .NET sections but keep Wine registry preamble (WINE REGISTRY Version …)."""
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
            keep = not key_matches(header)
        elif not seen_key:
            preamble.append(line)
        elif current:
            current.append(line)

    if not seen_key:
        return reg_text
    if current and keep:
        out.extend(current)
    # Ensure Wine header survives even if somehow empty
    text = "".join(out)
    if not text.lstrip("\ufeff").startswith("WINE REGISTRY"):
        raise SystemExit("ERROR: merge would drop WINE REGISTRY header — abort")
    return text


def main() -> int:
    if len(sys.argv) != 4:
        print(f"Usage: {sys.argv[0]} <old_system.reg> <target_system.reg> <backup.reg>", file=sys.stderr)
        return 2

    old_reg = Path(sys.argv[1]).read_text(encoding="utf-8", errors="replace")
    target = Path(sys.argv[2])
    backup = Path(sys.argv[3])

    sections = extract_sections(old_reg)
    if not sections:
        print("ERROR: no .NET sections found in old registry", file=sys.stderr)
        return 1

    original = target.read_text(encoding="utf-8", errors="replace")
    backup.write_text(original, encoding="utf-8")

    cleaned = remove_existing_sections(original)
    merged = cleaned.rstrip() + "\n\n" + "".join(sections)
    target.write_text(merged, encoding="utf-8")
    print(f"Merged {len(sections)} .NET registry sections into {target}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

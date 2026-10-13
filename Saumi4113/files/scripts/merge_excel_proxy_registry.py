#!/usr/bin/env python3
"""Merge SaumiExcelProxy.reg (HKCR) into Wine system.reg (Software\\\\Classes). Offline, no wine."""
from __future__ import annotations

import re
import sys
import time
from pathlib import Path

TS = int(time.time())


def convert_win_reg_to_wine_sections(reg_text: str) -> list[str]:
    sections: list[str] = []
    current: list[str] = []
    for line in reg_text.splitlines():
        if line.startswith("["):
            if current:
                sections.append("\n".join(current) + "\n")
            key = line.strip()[1:-1]
            if key.upper().startswith("HKEY_CLASSES_ROOT\\"):
                key = "Software\\\\Classes\\\\" + key[len("HKEY_CLASSES_ROOT\\") :].replace("\\", "\\\\")
            elif key.upper().startswith("HKCR\\"):
                key = "Software\\\\Classes\\\\" + key[len("HKCR\\") :].replace("\\", "\\\\")
            else:
                # skip non-HKCR
                current = []
                continue
            current = [f"[{key}] {TS}", f"#time={TS:x}"]
        elif current:
            if not line.strip() or line.startswith(";"):
                continue
            current.append(line)
    if current:
        sections.append("\n".join(current) + "\n")
    return sections


def section_key(section: str) -> str:
    m = re.match(r"^\[(.+?)\]", section)
    return m.group(1) if m else ""


def remove_keys(reg_text: str, keys: set[str]) -> str:
    out: list[str] = []
    current: list[str] = []
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
            header = section_key(line)
            current = [line]
            keep = header not in keys
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
        raise SystemExit("ERROR: merge would drop WINE REGISTRY header")
    return text


def main() -> int:
    if len(sys.argv) != 4:
        print(
            f"Usage: {sys.argv[0]} <SaumiExcelProxy.reg> <system.reg> <backup.reg>",
            file=sys.stderr,
        )
        return 2

    addon = Path(sys.argv[1]).read_text(encoding="utf-8", errors="replace")
    target = Path(sys.argv[2])
    backup = Path(sys.argv[3])

    sections = convert_win_reg_to_wine_sections(addon)
    if not sections:
        print("ERROR: no HKCR sections in Excel .reg", file=sys.stderr)
        return 1

    keys = {section_key(s) for s in sections}
    original = target.read_text(encoding="utf-8", errors="replace")
    backup.write_text(original, encoding="utf-8")
    cleaned = remove_keys(original, keys)
    merged = cleaned.rstrip() + "\n\n" + "\n".join(sections) + "\n"
    target.write_text(merged, encoding="utf-8")
    print(f"Merged {len(sections)} Excel.Application registry sections into {target}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

#!/usr/bin/env python3
"""Fix a tcc-built Win32 DLL so Wine GetProcAddress can find stdcall exports.

tcc writes _Name@N. Wine/Windows binary-search AddressOfNames, which MUST be
sorted. After renaming _Name@N -> Name the table is no longer sorted, and
import_dll reports "No implementation" — mmp32 then calls a stub and dies.
"""
from __future__ import annotations

import argparse
import re
import struct
import sys
from pathlib import Path

DECORATED = re.compile(rb"^_([A-Za-z0-9_]+)@\d+\x00")


def rva_to_off(secs, rva):
    for va, raw, span in secs:
        if va <= rva < va + span:
            return raw + (rva - va)
    raise SystemExit(f"RVA {rva:#x} not in any section")


def load_pe(data: bytes):
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    opt = pe + 24
    nsec = struct.unpack_from("<H", data, pe + 6)[0]
    soh = struct.unpack_from("<H", data, pe + 20)[0]
    sec = opt + soh
    secs = []
    for i in range(nsec):
        o = sec + i * 40
        vsz, va, rsz, raw = struct.unpack_from("<IIII", data, o + 8)
        secs.append((va, raw, max(vsz, rsz)))
    exp_rva = struct.unpack_from("<I", data, opt + 96)[0]
    return pe, opt, secs, exp_rva


def read_cstr(data, off):
    end = data.find(b"\x00", off)
    if end < 0:
        raise SystemExit("unterminated export name")
    return data[off:end]


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("dll", type=Path)
    args = ap.parse_args()
    data = bytearray(args.dll.read_bytes())
    _, opt, secs, exp_rva = load_pe(data)
    exp = rva_to_off(secs, exp_rva)
    n_names = struct.unpack_from("<I", data, exp + 24)[0]
    a_names = struct.unpack_from("<I", data, exp + 32)[0]
    a_ords = struct.unpack_from("<I", data, exp + 36)[0]
    names_off = rva_to_off(secs, a_names)
    ords_off = rva_to_off(secs, a_ords)

    stripped = 0
    entries = []
    for i in range(n_names):
        nrva = struct.unpack_from("<I", data, names_off + i * 4)[0]
        noff = rva_to_off(secs, nrva)
        m = DECORATED.match(data[noff : noff + 80])
        if m:
            plain = m.group(1) + b"\x00"
            data[noff : noff + len(plain)] = plain
            stripped += 1
        name = read_cstr(data, noff).decode("ascii")
        ordv = struct.unpack_from("<H", data, ords_off + i * 2)[0]
        entries.append((name, nrva, ordv))

    ordered = sorted(entries, key=lambda t: t[0])
    for i, (_name, nrva, ordv) in enumerate(ordered):
        struct.pack_into("<I", data, names_off + i * 4, nrva)
        struct.pack_into("<H", data, ords_off + i * 2, ordv)

    names = [t[0] for t in ordered]
    if names != sorted(names):
        raise SystemExit("export names still unsorted")
    args.dll.write_bytes(data)
    print(f"OK: {args.dll} names={n_names} stripped={stripped} sorted")
    for n in names:
        print(" ", n)
    return 0


if __name__ == "__main__":
    sys.exit(main())

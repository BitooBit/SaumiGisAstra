#!/usr/bin/env python3
"""Поставить managed .NET 4 dll в GAC с реальным PublicKeyToken.

Fusion ищет System, Version=4.0.0.0, PublicKeyToken=b77a5c561934e089 в
  GAC_MSIL/System/v4.0_4.0.0.0__b77a5c561934e089/System.dll
а не в ...__b03f5f7f11d50a3a (ECMA-ключ; у System.dll его нет).
"""
from __future__ import annotations

import argparse
import hashlib
import shutil
import struct
import sys
from pathlib import Path

ILONLY = 0x00000001

# ECMA-335 coded-index tables (tag bits, table ids)
RES_SCOPE = (2, (0x00, 0x1A, 0x23, 0x01))
TYPE_DEF_OR_REF = (2, (0x02, 0x01, 0x1B))
HAS_CONSTANT = (2, (0x04, 0x08, 0x17))
HAS_CA = (5, (0x06, 0x04, 0x01, 0x02, 0x08, 0x09, 0x0A, 0x00,
             0x0E, 0x17, 0x14, 0x11, 0x1A, 0x1B, 0x20, 0x23,
             0x26, 0x27, 0x28, 0x2A, 0x2C))
HAS_FIELD_MARSHAL = (1, (0x04, 0x08))
HAS_DECL = (2, (0x02, 0x06, 0x20))
MEMBER_REF_PARENT = (3, (0x02, 0x01, 0x1A, 0x06, 0x1B))
HAS_SEMANTICS = (1, (0x14, 0x17))
METHOD_DEF_OR_REF = (1, (0x06, 0x0A))
MEMBER_FORWARDED = (1, (0x04, 0x06))
IMPL = (2, (0x26, 0x23, 0x27))
# CustomAttributeType: 3 tag bits, slots 2=MethodDef 3=MemberRef
CA_TYPE = (3, (0x06, 0x0A))


def u16(data: bytes, off: int) -> int:
    return struct.unpack_from("<H", data, off)[0]


def u32(data: bytes, off: int) -> int:
    return struct.unpack_from("<I", data, off)[0]


def rva_to_off(data: bytes, rva: int) -> int | None:
    e_lfanew = u32(data, 0x3C)
    nsections = u16(data, e_lfanew + 6)
    opt_size = u16(data, e_lfanew + 20)
    sec = e_lfanew + 24 + opt_size
    for i in range(nsections):
        o = sec + i * 40
        vsize, va, rawsize, rawptr = struct.unpack_from("<IIII", data, o + 8)
        span = max(vsize, rawsize)
        if va <= rva < va + span:
            return rawptr + (rva - va)
    return None


def read_blob(heap: bytes, idx: int) -> bytes:
    if idx <= 0 or idx >= len(heap):
        return b""
    b0 = heap[idx]
    if (b0 & 0x80) == 0:
        size, start = b0, idx + 1
    elif (b0 & 0xC0) == 0x80:
        size = ((b0 & 0x3F) << 8) | heap[idx + 1]
        start = idx + 2
    else:
        size = ((b0 & 0x1F) << 24) | (heap[idx + 1] << 16) | (heap[idx + 2] << 8) | heap[idx + 3]
        start = idx + 4
    return heap[start : start + size]


def idx_sz(n: int) -> int:
    return 4 if n >= 65536 else 2


def coded_sz(rows: dict[int, int], tag_bits: int, tables: tuple[int, ...]) -> int:
    max_rows = max((rows.get(t, 0) for t in tables), default=0)
    return 4 if max_rows >= (1 << (16 - tag_bits)) else 2


def row_sizes(rows: dict[int, int], str_i: int, guid_i: int, blob_i: int) -> dict[int, int]:
    def c(spec: tuple[int, tuple[int, ...]]) -> int:
        return coded_sz(rows, spec[0], spec[1])

    i = idx_sz
    return {
        0x00: 2 + str_i + guid_i * 3,
        0x01: c(RES_SCOPE) + str_i * 2,
        0x02: 4 + str_i * 2 + c(TYPE_DEF_OR_REF) + i(rows.get(0x04, 0)) + i(rows.get(0x06, 0)),
        0x03: i(rows.get(0x04, 0)),
        0x04: 2 + str_i + blob_i,
        0x05: i(rows.get(0x06, 0)),
        0x06: 8 + str_i + blob_i + i(rows.get(0x08, 0)),
        0x07: i(rows.get(0x08, 0)),
        0x08: 4 + str_i,
        0x09: i(rows.get(0x02, 0)) + c(TYPE_DEF_OR_REF),
        0x0A: c(MEMBER_REF_PARENT) + str_i + blob_i,
        0x0B: 2 + c(HAS_CONSTANT) + blob_i,
        0x0C: c(HAS_CA) + c(CA_TYPE) + blob_i,
        0x0D: c(HAS_FIELD_MARSHAL) + blob_i,
        0x0E: 2 + c(HAS_DECL) + blob_i,
        0x0F: 2 + 4 + i(rows.get(0x02, 0)),
        0x10: 4 + i(rows.get(0x04, 0)),
        0x11: blob_i,
        0x12: i(rows.get(0x02, 0)) + i(rows.get(0x14, 0)),
        0x13: i(rows.get(0x14, 0)),
        0x14: 2 + str_i + c(TYPE_DEF_OR_REF),
        0x15: i(rows.get(0x02, 0)) + i(rows.get(0x17, 0)),
        0x16: i(rows.get(0x17, 0)),
        0x17: 2 + str_i + blob_i,
        0x18: 2 + i(rows.get(0x06, 0)) + c(HAS_SEMANTICS),
        0x19: i(rows.get(0x02, 0)) + c(METHOD_DEF_OR_REF) * 2,
        0x1A: str_i,
        0x1B: blob_i,
        0x1C: 2 + c(MEMBER_FORWARDED) + str_i + i(rows.get(0x1A, 0)),
        0x1D: 4 + i(rows.get(0x04, 0)),
        0x1E: 8,  # ENCLog: rarely present; Token(4)+FuncCode(4)
        0x1F: 4,  # ENCMap
        0x20: 4 + 8 + 4 + blob_i + str_i + str_i,
    }


def assembly_identity(path: Path) -> tuple[str, str, str, bool] | None:
    data = path.read_bytes()
    if len(data) < 0x40 or data[:2] != b"MZ":
        return None
    e_lfanew = u32(data, 0x3C)
    if e_lfanew + 26 > len(data) or data[e_lfanew : e_lfanew + 4] != b"PE\0\0":
        return None
    magic = u16(data, e_lfanew + 24)
    opt = e_lfanew + 24
    if magic == 0x10B:
        num_dd, dd0 = u32(data, opt + 92), opt + 96
    elif magic == 0x20B:
        num_dd, dd0 = u32(data, opt + 108), opt + 112
    else:
        return None
    if num_dd < 15:
        return None
    clr_rva, clr_sz = struct.unpack_from("<II", data, dd0 + 14 * 8)
    if clr_rva == 0 or clr_sz < 20:
        return None
    clr_off = rva_to_off(data, clr_rva)
    if clr_off is None:
        return None
    flags = u32(data, clr_off + 16)
    ilonly = (flags & ILONLY) != 0
    md_rva = u32(data, clr_off + 8)
    md_off = rva_to_off(data, md_rva)
    if md_off is None or data[md_off : md_off + 4] != b"BSJB":
        return None
    ver_len = u32(data, md_off + 12)
    streams_off = (md_off + 16 + ver_len + 3) & ~3
    nstreams = u16(data, streams_off + 2)
    p = streams_off + 4
    streams: dict[str, tuple[int, int]] = {}
    for _ in range(nstreams):
        soff, ssize = struct.unpack_from("<II", data, p)
        p += 8
        end = data.index(b"\0", p)
        name = data[p:end].decode("ascii", "replace")
        p = (end + 1 + 3) & ~3
        streams[name] = (md_off + soff, ssize)
    if "#~" not in streams or "#Strings" not in streams or "#Blob" not in streams:
        return None
    tilde_off, _ = streams["#~"]
    strings = data[streams["#Strings"][0] : streams["#Strings"][0] + streams["#Strings"][1]]
    blob = data[streams["#Blob"][0] : streams["#Blob"][0] + streams["#Blob"][1]]
    heap_sizes = data[tilde_off + 6]
    str_i = 4 if heap_sizes & 1 else 2
    guid_i = 4 if heap_sizes & 2 else 2
    blob_i = 4 if heap_sizes & 4 else 2
    valid = struct.unpack_from("<Q", data, tilde_off + 8)[0]
    q = tilde_off + 24
    rows: dict[int, int] = {}
    for bit in range(64):
        if valid & (1 << bit):
            rows[bit] = u32(data, q)
            q += 4
    sizes = row_sizes(rows, str_i, guid_i, blob_i)
    cur = q
    for tid in range(0x20):
        n = rows.get(tid, 0)
        if n and tid not in sizes:
            return None
        cur += n * sizes.get(tid, 0)
    if rows.get(0x20, 0) < 1:
        return None
    row = cur
    major, minor, build, rev = struct.unpack_from("<HHHH", data, row + 4)
    off = row + 16
    if blob_i == 4:
        pk_idx = u32(data, off)
        off += 4
    else:
        pk_idx = u16(data, off)
        off += 2
    name_idx = u32(data, off) if str_i == 4 else u16(data, off)
    name = strings[name_idx:].split(b"\0", 1)[0].decode("utf-8", "replace") if 0 <= name_idx < len(strings) else ""
    pk = read_blob(blob, pk_idx)
    token = hashlib.sha1(pk).digest()[-8:][::-1].hex() if pk else "0000000000000000"
    return name, f"{major}.{minor}.{build}.{rev}", token, ilonly


def install_one(dll: Path, gac_msil: Path, gac32: Path) -> str | None:
    ident = assembly_identity(dll)
    if ident is None:
        return None
    name, ver, token, ilonly = ident
    if not name or token == "0000000000000000":
        return None
    folder = f"v4.0_{ver}__{token}"
    dest_root = gac_msil if ilonly else gac32
    dest_dir = dest_root / name / folder
    dest_dir.mkdir(parents=True, exist_ok=True)
    shutil.copy2(dll, dest_dir / f"{name}.dll")
    # System.EnterpriseServices грузит native Wrapper/Thunk из той же папки GAC.
    if name == "System.EnterpriseServices":
        for extra in (
            "System.EnterpriseServices.Wrapper.dll",
            "System.EnterpriseServices.Thunk.dll",
        ):
            src = dll.parent / extra
            if src.is_file():
                shutil.copy2(src, dest_dir / extra)
    parent = dest_root / name
    for child in parent.iterdir():
        if child.is_dir() and child.name != folder:
            shutil.rmtree(child, ignore_errors=True)
    return f"{name} {ver} {token} {'MSIL' if ilonly else 'x86'}"


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--framework", required=True)
    ap.add_argument("--gac", required=True)
    ap.add_argument("--gac32", default="")
    args = ap.parse_args()
    fw = Path(args.framework)
    gac = Path(args.gac)
    gac32 = Path(args.gac32) if args.gac32 else gac.parent / "GAC_32"
    if not fw.is_dir():
        print(f"FAIL: нет {fw}", file=sys.stderr)
        return 1
    gac.mkdir(parents=True, exist_ok=True)
    gac32.mkdir(parents=True, exist_ok=True)

    n = 0
    for dll in sorted(fw.glob("*.dll")):
        if dll.name.startswith("._"):
            continue
        info = install_one(dll, gac, gac32)
        if info is None:
            continue
        n += 1
    print(f"GAC: {n} managed")
    sys_dll = fw / "System.dll"
    if sys_dll.is_file():
        ident = assembly_identity(sys_dll)
        print(f"System.dll identity {ident}")
        if not ident or ident[2] != "b77a5c561934e089" or ident[0] != "System":
            print("FAIL: System.dll token/name", file=sys.stderr)
            return 2
        expect = gac / "System" / f"v4.0_{ident[1]}__{ident[2]}" / "System.dll"
        if not expect.is_file():
            print(f"FAIL: нет {expect}", file=sys.stderr)
            return 3
        print(f"System.dll GAC OK {expect}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

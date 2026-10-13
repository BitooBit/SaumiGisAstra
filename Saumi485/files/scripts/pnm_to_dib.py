#!/usr/bin/env python3
"""PNM P4/P5/P6 from scanimage → 24bpp bottom-up DIB (no BITMAPFILEHEADER).

HP hpaio default is Lineart = P4 (1-bit). P5 gray / P6 RGB also ok.
"""
from __future__ import annotations

import struct
import sys
from pathlib import Path


def _read_token(fp) -> bytes:
    while True:
        ch = fp.read(1)
        if not ch:
            raise ValueError("unexpected EOF")
        if ch in b" \t\r\n":
            continue
        if ch == b"#":
            while ch and ch not in b"\n\r":
                ch = fp.read(1)
            continue
        tok = ch
        while True:
            ch = fp.read(1)
            if not ch or ch in b" \t\r\n":
                break
            tok += ch
        return tok


def _pack_dib(w: int, h: int, pixels: bytes) -> bytes:
    header = struct.pack(
        "<IiiHHIIiiII",
        40,
        w,
        h,
        1,
        24,
        0,
        len(pixels),
        0,
        0,
        0,
        0,
    )
    return header + pixels


def pnm_to_dib(data: bytes) -> bytes:
    import io

    fp = io.BytesIO(data)
    magic = _read_token(fp)
    if magic not in (b"P4", b"P5", b"P6"):
        raise ValueError(f"unsupported PNM {magic!r}")
    w = int(_read_token(fp))
    h = int(_read_token(fp))
    if w < 1 or h < 1 or w > 20000 or h > 20000:
        raise ValueError(f"bad size {w}x{h}")
    if magic != b"P4":
        maxv = int(_read_token(fp))
        if maxv > 255:
            raise ValueError("16-bit PNM not supported")
    raw = fp.read()
    stride = (w * 3 + 3) & ~3
    pixels = bytearray(stride * h)
    if magic == b"P4":
        rowb = (w + 7) // 8
        need = rowb * h
        if len(raw) < need:
            raise ValueError(f"truncated P4 {len(raw)}<{need}")
        for y in range(h):
            dst = (h - 1 - y) * stride
            src = y * rowb
            for x in range(w):
                bit = 7 - (x & 7)
                black = (raw[src + (x >> 3)] >> bit) & 1
                v = 0 if black else 255
                pixels[dst] = v
                pixels[dst + 1] = v
                pixels[dst + 2] = v
                dst += 3
    elif magic == b"P6":
        need = w * h * 3
        if len(raw) < need:
            raise ValueError("truncated P6")
        for y in range(h):
            dst = (h - 1 - y) * stride
            src = y * w * 3
            for x in range(w):
                r, g, b = raw[src], raw[src + 1], raw[src + 2]
                pixels[dst] = b
                pixels[dst + 1] = g
                pixels[dst + 2] = r
                dst += 3
                src += 3
    else:
        need = w * h
        if len(raw) < need:
            raise ValueError("truncated P5")
        for y in range(h):
            dst = (h - 1 - y) * stride
            src = y * w
            for x in range(w):
                g = raw[src]
                pixels[dst] = g
                pixels[dst + 1] = g
                pixels[dst + 2] = g
                dst += 3
                src += 1
    return _pack_dib(w, h, bytes(pixels))


def main() -> int:
    out = Path(sys.argv[1] if len(sys.argv) > 1 else "page.dib")
    data = sys.stdin.buffer.read()
    if not data:
        print("empty stdin", file=sys.stderr)
        return 1
    dib = pnm_to_dib(data)
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_bytes(dib)
    print(f"ok {out} {len(dib)} bytes magic={data[:2]!r}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

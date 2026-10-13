#!/usr/bin/env python3
"""16x16/32x32 BMP-in-ICO (4bpp + 24bpp) for Wine SYSICONINDEX.

Geometric letter badges (P/W/X on brand-color squares). Shapes+letters are
below copyright threshold; not Microsoft/Adobe assets.
Wine SHGFI_SMALLICON uses 16x16 4bpp — keep that crisp.
"""
from __future__ import annotations

import struct
from pathlib import Path

# B,G,R,0  — 4bpp palette
PAL = [
    (255, 255, 255, 0),  # 0 white
    (0, 0, 0, 0),  # 1 black
    (36, 36, 210, 0),  # 2 PDF red RGB 210,36,36
    (70, 115, 33, 0),  # 3 Excel green RGB 33,115,70
    (200, 200, 200, 0),  # 4 gray
    (154, 87, 43, 0),  # 5 Word blue RGB 43,87,154
    (255, 255, 255, 0),  # 6 white2
    (80, 80, 230, 0),  # 7 light red
    (40, 40, 40, 0),  # 8 dark
    (50, 90, 24, 0),  # 9 dark green
    (100, 55, 28, 0),  # 10 dark blue
    (0, 128, 128, 0),
    (128, 0, 0, 0),
    (128, 0, 128, 0),
    (128, 128, 0, 0),
    (160, 160, 160, 0),
]

# 5x7, 1 = foreground
FONT = {
    "P": [
        "11110",
        "10001",
        "10001",
        "11110",
        "10000",
        "10000",
        "10000",
    ],
    "W": [
        "10001",
        "10001",
        "10001",
        "10101",
        "10101",
        "01010",
        "01010",
    ],
    "X": [
        "10001",
        "01010",
        "01010",
        "00100",
        "01010",
        "01010",
        "10001",
    ],
}


def _blank(n: int, fill: int) -> list[list[int]]:
    return [[fill for _ in range(n)] for _ in range(n)]


def _blit_letter(g: list[list[int]], letter: str, color: int, fg: int = 0) -> None:
    n = len(g)
    glyph = FONT[letter]
    gh, gw = len(glyph), len(glyph[0])
    scale = 2 if n >= 28 else 1
    tw, th = gw * scale, gh * scale
    x0 = (n - tw) // 2
    y0 = (n - th) // 2
    for y, row in enumerate(glyph):
        for x, ch in enumerate(row):
            if ch != "1":
                continue
            for dy in range(scale):
                for dx in range(scale):
                    yy, xx = y0 + y * scale + dy, x0 + x * scale + dx
                    if 0 <= yy < n and 0 <= xx < n:
                        g[yy][xx] = fg


def badge(n: int, fill: int, letter: str) -> list[list[int]]:
    g = _blank(n, fill)
    # 1px rounded corners → white (reads as a tile, not a blob)
    for x, y in ((0, 0), (0, n - 1), (n - 1, 0), (n - 1, n - 1)):
        g[y][x] = 0
    _blit_letter(g, letter, fill, fg=0)
    return g


def glyph_pdf(n: int) -> list[list[int]]:
    return badge(n, 2, "P")


def glyph_doc(n: int) -> list[list[int]]:
    return badge(n, 5, "W")


def glyph_xls(n: int) -> list[list[int]]:
    return badge(n, 3, "X")


def _pad4(b: bytes) -> bytes:
    return b + b"\x00" * ((4 - (len(b) % 4)) % 4)


def _xor_4bpp(grid: list[list[int]]) -> bytes:
    n = len(grid)
    rows = []
    for y in range(n - 1, -1, -1):
        packed = bytearray()
        x = 0
        while x < n:
            hi = grid[y][x] & 0x0F
            lo = grid[y][x + 1] & 0x0F if x + 1 < n else 0
            packed.append((hi << 4) | lo)
            x += 2
        rows.append(_pad4(bytes(packed)))
    return b"".join(rows)


def _xor_24bpp(grid: list[list[int]]) -> bytes:
    n = len(grid)
    rows = []
    for y in range(n - 1, -1, -1):
        pix = bytearray()
        for x in range(n):
            b, g, r, _ = PAL[grid[y][x] & 0x0F]
            pix.extend((b, g, r))
        rows.append(_pad4(bytes(pix)))
    return b"".join(rows)


def _and_mask(n: int) -> bytes:
    row = _pad4(b"\x00" * ((n + 7) // 8))
    return row * n


def _dib(grid: list[list[int]], bitcount: int) -> bytes:
    n = len(grid)
    if bitcount == 4:
        xor = _xor_4bpp(grid)
        pal = b"".join(struct.pack("<BBBB", *c) for c in PAL)
    elif bitcount == 24:
        xor = _xor_24bpp(grid)
        pal = b""
    else:
        raise ValueError(bitcount)
    mask = _and_mask(n)
    bih = struct.pack(
        "<IiiHHIIiiII",
        40, n, n * 2, 1, bitcount, 0, len(xor) + len(mask), 0, 0,
        16 if bitcount == 4 else 0, 0,
    )
    return bih + pal + xor + mask


def _entry(w: int, h: int, bitcount: int, size: int, offset: int) -> bytes:
    colors = 16 if bitcount == 4 else 0
    return struct.pack("<BBBBHHII", w & 0xFF, h & 0xFF, colors, 0, 1, bitcount, size, offset)


def build_ico(glyph) -> bytes:
    imgs = [
        _dib(glyph(16), 4),
        _dib(glyph(16), 24),
        _dib(glyph(32), 4),
        _dib(glyph(32), 24),
    ]
    offset = 6 + 16 * len(imgs)
    entries = []
    sizes = [(16, 16, 4), (16, 16, 24), (32, 32, 4), (32, 32, 24)]
    for (w, h, bits), data in zip(sizes, imgs):
        entries.append(_entry(w, h, bits, len(data), offset))
        offset += len(data)
    return b"".join([struct.pack("<HHH", 0, 1, len(imgs)), *entries, *imgs])


def main() -> None:
    here = Path(__file__).resolve().parent
    (here / "pdf.ico").write_bytes(build_ico(glyph_pdf))
    (here / "doc.ico").write_bytes(build_ico(glyph_doc))
    (here / "xls.ico").write_bytes(build_ico(glyph_xls))
    print("wrote", here / "pdf.ico", here / "doc.ico", here / "xls.ico")


if __name__ == "__main__":
    main()

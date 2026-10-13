#!/usr/bin/env python3
"""Build Wine SYSICONINDEX ICOs from teambox/free-file-icons (MIT).

Source PNGs: app_icons/16px and app_icons/32px (pdf/doc/xls).
16x16 4bpp + 24bpp and 32x32 4bpp + 24bpp BMP-in-ICO.
"""
from __future__ import annotations

import struct
from pathlib import Path

from PIL import Image

HERE = Path(__file__).resolve().parent
ICONS = HERE / "app_icons"


def _pad4(b: bytes) -> bytes:
    return b + b"\x00" * ((4 - (len(b) % 4)) % 4)


def _and_mask(alpha: list[list[int]]) -> bytes:
    n = len(alpha)
    rows = []
    for y in range(n - 1, -1, -1):
        packed = bytearray()
        bit = 0
        acc = 0
        for x in range(n):
            acc <<= 1
            if alpha[y][x] < 128:
                acc |= 1
            bit += 1
            if bit == 8:
                packed.append(acc)
                bit = 0
                acc = 0
        if bit:
            acc <<= 8 - bit
            packed.append(acc)
        rows.append(_pad4(bytes(packed)))
    return b"".join(rows)


def _load(name: str, n: int) -> tuple[Image.Image, list[list[int]]]:
    src = ICONS / f"{n}px" / f"{name}.png"
    if not src.is_file():
        src = ICONS / "32px" / f"{name}.png"
    im = Image.open(src).convert("RGBA")
    if im.size != (n, n):
        im = im.resize((n, n), Image.Resampling.LANCZOS)
    alpha = [[im.getpixel((x, y))[3] for x in range(n)] for y in range(n)]
    # flatten on white — Wine 4bpp AND-mask often ignores alpha
    bg = Image.new("RGBA", (n, n), (255, 255, 255, 255))
    bg.alpha_composite(im)
    return bg.convert("RGB"), alpha


def _palette16(rgb: Image.Image) -> tuple[list[tuple[int, int, int, int]], Image.Image]:
    q = rgb.quantize(colors=16, method=Image.Quantize.MEDIANCUT)
    pal = q.getpalette() or []
    colors: list[tuple[int, int, int, int]] = []
    for i in range(16):
        r = pal[i * 3] if i * 3 < len(pal) else 0
        g = pal[i * 3 + 1] if i * 3 + 1 < len(pal) else 0
        b = pal[i * 3 + 2] if i * 3 + 2 < len(pal) else 0
        colors.append((b, g, r, 0))
    return colors, q


def _xor_4bpp(idx: Image.Image, n: int) -> bytes:
    pix = idx.load()
    rows = []
    for y in range(n - 1, -1, -1):
        packed = bytearray()
        x = 0
        while x < n:
            hi = pix[x, y] & 0x0F
            lo = pix[x + 1, y] & 0x0F if x + 1 < n else 0
            packed.append((hi << 4) | lo)
            x += 2
        rows.append(_pad4(bytes(packed)))
    return b"".join(rows)


def _xor_24bpp(rgb: Image.Image, n: int) -> bytes:
    pix = rgb.load()
    rows = []
    for y in range(n - 1, -1, -1):
        row = bytearray()
        for x in range(n):
            r, g, b = pix[x, y]
            row.extend((b, g, r))
        rows.append(_pad4(bytes(row)))
    return b"".join(rows)


def _dib4(rgb: Image.Image, alpha: list[list[int]], n: int) -> bytes:
    pal, idx = _palette16(rgb)
    xor = _xor_4bpp(idx, n)
    palb = b"".join(struct.pack("<BBBB", *c) for c in pal)
    mask = _and_mask(alpha)
    bih = struct.pack(
        "<IiiHHIIiiII",
        40, n, n * 2, 1, 4, 0, len(xor) + len(mask), 0, 0, 16, 0,
    )
    return bih + palb + xor + mask


def _dib24(rgb: Image.Image, alpha: list[list[int]], n: int) -> bytes:
    xor = _xor_24bpp(rgb, n)
    mask = _and_mask(alpha)
    bih = struct.pack(
        "<IiiHHIIiiII",
        40, n, n * 2, 1, 24, 0, len(xor) + len(mask), 0, 0, 0, 0,
    )
    return bih + xor + mask


def _entry(w: int, h: int, bitcount: int, size: int, offset: int) -> bytes:
    colors = 16 if bitcount == 4 else 0
    return struct.pack("<BBBBHHII", w & 0xFF, h & 0xFF, colors, 0, 1, bitcount, size, offset)


def build_ico(name: str) -> bytes:
    rgb16, a16 = _load(name, 16)
    rgb32, a32 = _load(name, 32)
    imgs = [
        _dib4(rgb16, a16, 16),
        _dib24(rgb16, a16, 16),
        _dib4(rgb32, a32, 32),
        _dib24(rgb32, a32, 32),
    ]
    sizes = [(16, 16, 4), (16, 16, 24), (32, 32, 4), (32, 32, 24)]
    offset = 6 + 16 * len(imgs)
    entries = []
    for (w, h, bits), data in zip(sizes, imgs):
        entries.append(_entry(w, h, bits, len(data), offset))
        offset += len(data)
    return b"".join([struct.pack("<HHH", 0, 1, len(imgs)), *entries, *imgs])


def main() -> None:
    if not (ICONS / "16px" / "pdf.png").is_file():
        raise SystemExit(f"FAIL: нет PNG в {ICONS}/16px")
    for name in ("pdf", "doc", "xls"):
        out = HERE / f"{name}.ico"
        out.write_bytes(build_ico(name))
        print("wrote", out)


if __name__ == "__main__":
    main()

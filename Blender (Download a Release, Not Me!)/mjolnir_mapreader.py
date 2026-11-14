"""
mjolnir_mapreader.py - read Halo Reach MCC .map files for the Mjolnir Forge Editor.

Pure Python (only needs the standard library; numpy is used when available for speed).
It does three things:

  1. Finds the .map file MCC currently has loaded (works for modded maps too).
  2. Reads the Forge palette ("Map Variant Palettes" in the scenario tag), so every
     forge object ID can be turned into a name and a tag path.
  3. Extracts render model geometry for palette objects, so objects that have no mesh
     in forge.blend can still be shown in Blender.

File format knowledge is ported from Reclaimer (github.com/Gravemind2401/Reclaimer)
and Assembly (github.com/XboxChaos/Assembly) plugin definitions.

It can also be run on its own to check a map file, without Blender or MCC:

    python mjolnir_mapreader.py "path/to/forge_halo.map"            (prints the palette)
    python mjolnir_mapreader.py "path/to/forge_halo.map" --obj out   (also exports every palette model as .obj)
"""

import os, sys, struct, zlib, re, time, glob, threading, math
from datetime import datetime

try:
    import numpy as np
except ImportError:  # Blender always has numpy; plain Python may not
    np = None

VERSION = "1.0"

class MapReadError(Exception):
    pass

def _log_default(msg):
    print("[Mjolnir map] " + msg)

# ---------------------------------------------------------------------------------------------
# Format tables
# ---------------------------------------------------------------------------------------------

HEAD_MAGIC = 0x68656164  # 'head'
FOOT_MAGIC = 0x666F6F74  # 'foot'

# Build string -> header profile, string id table
# (from Reclaimer's CacheType enum; unknown builds are detected by trial and validation)
KNOWN_BUILDS = {
    "Jun 24 2019 00:36:03": ("U0", "U0"), "Jul 30 2019 14:17:16": ("U0", "U0"),
    "Oct 24 2019 15:56:32": ("U0", "U0"), "Jan 30 2020 16:55:25": ("U0", "U2"),
    "Mar 24 2020 12:10:36": ("U0", "U2"),
    "Jun  5 2020 10:40:14": ("U3", "U2"), "Oct 15 2020 18:23:50": ("U3", "U2"),
    "Nov 24 2020 18:32:37": ("U3", "U2"), "Mar  4 2021 13:14:28": ("U3", "U6"),
    "May 26 2021 10:02:45": ("U3", "U6"),
    "Aug 11 2021 15:50:30": ("U8", "U8"), "Sep 13 2021 09:49:52": ("U8", "U8"),
    "Sep 17 2021 13:25:40": ("U8", "U8"),
    "Jan 13 2022 00:54:50": ("U10", "U8"), "Aug  5 2022 20:35:02": ("U10", "U8"),
    "Aug 31 2022 11:53:49": ("U10", "U8"), "Aug 31 2022 11:53:19": ("U10", "U8"),
    "Oct 12 2022 01:55:01": ("U10", "U8"), "Nov 16 2022 21:11:25": ("U10", "U8"),
    "Nov 16 2022 21:13:04": ("U10", "U8"),
    "Jun 21 2023 15:35:31": ("U10", "U13"), "Jun 27 2023 08:55:51": ("U10", "U13"),
    "Jun 27 2023 08:55:19": ("U10", "U13"), "Jul 16 2023 16:12:13": ("U10", "U13"),
    "Jul 16 2023 16:08:14": ("U10", "U13"),
}

# Header field offsets per profile. 'ptr64' = 64-bit pointer read with the metadata translator.
HEADER_PROFILES = {
    # MCC release .. update 2
    "U0": dict(index_ptr=16, string_count=348, string_index_ptr=356, string_table_ptr=360,
               ns_count=None, ns_ptr=None, scenario_name=444, file_count=704, file_table_ptr=708,
               file_index_ptr=716, virtual_base=760, section_offsets=1204, section_table=1220),
    # update 3 .. update 7
    "U3": dict(index_ptr=16, string_count=336, string_index_ptr=344, string_table_ptr=348,
               ns_count=352, ns_ptr=356, scenario_name=440, file_count=700, file_table_ptr=704,
               file_index_ptr=712, virtual_base=752, section_offsets=1196, section_table=1212),
    # update 8 / 9 (August 2021 header rewrite)
    "U8": dict(index_ptr=744, string_count=48, string_index_ptr=60, string_table_ptr=52,
               ns_count=64, ns_ptr=68, scenario_name=224, file_count=32, file_table_ptr=36,
               file_index_ptr=44, virtual_base=736, section_offsets=1196, section_table=1212),
    # update 10 and later (including the current game)
    "U10": dict(index_ptr=744, string_count=48, string_index_ptr=60, string_table_ptr=52,
                ns_count=64, ns_ptr=68, scenario_name=224, file_count=32, file_table_ptr=36,
                file_index_ptr=44, virtual_base=736, section_offsets=1228, section_table=1244),
}
# Profiles whose pointer expansion magic differs (MCC flights)
FLIGHT_BUILDS = {"Jun 24 2019 00:36:03", "Jul 30 2019 14:17:16"}

# String id namespace tables for builds before the U8 header (indexBits=17, namespaceBits=8).
# (namespace id, min, startIndex) from Assembly/Reclaimer MccHaloReachStrings.xml
_NS = lambda pairs, ns0_start: [(0, 0x4C9, ns0_start)] + [(i, 0, s) for i, s in pairs]
STRING_TABLES = {
    "U0": _NS([(1,0x4C9),(2,0xB2E),(3,0xC07),(4,0xC71),(5,0xD4A),(6,0xD70),(7,0xD75),(8,0x1434),
               (9,0x15A4),(0xA,0x15B8),(0xC,0x161A),(0xD,0x1632),(0xE,0x163F),(0xF,0x1668),(0x10,0x16C9)], 0x173C),
    "U1": _NS([(1,0x4C9),(2,0xB2E),(3,0xC07),(4,0xC71),(5,0xD4A),(6,0xD72),(7,0xD77),(8,0x1436),
               (9,0x15A7),(0xA,0x15BB),(0xC,0x161D),(0xD,0x1635),(0xE,0x1642),(0xF,0x166B),(0x10,0x16CC)], 0x173F),
    "U2": _NS([(1,0x4C9),(2,0xB2E),(3,0xC07),(4,0xC71),(5,0xD4A),(6,0xD72),(7,0xD77),(8,0x1437),
               (9,0x15A8),(0xA,0x15BC),(0xC,0x161E),(0xD,0x1636),(0xE,0x1643),(0xF,0x166C),(0x10,0x16CD)], 0x1740),
    "U6": _NS([(1,0x4C9),(2,0xB2E),(3,0xC07),(4,0xC71),(5,0xD4A),(6,0xD72),(7,0xD77),(8,0x1437),
               (9,0x15A8),(0xA,0x15BF),(0xC,0x1621),(0xD,0x1639),(0xE,0x1646),(0xF,0x166F),(0x10,0x16D1)], 0x1744),
}

# Tag layouts (MCC Reach). Offsets from Assembly's ReachMCC plugins and Reclaimer.
SCNR_MAP_VARIANT_PALETTES = 0x228   # tag block, element 0x14
PALETTE_SIZE, ENTRY_SIZE, VARIANT_SIZE = 0x14, 0x1C, 0x18
OBJE_MODEL_REF = 100                # object -> model (hlmt) tag reference
HLMT_RENDER_MODEL_REF = 0           # model -> render_model (mode)
ZONE_RESOURCE_ENTRIES, ZONE_FIXUP_DATA_SIZE, ZONE_FIXUP_DATA_PTR = 100, 328, 340
PLAY_SHARED_CACHES, PLAY_PAGES, PLAY_SEGMENTS = 12, 24, 60

# vertex formats: offsets of position / texcoord / normal and their encodings
VERTEX_FORMATS = {
    0x00: ("world",     ("f4", 0x00), ("f2", 0x10), ("s2n", 0x14)),
    0x01: ("rigid",     ("f4", 0x00), ("u2n", 0x10), ("s2n", 0x14)),
    0x02: ("skinned",   ("f4", 0x00), ("u2n", 0x10), ("s2n", 0x14)),
    0x0F: ("decorator", ("f4", 0x00), ("f4", 0x0C), ("f4", 0x14)),
}

# ---------------------------------------------------------------------------------------------
# Cache file
# ---------------------------------------------------------------------------------------------

class Tag:
    __slots__ = ("index", "group", "meta", "path", "ptr")
    def __init__(self, index, group, meta, path, ptr):
        self.index, self.group, self.meta, self.path, self.ptr = index, group, meta, path, ptr
    def __repr__(self): return f"[{self.group}] {self.path}"

class CacheFile:
    """A Halo Reach MCC cache (.map) file opened for reading."""

    def __init__(self, path, log=_log_default, header_only_profile=None):
        self.path = os.path.abspath(path)
        self.log = log
        self.f = open(self.path, "rb")
        self.size = os.path.getsize(self.path)
        try:
            if header_only_profile:  # resource-only files (shared.map): just need section offsets
                p = HEADER_PROFILES[header_only_profile]
                self.section_offsets = [self.u32(p["section_offsets"] + 4 * i) for i in range(4)]
            else:
                self._open()
        except Exception:
            self.f.close()
            raise

    def close(self):
        try: self.f.close()
        except Exception: pass
    def __enter__(self): return self
    def __exit__(self, *a): self.close()

    # -- raw reads ---------------------------------------------------------------------------
    def read(self, offset, size):
        if offset < 0 or offset + size > self.size:
            raise MapReadError(f"Read outside file (0x{offset:X}, {size} bytes)")
        self.f.seek(offset)
        return self.f.read(size)
    def u8(self, o):  return self.read(o, 1)[0]
    def i16(self, o): return struct.unpack_from("<h", self.read(o, 2))[0]
    def u16(self, o): return struct.unpack_from("<H", self.read(o, 2))[0]
    def i32(self, o): return struct.unpack_from("<i", self.read(o, 4))[0]
    def u32(self, o): return struct.unpack_from("<I", self.read(o, 4))[0]
    def i64(self, o): return struct.unpack_from("<q", self.read(o, 8))[0]
    def f32(self, o): return struct.unpack_from("<f", self.read(o, 4))[0]
    def cstr(self, o, maxlen=256):
        b = self.read(o, min(maxlen, self.size - o))
        return b.split(b"\0", 1)[0].decode("latin-1")

    # -- header -------------------------------------------------------------------------------
    def _open(self):
        hdr = self.read(0, 0x800)
        magic, version = struct.unpack_from("<ii", hdr, 0)
        if magic != HEAD_MAGIC:
            raise MapReadError("Not a little-endian Halo cache file (bad header magic)")

        build_at = 288
        if version == 13 and struct.unpack_from("<i", hdr, 64)[0] != 0:
            build_at = 160
        self.build = hdr[build_at:build_at + 32].split(b"\0", 1)[0].decode("latin-1")
        if not re.match(r"^[A-Z][a-z]{2} [ \d]\d \d{4} \d\d:\d\d:\d\d$", self.build):
            raise MapReadError(f"Unrecognised build string '{self.build}' - is this a Halo Reach MCC map?")

        known = KNOWN_BUILDS.get(self.build)
        if build_at == 160:
            profiles = ["U10", "U8"]
            strings = ["U13", "U8"]
        else:
            profiles = ["U3", "U0"]
            strings = ["U6", "U2", "U1", "U0"]
        if known:
            profiles.remove(known[0]); profiles.insert(0, known[0])
            strings.remove(known[1]); strings.insert(0, known[1])

        self.expand_magic = 0x10000000 if self.build in FLIGHT_BUILDS else 0x50000000

        errors = []
        for profile in profiles:
            for string_cfg in strings:
                try:
                    self._load(profile, string_cfg)
                    self.profile, self.string_cfg = profile, string_cfg
                    note = "" if known else " (unknown build, layout detected automatically)"
                    self.log(f"Opened '{os.path.basename(self.path)}' build '{self.build}', layout {profile}/{string_cfg}{note}")
                    return
                except MapReadError as e:
                    errors.append(f"{profile}/{string_cfg}: {e}")
        raise MapReadError("Could not read map with any known layout:\n  " + "\n  ".join(errors))

    def _load(self, profile, string_cfg):
        p = HEADER_PROFILES[profile]
        self.hp = p
        self.section_offsets = [self.u32(p["section_offsets"] + 4 * i) for i in range(4)]
        self.sections = [(self.u32(p["section_table"] + 8 * i), self.u32(p["section_table"] + 8 * i + 4)) for i in range(4)]
        virtual_base = self.i64(p["virtual_base"])
        self.meta_magic = virtual_base - ((self.sections[2][0] + self.section_offsets[2]) & 0xFFFFFFFF)
        self.scenario_name = self.cstr(p["scenario_name"], 256)

        # tag index
        idx = self.meta64(self.i64(p["index_ptr"]))
        class_count, = struct.unpack_from("<i", self.read(idx, 4))
        class_ptr = self.meta64(self.i64(idx + 8))
        tag_count = self.i32(idx + 16)
        tag_ptr = self.meta64(self.i64(idx + 24))
        if not (0 < class_count < 512 and 0 < tag_count < 200000):
            raise MapReadError(f"implausible tag index ({class_count} groups, {tag_count} tags)")

        raw = self.read(class_ptr, 16 * class_count)
        self.group_codes, group_name_ids = [], []
        for i in range(class_count):
            code = raw[16 * i:16 * i + 4][::-1].decode("latin-1")
            self.group_codes.append(code)
            group_name_ids.append(struct.unpack_from("<i", raw, 16 * i + 12)[0])

        # tag names
        file_count = self.i32(p["file_count"])
        name_idx = self.header_addr(self.u32(p["file_index_ptr"]))
        name_tbl = self.header_addr(self.u32(p["file_table_ptr"]))
        if file_count != tag_count:
            raise MapReadError(f"tag name count {file_count} != tag count {tag_count}")
        offsets = struct.unpack_from(f"<{tag_count}i", self.read(name_idx, 4 * tag_count))
        blob_end = max(o for o in offsets) + 512 if offsets else 0
        blob = self.read(name_tbl, min(blob_end, self.size - name_tbl))

        raw = self.read(tag_ptr, 8 * tag_count)
        self.tags = {}
        self.tags_by_group = {}
        for i in range(tag_count):
            gi, _, ptr = struct.unpack_from("<hhI", raw, 8 * i)
            if gi < 0 or gi >= class_count: continue
            o = offsets[i]
            name = blob[o:blob.index(b"\0", o)].decode("latin-1") if o >= 0 else None
            group = self.group_codes[gi]
            tag = Tag(i, group, self.meta(ptr) if ptr else None, name, ptr)
            self.tags[i] = tag
            self.tags_by_group.setdefault(group, []).append(tag)

        scenarios = [t for t in self.tags_by_group.get("scnr", []) if t.path and t.path.lower() == self.scenario_name.lower()]
        if len(scenarios) != 1:
            raise MapReadError(f"scenario tag '{self.scenario_name}' not found")
        self.scenario = scenarios[0]

        # string ids
        self._load_strings(p, string_cfg)
        names = {code: self.string(sid) for code, sid in zip(self.group_codes, group_name_ids)}
        checks = {"scnr": "scenario", "mode": "render_model", "hlmt": "model", "weap": "weapon"}
        good = sum(1 for k, v in checks.items() if names.get(k) == v)
        present = sum(1 for k in checks if k in names)
        if present == 0 or good < present:
            raise MapReadError(f"string ids don't decode correctly (got scnr='{names.get('scnr')}')")

    # -- address translation --------------------------------------------------------------------
    def meta(self, ptr32):
        """Tag-data pointer (32-bit, contracted) -> file offset."""
        return ((ptr32 & 0xFFFFFFFF) << 2) + self.expand_magic - self.meta_magic
    def meta64(self, ptr64):
        return ptr64 - self.meta_magic
    def header_addr(self, ptr32):
        return ((ptr32 & 0xFFFFFFFF) + self.section_offsets[0]) & 0xFFFFFFFF

    # -- string ids -----------------------------------------------------------------------------
    def _load_strings(self, p, cfg):
        count = self.i32(p["string_count"])
        if not (0 < count < 500000):
            raise MapReadError(f"implausible string count {count}")
        idx = self.header_addr(self.u32(p["string_index_ptr"]))
        tbl = self.header_addr(self.u32(p["string_table_ptr"]))
        offsets = struct.unpack_from(f"<{count}i", self.read(idx, 4 * count))
        end = max(offsets) + 1024
        blob = self.read(tbl, min(end, self.size - tbl))
        strings = []
        for o in offsets:
            if o < 0: strings.append(None); continue
            z = blob.find(b"\0", o)
            strings.append(blob[o:z].decode("latin-1"))
        self.strings = strings

        if cfg in STRING_TABLES:
            self.sid_index_bits = 17
            self.sid_namespaces = {ns: (mn, st) for ns, mn, st in STRING_TABLES[cfg]}
        else:  # header based (U8+): indexBits 17 for U8, 19 for U13
            self.sid_index_bits = 19 if cfg == "U13" else 17
            mask = (1 << self.sid_index_bits) - 1
            ns_count = self.i32(p["ns_count"])
            self.sid_namespaces = {}
            if ns_count > 1:
                ns_tbl = self.header_addr(self.u32(p["ns_ptr"]))
                arr = struct.unpack_from(f"<{ns_count}i", self.read(ns_tbl, 4 * ns_count))
                start = arr[0] & mask
                for i in range(1, ns_count):
                    self.sid_namespaces[i] = (0, start)
                    start += arr[i] & mask
                self.sid_namespaces[0] = (arr[0] & mask, start)

    def string(self, sid):
        if sid == 0 or sid == -1: return ""
        sid &= 0xFFFFFFFF
        bits = self.sid_index_bits
        index = sid & ((1 << bits) - 1)
        ns = (sid >> bits) & 0xFF
        if self.sid_namespaces:
            while ns not in self.sid_namespaces and ns > 0: ns -= 1
            mn, st = self.sid_namespaces.get(ns, (0, 0))
            index = index if index < mn else index - mn + st
        if 0 <= index < len(self.strings):
            return self.strings[index] or ""
        return f"0x{sid:08X}"

    # -- tag helpers ----------------------------------------------------------------------------
    def block(self, offset):
        """Read a tag block (count, pointer) at file offset -> (count, file offset of first element)."""
        count, ptr = struct.unpack_from("<iI", self.read(offset, 8))
        if count <= 0 or ptr == 0: return 0, 0
        addr = self.meta(ptr)
        if addr < 0 or addr >= self.size: return 0, 0
        return count, addr

    def tag_ref(self, offset):
        """Read a tag reference at file offset -> Tag or None."""
        group_raw = self.read(offset, 4)
        datum = self.u32(offset + 12)
        if datum == 0xFFFFFFFF: return None
        return self.tags.get(datum & 0xFFFF)

    # -- palette --------------------------------------------------------------------------------
    def read_palette(self):
        """Returns list of dicts, one per forge object type id, in palette order."""
        base = self.scenario.meta
        def looks_right(off):
            try:
                n, a = self.block(base + off)
                return 0 < n < 64 and self.string(self.i32(a)).startswith("ff_")
            except MapReadError:
                return False
        offset = SCNR_MAP_VARIANT_PALETTES
        if not looks_right(offset):  # older builds lay the scenario out slightly differently: search nearby
            found = [o for o in range(SCNR_MAP_VARIANT_PALETTES - 0x80, SCNR_MAP_VARIANT_PALETTES + 0x84, 4) if looks_right(o)]
            if found:
                offset = min(found, key=lambda o: abs(o - SCNR_MAP_VARIANT_PALETTES))
                self.log(f"Map Variant Palettes found at scenario+0x{offset:X} (expected 0x{SCNR_MAP_VARIANT_PALETTES:X})")
        cat_count, cat_addr = self.block(base + offset)
        if cat_count == 0:
            raise MapReadError("Scenario has no Map Variant Palettes (not a forge-capable map?)")
        result = []
        entry_index = 0
        for c in range(cat_count):
            co = cat_addr + c * PALETTE_SIZE
            cat_name = self.string(self.i32(co))
            hidden = bool(self.u8(co + 4) & 1)
            n_entries, e_addr = self.block(co + 8)
            for e in range(n_entries):
                eo = e_addr + e * ENTRY_SIZE
                entry_name = self.string(self.i32(eo))
                n_var, v_addr = self.block(eo + 4)
                for v in range(n_var):
                    vo = v_addr + v * VARIANT_SIZE
                    display = self.string(self.i32(vo))
                    tag = self.tag_ref(vo + 4)
                    variant_name = self.string(self.i32(vo + 0x14))
                    result.append(dict(
                        id=(entry_index << 8) | v,
                        category=cat_name, category_hidden=hidden,
                        entry=entry_name, display=display, variant=variant_name,
                        name=display if display else entry_name,
                        tag_group=tag.group if tag else None,
                        tag_path=tag.path if tag else None,
                        tag_index=tag.index if tag else -1,
                    ))
                entry_index += 1
        if result and not result[0]["category"].startswith("ff_"):
            self.log(f"Warning: first palette category is '{result[0]['category']}', expected ff_... - palette offsets may be wrong for this build")
        return result

    # -- models ---------------------------------------------------------------------------------
    def render_model_for_object(self, tag_index):
        obj = self.tags.get(tag_index)
        if obj is None or obj.meta is None: return None
        hlmt = self.tag_ref(obj.meta + OBJE_MODEL_REF)
        if hlmt is None or hlmt.group != "hlmt" or hlmt.meta is None: return None
        mode = self.tag_ref(hlmt.meta + HLMT_RENDER_MODEL_REF)
        if mode is None or mode.group != "mode" or mode.meta is None: return None
        return mode

    def render_model_bounds(self, mode):
        """Compression bounding box of a render model: ((xmin,xmax),(ymin,ymax),(zmin,zmax))."""
        n, addr = self.block(mode.meta + 116)
        if n == 0: return None
        v = struct.unpack_from("<10f", self.read(addr + 4, 40))
        return (v[0], v[1]), (v[2], v[3]), (v[4], v[5]), (v[6], v[7]), (v[8], v[9])

    def read_render_model(self, mode, permutations=None):
        """Decode a render model. permutations: optional {region name: permutation name}.
        Returns dict(name, meshes=[dict(positions, normals, uvs, triangles, materials, tri_materials)], bounds)."""
        return RenderModelReader(self, mode).read(permutations)

    # -- resources ------------------------------------------------------------------------------
    def _resource_tables(self):
        if hasattr(self, "_res"): return self._res
        zone = self.tags_by_group.get("zone", [None])[0]
        play = self.tags_by_group.get("play", [None])[0]
        if zone is None: raise MapReadError("No resource gestalt (zone) tag")
        play_meta = play.meta if (play is not None and play.ptr) else zone.meta + 28

        entries = self.block(zone.meta + ZONE_RESOURCE_ENTRIES)
        fixup_size = self.i32(zone.meta + ZONE_FIXUP_DATA_SIZE)
        fixup_ptr = self.u32(zone.meta + ZONE_FIXUP_DATA_PTR)
        fixup_addr = self.meta(fixup_ptr)

        n_sc, sc_addr = self.block(play_meta + PLAY_SHARED_CACHES)
        shared = [self.cstr(sc_addr + 264 * i, 32) for i in range(n_sc)]
        n_pg, pg_addr = self.block(play_meta + PLAY_PAGES)
        pages = []
        for i in range(n_pg):
            o = pg_addr + 88 * i
            cache_index = self.i16(o + 4)
            data_off, comp, decomp = struct.unpack_from("<iii", self.read(o + 8, 12))
            pages.append((cache_index, data_off, comp, decomp))
        n_sg, sg_addr = self.block(play_meta + PLAY_SEGMENTS)
        segments = []
        for i in range(n_sg):
            segments.append(struct.unpack_from("<hhii", self.read(sg_addr + 16 * i, 12)))
        self._res = dict(entries=entries, fixup_addr=fixup_addr, fixup_size=fixup_size,
                         shared=shared, pages=pages, segments=segments)
        return self._res

    def resource_entry(self, resource_index):
        r = self._resource_tables()
        count, addr = r["entries"]
        if not (0 <= resource_index < count): raise MapReadError(f"resource index {resource_index} out of range")
        o = addr + 64 * resource_index
        fixup_offset, fixup_size = struct.unpack_from("<ii", self.read(o + 20, 8))
        segment_index = self.i16(o + 34)
        n_fix, fix_addr = self.block(o + 40)
        fixups = struct.unpack_from(f"<{2 * n_fix}i", self.read(fix_addr, 8 * n_fix)) if n_fix else ()
        fixups = [fixups[2 * i + 1] & 0x0FFFFFFF for i in range(n_fix)]
        return dict(fixup_offset=fixup_offset, fixup_size=fixup_size, segment=segment_index, fixups=fixups)

    def read_resource(self, resource_index):
        r = self._resource_tables()
        entry = self.resource_entry(resource_index)
        if entry["segment"] < 0: raise MapReadError("resource has no data segment")
        prim_page, sec_page, prim_off, sec_off = r["segments"][entry["segment"]]

        page_index, seg_off = (sec_page, sec_off) if sec_page >= 0 else (prim_page, prim_off)
        if page_index < 0 or seg_off < 0: raise MapReadError("resource page missing")
        page = r["pages"][page_index]
        if page[1] < 0 or page[2] == 0:
            page_index, seg_off = prim_page, prim_off
            page = r["pages"][page_index]
        cache_index, data_off, comp, decomp = page

        target = self
        if cache_index >= 0:
            target = self.shared_cache(r["shared"][cache_index])
        data_table = target.section_offsets[1]
        raw = target.read(data_table + data_off, comp)
        if comp != decomp:
            try:
                raw = zlib.decompressobj(-15).decompress(raw)
            except zlib.error:
                raw = zlib.decompress(raw)
        return raw[seg_off:], entry

    def shared_cache(self, name):
        if not hasattr(self, "_shared"): self._shared = {}
        base = os.path.basename(name.replace("\\", "/"))
        if not base.lower().endswith(".map"): base += ".map"
        if base in self._shared: return self._shared[base]
        dirs = [os.path.dirname(self.path)] + list(getattr(self, "shared_search_dirs", []))
        for d in dirs:
            p = os.path.join(d, base)
            if os.path.isfile(p):
                self.log(f"Using shared resource file {p}")
                c = CacheFile(p, log=self.log, header_only_profile=self.profile)
                self._shared[base] = c
                return c
        raise MapReadError(f"Shared resource map '{base}' not found next to the map or in the stock maps folder")

    def close_all(self):
        for c in getattr(self, "_shared", {}).values(): c.close()
        self.close()

# ---------------------------------------------------------------------------------------------
# Render model decoding
# ---------------------------------------------------------------------------------------------

class RenderModelReader:
    def __init__(self, cache, tag):
        self.c, self.tag = cache, tag
        self.m = tag.meta

    def read(self, permutations=None):
        c, m = self.c, self.m
        name = c.string(c.i32(m))
        bounds = c.render_model_bounds(self.tag)
        if bounds is None: raise MapReadError("render model has no bounding box")
        (xb, yb, zb, ub, vb) = bounds

        sections = []
        n, addr = c.block(m + 104)
        for i in range(n):
            o = addr + 92 * i
            nsub, sub_addr = c.block(o)
            nset, set_addr = c.block(o + 12)
            submeshes = [struct.unpack_from("<hxxii", c.read(sub_addr + 24 * s, 12)) for s in range(nsub)]
            subsets = [struct.unpack_from("<iiH", c.read(set_addr + 16 * s, 10)) for s in range(nset)]
            sections.append(dict(
                submeshes=submeshes, subsets=subsets,
                vb=c.i16(o + 24), ib=c.i16(o + 40), flags=c.u16(o + 44),
                node=c.u8(o + 46), vformat=c.u8(o + 47), iformat=c.u8(o + 50)))

        shaders, shader_tags = [], {}
        n, addr = c.block(m + 72)
        for i in range(n):
            t = c.tag_ref(addr + 44 * i)
            shaders.append(t.path if t else None)
            if t is not None: shader_tags[t.path] = t.index

        # regions / permutations: take the first permutation of each region so damage states
        # and alternates don't overlap. Instanced geometry is selected the same way.
        n, addr = c.block(m + 12)
        wanted_sections, wanted_instances = set(), set()
        for r in range(n):
            ro = addr + 16 * r
            np_, p_addr = c.block(ro + 4)
            if np_ == 0: continue
            pick = 0
            perm_names = [c.string(c.i32(p_addr + 24 * pi)) for pi in range(np_)]
            wanted = (permutations or {}).get(c.string(c.i32(ro)))
            if wanted in perm_names:
                pick = perm_names.index(wanted)
            else:
                for pi, pn in enumerate(perm_names):
                    if pn in ("default", "base"):
                        pick = pi; break
            po = p_addr + 24 * pick
            sec_index, sec_count = struct.unpack_from("<hh", c.read(po + 4, 4))
            for s in range(sec_index, sec_index + max(sec_count, 0)):
                wanted_sections.add(s)
            masks = struct.unpack_from("<4I", c.read(po + 8, 16))
            for k in range(128):
                if masks[k // 32] & (1 << (k % 32)): wanted_instances.add(k)
        if not wanted_sections and sections:
            wanted_sections = set(range(len(sections)))

        instanced_section = c.i32(m + 28)
        instances = []
        n, addr = c.block(m + 32)
        for i in range(n):
            o = addr + 60 * i
            scale = c.f32(o + 8)
            mat = struct.unpack_from("<12f", c.read(o + 12, 48))
            instances.append((scale, mat))

        resource_id = c.i32(m + 248)
        data, entry = c.read_resource(resource_id & 0xFFFF)
        vb_info, ib_info = self._buffer_info(entry)
        fix = entry["fixups"]

        meshes = []
        decoded_vb = {}
        for si, sec in enumerate(sections):
            is_instanced = (si == instanced_section)
            if not is_instanced and si not in wanted_sections: continue
            if sec["vb"] < 0 or sec["vb"] >= len(vb_info): continue
            vcount, vlen = vb_info[sec["vb"]]
            if vcount == 0: continue
            if sec["vb"] not in decoded_vb:
                off = fix[sec["vb"]]
                decoded_vb[sec["vb"]] = decode_vertices(data[off:off + vlen], vcount, sec["vformat"], xb, yb, zb, ub, vb)
            pos, nrm, uv = decoded_vb[sec["vb"]]

            unindexed = sec["ib"] < 0 or (sec["flags"] & (1 << 4))
            if unindexed:
                indices = list(range(vcount))
                layout = 3 if sec["iformat"] == 3 else 5
            else:
                if sec["ib"] >= len(ib_info): continue
                ifmt, ilen = ib_info[sec["ib"]]
                off = fix[len(vb_info) * 2 + sec["ib"]]
                raw = data[off:off + ilen]
                indices = None
                layout = ifmt

            if is_instanced:
                for k, subset in enumerate(sec["subsets"]):
                    if k >= len(instances) or k not in wanted_instances: continue
                    istart, ilen, sm = subset
                    shader = sec["submeshes"][sm][0] if sm < len(sec["submeshes"]) else -1
                    if indices is None:
                        tris, _ = decode_triangles(raw, vcount, [(shader, istart, ilen)], layout)
                    else:
                        tris = indices_to_triangles(indices[istart:istart + ilen], layout)
                    mesh = build_mesh(pos, nrm, uv, tris, [shader] * len(tris), shaders)
                    mesh = transform_mesh(mesh, *instances[k])
                    meshes.append(mesh)
            else:
                if indices is None:
                    tris, tri_shader = decode_triangles(raw, vcount, sec["submeshes"], layout)
                else:
                    tris, tri_shader = [], []
                    for shader, istart, ilen in sec["submeshes"]:
                        t = indices_to_triangles(indices[istart:istart + ilen], layout)
                        tris.extend(t); tri_shader.extend([shader] * len(t))
                meshes.append(build_mesh(pos, nrm, uv, tris, tri_shader, shaders))

        return dict(name=name, path=self.tag.path, meshes=[mm for mm in meshes if mm["triangles"]], shader_tags=shader_tags,
                    bounds=(xb, yb, zb))

    def _buffer_info(self, entry):
        c = self.c
        r = c._resource_tables()
        base = r["fixup_addr"] + entry["fixup_offset"]
        tail = base + entry["fixup_size"] - 24
        vb_count = c.i32(tail)
        ib_count = c.i32(tail + 12)
        if not (0 <= vb_count < 1000000 and 0 <= ib_count < 1000000) or base + 28 * (vb_count + ib_count) > tail + 24:
            raise MapReadError(f"bad geometry buffer counts ({vb_count}, {ib_count})")
        vb = [struct.unpack_from("<ixxxxi", c.read(base + 28 * i, 12)) for i in range(vb_count)]
        o = base + 28 * vb_count + 12 * vb_count
        ib = [struct.unpack_from("<ixxxxi", c.read(o + 28 * i, 12)) for i in range(ib_count)]
        return vb, ib

def _unorm(v, b): return b[0] + v * (b[1] - b[0])

def decode_vertices(data, count, vformat, xb, yb, zb, ub, vb):
    stride = len(data) // count if count else 0
    fmt = VERTEX_FORMATS.get(vformat)
    if stride < 12:
        raise MapReadError(f"vertex stride {stride} too small")
    pos_off, uv_spec, nrm_spec = (0, None, None) if fmt is None else (fmt[1][1], fmt[2], fmt[3])

    if np is not None:
        buf = np.frombuffer(data[:stride * count], dtype=np.uint8).reshape(count, stride)
        def field(off, dtype, n):
            size = np.dtype(dtype).itemsize * n
            if off + size > stride: return None
            return np.ascontiguousarray(buf[:, off:off + size]).view(dtype).reshape(count, n).astype(np.float64)
        p = field(pos_off, "<f4", 3)
        p[:, 0] = xb[0] + p[:, 0] * (xb[1] - xb[0])
        p[:, 1] = yb[0] + p[:, 1] * (yb[1] - yb[0])
        p[:, 2] = zb[0] + p[:, 2] * (zb[1] - zb[0])
        uv = nrm = None
        if uv_spec:
            kind, off = uv_spec
            if kind == "f2": uv = field(off, "<f2", 2)
            elif kind == "u2n":
                uv = field(off, "<u2", 2)
                if uv is not None: uv /= 65535.0
            else: uv = field(off, "<f4", 2)
            if uv is not None:
                uv[:, 0] = ub[0] + uv[:, 0] * (ub[1] - ub[0])
                uv[:, 1] = vb[0] + uv[:, 1] * (vb[1] - vb[0])
        if nrm_spec:
            kind, off = nrm_spec
            if kind == "s2n":
                nrm = field(off, "<i2", 3)
                if nrm is not None: nrm = np.clip(nrm / 32767.0, -1, 1)
            else: nrm = field(off, "<f4", 3)
        return p.tolist(), (nrm.tolist() if nrm is not None else None), (uv.tolist() if uv is not None else None)

    pos, nrm, uv = [], ([] if nrm_spec else None), ([] if uv_spec else None)
    for i in range(count):
        o = i * stride
        x, y, z = struct.unpack_from("<3f", data, o + pos_off)
        pos.append((_unorm(x, xb), _unorm(y, yb), _unorm(z, zb)))
        if uv_spec:
            kind, off = uv_spec
            if kind == "f2": u, v = struct.unpack_from("<2e", data, o + off)
            elif kind == "u2n":
                u, v = struct.unpack_from("<2H", data, o + off); u /= 65535.0; v /= 65535.0
            else: u, v = struct.unpack_from("<2f", data, o + off)
            uv.append((_unorm(u, ub), _unorm(v, vb)))
        if nrm_spec:
            kind, off = nrm_spec
            if kind == "s2n":
                a, b, cc = struct.unpack_from("<3h", data, o + off)
                nrm.append((max(-1, a / 32767.0), max(-1, b / 32767.0), max(-1, cc / 32767.0)))
            else: nrm.append(struct.unpack_from("<3f", data, o + off))
    return pos, nrm, uv

def indices_to_triangles(idx, layout):
    """layout 3 = triangle list, 0 or 5 = triangle strip (Reclaimer treats Default as strip)."""
    tris = []
    if layout == 3:
        for i in range(0, len(idx) - 2, 3):
            a, b, c = idx[i], idx[i + 1], idx[i + 2]
            if a != b and b != c and a != c: tris.append((a, b, c))
        return tris
    i0 = i1 = 0
    for n, i2 in enumerate(idx):
        if n >= 2 and i0 != i1 and i0 != i2 and i1 != i2:
            tris.append((i0, i1, i2) if n % 2 == 0 else (i0, i2, i1))
        i0, i1 = i1, i2
    return tris

def decode_triangles(raw, vcount, submeshes, layout, info=None):
    """Turn an index buffer into triangles, checking the reading against the vertex count.
    The expected reading (16-bit unless the buffer has >65535 vertices, declared primitive type) is
    used when it looks sane; otherwise the other index width / primitive type is tried and the most
    consistent result kept. submeshes: [(shader index, index start, index count)].
    Returns (triangles, triangle shader indices)."""
    expected_w = 4 if vcount > 0xFFFF else 2

    def attempt(w, lay):
        if not raw or len(raw) % w: return None
        idx = struct.unpack_from(f"<{len(raw) // w}{'I' if w == 4 else 'H'}", raw)
        tris, shaders_, bad, possible = [], [], 0, 0
        for shader, start, count in submeshes:
            part = idx[start:start + count]
            possible += len(part) // 3 if lay == 3 else max(len(part) - 2, 0)
            for t in indices_to_triangles(part, lay):
                if t[0] < vcount and t[1] < vcount and t[2] < vcount:
                    tris.append(t); shaders_.append(shader)
                else:
                    bad += 1
        ratio = len(tris) / possible if possible else 0.0
        return dict(tris=tris, shaders=shaders_, bad=bad, ratio=ratio, width=w, layout=lay,
                    indices=len(idx), max_index=max(idx) if idx else 0)

    def sane(r):  # strips lose some triangles to restart joins; lists should lose almost none
        return r is not None and r["tris"] and r["bad"] == 0 and r["ratio"] >= (0.8 if r["layout"] == 3 else 0.4)

    best = attempt(expected_w, layout)
    if not sane(best):
        def score(r): return -1e18 if r is None else len(r["tris"]) * (0.5 + r["ratio"]) - 4 * r["bad"]
        other_w = 2 if expected_w == 4 else 4
        cands = [best, attempt(other_w, layout)]
        declared = max(cands, key=score)
        if not sane(declared):  # declared primitive type looks wrong too: try the other one
            alt = 5 if layout == 3 else 3
            cands += [attempt(expected_w, alt), attempt(other_w, alt)]
        best = max(cands, key=score)
    if best is None: return [], []
    if info is not None:
        info.update(width=best["width"], layout=best["layout"], dropped=best["bad"], indices=best["indices"],
                    max_index=best["max_index"], kept_ratio=round(best["ratio"], 2))
    return best["tris"], best["shaders"]

def build_mesh(pos, nrm, uv, tris, tri_shader, shaders):
    """Compact a mesh down to the vertices its triangles use."""
    remap, out_pos, out_nrm, out_uv, out_tris = {}, [], [], [], []
    for t in tris:
        nt = []
        for v in t:
            if v >= len(pos): break
            j = remap.get(v)
            if j is None:
                j = remap[v] = len(out_pos)
                out_pos.append(pos[v])
                if nrm is not None: out_nrm.append(nrm[v])
                if uv is not None: out_uv.append(uv[v])
            nt.append(j)
        else:
            out_tris.append(tuple(nt))
            continue
    mats = []
    mat_lookup = {}
    tri_mat = []
    for s in tri_shader[:len(out_tris)]:
        name = shaders[s] if 0 <= s < len(shaders) and shaders[s] else "default"
        if name not in mat_lookup:
            mat_lookup[name] = len(mats); mats.append(name)
        tri_mat.append(mat_lookup[name])
    return dict(positions=out_pos, normals=out_nrm if nrm is not None else None,
                uvs=out_uv if uv is not None else None, triangles=out_tris,
                materials=mats, tri_materials=tri_mat)

def transform_mesh(mesh, scale, m):
    # 4x3 row-vector matrix: rows 0-2 rotation, row 3 translation
    def tf(p, translate=True):
        x, y, z = p[0] * scale, p[1] * scale, p[2] * scale
        r = (x * m[0] + y * m[3] + z * m[6], x * m[1] + y * m[4] + z * m[7], x * m[2] + y * m[5] + z * m[8])
        return (r[0] + m[9], r[1] + m[10], r[2] + m[11]) if translate else r
    mesh = dict(mesh)
    mesh["positions"] = [tf(p) for p in mesh["positions"]]
    if mesh["normals"]:
        mesh["normals"] = [tf((n[0] / scale, n[1] / scale, n[2] / scale) if scale else n, False) for n in mesh["normals"]]
    return mesh

def write_png(pixels, path):
    """Write an RGBA uint8 (H,W,4) array as PNG with the standard library only."""
    h, w = pixels.shape[:2]
    raw = b"".join(b"\0" + pixels[y].tobytes() for y in range(h))
    def chunk(t, d): return struct.pack(">I", len(d)) + t + d + struct.pack(">I", zlib.crc32(t + d) & 0xFFFFFFFF)
    with open(path, "wb") as f:
        f.write(b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, 6, 0, 0, 0))
                + chunk(b"IDAT", zlib.compress(raw, 6)) + chunk(b"IEND", b""))

def write_obj(model, path):
    """OBJ in the usual Y-up convention (Halo/Blender Z-up is converted), so default import settings work."""
    with open(path, "w") as f:
        f.write(f"# {model['path']}\n")
        base = 1
        for mi, mesh in enumerate(model["meshes"]):
            f.write(f"o mesh{mi}\n")
            for x, y, z in mesh["positions"]: f.write("v %.6f %.6f %.6f\n" % (x, z, -y))
            for t in mesh["triangles"]:
                f.write("f %d %d %d\n" % (t[0] + base, t[1] + base, t[2] + base))
            base += len(mesh["positions"])

# ---------------------------------------------------------------------------------------------
# Whole objects: model variants, attached objects (turrets, guns) and vehicle weapons
# ---------------------------------------------------------------------------------------------
OBJE_DEFAULT_VARIANT = 96
HLMT_VARIANTS, HLMT_VARIANT_SIZE = 0x84, 0x38
VEHI_WEAPONS, VEHI_RIGHT_HAND_NODE, VEHI_PREFERRED_GUN_NODE = 0x42C, 0x340, 0x348
OBJECT_GROUPS = {"weap", "vehi", "scen", "bloc", "crat", "mach", "ctrl", "eqip", "bipd", "gint", "term", "ssce", "efsc", "proj"}
MAX_ATTACH_DEPTH = 3

def _quat_matrix(x, y, z, w):
    n = math.sqrt(x * x + y * y + z * z + w * w) or 1.0
    x, y, z, w = x / n, y / n, z / n, w / n
    return [[1 - 2 * (y * y + z * z), 2 * (x * y - z * w), 2 * (x * z + y * w), 0],
            [2 * (x * y + z * w), 1 - 2 * (x * x + z * z), 2 * (y * z - x * w), 0],
            [2 * (x * z - y * w), 2 * (y * z + x * w), 1 - 2 * (x * x + y * y), 0],
            [0, 0, 0, 1]]

def _mat_mul(a, b):
    return [[sum(a[i][k] * b[k][j] for k in range(4)) for j in range(4)] for i in range(4)]

def _mat_translate(x, y, z):
    return [[1, 0, 0, x], [0, 1, 0, y], [0, 0, 1, z], [0, 0, 0, 1]]

def _mat_identity():
    return [[1.0 if i == j else 0.0 for j in range(4)] for i in range(4)]

def _mat_invert_affine(m):
    """Inverse of an affine 4x4 (general 3x3 part)."""
    a = [row[:3] for row in m[:3]]
    det = (a[0][0] * (a[1][1] * a[2][2] - a[1][2] * a[2][1]) - a[0][1] * (a[1][0] * a[2][2] - a[1][2] * a[2][0])
           + a[0][2] * (a[1][0] * a[2][1] - a[1][1] * a[2][0]))
    if abs(det) < 1e-12: return None
    inv = [[(a[(j + 1) % 3][(i + 1) % 3] * a[(j + 2) % 3][(i + 2) % 3] - a[(j + 1) % 3][(i + 2) % 3] * a[(j + 2) % 3][(i + 1) % 3]) / det
            for j in range(3)] for i in range(3)]
    t = [m[0][3], m[1][3], m[2][3]]
    it = [-sum(inv[i][k] * t[k] for k in range(3)) for i in range(3)]
    return [inv[0] + [it[0]], inv[1] + [it[1]], inv[2] + [it[2]], [0, 0, 0, 1]]

def _transform_model(model, m):
    """Apply a 4x4 (column-vector) transform to every mesh of a decoded model, in place."""
    if m == _mat_identity(): return model
    r = [row[:3] for row in m[:3]]
    for mesh in model["meshes"]:
        mesh["positions"] = [(r[0][0] * x + r[0][1] * y + r[0][2] * z + m[0][3],
                              r[1][0] * x + r[1][1] * y + r[1][2] * z + m[1][3],
                              r[2][0] * x + r[2][1] * y + r[2][2] * z + m[2][3]) for x, y, z in mesh["positions"]]
        if mesh["normals"]:
            out = []
            for x, y, z in mesh["normals"]:
                nx, ny, nz = r[0][0] * x + r[0][1] * y + r[0][2] * z, r[1][0] * x + r[1][1] * y + r[1][2] * z, r[2][0] * x + r[2][1] * y + r[2][2] * z
                l = math.sqrt(nx * nx + ny * ny + nz * nz) or 1.0
                out.append((nx / l, ny / l, nz / l))
            mesh["normals"] = out
    return model

def _object_methods(cls):
    def node_transforms(self, mode):
        """World (model space) transform of each render model node, default pose."""
        n, addr = self.block(mode.meta + 48)
        nodes = []
        for i in range(n):
            o = addr + 96 * i
            name = self.string(self.i32(o))
            parent = self.i16(o + 4)
            pos = struct.unpack_from("<3f", self.read(o + 12, 12))
            rot = struct.unpack_from("<4f", self.read(o + 24, 16))
            inv = struct.unpack_from("<12f", self.read(o + 44, 48))
            nodes.append((name, parent, pos, rot, inv))
        world = []
        for i, (name, parent, pos, rot, inv) in enumerate(nodes):
            # preferred: invert the stored inverse bind transform (4x3, row-vector layout)
            m = [[inv[0], inv[3], inv[6], inv[9]], [inv[1], inv[4], inv[7], inv[10]],
                 [inv[2], inv[5], inv[8], inv[11]], [0, 0, 0, 1]]
            w = _mat_invert_affine(m) if any(abs(v) > 1e-9 for v in inv[:9]) else None
            if w is None:  # fall back to chaining the local transforms
                local = _mat_mul(_mat_translate(*pos), _quat_matrix(*rot))
                w = _mat_mul(world[parent], local) if 0 <= parent < len(world) else local
            world.append(w)
        return {nodes[i][0]: world[i] for i in range(len(nodes))}, world

    def marker_transform(self, mode, name, node_cache=None):
        """Model-space transform of a marker (or node) by name, or None."""
        if not name: return None
        by_name, world = node_cache or self.node_transforms(mode)
        n, addr = self.block(mode.meta + 60)
        for g in range(n):
            go = addr + 16 * g
            if self.string(self.i32(go)) != name: continue
            nm, m_addr = self.block(go + 4)
            if nm == 0: continue
            node = self.u8(m_addr + 2)
            pos = struct.unpack_from("<3f", self.read(m_addr + 4, 12))
            rot = struct.unpack_from("<4f", self.read(m_addr + 16, 16))
            local = _mat_mul(_mat_translate(*pos), _quat_matrix(*rot))
            return _mat_mul(world[node], local) if node < len(world) else local
        return by_name.get(name)

    def model_variant(self, hlmt, name):
        """-> (variant name, {region: permutation}, [(parent marker, child marker, child variant, child tag)])"""
        n, addr = self.block(hlmt.meta + HLMT_VARIANTS)
        if n == 0: return None, {}, []
        names = [self.string(self.i32(addr + HLMT_VARIANT_SIZE * i)) for i in range(n)]
        if name in names: i = names.index(name)
        elif "default" in names: i = names.index("default")
        else: i = 0
        vo = addr + HLMT_VARIANT_SIZE * i
        perms = {}
        nr, r_addr = self.block(vo + 0x14)
        for r in range(nr):
            ro = r_addr + 0x18 * r
            region = self.string(self.i32(ro))
            np_, p_addr = self.block(ro + 0x8)
            if np_: perms[region] = self.string(self.i32(p_addr))
        objects = []
        no, o_addr = self.block(vo + 0x20)
        for k in range(no):
            oo = o_addr + 0x20 * k
            child = self.tag_ref(oo + 0xC)
            if child is None: continue
            objects.append((self.string(self.i32(oo)), self.string(self.i32(oo + 4)), self.string(self.i32(oo + 8)), child))
        return names[i], perms, objects

    def object_parts(self, tag_index, variant="", transform=None, depth=0, log=None):
        """Every render model making up an object: its own model plus attached objects and vehicle weapons.
        Returns [dict(mode=Tag, permutations={}, transform=4x4, source=tag path)]."""
        obj = self.tags.get(tag_index)
        if obj is None or obj.meta is None: return []
        transform = transform or _mat_identity()
        hlmt = self.tag_ref(obj.meta + OBJE_MODEL_REF)
        if hlmt is None or hlmt.group != "hlmt" or hlmt.meta is None: return []
        mode = self.tag_ref(hlmt.meta + HLMT_RENDER_MODEL_REF)
        if mode is None or mode.group != "mode" or mode.meta is None: return []
        if not variant:
            variant = self.string(self.i32(obj.meta + OBJE_DEFAULT_VARIANT))
        _, perms, attached = self.model_variant(hlmt, variant)
        parts = [dict(mode=mode, permutations=perms, transform=transform, source=obj.path)]
        if depth >= MAX_ATTACH_DEPTH: return parts

        nodes = None
        def place(child_index, child_variant, parent_marker, child_marker):
            nonlocal nodes
            try:
                if nodes is None: nodes = self.node_transforms(mode)
                pm = self.marker_transform(mode, parent_marker, nodes) or _mat_identity()
                child_tf = _mat_mul(transform, pm)
                if child_marker:
                    sub = self.render_model_for_object(child_index)
                    cm = self.marker_transform(sub, child_marker) if sub else None
                    inv = _mat_invert_affine(cm) if cm else None
                    if inv: child_tf = _mat_mul(child_tf, inv)
                parts.extend(self.object_parts(child_index, child_variant, child_tf, depth + 1, log))
            except MapReadError as e:
                if log: log(f"attachment {self.tags[child_index].path} skipped: {e}")

        seen = set()
        for parent_marker, child_marker, child_variant, child in attached:
            if child.group in OBJECT_GROUPS:
                place(child.index, child_variant, parent_marker, child_marker); seen.add(child.index)

        if obj.group == "vehi":  # initial weapons, held at the vehicle's gun node
            try:
                gun = self.string(self.i32(obj.meta + VEHI_PREFERRED_GUN_NODE)) or self.string(self.i32(obj.meta + VEHI_RIGHT_HAND_NODE))
                n, addr = self.block(obj.meta + VEHI_WEAPONS)
                for i in range(n):
                    w = self.tag_ref(addr + 0x14 * i)
                    if w is None or w.index in seen or w.group != "weap": continue
                    place(w.index, self.string(self.i32(addr + 0x14 * i + 0x10)), gun, None)
            except MapReadError:
                pass
        return parts

    def read_object_model(self, tag_index, variant="", log=None):
        """Decode a whole object (with attachments) into one model in the object's space."""
        parts = self.object_parts(tag_index, variant, log=log)
        if not parts: return None
        result = dict(name=parts[0]["mode"].path, path=self.tags[tag_index].path, meshes=[], shader_tags={}, parts=[])
        for i, part in enumerate(parts):
            try:
                model = self.read_render_model(part["mode"], part["permutations"])
            except MapReadError as e:
                if i == 0: raise
                if log: log(f"attached model {part['source']} skipped: {e}")
                continue
            _transform_model(model, part["transform"])
            result["meshes"] += model["meshes"]
            result["shader_tags"].update(model["shader_tags"])
            result["parts"].append(part["source"])
            if i == 0: result["bounds"] = model["bounds"]
        return result

    cls.node_transforms = node_transforms
    cls.marker_transform = marker_transform
    cls.model_variant = model_variant
    cls.object_parts = object_parts
    cls.read_object_model = read_object_model
_object_methods(CacheFile)

# ---------------------------------------------------------------------------------------------
# Level geometry (scenario_structure_bsp + lightmap data)
# ---------------------------------------------------------------------------------------------
# offsets for update 13+ layouts, then older MCC builds (from Reclaimer's version configs)
BSP_LAYOUTS = {
    "U13": dict(scnr_bsps=80, scnr_lightmap=1800, clusters=312, shaders=324, instances=600, bboxes=1116, instances_res=1312),
    "old": dict(scnr_bsps=76, scnr_lightmap=1856, clusters=312, shaders=324, instances=612, bboxes=1140, instances_res=1336),
}
CLUSTER_SIZE, CLUSTER_SECTION = 140, 64
LBSP_SECTIONS, LBSP_RESOURCE = 124, 268
SECTION_SIZE = 92

def _bsp_methods(cls):
    def bsp_layout(self):
        return BSP_LAYOUTS["U13" if self.string_cfg == "U13" else "old"]

    def structure_bsps(self):
        """[(bsp Tag, lightmap data Tag or None)] for the scenario."""
        L = self.bsp_layout()
        n, addr = self.block(self.scenario.meta + L["scnr_bsps"])
        bsps = [self.tag_ref(addr + 0xAC * i) for i in range(n)]
        lightmaps = []
        sldt = self.tag_ref(self.scenario.meta + L["scnr_lightmap"])
        if sldt is not None and sldt.meta is not None:
            nl, l_addr = self.block(sldt.meta + 4)
            lightmaps = [self.tag_ref(l_addr + 32 * i) for i in range(nl)]
        out = []
        for i, b in enumerate(bsps):
            if b is None or b.group != "sbsp" or b.meta is None: continue
            lm = lightmaps[i] if i < len(lightmaps) else None
            out.append((b, lm if (lm is not None and lm.meta is not None) else None))
        return out

    def read_bsp(self, bsp, lightmap, log=None):
        """Decode one BSP. Returns dict(name, sections={index: mesh}, clusters=[section index],
        instances=[(name, section index, scale, 4x3 matrix)], shader_tags)."""
        if lightmap is None: raise MapReadError(f"{bsp.path}: no lightmap data (geometry lives there in Reach)")
        L = self.bsp_layout()
        m = bsp.meta

        shaders, shader_tags = [], {}
        n, addr = self.block(m + L["shaders"])
        for i in range(n):
            t = self.tag_ref(addr + 44 * i)
            shaders.append(t.path if t else None)
            if t is not None: shader_tags[t.path] = t.index

        n, addr = self.block(m + L["clusters"])
        clusters = [self.i16(addr + CLUSTER_SIZE * i + CLUSTER_SECTION) for i in range(n)]

        n, addr = self.block(m + L["bboxes"])
        bounds = []
        for i in range(n):
            v = struct.unpack_from("<10f", self.read(addr + 52 * i + 4, 40))
            bounds.append(((v[0], v[1]), (v[2], v[3]), (v[4], v[5]), (v[6], v[7]), (v[8], v[9])))

        # instanced geometry: names in the tag, transforms in the instances resource definition
        n_inst, inst_addr = self.block(m + L["instances"])
        names = [self.string(self.i32(inst_addr + 4 * i)) for i in range(n_inst)]
        instances = []
        if n_inst:
            try:
                entry = self.resource_entry(self.i32(m + L["instances_res"]) & 0xFFFF)
                if len(entry["fixups"]) < 10: raise MapReadError("unexpected instance resource layout")
                base = self._resource_tables()["fixup_addr"] + entry["fixup_offset"] + entry["fixups"][-10]
                for i in range(n_inst):
                    o = base + 156 * i
                    scale = self.f32(o)
                    mat = struct.unpack_from("<12f", self.read(o + 4, 48))
                    section = self.i16(o + 58)
                    instances.append((names[i], section, scale, mat))
            except MapReadError as e:
                if log: log(f"{bsp.path}: instanced geometry skipped ({e})")

        # sections + geometry resource come from the lightmap data tag
        lm = lightmap.meta
        n_sec, s_addr = self.block(lm + LBSP_SECTIONS)
        sections = []
        for i in range(n_sec):
            o = s_addr + SECTION_SIZE * i
            nsub, sub_addr = self.block(o)
            submeshes = [struct.unpack_from("<hxxii", self.read(sub_addr + 24 * k, 12)) for k in range(nsub)]
            sections.append(dict(submeshes=submeshes, vb=self.i16(o + 24), ib=self.i16(o + 40),
                                 flags=self.u16(o + 44), vformat=self.u8(o + 47), iformat=self.u8(o + 50)))
        res = self.i32(lm + LBSP_RESOURCE) & 0xFFFF
        if res == 0xFFFF: raise MapReadError(f"{bsp.path}: has no geometry resource (unused BSP)")
        data, entry = self.read_resource(res)
        vb_info, ib_info = RenderModelReader(self, bsp)._buffer_info(entry)
        fix = entry["fixups"]

        wanted = set(c for c in clusters if c >= 0) | set(i[1] for i in instances if i[1] >= 0)
        meshes = {}
        identity = (0.0, 1.0)
        report = dict(cluster_count=len(clusters), cluster_sections=sorted(set(clusters))[:20], instance_count=n_inst,
                      section_count=n_sec, bbox_count=len(bounds), vb_count=len(vb_info), ib_count=len(ib_info),
                      skipped={}, formats={}, empty=[], cluster_details=[], dropped=0, width_fixed=0)
        cluster_set = set(clusters)
        try:
            report["world_bounds"] = struct.unpack_from("<6f", self.read(m + 240, 24))
        except MapReadError:
            pass
        def skip(si, why):
            report["skipped"].setdefault(why, []).append(si)
        for si in sorted(wanted):
            if si >= len(sections): continue
            sec = sections[si]
            report["formats"][sec["vformat"]] = report["formats"].get(sec["vformat"], 0) + 1
            try:
                if sec["vb"] < 0 or sec["vb"] >= len(vb_info):
                    skip(si, f"vertex buffer index {sec['vb']} out of range"); continue
                vcount, vlen = vb_info[sec["vb"]]
                if vcount == 0:
                    skip(si, "vertex count 0"); continue
                xb, yb, zb, ub, vbb = bounds[si] if si < len(bounds) and any(bounds[si][0]) else (identity,) * 5
                off = fix[sec["vb"]]
                pos, nrm, uv = decode_vertices(data[off:off + vlen], vcount, sec["vformat"], xb, yb, zb, ub, vbb)
                info = dict(section=si, verts=vcount, stride=vlen // vcount if vcount else 0, vb=sec["vb"], ib=sec["ib"],
                            iformat=sec["iformat"], flags=sec["flags"], submeshes=[(st, ct) for _, st, ct in sec["submeshes"]][:4])
                if sec["ib"] < 0 or (sec["flags"] & (1 << 4)):
                    layout = 3 if sec["iformat"] == 3 else 5
                    tris, tri_shader = [], []
                    for shader, istart, ilen in sec["submeshes"]:
                        t = indices_to_triangles(list(range(istart, min(istart + ilen, vcount))), layout)
                        tris.extend(t); tri_shader.extend([shader] * len(t))
                    info["unindexed"] = True
                else:
                    if sec["ib"] >= len(ib_info): skip(si, f"index buffer {sec['ib']} out of range"); continue
                    ifmt, ilen = ib_info[sec["ib"]]
                    off = fix[len(vb_info) * 2 + sec["ib"]]
                    info["declared_layout"] = ifmt
                    tris, tri_shader = decode_triangles(data[off:off + ilen], vcount, sec["submeshes"], ifmt, info)
                info["triangles"] = len(tris)
                if si in cluster_set and len(report["cluster_details"]) < 4: report["cluster_details"].append(info)
                if info.get("dropped"): report["dropped"] += info["dropped"]
                if info.get("width") and info["width"] != (4 if vcount > 0xFFFF else 2): report["width_fixed"] += 1
                mesh = build_mesh(pos, nrm, uv, tris, tri_shader, shaders)
                if mesh["triangles"]: meshes[si] = mesh
                else: report["empty"].append((si, vcount, info.get("indices"), info.get("layout"), sec["iformat"]))
            except (MapReadError, IndexError, struct.error, ValueError) as e:
                skip(si, f"{type(e).__name__}: {e}")
                if log: log(f"{bsp.path}: section {si} skipped ({e})")
        return dict(name=bsp.path.replace("\\", "/").split("/")[-1], path=bsp.path, sections=meshes,
                    clusters=[c for c in clusters if c in meshes],
                    instances=[i for i in instances if i[1] in meshes], shader_tags=shader_tags, report=report)

    cls.bsp_layout = bsp_layout
    cls.structure_bsps = structure_bsps
    cls.read_bsp = read_bsp
_bsp_methods(CacheFile)

# ---------------------------------------------------------------------------------------------
# Shaders and bitmaps
# ---------------------------------------------------------------------------------------------
# shader (rmsh/rmtr/...) layout, MCC Reach (from Reclaimer HaloReach.ShaderTag)
SHADER_RMDF_REF, SHADER_OPTIONS, SHADER_PROPERTIES = 0, 32, 56
SHADER_PROP_SIZE, SHADER_MAP_SIZE = 172, 24
RMT2_ARGUMENTS, RMT2_USAGES = 72, 108
RMDF_CATEGORIES = 16
# bitmap layout, MCC Reach (from Reclaimer HaloReach.BitmapTag)
BITM_BITMAPS, BITM_RESOURCES, BITM_INTERLEAVED = 124, 168, 180
BITM_DATA_SIZE = 56

BITMAP_FORMATS = {0: "A8", 1: "Y8", 2: "AY8", 3: "A8Y8", 6: "R5G6B5", 8: "A1R5G5B5", 9: "A4R4G4B4",
                  10: "X8R8G8B8", 11: "A8R8G8B8", 14: "DXT1", 15: "DXT3", 16: "DXT5", 22: "U8V8",
                  31: "DXT5a", 38: "DXN", 42: "DXT5a_alpha", 43: "DXT5a_mono", 44: "DXN_mono_alpha"}
BLOCK_BYTES = {"DXT1": 8, "DXT3": 16, "DXT5": 16, "DXT5a": 8, "DXT5a_alpha": 8, "DXT5a_mono": 8, "DXN": 16, "DXN_mono_alpha": 16}
PIXEL_BYTES = {"A8": 1, "Y8": 1, "AY8": 1, "A8Y8": 2, "R5G6B5": 2, "A1R5G5B5": 2, "A4R4G4B4": 2,
               "X8R8G8B8": 4, "A8R8G8B8": 4, "U8V8": 2}

def _shader_methods(cls):
    def read_shader(self, tag_index):
        """Texture slots and constant colours of a shader tag.
        Returns dict(textures={usage: (bitmap_tag_index, (tile_u, tile_v))}, tints={name: (r,g,b,a)}, options={category: option})"""
        t = self.tags.get(tag_index)
        if t is None or t.meta is None: return None
        m = t.meta
        options = {}
        try:
            rmdf = self.tag_ref(m + SHADER_RMDF_REF)
            n_opt, o_addr = self.block(m + SHADER_OPTIONS)
            if rmdf is not None and rmdf.meta is not None:
                n_cat, c_addr = self.block(rmdf.meta + RMDF_CATEGORIES)
                for i in range(min(n_cat, n_opt)):
                    cat = self.string(self.i32(c_addr + 24 * i))
                    opt_index = self.i16(o_addr + 2 * i)
                    n_o, oo_addr = self.block(c_addr + 24 * i + 4)
                    if 0 <= opt_index < n_o:
                        options[cat] = self.string(self.i32(oo_addr + 28 * opt_index))
        except MapReadError:
            pass

        n_props, p_addr = self.block(m + SHADER_PROPERTIES)
        if n_props == 0: return dict(textures={}, tints={}, options=options)
        template = self.tag_ref(p_addr)
        n_maps, maps_addr = self.block(p_addr + 16)
        n_tile, tile_addr = self.block(p_addr + 28)
        tiling = [struct.unpack_from("<4f", self.read(tile_addr + 16 * i, 16)) for i in range(n_tile)]
        args, usages = [], []
        if template is not None and template.meta is not None:
            n, a = self.block(template.meta + RMT2_ARGUMENTS)
            args = [self.string(self.i32(a + 4 * i)) for i in range(n)]
            n, a = self.block(template.meta + RMT2_USAGES)
            usages = [self.string(self.i32(a + 4 * i)) for i in range(n)]
        textures, tints = {}, {}
        for i, usage in enumerate(usages):
            if i >= n_maps: break
            bm = self.tag_ref(maps_addr + SHADER_MAP_SIZE * i)
            if bm is None or bm.group != "bitm": continue
            k = args.index(usage) if usage in args else -1
            tile = tiling[k][:2] if 0 <= k < len(tiling) else (1.0, 1.0)
            if usage not in textures: textures[usage] = (bm.index, tile)
            mm = re.match(r"^(\w+?)_m_\d$", usage)  # terrain blends (base_map_m_0...): use the first layer
            if mm and mm.group(1) not in textures: textures[mm.group(1)] = (bm.index, tile)
        for i, arg in enumerate(args):
            if arg in usages or i >= len(tiling): continue
            if arg.endswith("_color") or arg.endswith("_tint"):
                tints[arg] = tiling[i]
        return dict(textures=textures, tints=tints, options=options)

    def read_bitmap(self, tag_index, submap=0, max_size=4096):
        """Decode one bitmap (largest mip) to an RGBA uint8 numpy array (rows top to bottom).
        Returns dict(width, height, format, curve, pixels) or raises MapReadError."""
        if np is None: raise MapReadError("numpy is needed to decode textures")
        t = self.tags.get(tag_index)
        if t is None or t.meta is None: raise MapReadError("bitmap tag missing")
        m = t.meta
        n_bm, bm_addr = self.block(m + BITM_BITMAPS)
        if submap >= n_bm: raise MapReadError("bitmap has no images")
        d = bm_addr + BITM_DATA_SIZE * submap
        width, height = self.i16(d), self.i16(d + 2)
        btype, fmt_code = self.u8(d + 6), self.i16(d + 8)
        curve, interleaved = self.u8(d + 17), self.u8(d + 18)
        fmt = BITMAP_FORMATS.get(fmt_code)
        if fmt is None: raise MapReadError(f"unsupported texture format {fmt_code}")
        if btype not in (0, 3): raise MapReadError("not a 2D texture")  # skip cube maps / 3D
        if width <= 0 or height <= 0: raise MapReadError("bad size")

        n_il, il_addr = self.block(m + BITM_INTERLEAVED)
        if n_il:
            res = self.i32(il_addr + 8 * interleaved)
        else:
            n_res, res_addr = self.block(m + BITM_RESOURCES)
            if submap >= n_res: raise MapReadError("bitmap has no pixel data")
            res = self.i32(res_addr + 8 * submap)
        data, _ = self.read_resource(res & 0xFFFF)

        if fmt in BLOCK_BYTES:
            bw, bh = (width + 3) // 4, (height + 3) // 4
            need = bw * bh * BLOCK_BYTES[fmt]
        else:
            need = width * height * PIXEL_BYTES[fmt]
        if len(data) < need: raise MapReadError(f"pixel data too short ({len(data)} < {need})")
        pixels = decode_pixels(data[:need], width, height, fmt)
        return dict(width=width, height=height, format=fmt, curve=curve, pixels=pixels)

    cls.read_shader = read_shader
    cls.read_bitmap = read_bitmap
_shader_methods(CacheFile)

# ---- block compression decoders (numpy) ----

def _rgb565(c):
    r = ((c >> 11) & 31).astype(np.float32) * (255.0 / 31)
    g = ((c >> 5) & 63).astype(np.float32) * (255.0 / 63)
    b = (c & 31).astype(np.float32) * (255.0 / 31)
    return np.stack([r, g, b], -1)

def _bc1_color(blocks, force_four=False):
    """blocks: (N,8) uint8 -> (N,16,4) uint8"""
    c0 = blocks[:, 0].astype(np.uint16) | (blocks[:, 1].astype(np.uint16) << 8)
    c1 = blocks[:, 2].astype(np.uint16) | (blocks[:, 3].astype(np.uint16) << 8)
    p0, p1 = _rgb565(c0), _rgb565(c1)
    four = (c0 > c1) | force_four
    p2 = np.where(four[:, None], (2 * p0 + p1) / 3, (p0 + p1) / 2)
    p3 = np.where(four[:, None], (p0 + 2 * p1) / 3, 0)
    pal = np.stack([p0, p1, p2, p3], 1)                                   # (N,4,3)
    alpha = np.full((len(blocks), 4), 255, np.float32)
    alpha[:, 3] = np.where(four, 255, 0)
    bits = blocks[:, 4:8].copy().view("<u4")[:, 0]
    idx = (bits[:, None] >> (2 * np.arange(16, dtype=np.uint32))) & 3      # (N,16)
    rgb = np.take_along_axis(pal, idx[:, :, None].astype(np.int64).repeat(3, 2), 1)
    a = np.take_along_axis(alpha, idx.astype(np.int64), 1)
    return np.concatenate([rgb, a[:, :, None]], 2).round().clip(0, 255).astype(np.uint8)

def _bc4(blocks, signed=False):
    """blocks: (N,8) -> (N,16) float values 0..255 (unsigned) or -1..1 (signed)"""
    if signed:
        a0 = blocks[:, 0].view(np.int8).astype(np.float32).clip(-127, 127) / 127.0
        a1 = blocks[:, 1].view(np.int8).astype(np.float32).clip(-127, 127) / 127.0
    else:
        a0 = blocks[:, 0].astype(np.float32); a1 = blocks[:, 1].astype(np.float32)
    eight = a0 > a1
    pal = [a0, a1]
    for i in range(1, 7):  # 6 interpolated (8-value mode) or 4 interpolated + min/max (6-value mode)
        pal.append(np.where(eight, ((7 - i) * a0 + i * a1) / 7, ((5 - i) * a0 + i * a1) / 5 if i <= 4 else 0))
    lo, hi = (-1.0, 1.0) if signed else (0.0, 255.0)
    pal[6] = np.where(eight, pal[6], lo); pal[7] = np.where(eight, pal[7], hi)
    pal = np.stack(pal, 1)                                                 # (N,8)
    bits = np.zeros(len(blocks), np.uint64)
    for i in range(6): bits |= blocks[:, 2 + i].astype(np.uint64) << np.uint64(8 * i)
    idx = (bits[:, None] >> (np.uint64(3) * np.arange(16, dtype=np.uint64))) & np.uint64(7)
    return np.take_along_axis(pal, idx.astype(np.int64), 1)

def _unblock(texels, width, height):
    """(N,16,C) block-ordered texels -> (H,W,C) image"""
    bw, bh = (width + 3) // 4, (height + 3) // 4
    c = texels.shape[-1]
    img = texels.reshape(bh, bw, 4, 4, c).transpose(0, 2, 1, 3, 4).reshape(bh * 4, bw * 4, c)
    return img[:height, :width]

def decode_pixels(data, width, height, fmt):
    buf = np.frombuffer(data, np.uint8)
    if fmt in BLOCK_BYTES:
        blocks = buf.reshape(-1, BLOCK_BYTES[fmt])
        if fmt == "DXT1":
            tex = _bc1_color(blocks)
        elif fmt == "DXT3":
            tex = _bc1_color(blocks[:, 8:], force_four=True)
            a = blocks[:, :8].copy().view("<u8")[:, 0]
            tex[:, :, 3] = (((a[:, None] >> (np.uint64(4) * np.arange(16, dtype=np.uint64))) & np.uint64(15)) * 17).astype(np.uint8)
        elif fmt == "DXT5":
            tex = _bc1_color(blocks[:, 8:], force_four=True)
            tex[:, :, 3] = _bc4(blocks[:, :8]).round().clip(0, 255).astype(np.uint8)
        elif fmt in ("DXT5a", "DXT5a_mono", "DXT5a_alpha"):
            v = _bc4(blocks).round().clip(0, 255).astype(np.uint8)
            tex = np.empty(v.shape + (4,), np.uint8)
            if fmt == "DXT5a_alpha":
                tex[..., :3] = 255; tex[..., 3] = v
            else:
                tex[..., :3] = v[..., None]; tex[..., 3] = v if fmt == "DXT5a" else 255
        elif fmt == "DXN":  # PC: signed BC5 normal map, blue rebuilt from red/green
            x, y = _bc4(blocks[:, :8], True), _bc4(blocks[:, 8:], True)
            z = np.sqrt(np.clip(1 - x * x - y * y, 0, 1))
            tex = np.stack([x, y, z], -1) * 127.5 + 127.5
            tex = np.concatenate([tex, np.full(x.shape + (1,), 255.0)], -1).round().clip(0, 255).astype(np.uint8)
        else:  # DXN_mono_alpha: unsigned BC5, R = grey, G = alpha
            r, g = _bc4(blocks[:, :8]), _bc4(blocks[:, 8:])
            tex = np.stack([r, r, r, g], -1).round().clip(0, 255).astype(np.uint8)
        return _unblock(tex, width, height)

    n = width * height
    if fmt in ("A8R8G8B8", "X8R8G8B8"):
        p = buf[:n * 4].reshape(height, width, 4)[..., [2, 1, 0, 3]].copy()
        if fmt == "X8R8G8B8": p[..., 3] = 255
        return p
    if fmt in ("A8", "Y8", "AY8"):
        v = buf[:n].reshape(height, width)
        out = np.empty((height, width, 4), np.uint8)
        out[..., :3] = 255 if fmt == "A8" else v[..., None]
        out[..., 3] = 255 if fmt == "Y8" else v
        return out
    if fmt == "A8Y8":
        v = buf[:n * 2].reshape(height, width, 2)
        return np.stack([v[..., 1], v[..., 1], v[..., 1], v[..., 0]], -1)
    if fmt == "U8V8":
        v = buf[:n * 2].view(np.int8).reshape(height, width, 2).astype(np.float32) / 127.0
        z = np.sqrt(np.clip(1 - (v ** 2).sum(-1), 0, 1))
        rgb = np.concatenate([v, z[..., None]], -1) * 127.5 + 127.5
        return np.concatenate([rgb, np.full((height, width, 1), 255.0)], -1).round().clip(0, 255).astype(np.uint8)
    v = buf[:n * 2].view("<u2").reshape(height, width).astype(np.uint32)
    if fmt == "R5G6B5":
        rgb = _rgb565(v); a = np.full((height, width, 1), 255.0)
        return np.concatenate([rgb, a], -1).round().astype(np.uint8)
    if fmt == "A1R5G5B5":
        ch = [((v >> 10) & 31) * 255 / 31, ((v >> 5) & 31) * 255 / 31, (v & 31) * 255 / 31, (v >> 15) * 255]
    else:  # A4R4G4B4
        ch = [((v >> 8) & 15) * 17, ((v >> 4) & 15) * 17, (v & 15) * 17, (v >> 12) * 17]
    return np.stack(ch, -1).round().astype(np.uint8)

# ---------------------------------------------------------------------------------------------
# Finding the map file MCC has loaded (Windows)
# ---------------------------------------------------------------------------------------------

MCC_EXES = ("MCC-Win64-Shipping.exe", "MCCWinStore-Win64-Shipping.exe")
SHARED_MAPS = {"shared", "campaign", "mainmenu"}

# Forge editor display name -> map file name (ForgeLib/Map.cs)
MAP_FILES = {
    "Boardwalk": "50_panopticon", "Boneyard": "70_boneyard", "Countdown": "45_launch_station",
    "Powerhouse": "30_settlement", "Reflection": "52_ivory_tower", "Spire": "35_island",
    "Sword Base": "20_sword_slayer", "Zealot": "45_aftship", "Anchor 9": "dlc_slayer",
    "Breakpoint": "dlc_invasion", "Tempest": "dlc_medium", "Condemned": "condemned",
    "Highlands": "trainingpreserve", "Battle Canyon": "cex_beavercreek", "Penance": "cex_damnation",
    "Ridgeline": "cex_timberland", "Solitary": "cex_prisoner", "High Noon": "cex_hangemhigh",
    "Breakneck": "cex_headlong", "Forge World": "forge_halo",
    "Creep's Forge World": "creep_forge_halo", "Creep's Forge World (Night)": "creep_forge_halo_night",
}

def _kernel32():
    import ctypes
    from ctypes import wintypes
    k32 = ctypes.WinDLL("kernel32", use_last_error=True)
    H, D, B = wintypes.HANDLE, wintypes.DWORD, wintypes.BOOL
    k32.CreateToolhelp32Snapshot.argtypes, k32.CreateToolhelp32Snapshot.restype = [D, D], H
    k32.Process32FirstW.argtypes = k32.Process32NextW.argtypes = [H, ctypes.c_void_p]
    k32.Process32FirstW.restype = k32.Process32NextW.restype = B
    k32.OpenProcess.argtypes, k32.OpenProcess.restype = [D, B, D], H
    k32.CloseHandle.argtypes, k32.CloseHandle.restype = [H], B
    k32.QueryFullProcessImageNameW.argtypes = [H, D, wintypes.LPWSTR, ctypes.POINTER(D)]
    k32.QueryFullProcessImageNameW.restype = B
    k32.GetCurrentProcess.argtypes, k32.GetCurrentProcess.restype = [], H
    k32.DuplicateHandle.argtypes = [H, H, H, ctypes.POINTER(H), D, B, D]
    k32.DuplicateHandle.restype = B
    k32.GetFileType.argtypes, k32.GetFileType.restype = [H], D
    k32.GetFinalPathNameByHandleW.argtypes = [H, wintypes.LPWSTR, D, D]
    k32.GetFinalPathNameByHandleW.restype = D
    return k32

def find_mcc_process():
    """Returns (pid, exe_path) of MCC, or (None, None)."""
    if os.name != "nt": return None, None
    import ctypes
    from ctypes import wintypes
    k32 = _kernel32()

    class PROCESSENTRY32W(ctypes.Structure):
        _fields_ = [("dwSize", wintypes.DWORD), ("cntUsage", wintypes.DWORD), ("th32ProcessID", wintypes.DWORD),
                    ("th32DefaultHeapID", ctypes.c_size_t), ("th32ModuleID", wintypes.DWORD),
                    ("cntThreads", wintypes.DWORD), ("th32ParentProcessID", wintypes.DWORD),
                    ("pcPriClassBase", wintypes.LONG), ("dwFlags", wintypes.DWORD),
                    ("szExeFile", wintypes.WCHAR * 260)]
    snap = k32.CreateToolhelp32Snapshot(0x2, 0)
    pid = None
    try:
        e = PROCESSENTRY32W(); e.dwSize = ctypes.sizeof(e)
        ok = k32.Process32FirstW(snap, ctypes.byref(e))
        while ok:
            if e.szExeFile.lower() in (x.lower() for x in MCC_EXES):
                pid = e.th32ProcessID; break
            ok = k32.Process32NextW(snap, ctypes.byref(e))
    finally:
        k32.CloseHandle(snap)
    if pid is None: return None, None

    exe = None
    h = k32.OpenProcess(0x1000, False, pid)  # PROCESS_QUERY_LIMITED_INFORMATION
    if h:
        buf = ctypes.create_unicode_buffer(1024); size = wintypes.DWORD(1024)
        if k32.QueryFullProcessImageNameW(h, 0, buf, ctypes.byref(size)): exe = buf.value
        k32.CloseHandle(h)
    return pid, exe

def open_files_of_process(pid, suffix=".map", timeout=15.0):
    """Paths of files with the given suffix that process `pid` has open (via the system handle table)."""
    if os.name != "nt" or pid is None: return []
    result, err = [], []
    def work():
        try: result.extend(_open_files(pid, suffix))
        except Exception as e: err.append(e)
    t = threading.Thread(target=work, daemon=True)
    t.start(); t.join(timeout)
    if t.is_alive(): raise MapReadError("Timed out listing MCC's open files")
    if err: raise err[0]
    return result

def _open_files(pid, suffix):
    import ctypes, msvcrt, tempfile
    from ctypes import wintypes
    ntdll = ctypes.WinDLL("ntdll")
    ntdll.NtQuerySystemInformation.argtypes = [wintypes.ULONG, ctypes.c_void_p, wintypes.ULONG, ctypes.POINTER(wintypes.ULONG)]
    ntdll.NtQuerySystemInformation.restype = ctypes.c_long
    k32 = _kernel32()

    class ENTRY(ctypes.Structure):
        _fields_ = [("Object", ctypes.c_void_p), ("UniqueProcessId", ctypes.c_size_t),
                    ("HandleValue", ctypes.c_size_t), ("GrantedAccess", wintypes.ULONG),
                    ("CreatorBackTraceIndex", wintypes.USHORT), ("ObjectTypeIndex", wintypes.USHORT),
                    ("HandleAttributes", wintypes.ULONG), ("Reserved", wintypes.ULONG)]

    def snapshot():
        size = 0x400000
        while True:
            buf = ctypes.create_string_buffer(size)
            ret = wintypes.ULONG(0)
            status = ntdll.NtQuerySystemInformation(64, buf, size, ctypes.byref(ret)) & 0xFFFFFFFF
            if status == 0xC0000004:  # STATUS_INFO_LENGTH_MISMATCH
                size = max(size * 2, ret.value + 0x100000); continue
            if status != 0: raise MapReadError(f"NtQuerySystemInformation failed 0x{status:08X}")
            count = ctypes.c_size_t.from_buffer(buf, 0).value
            arr = (ENTRY * count).from_buffer(buf, 16)
            return [(e.UniqueProcessId, e.HandleValue, e.ObjectTypeIndex) for e in arr]

    # find which object type index means "File" by looking at a file handle we own
    tmp = tempfile.TemporaryFile()
    try:
        own = msvcrt.get_osfhandle(tmp.fileno())
        entries = snapshot()
        mine = os.getpid()
        file_type = next((ti for p, hv, ti in entries if p == mine and hv == own), None)
    finally:
        tmp.close()

    hproc = k32.OpenProcess(0x0040, False, pid)  # PROCESS_DUP_HANDLE
    if not hproc: raise MapReadError("Can't open MCC process to list its files (try running Blender as administrator)")
    paths = []
    try:
        me = k32.GetCurrentProcess()
        buf = ctypes.create_unicode_buffer(1024)
        for p, hv, ti in entries:
            if p != pid or (file_type is not None and ti != file_type): continue
            dup = wintypes.HANDLE()
            if not k32.DuplicateHandle(hproc, wintypes.HANDLE(hv), me, ctypes.byref(dup), 0, False, 2): continue
            try:
                if k32.GetFileType(dup) != 1: continue  # FILE_TYPE_DISK only (skip pipes)
                n = k32.GetFinalPathNameByHandleW(dup, buf, 1024, 0)
                if 0 < n < 1024:
                    path = buf.value
                    if path.startswith("\\\\?\\UNC\\"): path = "\\\\" + path[8:]
                    elif path.startswith("\\\\?\\"): path = path[4:]
                    if path.lower().endswith(suffix): paths.append(path)
            finally:
                k32.CloseHandle(dup)
    finally:
        k32.CloseHandle(hproc)
    return sorted(set(paths))

def stock_maps_dir(exe_path):
    """...\\Halo The Master Chief Collection\\mcc\\binaries\\win64\\MCC-Win64-Shipping.exe -> ...\\haloreach\\maps"""
    if not exe_path: return None
    root = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(exe_path))))
    d = os.path.join(root, "haloreach", "maps")
    return d if os.path.isdir(d) else None

def workshop_dir(exe_path):
    if not exe_path: return None
    root = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(exe_path))))
    steamapps = os.path.dirname(os.path.dirname(root))  # ...\steamapps\common\<game> -> ...\steamapps
    d = os.path.join(steamapps, "workshop", "content", "976730")
    return d if os.path.isdir(d) else None

def locate_loaded_map(map_display_name=None, override=None, extra_dirs=(), log=_log_default):
    """
    Work out which .map file MCC is playing. Returns (path, stock_maps_dir).
    Order: explicit override -> files MCC has open -> search stock/workshop/extra folders.
    """
    pid, exe = find_mcc_process()
    stock = stock_maps_dir(exe)
    if override:
        if not os.path.isfile(override): raise MapReadError(f"Map file override '{override}' doesn't exist")
        return os.path.abspath(override), stock

    expected = MAP_FILES.get(map_display_name or "", None)
    candidates = []
    try:
        open_maps = open_files_of_process(pid)
        log("MCC has open: " + (", ".join(open_maps) if open_maps else "(no .map files found)"))
        candidates = [p for p in open_maps if os.path.splitext(os.path.basename(p))[0].lower() not in SHARED_MAPS]
    except Exception as e:
        log(f"Couldn't list MCC's open files ({e}); falling back to searching folders")

    if candidates:
        if expected:
            match = [p for p in candidates if os.path.splitext(os.path.basename(p))[0].lower() == expected]
            if match: return match[-1], stock
        if len(candidates) == 1: return candidates[0], stock
        log("Several maps open: " + ", ".join(candidates))

    if not expected:
        raise MapReadError(f"Don't know the file name for map '{map_display_name}'. Set the map file in the Forge Scene panel.")

    found = []
    dirs = [d for d in list(extra_dirs) + [workshop_dir(exe), stock] if d and os.path.isdir(d)]
    for d in dirs:
        found += glob.glob(os.path.join(d, "**", expected + ".map"), recursive=True)
    found = sorted(set(os.path.abspath(p) for p in found))
    if not found:
        raise MapReadError(f"Couldn't find {expected}.map. Set the map file in the Forge Scene panel.")
    if len(found) > 1:
        non_stock = [p for p in found if not stock or os.path.dirname(p).lower() != os.path.abspath(stock).lower()]
        pick = max(non_stock or found, key=os.path.getmtime)
        log(f"Found {len(found)} copies of {expected}.map, using most recently modified: {pick}. "
            "If that's the wrong one, set the map file in the Forge Scene panel.")
        return pick, stock
    return found[0], stock

# ---------------------------------------------------------------------------------------------
# Command line test
# ---------------------------------------------------------------------------------------------

def main(argv):
    import argparse
    ap = argparse.ArgumentParser(description="Inspect a Halo Reach MCC .map for the Mjolnir Forge Editor")
    ap.add_argument("map", nargs="?", help=".map file (omit to auto-detect the map MCC has loaded)")
    ap.add_argument("--obj", metavar="DIR", help="export every palette object's model as .obj into DIR")
    ap.add_argument("--bsp", metavar="DIR", help="export the level geometry (clusters + instances) as .obj into DIR")
    ap.add_argument("--textures", metavar="DIR", help="with --obj: also save each model's textures as .png into DIR")
    ap.add_argument("--shared", metavar="DIR", help="folder containing shared.map (default: next to the map)")
    a = ap.parse_args(argv)

    path = a.map
    if not path:
        path, stock = locate_loaded_map(None)
        print("MCC map:", path)
    t0 = time.time()
    with CacheFile(path) as c:
        if a.shared: c.shared_search_dirs = [a.shared]
        print(f"scenario: {c.scenario_name}, {len(c.tags)} tags")
        pal = c.read_palette()
        last = None
        for e in pal:
            if e["category"] != last:
                print(f"\n== {e['category']}{' (hidden)' if e['category_hidden'] else ''}")
                last = e["category"]
            print(f"  0x{e['id']:04X}  {e['name']:<36} {e['tag_group'] or '----'}  {e['tag_path']}")
        print(f"\n{len(pal)} palette items, read in {time.time() - t0:.2f}s")

        if a.bsp:
            os.makedirs(a.bsp, exist_ok=True)
            for bsp, lm in c.structure_bsps():
                t0 = time.time()
                try:
                    level = c.read_bsp(bsp, lm, log=None)
                except MapReadError as e:
                    print(f"bsp {bsp.path}: skipped - {e}"); continue
                meshes = [level["sections"][si] for si in level["clusters"]]
                for name, si, scale, mat in level["instances"]:
                    meshes.append(transform_mesh(level["sections"][si], scale, mat))
                out = os.path.join(a.bsp, level["name"] + ".obj")
                write_obj(dict(path=level["path"], meshes=meshes), out)
                print(f"bsp {level['path']}: {len(level['clusters'])} clusters, {len(level['instances'])} instances, "
                      f"{sum(len(m['triangles']) for m in meshes)} triangles in {time.time() - t0:.1f}s -> {out}")
                r = level["report"]
                print(f"  layout {c.profile}/{c.string_cfg}, tag: {r['cluster_count']} clusters -> sections {r['cluster_sections']}, "
                      f"{r['instance_count']} instances, {r['section_count']} sections, {r['bbox_count']} bounding boxes, "
                      f"{r['vb_count']} vertex / {r['ib_count']} index buffers, vertex formats {r['formats']}")
                if "world_bounds" in r: print("  bsp world bounds", tuple(round(v, 1) for v in r["world_bounds"]))
                for why, secs in r["skipped"].items(): print(f"  skipped {len(secs)} sections ({why}): {secs[:10]}")
                if r["empty"]: print(f"  {len(r['empty'])} sections decoded to no triangles (section, verts, indices, layout, iformat): {r['empty'][:5]}")
                if r["dropped"] or r["width_fixed"]: print(f"  index check: {r['dropped']} triangles dropped as invalid, {r['width_fixed']} sections needed the other index width")
                for d in r["cluster_details"]: print("  cluster section", d)
                cl = [level["sections"][si] for si in level["clusters"]]
                if cl:
                    pts = [p for mm in cl for p in mm["positions"]]
                    print("  cluster geometry spans", [(round(min(p[i] for p in pts), 1), round(max(p[i] for p in pts), 1)) for i in range(3)])
                    s0 = level["clusters"][0]
                    print(f"  first cluster section {s0}: {len(level['sections'][s0]['positions'])} verts, {len(level['sections'][s0]['triangles'])} tris")

        if a.obj:
            os.makedirs(a.obj, exist_ok=True)
            done, ok, failed = set(), 0, 0
            for e in pal:
                if e["tag_index"] < 0 or e["tag_index"] in done: continue
                done.add(e["tag_index"])
                try:
                    model = c.read_object_model(e["tag_index"], e["variant"], log=print)
                    if model is None: continue
                    if len(model["parts"]) > 1: print(f"  {e['name']}: attached {', '.join(model['parts'][1:])}")
                    fn = re.sub(r"[^\w\-.]+", "_", e["name"]) + ".obj"
                    write_obj(model, os.path.join(a.obj, fn))
                    ok += 1
                    if a.textures:
                        os.makedirs(a.textures, exist_ok=True)
                        for spath, sidx in model["shader_tags"].items():
                            info = c.read_shader(sidx)
                            if not info: continue
                            print(f"  shader {spath}: {', '.join(info['textures'])} {info['tints']} {info['options']}")
                            for usage, (bidx, tile) in info["textures"].items():
                                out = os.path.join(a.textures, re.sub(r"[^\w\-.]+", "_", c.tags[bidx].path) + ".png")
                                if os.path.exists(out): continue
                                try:
                                    bm = c.read_bitmap(bidx)
                                    write_png(bm["pixels"], out)
                                except Exception as ex:
                                    print(f"    {usage}: {ex}")
                except Exception as ex:
                    failed += 1
                    print(f"  model failed for {e['name']}: {ex}")
            print(f"exported {ok} models ({failed} failed) to {a.obj}")
        c.close_all()

if __name__ == "__main__":
    main(sys.argv[1:])

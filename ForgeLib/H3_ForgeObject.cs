using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ForgeLib.Halo3 {
    [Flags]
    public enum PlacementFlags : ushort {
        OCCUPIED_SLOT = 1,
        OBJECT_EDITED = 2,
        NEVER_CREATED_SCENARIO_OBJECT = 4,
        SCENARIO_OBJECT_BIT = 8,
        PLACEMENT_CREATE_AT_REST_BIT = 16,
        SCENARIO_OBJECT_REMOVED = 32,
        OBJECT_SUSPENDED = 64,
        OBJECT_CANDY_MONITORED = 128,
        SPAWNS_ATTACHED = 256,
        HARD_ATTACHMENT = 512
    }

    [Flags]
    public enum ObjectFlags : byte {
        UniqueSpawn    = 0b00000001,
        HideAtStart    = 0b00000010,
        Symmetric      = 0b00000100,
        Asymmetric     = 0b00001000,
        TimerOnDeath   = 0b00010000,
        TimerOnDisturb = 0b00100000,
        PhysicsFixed   = 0b01000000,
        PhysicsPhased  = 0b10000000
    }

    public enum Scenario : byte {
        CTF,
        SLAYER,
        ODDBALL,
        KING,
        JUGGERNAUT,
        BITS,
        TERRITORIES,
        ASSAULT,
        HALO2_COUNT,
        VIP,
        INFECTION,
        TARGET_TRAINING,
        COUNT
    }

    public enum ObjectType : byte {
        ORDINARY,
        WEAPON,
        GRENADE,
        PROJECTILE,
        POWERUP,
        EQUIPMENT,
        LIGHT_LAND_VEHICLE,
        HEAVY_LAND_VEHICLE,
        FLYING_VEHICLE,
        TELEPORTER_2WAY,
        TELEPORTER_SENDER,
        TELEPORTER_RECEIVER,
        PLAYER_SPAWN_LOCATION,
        PLAYER_RESPAWN_ZONE,
        ODDBALL_SPAWN_LOCATION,
        CTF_FLAG_SPAWN_LOCATION,
        TARGET_SPAWN_LOCATION,
        CTF_FLAG_RETURN_AREA,
        KOTH_HILL_AREA,
        INFECTION_SAFE_AREA,
        TERRITORY_AREA,
        VIP_INFLUENCE_AREA,
        VIP_DESTINATION_ZONE,
        JUGGERNAUT_DESTINATION_ZONE
    }

    public enum Map : int {
        CONSTRUCT = 0x4F,
        EPITAPH = 0x50,
        GUARDIAN = 0x51,
        HIGH_GROUND = 0x52,
        ISOLATION = 0x53,
        LAST_RESORT = 0x54,
        NARROWS = 0x55,
        SANDTRAP = 0x56,
        SNOWBOUND = 0x57,
        THE_PIT = 0x58,
        VALHALLA = 0x59,
        FOUNDRY = 0x5A,
        RATS_NEST = 0x5B,
        STANDOFF = 0x5C,
        AVALANCHE = 0x5D,
        BLACKOUT = 0x5E,
        GHOST_TOWN = 0x5F,
        COLD_STORAGE = 0x60,
        ASSEMBLY = 0x61,
        ORBITAL = 0x62,
        SANDBOX = 0x2DA,
        CITADEL = 0x64,
        HERETIC = 0x65,
        LONGSHORE = 0x66
    }

    public enum SaveGameType : uint {
        FILE_TYPE_FIRST,
        CAMPAIGN,
        FILE_TYPE_FIRST_GAME_VARIANT,
        VARIANT_CTF,
        VARIANT_SLAYER,
        VARIANT_ODDBALL,
        VARIANT_KING,
        VARIANT_JUGGERNAUT,
        VARIANT_TERRITORIES,
        VARIANT_ASSAULT,
        VARIANT_INFECTION,
        VARIANT_VIP,
        FILE_TYPE_LAST_GAME_VARIANT,
        MAP_VARIANT,
        FILM,
        FILM_CLIP,
        SCREEN_SHOT,
        FILE_TYPE_COUNT,
        FILE_TYPE_ANY,
        FILE_TYPE_ANY_GAME_VARIANT,
        FILE_TYPE_NONE,
        FILE_TYPE_BITS
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct H3_ForgeObject {
        public PlacementFlags placement;
        public ushort reuse_timeout;
        public int object_index;
        public int helper_index;
        public int definition_index;
        public Transform transform;
        public ObjectIdentifier spawn_attached_to;
        public MP_Data MP_props;

        public const int size = 84;// 0x54

        public override string ToString() => $"{definition_index} {transform} ({spawn_attached_to})";
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ObjectIdentifier {
        public int id;
        public ushort bsp_index;
        public byte type;
        public byte source;

        public const int size = 8;

        public override string ToString() => $"{(id != -1 ? id.ToString() : "DETACHED")}";
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MP_Data {
        public byte game_engine_flags;
        public Scenario scenario;
        public ObjectFlags placement;
        public Color team;
        public byte shared_storage;// Teleporter channel / spawn sequence / spare clips / etc
        public byte spawn_time_seconds;// 0: Never
        public ObjectType type;
        public Shape shape;
        public ShapeData shape_dims;

        public const int size = 24;
    }



    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SaveGame {
        public ulong unique_id;
        private fixed sbyte display_name[32];// utf16le
        private fixed sbyte description[128];
        private fixed sbyte author[16];
        public SaveGameType e_saved_game_file_type;
        public byte author_is_xuid_online;
        private fixed byte pad0[3];
        public fixed byte author_xuid[8];// hex
        public ulong byte_size;
        public ulong date;
        public int length_seconds;
        public int e_campaign_id;
        public Map e_map_id;
        public int e_game_engine_type;
        public int e_campaign_difficulty_level;
        public short hopper_id;
        private short pad;
        public ulong game_id;
        public short map_variant_version;
        public short scenario_objects;
        public short variant_objects;
        public short quotas;
        public Map e_map_id_2;
        public Bounds bounds;
        public int e_scenario_game_engine;
        public float max_budget;
        public float spent_budget;
        public short showing_helpers;
        public short built_in;
        public uint original_map_signature_hash;

        public const int size = 304;// 0x130

        public string DisplayName { get { fixed (sbyte* ptr = &display_name[0]) { return new string(ptr, 0, 32, Encoding.Unicode); } } }
        public string Description { get { fixed (sbyte* ptr = &description[0]) { return new string(ptr, 0, 128); } } }
        public string Author { get { fixed (sbyte* ptr = &author[0]) { return new string(ptr, 0, 16); } } }
        public DateTime Date => Epoch + TimeSpan.FromSeconds(date);

        static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Quota {
        public int object_definition_index;
        public byte min_count;
        public byte max_count;
        public byte count;
        public byte max_allowed;
        public float price;

        public const int size = 12;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct H3_MapVariant {
        public SaveGame data;
        //public fixed H3_ForgeObject objects[640];
        private fixed byte objects[640 * H3_ForgeObject.size];
        public fixed short object_type_start_index[14];
        //public fixed Quota quotas[256];
        private fixed byte quotas[256 * Quota.size];
        public fixed int gamestate_indices[80];

        public H3_ForgeObject* GetForgeObjects() {
            //return (H3_ForgeObject*)&objects[0];
            fixed (byte* p = &objects[0]) { return (H3_ForgeObject*)p; }
        }

        public Quota* GetQuotes() {
            fixed (byte* p = &objects[0]) { return (Quota*)p; }
        }
    }
}

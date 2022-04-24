namespace ForgeLib {
    public static class MapUtil {
        public static Map FromIdString(string id) {
            switch (id) {
                case "50_panopticon": return Map.Boardwalk;
                case "70_boneyard": return Map.Boneyard;
                case "45_launch_station": return Map.Countdown;
                case "30_settlement": return Map.Powerhouse;
                case "52_ivory_tower": return Map.Reflection;
                case "35_island": return Map.Spire;
                case "20_sword_slayer": return Map.Sword_Base;
                case "45_aftship": return Map.Zealot;
                case "dlc_slayer": return Map.Anchor_9;
                case "dlc_invasion": return Map.Breakpoint;
                case "dlc_medium": return Map.Tempest;
                case "condemned": return Map.Condemned;
                case "trainingpreserve": return Map.Highlands;
                case "cex_beavercreek": return Map.Battle_Canyon;
                case "cex_damnation": return Map.Penance;
                case "cex_timberland": return Map.Ridgeline;
                case "cex_prisoner": return Map.Solitary;
                case "cex_hangemhigh": return Map.High_Noon;
                case "cex_headlong": return Map.Breakneck;
                case "forge_halo": return Map.Forge_World;
                default: return Map.None;
            }
        }

        public static Map FromH3_Enum(Halo3.Map map) => Map.Construct + (map - Halo3.Map.CONSTRUCT);

        public static string ToString(Map map) {
            switch (map) {
                case Map.None: return "None";
                default: return "Unknown";

                // Halo Reach
                case Map.Boardwalk: return "Boardwalk";
                case Map.Boneyard: return "Boneyard";
                case Map.Countdown: return "Countdown";
                case Map.Powerhouse: return "Powerhouse";
                case Map.Reflection: return "Reflection";
                case Map.Spire: return "Spire";
                case Map.Sword_Base: return "Sword Base";
                case Map.Zealot: return "Zealot";
                case Map.Anchor_9: return "Anchor 9";
                case Map.Breakpoint: return "Breakpoint";
                case Map.Tempest: return "Tempest";
                case Map.Condemned: return "Condemned";
                case Map.Highlands: return "Highlands";
                case Map.Battle_Canyon: return "Battle Canyon";
                case Map.Penance: return "Penance";
                case Map.Ridgeline: return "Ridgeline";
                case Map.Solitary: return "Solitary";
                case Map.High_Noon: return "High Noon";
                case Map.Breakneck: return "Breakneck";
                case Map.Forge_World: return "Forge World";

                // Halo 3
                case Map.Construct: return "Construct";
                case Map.Epitaph: return "Epitaph";
                case Map.Guardian: return "Guardian";
                case Map.HighGround: return "High Ground";
                case Map.Isolation: return "Isolation";
                case Map.LastResort: return "Last Resort";
                case Map.Narrows: return "Narrows";
                case Map.Sandtrap: return "Sandtrap";
                case Map.Snowbound: return "Snowbound";
                case Map.ThePit: return "The Pit";
                case Map.Valhalla: return "Valhalla";
                case Map.Foundry: return "Foundry";
                case Map.RatsNest: return "Rats Nest";
                case Map.Standoff: return "Standoff";
                case Map.Avalanche: return "Avalanche";
                case Map.Blackout: return "Blackout";
                case Map.GhostTown: return "Ghost Town";
                case Map.ColdStorage: return "Cold Storage";
                case Map.Assembly: return "Assembly";
                case Map.Orbital: return "Orbital";
                case Map.Sandbox: return "Sandbox";
                case Map.Citadel: return "Citadel";
                case Map.Heretic: return "Heretic";
                case Map.Longshore: return "Longshore";
            }
        }
    }
}

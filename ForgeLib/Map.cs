namespace ForgeLib {
    public static class MapUtil {
        public static Map FromId(string id) {
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

        public static string ToString(Map map) {
            switch (map) {
                case Map.None: return "None";
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
                default: return "Unknown";
            }
        }
    }
}

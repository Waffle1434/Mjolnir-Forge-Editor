using System.Collections.Generic;

namespace ForgeLib {
    public static class ItemParser {
        public class TwoWayDictionary<T1, T2> {
            Dictionary<T1, T2> t1Tot2 = new Dictionary<T1, T2>();
            Dictionary<T2, T1> t2Tot1 = new Dictionary<T2, T1>();

            public T2 this[T1 key] {
                get => t1Tot2[key];
                set {
                    t1Tot2[key] = value;
                    t2Tot1[value] = key;
                }
            }
            public T1 this[T2 key] {
                get => t2Tot1[key];
                set {
                    t2Tot1[key] = value;
                    t1Tot2[value] = key;
                }
            }

            public bool TryGetValue(T1 key, out T2 value) => t1Tot2.TryGetValue(key, out value);
            public bool TryGetValue(T2 key, out T1 value) => t2Tot1.TryGetValue(key, out value);
        }

        static TwoWayDictionary<int, string> universalTypes = new TwoWayDictionary<int, string>();
        static Dictionary<Map, TwoWayDictionary<int, string>> maps = new Dictionary<Map, TwoWayDictionary<int, string>>();
        static TwoWayDictionary<int, string> forgeWorldTypes = new TwoWayDictionary<int, string>();

        static ItemParser() {
            #region Universal
            #region Weapons Human
            universalTypes[0x0000] = "Assault Rifle";
            universalTypes[0x0100] = "DMR";
            universalTypes[0x0200] = "Grenade Launcher";
            universalTypes[0x0300] = "Magnum";
            universalTypes[0x0400] = "Rocket Launcher";
            universalTypes[0x0500] = "Shotgun";
            universalTypes[0x0600] = "Sniper Rifle";
            universalTypes[0x0700] = "Spartan Laser";
            universalTypes[0x0800] = "Frag Grenade";
            universalTypes[0x0900] = "Mounted Machinegun";
            #endregion
            #region Weapons Covenant
            universalTypes[0x0A00] = "Concussion Rifle";
            universalTypes[0x0B00] = "Energy Sword";
            universalTypes[0x0C00] = "Fuel Rod Gun";
            universalTypes[0x0D00] = "Gravity Hammer";
            universalTypes[0x0E00] = "Focus Rifle";
            universalTypes[0x0F00] = "Needle Rifle";
            universalTypes[0x1000] = "Needler";
            universalTypes[0x1100] = "Plasma Launcher";
            universalTypes[0x1200] = "Plasma Pistol";
            universalTypes[0x1300] = "Plasma Repeater";
            universalTypes[0x1400] = "Plasma Rifle";
            universalTypes[0x1500] = "Spiker";
            universalTypes[0x1600] = "Plasma Grenade";
            universalTypes[0x1700] = "Plasma Turret";
            #endregion
            #region Armor Abilities
            universalTypes[0x1800] = "Active Camouflage";
            universalTypes[0x1900] = "Armor Lock";
            universalTypes[0x1A00] = "Drop Shield";
            universalTypes[0x1B00] = "Evade";
            universalTypes[0x1C00] = "Hologram";
            universalTypes[0x1D00] = "Jet Pack";
            universalTypes[0x1E00] = "Sprint";
            #endregion
            #endregion

            #region Forge World
            maps[Map.Forge_World] = forgeWorldTypes;
            #region Vehicles
            forgeWorldTypes[0x1F00] = "Banshee";
            forgeWorldTypes[0x2000] = "Falcon";
            forgeWorldTypes[0x2100] = "Ghost";
            forgeWorldTypes[0x2200] = "Mongoose";
            forgeWorldTypes[0x2300] = "Revenant";
            forgeWorldTypes[0x2400] = "Scorpion";
            forgeWorldTypes[0x2500] = "Shade Turret";
            forgeWorldTypes[0x2600] = "Warthog, Default";
            forgeWorldTypes[0x2601] = "Warthog, Gauss";
            forgeWorldTypes[0x2602] = "Warthog, Rocket";
            forgeWorldTypes[0x2700] = "Wraith";
            #endregion
            #region Gadgets
            forgeWorldTypes[0x2800] = "Fusion Coil";
            forgeWorldTypes[0x2801] = "Landmine";
            forgeWorldTypes[0x2802] = "Plasma Battery";
            forgeWorldTypes[0x2803] = "Propane Tank";
            forgeWorldTypes[0x2900] = "Health Station";
            forgeWorldTypes[0x2A00] = "Camo Powerup";
            forgeWorldTypes[0x2A01] = "Overshield";
            forgeWorldTypes[0x2A02] = "Custom Powerup";
            forgeWorldTypes[0x2B00] = "Cannon, Man";
            forgeWorldTypes[0x2B01] = "Cannon, Man, Heavy";
            forgeWorldTypes[0x2B02] = "Cannon, Man, Light";
            forgeWorldTypes[0x2B03] = "Cannon, Vehicle";
            forgeWorldTypes[0x2B04] = "Gravity Lift";
            forgeWorldTypes[0x2C00] = "One Way Shield 2";
            forgeWorldTypes[0x2D00] = "One Way Shield 3";
            forgeWorldTypes[0x2E00] = "One Way Shield 4";
            forgeWorldTypes[0x2F00] = "FX:Colorblind";
            forgeWorldTypes[0x2F01] = "FX:Next Gen";
            forgeWorldTypes[0x2F02] = "FX:Juicy";
            forgeWorldTypes[0x2F03] = "FX:Nova";
            forgeWorldTypes[0x2F04] = "FX:Olde Timey";
            forgeWorldTypes[0x2F05] = "FX:Pen And Ink";
            forgeWorldTypes[0x2F06] = "FX:Purple";
            forgeWorldTypes[0x2F07] = "FX:Green";
            forgeWorldTypes[0x2F08] = "FX:Orange";
            forgeWorldTypes[0x3000] = "Shield Door, Small";
            forgeWorldTypes[0x3100] = "Shield Door, Medium";
            forgeWorldTypes[0x3200] = "Shield Door, Large";
            forgeWorldTypes[0x3300] = "Receiver Node";
            forgeWorldTypes[0x3301] = "Sender Node";
            forgeWorldTypes[0x3302] = "Two-Way Node";
            forgeWorldTypes[0x3400] = "Die";
            forgeWorldTypes[0x3401] = "Golf Ball";
            forgeWorldTypes[0x3402] = "Golf Club";
            forgeWorldTypes[0x3403] = "Kill Ball";
            forgeWorldTypes[0x3404] = "Soccer Ball";
            forgeWorldTypes[0x3405] = "Tin Cup";
            forgeWorldTypes[0x3500] = "Light, Red";
            forgeWorldTypes[0x3501] = "Light, Blue";
            forgeWorldTypes[0x3502] = "Light, Green";
            forgeWorldTypes[0x3503] = "Light, Orange";
            forgeWorldTypes[0x3504] = "Light, Purple";
            forgeWorldTypes[0x3505] = "Light, Yellow";
            forgeWorldTypes[0x3506] = "Light, White";
            forgeWorldTypes[0x3507] = "Light, Red, Flashing";
            forgeWorldTypes[0x3508] = "Light, Yellow, Flashing";
            #endregion
            #region Spawning
            forgeWorldTypes[0x3600] = "Initial Spawn";
            forgeWorldTypes[0x3700] = "Respawn Point";
            forgeWorldTypes[0x3800] = "Initial Loadout Camera";
            forgeWorldTypes[0x3900] = "Respawn Zone";
            forgeWorldTypes[0x3A00] = "Respawn Zone, Weak";
            forgeWorldTypes[0x3B00] = "Respawn Zone, Anti";
            forgeWorldTypes[0x3C00] = "Safe Boundary";
            forgeWorldTypes[0x3C01] = "Soft Safe Boundary";
            forgeWorldTypes[0x3D00] = "Kill Boundary";
            forgeWorldTypes[0x3D01] = "Soft Kill Boundary";
            #endregion
            #region Objectives
            forgeWorldTypes[0x3E00] = "Flag Stand";
            forgeWorldTypes[0x3F00] = "Capture Plate";
            forgeWorldTypes[0x4000] = "Hill Marker";
            #endregion
            #region Scenery
            forgeWorldTypes[0x4100] = "Barricade, Small";
            forgeWorldTypes[0x4101] = "Barricade, Large";
            forgeWorldTypes[0x4102] = "Covenant Barrier";
            forgeWorldTypes[0x4103] = "Portable Shield";
            forgeWorldTypes[0x4200] = "Camping Stool";
            forgeWorldTypes[0x4300] = "Crate, Heavy Duty";
            forgeWorldTypes[0x4301] = "Crate, Heavy, Small";
            forgeWorldTypes[0x4302] = "Covenant Crate";
            forgeWorldTypes[0x4303] = "Crate, Half Open";
            forgeWorldTypes[0x4400] = "Sandbag Wall";
            forgeWorldTypes[0x4401] = "Sandbag, Turret Wall";
            forgeWorldTypes[0x4402] = "Sandbag Corner, 45";
            forgeWorldTypes[0x4403] = "Sandbag Corner, 90";
            forgeWorldTypes[0x4404] = "Sandbag Endcap";
            forgeWorldTypes[0x4500] = "Street Cone";
            #endregion
            #region Structure
            #region Building Blocks
            forgeWorldTypes[0x4600] = "Block, 1x1";
            forgeWorldTypes[0x4601] = "Block, 1x1, Flat";
            forgeWorldTypes[0x4602] = "Block, 1x1, Short";
            forgeWorldTypes[0x4603] = "Block, 1x1, Tall";
            forgeWorldTypes[0x4604] = "Block, 1x1, Tall And Thin";
            forgeWorldTypes[0x4605] = "Block, 1x2";
            forgeWorldTypes[0x4606] = "Block, 1x4";
            forgeWorldTypes[0x4607] = "Block, 2x1, Flat";
            forgeWorldTypes[0x4608] = "Block, 2x2";
            forgeWorldTypes[0x4609] = "Block, 2x2, Flat";
            forgeWorldTypes[0x460A] = "Block, 2x2, Short";
            forgeWorldTypes[0x460B] = "Block, 2x2, Tall";
            forgeWorldTypes[0x460C] = "Block, 2x3";
            forgeWorldTypes[0x460D] = "Block, 2x4";
            forgeWorldTypes[0x460E] = "Block, 3x1, Flat";
            forgeWorldTypes[0x460F] = "Block, 3x3";
            forgeWorldTypes[0x4610] = "Block, 3x3, Flat";
            forgeWorldTypes[0x4611] = "Block, 3x3, Short";
            forgeWorldTypes[0x4612] = "Block, 3x3, Tall";
            forgeWorldTypes[0x4613] = "Block, 3x4";
            forgeWorldTypes[0x4614] = "Block, 4x4";
            forgeWorldTypes[0x4615] = "Block, 4x4, Flat";
            forgeWorldTypes[0x4616] = "Block, 4x4, Short";
            forgeWorldTypes[0x4617] = "Block, 4x4, Tall";
            forgeWorldTypes[0x4618] = "Block, 5x1, Short";
            forgeWorldTypes[0x4619] = "Block, 5x5, Flat";
            #endregion
            #region Bridges And Platforms
            forgeWorldTypes[0x4700] = "Bridge, Small";
            forgeWorldTypes[0x4701] = "Bridge, Medium";
            forgeWorldTypes[0x4702] = "Bridge, Large";
            forgeWorldTypes[0x4703] = "Bridge, XLarge";
            forgeWorldTypes[0x4704] = "Bridge, Diagonal";
            forgeWorldTypes[0x4705] = "Bridge, Diag, Small";
            forgeWorldTypes[0x4706] = "Dish";
            forgeWorldTypes[0x4707] = "Dish, Open";
            forgeWorldTypes[0x4708] = "Corner, 45 Degrees";
            forgeWorldTypes[0x4709] = "Corner, 2x2";
            forgeWorldTypes[0x470A] = "Corner, 4x4";
            forgeWorldTypes[0x470B] = "Landing Pad";
            forgeWorldTypes[0x470C] = "Platform, Ramped";
            forgeWorldTypes[0x470D] = "Platform, Large";
            forgeWorldTypes[0x470E] = "Platform, XL";
            forgeWorldTypes[0x470F] = "Platform, XXL";
            forgeWorldTypes[0x4710] = "Platform, Y";
            forgeWorldTypes[0x4711] = "Platform, Y, Large";
            forgeWorldTypes[0x4712] = "Sniper Nest";
            forgeWorldTypes[0x4713] = "Staircase";
            #endregion
            #region Buildings
            forgeWorldTypes[0x4714] = "Walkway, Large";
            forgeWorldTypes[0x4800] = "Bunker, Small";
            forgeWorldTypes[0x4801] = "Bunker, Small, Covered";
            forgeWorldTypes[0x4802] = "Bunker, Box";
            forgeWorldTypes[0x4803] = "Bunker, Round";
            forgeWorldTypes[0x4804] = "Bunker, Ramp";
            forgeWorldTypes[0x4805] = "Pyramid";
            forgeWorldTypes[0x4806] = "Tower, 2 Story";
            forgeWorldTypes[0x4807] = "Tower, 3 Story";
            forgeWorldTypes[0x4808] = "Tower, Tall";
            forgeWorldTypes[0x4809] = "Room, Double";
            forgeWorldTypes[0x480A] = "Room, Triple";
            #endregion
            #region Decorative
            forgeWorldTypes[0x4900] = "Antenna, Small";
            forgeWorldTypes[0x4901] = "Antenna, Satellite";
            forgeWorldTypes[0x4902] = "Brace";
            forgeWorldTypes[0x4903] = "Brace, Large";
            forgeWorldTypes[0x4904] = "Brace, Tunnel";
            forgeWorldTypes[0x4905] = "Column";
            forgeWorldTypes[0x4906] = "Cover";
            forgeWorldTypes[0x4907] = "Cover, Crenellation";
            forgeWorldTypes[0x4908] = "Cover, Glass";
            forgeWorldTypes[0x4909] = "Glass Sail";
            forgeWorldTypes[0x490A] = "Railing, Small";
            forgeWorldTypes[0x490B] = "Railing, Medium";
            forgeWorldTypes[0x490C] = "Railing, Long";
            forgeWorldTypes[0x490D] = "Teleporter Frame";
            forgeWorldTypes[0x490E] = "Strut";
            forgeWorldTypes[0x490F] = "Large Walkway Cover";
            #endregion
            #region Doors, Windows, And Walls
            forgeWorldTypes[0x4A00] = "Door";
            forgeWorldTypes[0x4A01] = "Door, Double";
            forgeWorldTypes[0x4A02] = "Window";
            forgeWorldTypes[0x4A03] = "Window, Double";
            forgeWorldTypes[0x4A04] = "Wall";
            forgeWorldTypes[0x4A05] = "Wall, Double";
            forgeWorldTypes[0x4A06] = "Wall, Corner";
            forgeWorldTypes[0x4A07] = "Wall, Curved";
            forgeWorldTypes[0x4A08] = "Wall, Coliseum";
            forgeWorldTypes[0x4A09] = "Window, Colesium";
            forgeWorldTypes[0x4A0A] = "Tunnel, Short";
            forgeWorldTypes[0x4A0B] = "Tunnel, Long";
            #endregion
            #region Inclines
            forgeWorldTypes[0x4B00] = "Bank, 1x1";
            forgeWorldTypes[0x4B01] = "Bank, 1x2";
            forgeWorldTypes[0x4B02] = "Bank, 2x1";
            forgeWorldTypes[0x4B03] = "Bank, 2x2";
            forgeWorldTypes[0x4B04] = "Ramp, 1x2";
            forgeWorldTypes[0x4B05] = "Ramp, 1x2, Shallow";
            forgeWorldTypes[0x4B06] = "Ramp, 2x2";
            forgeWorldTypes[0x4B07] = "Ramp, 2x2, Steep";
            forgeWorldTypes[0x4B08] = "Ramp, Circular, Small";
            forgeWorldTypes[0x4B09] = "Ramp, Circular, Large";
            forgeWorldTypes[0x4B0A] = "Ramp, Bridge, Small";
            forgeWorldTypes[0x4B0B] = "Ramp, Bridge, Medium";
            forgeWorldTypes[0x4B0C] = "Ramp, Bridge, Large";
            forgeWorldTypes[0x4B0D] = "Ramp, XL";
            forgeWorldTypes[0x4B0E] = "Ramp, Stunt";
            #endregion
            #region Natural
            forgeWorldTypes[0x4C00] = "Rock, Small";
            forgeWorldTypes[0x4C01] = "Rock, Flat";
            forgeWorldTypes[0x4C02] = "Rock, Medium 1";
            forgeWorldTypes[0x4C03] = "Rock, Medium 2";
            forgeWorldTypes[0x4C04] = "Rock, Spire 1";
            forgeWorldTypes[0x4C05] = "Rock, Spire 2";
            forgeWorldTypes[0x4C06] = "Rock, Seastack";
            forgeWorldTypes[0x4C07] = "Rock, Arch";
            #endregion
            forgeWorldTypes[0x4D00] = "Grid";
            #endregion
            #region Vehicles (MCC)
            forgeWorldTypes[0x5900] = "Falcon, Nose Gun";
            forgeWorldTypes[0x5901] = "Falcon, Grenadier";
            forgeWorldTypes[0x5902] = "Falcon, Transport";
            forgeWorldTypes[0x5A00] = "Warthog, Transport";
            forgeWorldTypes[0x5B00] = "Sabre";
            forgeWorldTypes[0x5C00] = "Seraph";
            forgeWorldTypes[0x5D00] = "Cart, Electric";
            forgeWorldTypes[0x5E00] = "Forklift";
            forgeWorldTypes[0x5F00] = "Pickup";
            forgeWorldTypes[0x6000] = "Truck Cab";
            forgeWorldTypes[0x6100] = "Van, Oni";
            forgeWorldTypes[0x6200] = "Shade, Fuel Rod";
            #endregion
            #region Gadgets (MCC)
            forgeWorldTypes[0x6300] = "Cannon, Man, Forerunner";
            forgeWorldTypes[0x6301] = "Cannon, Man, Heavy, Forerunner";
            forgeWorldTypes[0x6302] = "Cannon, Man, Light, Forerunner";
            forgeWorldTypes[0x6303] = "Gravity Lift, Forerunner";
            forgeWorldTypes[0x6304] = "Gravity Lift, Tall, Forerunner";
            forgeWorldTypes[0x6305] = "Cannon, Man, Human";
            forgeWorldTypes[0x6400] = "One Way Shield 1";
            forgeWorldTypes[0x6500] = "One Way Shield 5";
            forgeWorldTypes[0x6600] = "Shield Wall, Small";
            forgeWorldTypes[0x6700] = "Shield Wall, Medium";
            forgeWorldTypes[0x6800] = "Shield Wall, Large";
            forgeWorldTypes[0x6900] = "Shield Wall, X-Large";
            forgeWorldTypes[0x6A00] = "One Way Shield 2";
            forgeWorldTypes[0x6B00] = "One Way Shield 3";
            forgeWorldTypes[0x6C00] = "One Way Shield 4";
            forgeWorldTypes[0x6D00] = "Shield Door, Small";
            forgeWorldTypes[0x6E00] = "Shield Door, Small 1";
            forgeWorldTypes[0x6F00] = "Shield Door, Large";
            forgeWorldTypes[0x7000] = "Shield Door, Large 1";
            forgeWorldTypes[0x7100] = "Ammo Cabinet";
            forgeWorldTypes[0x7200] = "Spnkr Ammo";
            forgeWorldTypes[0x7300] = "Sniper Ammo";
            #endregion
            #region Scenery (MCC)
            forgeWorldTypes[0x7400] = "Jersey Barrier";
            forgeWorldTypes[0x7401] = "Jersey Barrier, Short";
            forgeWorldTypes[0x7402] = "Heavy Barrier";
            forgeWorldTypes[0x7500] = "Small, Closed";
            forgeWorldTypes[0x7501] = "Crate, Metal, Multi";
            forgeWorldTypes[0x7502] = "Crate, Metal, Single";
            forgeWorldTypes[0x7503] = "Crate, Fully Open";
            forgeWorldTypes[0x7504] = "Crate, Forerunner, Small";
            forgeWorldTypes[0x7505] = "Crate, Forerunner, Large";
            forgeWorldTypes[0x7600] = "Pallet";
            forgeWorldTypes[0x7601] = "Pallet, Large";
            forgeWorldTypes[0x7602] = "Pallet, Metal";
            forgeWorldTypes[0x7700] = "Driftwood 1";
            forgeWorldTypes[0x7701] = "Driftwood 2";
            forgeWorldTypes[0x7702] = "Driftwood 3";
            forgeWorldTypes[0x7800] = "Phantom";
            forgeWorldTypes[0x7801] = "Spirit";
            forgeWorldTypes[0x7802] = "Pelican";
            forgeWorldTypes[0x7803] = "Drop Pod, Elite";
            forgeWorldTypes[0x7804] = "Anti Air Gun";
            forgeWorldTypes[0x7900] = "Cargo Truck, Destroyed";
            forgeWorldTypes[0x7901] = "Falcon, Destroyed";
            forgeWorldTypes[0x7902] = "Warthog, Destroyed";
            forgeWorldTypes[0x7A00] = "Folding Chair";
            forgeWorldTypes[0x7B00] = "Dumpster";
            forgeWorldTypes[0x7C00] = "Dumpster, Tall";
            forgeWorldTypes[0x7D00] = "Equipment Case";
            forgeWorldTypes[0x7E00] = "Monitor";
            forgeWorldTypes[0x7F00] = "Plasma Storage";
            forgeWorldTypes[0x8000] = "Camping Stool, Covenant";
            forgeWorldTypes[0x8100] = "Covenant Antenna";
            forgeWorldTypes[0x8200] = "Fuel Storage";
            forgeWorldTypes[0x8300] = "Engine Cart";
            forgeWorldTypes[0x8400] = "Missile Cart";
            #endregion
            #region Structure (MCC)
            forgeWorldTypes[0x8500] = "Bridge";
            forgeWorldTypes[0x8501] = "Platform, Covenant";
            forgeWorldTypes[0x8502] = "Catwalk, Straight";
            forgeWorldTypes[0x8503] = "Catwalk, Short";
            forgeWorldTypes[0x8504] = "Catwalk, Bend, Left";
            forgeWorldTypes[0x8505] = "Catwalk, Bend, Right";
            forgeWorldTypes[0x8506] = "Catwalk, Angled";
            forgeWorldTypes[0x8507] = "Catwalk, Large";
            forgeWorldTypes[0x8600] = "Bunker, Overlook";
            forgeWorldTypes[0x8601] = "Gunners Nest";
            forgeWorldTypes[0x8700] = "Cover, Small";
            forgeWorldTypes[0x8701] = "Block, Large";
            forgeWorldTypes[0x8702] = "Blocker, Hallway";
            forgeWorldTypes[0x8703] = "Column, Stone";
            forgeWorldTypes[0x8704] = "Tombstone";
            forgeWorldTypes[0x8705] = "Cover, Large, Stone";
            forgeWorldTypes[0x8706] = "Cover, Large";
            forgeWorldTypes[0x8707] = "Walkway Cover";
            forgeWorldTypes[0x8708] = "Walkway Cover, Short";
            forgeWorldTypes[0x8709] = "Cover, Large, Human";
            forgeWorldTypes[0x870A] = "I-Beam";
            forgeWorldTypes[0x8800] = "Wall";
            forgeWorldTypes[0x8801] = "Door";
            forgeWorldTypes[0x8802] = "Door, Human";
            forgeWorldTypes[0x8803] = "Door A, Forerunner";
            forgeWorldTypes[0x8804] = "Door B, Forerunner";
            forgeWorldTypes[0x8805] = "Door C, Forerunner";
            forgeWorldTypes[0x8806] = "Door D, Forerunner";
            forgeWorldTypes[0x8807] = "Door E, Forerunner";
            forgeWorldTypes[0x8808] = "Door F, Forerunner";
            forgeWorldTypes[0x8809] = "Door G, Forerunner";
            forgeWorldTypes[0x880A] = "Door H, Forerunner";
            forgeWorldTypes[0x880B] = "Wall, Small, Forerunner";
            forgeWorldTypes[0x880C] = "Wall, Large, Forerunner";
            forgeWorldTypes[0x8900] = "Rock, Spire 3";
            forgeWorldTypes[0x8901] = "Tree, Dead";
            #endregion
            forgeWorldTypes[0x8D00] = "Target Designator";// Other (MCC)
            #endregion
        }

        public static bool TryTypeToName(int type, Map map, out string name) {
            if (universalTypes.TryGetValue(type, out name)) return true;
            if (maps.TryGetValue(map, out TwoWayDictionary<int, string> mapTypes) && mapTypes.TryGetValue(type, out name)) return true;

            return false;
        }

        public static bool TryNameToType(string name, Map map, out int type) {
            if (universalTypes.TryGetValue(name, out type)) return true;
            if (maps.TryGetValue(map, out TwoWayDictionary<int, string> mapTypes) && mapTypes.TryGetValue(name, out type)) return true;

            return false;
        }
    }
}

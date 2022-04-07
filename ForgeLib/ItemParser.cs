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

        static void AddNext<T>(this TwoWayDictionary<int, T> dict, ref int key, T value) {
            dict[key] = value;
            key = (key + (1 << 8)) & 0xFF00;
        }
        static void AddSubcategory<T>(this TwoWayDictionary<int, T> dict, ref int key, params T[] values) {
            foreach (T value in values)
                dict[key++] = value;
            key = (key + (1 << 8)) & 0xFF00;
        }

        static TwoWayDictionary<int, string> universalTypes = new TwoWayDictionary<int, string>();
        static Dictionary<Map, TwoWayDictionary<int, string>> maps = new Dictionary<Map, TwoWayDictionary<int, string>>();

        static ItemParser() {
            #region Universal
            #region Weapons Human
            int key = 0;
            universalTypes.AddNext(ref key, "Assault Rifle");
            universalTypes.AddNext(ref key, "DMR");
            universalTypes.AddNext(ref key, "Grenade Launcher");
            universalTypes.AddNext(ref key, "Magnum");
            universalTypes.AddNext(ref key, "Rocket Launcher");
            universalTypes.AddNext(ref key, "Shotgun");
            universalTypes.AddNext(ref key, "Sniper Rifle");
            universalTypes.AddNext(ref key, "Spartan Laser");
            universalTypes.AddNext(ref key, "Frag Grenade");
            universalTypes.AddNext(ref key, "Mounted Machinegun");
            #endregion
            #region Weapons Covenant
            universalTypes.AddNext(ref key, "Concussion Rifle");
            universalTypes.AddNext(ref key, "Energy Sword");
            universalTypes.AddNext(ref key, "Fuel Rod Gun");
            universalTypes.AddNext(ref key, "Gravity Hammer");
            universalTypes.AddNext(ref key, "Focus Rifle");
            universalTypes.AddNext(ref key, "Needle Rifle");
            universalTypes.AddNext(ref key, "Needler");
            universalTypes.AddNext(ref key, "Plasma Launcher");
            universalTypes.AddNext(ref key, "Plasma Pistol");
            universalTypes.AddNext(ref key, "Plasma Repeater");
            universalTypes.AddNext(ref key, "Plasma Rifle");
            universalTypes.AddNext(ref key, "Spiker");
            universalTypes.AddNext(ref key, "Plasma Grenade");
            universalTypes.AddNext(ref key, "Plasma Turret");
            #endregion
            #region Armor Abilities
            universalTypes.AddNext(ref key, "Active Camouflage");
            universalTypes.AddNext(ref key, "Armor Lock");
            universalTypes.AddNext(ref key, "Drop Shield");
            universalTypes.AddNext(ref key, "Evade");
            universalTypes.AddNext(ref key, "Hologram");
            universalTypes.AddNext(ref key, "Jet Pack");
            universalTypes.AddNext(ref key, "Sprint");
            #endregion
            #endregion

            #region Forge World
            TwoWayDictionary<int, string> forgeWorld = new TwoWayDictionary<int, string>();
            maps[Map.Forge_World] = forgeWorld;
            #region Vehicles
            forgeWorld.AddNext(ref key, "Banshee");
            forgeWorld.AddNext(ref key, "Falcon");
            forgeWorld.AddNext(ref key, "Ghost");
            forgeWorld.AddNext(ref key, "Mongoose");
            forgeWorld.AddNext(ref key, "Revenant");
            forgeWorld.AddNext(ref key, "Scorpion");
            forgeWorld.AddNext(ref key, "Shade Turret");
            forgeWorld.AddSubcategory(ref key, "Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            forgeWorld.AddNext(ref key, "Wraith");
            #endregion
            #region Gadgets
            forgeWorld.AddSubcategory(ref key, "Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            forgeWorld.AddNext(ref key, "Health Station");
            forgeWorld.AddSubcategory(ref key, "Camo Powerup", "Overshield", "Custom Powerup");
            forgeWorld.AddSubcategory(ref key,
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Vehicle",
                "Gravity Lift");
            forgeWorld.AddNext(ref key, "One Way Shield 2");
            forgeWorld.AddNext(ref key, "One Way Shield 3");
            forgeWorld.AddNext(ref key, "One Way Shield 4");
            forgeWorld.AddSubcategory(ref key,
                "FX:Colorblind",
                "FX:Next Gen",
                "FX:Juicy",
                "FX:Nova",
                "FX:Olde Timey",
                "FX:Pen And Ink",
                "FX:Purple",
                "FX:Green",
                "FX:Orange");
            forgeWorld.AddNext(ref key, "Shield Door, Small");
            forgeWorld.AddNext(ref key, "Shield Door, Medium");
            forgeWorld.AddNext(ref key, "Shield Door, Large");
            forgeWorld.AddSubcategory(ref key, "Receiver Node", "Sender Node", "Two-Way Node");
            forgeWorld.AddSubcategory(ref key, "Die", "Golf Ball", "Golf Club", "Kill Ball", "Soccer Ball", "Tin Cup");
            forgeWorld.AddSubcategory(ref key,
                "Light, Red",
                "Light, Blue",
                "Light, Green",
                "Light, Orange",
                "Light, Purple",
                "Light, Yellow",
                "Light, White",
                "Light, Red, Flashing",
                "Light, Yellow, Flashing");
            #endregion
            #region Spawning
            forgeWorld.AddNext(ref key, "Initial Spawn");
            forgeWorld.AddNext(ref key, "Respawn Point");
            forgeWorld.AddNext(ref key, "Initial Loadout Camera");
            forgeWorld.AddNext(ref key, "Respawn Zone");
            forgeWorld.AddNext(ref key, "Respawn Zone, Weak");
            forgeWorld.AddNext(ref key, "Respawn Zone, Anti");
            forgeWorld.AddSubcategory(ref key, "Safe Boundary", "Soft Safe Boundary");
            forgeWorld.AddSubcategory(ref key, "Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            forgeWorld.AddNext(ref key, "Flag Stand");
            forgeWorld.AddNext(ref key, "Capture Plate");
            forgeWorld.AddNext(ref key, "Hill Marker");
            #endregion
            #region Scenery
            forgeWorld.AddSubcategory(ref key,
                "Barricade, Small",
                "Barricade, Large",
                "Covenant Barrier",
                "Portable Shield");
            forgeWorld.AddNext(ref key, "Camping Stool");
            forgeWorld.AddSubcategory(ref key,
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open");
            forgeWorld.AddSubcategory(ref key,
                "Sandbag Wall",
                "Sandbag, Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            forgeWorld.AddNext(ref key, "Street Cone");
            #endregion
            #region Structure
            #region Building Blocks
            forgeWorld.AddSubcategory(ref key,
                "Block, 1x1",
                "Block, 1x1, Flat",
                "Block, 1x1, Short",
                "Block, 1x1, Tall",
                "Block, 1x1, Tall And Thin",
                "Block, 1x2",
                "Block, 1x4",
                "Block, 2x1, Flat",
                "Block, 2x2",
                "Block, 2x2, Flat",
                "Block, 2x2, Short",
                "Block, 2x2, Tall",
                "Block, 2x3",
                "Block, 2x4",
                "Block, 3x1, Flat",
                "Block, 3x3",
                "Block, 3x3, Flat",
                "Block, 3x3, Short",
                "Block, 3x3, Tall",
                "Block, 3x4",
                "Block, 4x4",
                "Block, 4x4, Flat",
                "Block, 4x4, Short",
                "Block, 4x4, Tall",
                "Block, 5x1, Short",
                "Block, 5x5, Flat");
            #endregion
            #region Bridges And Platforms
            forgeWorld.AddSubcategory(ref key,
                "Bridge, Small",
                "Bridge, Medium",
                "Bridge, Large",
                "Bridge, XLarge",
                "Bridge, Diagonal",
                "Bridge, Diag, Small",
                "Dish",
                "Dish, Open",
                "Corner, 45 Degrees",
                "Corner, 2x2",
                "Corner, 4x4",
                "Landing Pad",
                "Platform, Ramped",
                "Platform, Large",
                "Platform, XL",
                "Platform, XXL",
                "Platform, Y",
                "Platform, Y, Large",
                "Sniper Nest",
                "Staircase",
                "Walkway, Large");
            #endregion
            #region Buildings
            forgeWorld.AddSubcategory(ref key,
                "Bunker, Small",
                "Bunker, Small, Covered",
                "Bunker, Box",
                "Bunker, Round",
                "Bunker, Ramp",
                "Pyramid",
                "Tower, 2 Story",
                "Tower, 3 Story",
                "Tower, Tall",
                "Room, Double",
                "Room, Triple");
            #endregion
            #region Decorative
            forgeWorld.AddSubcategory(ref key,
                "Antenna, Small",
                "Antenna, Satellite",
                "Brace",
                "Brace, Large",
                "Brace, Tunnel",
                "Column",
                "Cover",
                "Cover, Crenellation",
                "Cover, Glass",
                "Glass Sail",
                "Railing, Small",
                "Railing, Medium",
                "Railing, Long",
                "Teleporter Frame",
                "Strut",
                "Large Walkway Cover");
            #endregion
            #region Doors, Windows, And Walls
            forgeWorld.AddSubcategory(ref key,
                "Door",
                "Door, Double",
                "Window",
                "Window, Double",
                "Wall",
                "Wall, Double",
                "Wall, Corner",
                "Wall, Curved",
                "Wall, Coliseum",
                "Window, Colesium",
                "Tunnel, Short",
                "Tunnel, Long");
            #endregion
            #region Inclines
            forgeWorld.AddSubcategory(ref key,
                "Bank, 1x1",
                "Bank, 1x2",
                "Bank, 2x1",
                "Bank, 2x2",
                "Ramp, 1x2",
                "Ramp, 1x2, Shallow",
                "Ramp, 2x2",
                "Ramp, 2x2, Steep",
                "Ramp, Circular, Small",
                "Ramp, Circular, Large",
                "Ramp, Bridge, Small",
                "Ramp, Bridge, Medium",
                "Ramp, Bridge, Large",
                "Ramp, XL",
                "Ramp, Stunt");
            #endregion
            #region Natural
            forgeWorld.AddSubcategory(ref key,
                "Rock, Small",
                "Rock, Flat",
                "Rock, Medium 1",
                "Rock, Medium 2",
                "Rock, Spire 1",
                "Rock, Spire 2",
                "Rock, Seastack",
                "Rock, Arch");
            #endregion
            forgeWorld.AddNext(ref key, "Grid");
            #endregion
            #region Hidden Structure Blocks
            forgeWorld.AddNext(ref key, "Block, 2x2, Invisible");
            forgeWorld.AddNext(ref key, "Block, 1x1, Invisible");
            forgeWorld.AddNext(ref key, "Block, 2x2x2, Invisible");
            forgeWorld.AddNext(ref key, "Block, 4x4x2, Invisible");
            forgeWorld.AddNext(ref key, "Block, 4x4x4, Invisible");
            forgeWorld.AddNext(ref key, "Block, 2x1, Flat, Invisible");
            forgeWorld.AddNext(ref key, "Block, 1x1, Flat, Invisible");
            forgeWorld.AddNext(ref key, "Block, 1x1, Small, Invisible");
            forgeWorld.AddNext(ref key, "Block, 2x2, Flat, Invisible");
            forgeWorld.AddNext(ref key, "Block, 4x2, Flat, Invisible");
            forgeWorld.AddNext(ref key, "Block, 4x4, Flat, Invisible");
            #endregion
            #region Vehicles (MCC)
            forgeWorld.AddSubcategory(ref key, "Falcon, Nose Gun", "Falcon, Grenadier", "Falcon, Transport");
            forgeWorld.AddNext(ref key, "Warthog, Transport");
            forgeWorld.AddNext(ref key, "Sabre");
            forgeWorld.AddNext(ref key, "Seraph");
            forgeWorld.AddNext(ref key, "Cart, Electric");
            forgeWorld.AddNext(ref key, "Forklift");
            forgeWorld.AddNext(ref key, "Pickup");
            forgeWorld.AddNext(ref key, "Truck Cab");
            forgeWorld.AddNext(ref key, "Van, Oni");
            forgeWorld.AddNext(ref key, "Shade, Fuel Rod");
            #endregion
            #region Gadgets (MCC)
            forgeWorld.AddSubcategory(ref key,
                "Cannon, Man, Forerunner",
                "Cannon, Man, Heavy, Forerunner",
                "Cannon, Man, Light, Forerunner",
                "Gravity Lift, Forerunner",
                "Gravity Lift, Tall, Forerunner",
                "Cannon, Man, Human");
            forgeWorld.AddNext(ref key, "One Way Shield 1");
            forgeWorld.AddNext(ref key, "One Way Shield 5");
            forgeWorld.AddNext(ref key, "Shield Wall, Small");
            forgeWorld.AddNext(ref key, "Shield Wall, Medium");
            forgeWorld.AddNext(ref key, "Shield Wall, Large");
            forgeWorld.AddNext(ref key, "Shield Wall, X-Large");
            forgeWorld.AddNext(ref key, "One Way Shield 2");
            forgeWorld.AddNext(ref key, "One Way Shield 3");
            forgeWorld.AddNext(ref key, "One Way Shield 4");
            forgeWorld.AddNext(ref key, "Shield Door, Small");
            forgeWorld.AddNext(ref key, "Shield Door, Small 1");
            forgeWorld.AddNext(ref key, "Shield Door, Large");
            forgeWorld.AddNext(ref key, "Shield Door, Large 1");
            forgeWorld.AddNext(ref key, "Ammo Cabinet");
            forgeWorld.AddNext(ref key, "Spnkr Ammo");
            forgeWorld.AddNext(ref key, "Sniper Ammo");
            #endregion
            #region Scenery (MCC)
            forgeWorld.AddSubcategory(ref key, "Jersey Barrier", "Jersey Barrier, Short", "Heavy Barrier");
            forgeWorld.AddSubcategory(ref key,
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Fully Open",
                "Crate, Forerunner, Small",
                "Crate, Forerunner, Large");
            forgeWorld.AddSubcategory(ref key, "Pallet", "Pallet, Large", "Pallet, Metal");
            forgeWorld.AddSubcategory(ref key, "Driftwood 1", "Driftwood 2", "Driftwood 3");
            forgeWorld.AddSubcategory(ref key, "Phantom", "Spirit", "Pelican", "Drop Pod, Elite", "Anti Air Gun");
            forgeWorld.AddSubcategory(ref key, "Cargo Truck, Destroyed", "Falcon, Destroyed", "Warthog, Destroyed");
            forgeWorld.AddNext(ref key, "Folding Chair");
            forgeWorld.AddNext(ref key, "Dumpster");
            forgeWorld.AddNext(ref key, "Dumpster, Tall");
            forgeWorld.AddNext(ref key, "Equipment Case");
            forgeWorld.AddNext(ref key, "Monitor");
            forgeWorld.AddNext(ref key, "Plasma Storage");
            forgeWorld.AddNext(ref key, "Camping Stool, Covenant");
            forgeWorld.AddNext(ref key, "Covenant Antenna");
            forgeWorld.AddNext(ref key, "Fuel Storage");
            forgeWorld.AddNext(ref key, "Engine Cart");
            forgeWorld.AddNext(ref key, "Missile Cart");
            #endregion
            #region Structure (MCC)
            forgeWorld.AddSubcategory(ref key,
                "Bridge",
                "Platform, Covenant",
                "Catwalk, Straight",
                "Catwalk, Short",
                "Catwalk, Bend, Left",
                "Catwalk, Bend, Right",
                "Catwalk, Angled",
                "Catwalk, Large");
            forgeWorld.AddSubcategory(ref key, "Bunker, Overlook", "Gunners Nest");
            forgeWorld.AddSubcategory(ref key,
                "Cover, Small",
                "Block, Large",
                "Blocker, Hallway",
                "Column, Stone",
                "Tombstone",
                "Cover, Large, Stone",
                "Cover, Large",
                "Walkway Cover",
                "Walkway Cover, Short",
                "Cover, Large, Human",
                "I-Beam");
            forgeWorld.AddSubcategory(ref key,
                "Wall (MCC)",
                "Door (MCC)",
                "Door, Human",
                "Door A, Forerunner",
                "Door B, Forerunner",
                "Door C, Forerunner",
                "Door D, Forerunner",
                "Door E, Forerunner",
                "Door F, Forerunner",
                "Door G, Forerunner",
                "Door H, Forerunner",
                "Wall, Small, Forerunner",
                "Wall, Large, Forerunner");
            forgeWorld.AddSubcategory(ref key, "Rock, Spire 3", "Tree, Dead");
            #endregion
            #region Hidden Misc
            forgeWorld.AddNext(ref key, "Generator");
            forgeWorld.AddNext(ref key, "Vending Machine");
            forgeWorld.AddNext(ref key, "Dinghy");
            #endregion
            forgeWorld.AddNext(ref key, "Target Designator");// Other (MCC)
            forgeWorld.AddNext(ref key, "Pelican, Hovering");
            forgeWorld.AddNext(ref key, "Phantom, Hovering");
            #endregion

            #region Tempest
            TwoWayDictionary<int, string> tempest = new TwoWayDictionary<int, string>();
            maps[Map.Tempest] = tempest;
            key = 0x1F00;
            #region Vehicles
            tempest.AddNext(ref key, "Banshee");
            tempest.AddNext(ref key, "Ghost");
            tempest.AddNext(ref key, "Mongoose");
            tempest.AddSubcategory(ref key, "Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            tempest.AddNext(ref key, "Wraith");
            tempest.AddNext(ref key, "Scorpion");
            #endregion
            #region Gadgets
            // exact same as FW?!
            tempest.AddSubcategory(ref key, "Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            tempest.AddNext(ref key, "Health Station");
            tempest.AddSubcategory(ref key, "Camo Powerup", "Overshield", "Custom Powerup");
            tempest.AddSubcategory(ref key,
                "Cannon, Man, Forerunner",
                "Cannon, Man, Heavy, Forerunner",
                "Cannon, Man, Light, Forerunner",
                "Cannon, Vehicle",
                "Gravity Lift");
            tempest.AddNext(ref key, "One Way Shield 2");
            tempest.AddNext(ref key, "One Way Shield 3");
            tempest.AddNext(ref key, "One Way Shield 4");
            tempest.AddSubcategory(ref key,
                "FX:Colorblind",
                "FX:Next Gen",
                "FX:Juicy",
                "FX:Nova",
                "FX:Olde Timey",
                "FX:Pen And Ink",
                "FX:Purple",
                "FX:Green",
                "FX:Orange");
            tempest.AddNext(ref key, "Shield Door, Small");
            tempest.AddNext(ref key, "Shield Door, Medium");
            tempest.AddNext(ref key, "Shield Door, Large");
            tempest.AddSubcategory(ref key, "Receiver Node", "Sender Node", "Two-Way Node");
            tempest.AddSubcategory(ref key, "Die", "Golf Ball", "Golf Club", "Kill Ball", "Soccer Ball", "Tin Cup");
            tempest.AddSubcategory(ref key,
                "Light, Red",
                "Light, Blue",
                "Light, Green",
                "Light, Orange",
                "Light, Purple",
                "Light, Yellow",
                "Light, White",
                "Light, Red, Flashing",
                "Light, Yellow, Flashing");
            #endregion
            #region Spawning
            // same as FW
            tempest.AddNext(ref key, "Initial Spawn");
            tempest.AddNext(ref key, "Respawn Point");
            tempest.AddNext(ref key, "Initial Loadout Camera");
            tempest.AddNext(ref key, "Respawn Zone");
            tempest.AddNext(ref key, "Respawn Zone, Weak");
            tempest.AddNext(ref key, "Respawn Zone, Anti");
            tempest.AddSubcategory(ref key, "Safe Boundary", "Soft Safe Boundary");
            tempest.AddSubcategory(ref key, "Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            // same as FW
            tempest.AddNext(ref key, "Flag Stand");
            tempest.AddNext(ref key, "Capture Plate");
            tempest.AddNext(ref key, "Hill Marker");
            #endregion
            #region Scenery
            tempest.AddSubcategory(ref key,
                "Barricade, Small",
                "Barricade, Large",
                "Covenant Barrier");
            tempest.AddSubcategory(ref key,
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open");
            tempest.AddSubcategory(ref key,
                "Sandbag Wall",
                "Sandbag, Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            tempest.AddNext(ref key, "Street Cone");
            tempest.AddSubcategory(ref key, "Driftwood 1", "Driftwood 2", "Driftwood 3");
            #endregion
            #region Structure
            #region Building Blocks
            //FW
            tempest.AddSubcategory(ref key,
                "Block, 1x1",
                "Block, 1x1, Flat",
                "Block, 1x1, Short",
                "Block, 1x1, Tall",
                "Block, 1x1, Tall And Thin",
                "Block, 1x2",
                "Block, 1x4",
                "Block, 2x1, Flat",
                "Block, 2x2",
                "Block, 2x2, Flat",
                "Block, 2x2, Short",
                "Block, 2x2, Tall",
                "Block, 2x3",
                "Block, 2x4",
                "Block, 3x1, Flat",
                "Block, 3x3",
                "Block, 3x3, Flat",
                "Block, 3x3, Short",
                "Block, 3x3, Tall",
                "Block, 3x4",
                "Block, 4x4",
                "Block, 4x4, Flat",
                "Block, 4x4, Short",
                "Block, 4x4, Tall",
                "Block, 5x1, Short",
                "Block, 5x5, Flat");
            #endregion
            #region Bridges And Platforms
            //FW
            tempest.AddSubcategory(ref key,
                "Bridge, Small",
                "Bridge, Medium",
                "Bridge, Large",
                "Bridge, XLarge",
                "Bridge, Diagonal",
                "Bridge, Diag, Small",
                "Dish",
                "Dish, Open",
                "Corner, 45 Degrees",
                "Corner, 2x2",
                "Corner, 4x4",
                "Landing Pad",
                "Platform, Ramped",
                "Platform, Large",
                "Platform, XL",
                "Platform, XXL",
                "Platform, Y",
                "Platform, Y, Large",
                "Sniper Nest",
                "Staircase",
                "Walkway, Large");
            #endregion
            #region Buildings
            //FW
            tempest.AddSubcategory(ref key,
                "Bunker, Small",
                "Bunker, Small, Covered",
                "Bunker, Box",
                "Bunker, Round",
                "Bunker, Ramp",
                "Pyramid",
                "Tower, 2 Story",
                "Tower, 3 Story",
                "Tower, Tall",
                "Room, Double",
                "Room, Triple");
            #endregion
            #region Decorative
            //FW
            tempest.AddSubcategory(ref key,
                "Antenna, Small",
                "Antenna, Satellite",
                "Brace",
                "Brace, Large",
                "Brace, Tunnel",
                "Column",
                "Cover",
                "Cover, Crenellation",
                "Cover, Glass",
                "Glass Sail",
                "Railing, Small",
                "Railing, Medium",
                "Railing, Long",
                "Teleporter Frame",
                "Strut",
                "Large Walkway Cover");
            #endregion
            #region Doors, Windows, And Walls
            //FW
            tempest.AddSubcategory(ref key,
                "Door",
                "Door, Double",
                "Window",
                "Window, Double",
                "Wall",
                "Wall, Double",
                "Wall, Corner",
                "Wall, Curved",
                "Wall, Coliseum",
                "Window, Colesium",
                "Tunnel, Short",
                "Tunnel, Long");
            #endregion
            #region Inclines
            //FW
            tempest.AddSubcategory(ref key,
                "Bank, 1x1",
                "Bank, 1x2",
                "Bank, 2x1",
                "Bank, 2x2",
                "Ramp, 1x2",
                "Ramp, 1x2, Shallow",
                "Ramp, 2x2",
                "Ramp, 2x2, Steep",
                "Ramp, Circular, Small",
                "Ramp, Circular, Large",
                "Ramp, Bridge, Small",
                "Ramp, Bridge, Medium",
                "Ramp, Bridge, Large",
                "Ramp, XL",
                "Ramp, Stunt");
            #endregion
            #region Natural
            tempest.AddSubcategory(ref key,
                "Rock, Small",
                "Rock, Flat",
                "Rock, Medium 1",
                "Rock, Medium 2",
                "Rock, Spire 1",
                "Rock, Spire 2",
                "Rock, Arch",
                "Rock, Small 1",
                "Rock, Spire 3");
            #endregion
            tempest.AddNext(ref key, "Grid");
            #endregion
            #region Hidden Structure Blocks
            tempest.AddNext(ref key, "Block, 2x2, Invisible");
            tempest.AddNext(ref key, "Block, 1x1, Invisible");
            tempest.AddNext(ref key, "Block, 2x2x2, Invisible");
            tempest.AddNext(ref key, "Block, 4x4x2, Invisible");
            tempest.AddNext(ref key, "Block, 4x4x4, Invisible");
            tempest.AddNext(ref key, "Block, 2x1, Flat, Invisible");
            tempest.AddNext(ref key, "Block, 1x1, Flat, Invisible");
            tempest.AddNext(ref key, "Block, 1x1, Small, Invisible");
            tempest.AddNext(ref key, "Block, 2x2, Flat, Invisible");
            tempest.AddNext(ref key, "Block, 4x2, Flat, Invisible");
            tempest.AddNext(ref key, "Block, 4x4, Flat, Invisible");
            tempest.AddNext(ref key, "Destination Delta(?!)");
            #endregion
            #region Vehicles (MCC)
            //FW
            tempest.AddSubcategory(ref key, "Falcon, Nose Gun", "Falcon, Grenadier", "Falcon, Transport");
            tempest.AddNext(ref key, "Warthog, Transport");
            tempest.AddNext(ref key, "Sabre");
            tempest.AddNext(ref key, "Seraph");
            tempest.AddNext(ref key, "Cart, Electric");
            tempest.AddNext(ref key, "Forklift");
            tempest.AddNext(ref key, "Pickup");
            tempest.AddNext(ref key, "Truck Cab");
            tempest.AddNext(ref key, "Van, Oni");
            tempest.AddNext(ref key, "Shade, Fuel Rod");
            #endregion
            #region Gadgets (MCC)
            tempest.AddSubcategory(ref key,
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Gravity Lift, Forerunner",
                "Gravity Lift, Tall, Forerunner",
                "Cannon, Man, Human");
            tempest.AddNext(ref key, "One Way Shield 1");
            tempest.AddNext(ref key, "One Way Shield 5");
            tempest.AddNext(ref key, "Shield Wall, Small");
            tempest.AddNext(ref key, "Shield Wall, Medium");
            tempest.AddNext(ref key, "Shield Wall, Large");
            tempest.AddNext(ref key, "Shield Wall, X-Large");
            tempest.AddNext(ref key, "One Way Shield 2");
            tempest.AddNext(ref key, "One Way Shield 3");
            tempest.AddNext(ref key, "One Way Shield 4");
            tempest.AddNext(ref key, "Shield Door, Small");
            tempest.AddNext(ref key, "Shield Door, Small 1");
            tempest.AddNext(ref key, "Shield Door, Large");
            tempest.AddNext(ref key, "Shield Door, Large 1");
            tempest.AddNext(ref key, "Ammo Cabinet");
            tempest.AddNext(ref key, "Spnkr Ammo");
            tempest.AddNext(ref key, "Sniper Ammo");
            #endregion
            #region Scenery (MCC)
            tempest.AddSubcategory(ref key, "Jersey Barrier", "Jersey Barrier, Short", "Heavy Barrier");
            tempest.AddSubcategory(ref key,
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Fully Open",
                "Crate, Forerunner, Small",
                "Crate, Forerunner, Large");
            tempest.AddSubcategory(ref key, "Pallet", "Pallet, Large", "Pallet, Metal");
            tempest.AddSubcategory(ref key, "Phantom", "Spirit", "Pelican", "Drop Pod, Elite", "Anti Air Gun");
            tempest.AddSubcategory(ref key, "Cargo Truck, Destroyed", "Falcon, Destroyed", "Warthog, Destroyed");
            tempest.AddNext(ref key, "Folding Chair");
            tempest.AddNext(ref key, "Dumpster");
            tempest.AddNext(ref key, "Dumpster, Tall");
            tempest.AddNext(ref key, "Equipment Case");
            tempest.AddNext(ref key, "Monitor");
            tempest.AddNext(ref key, "Plasma Storage");
            tempest.AddNext(ref key, "Camping Stool, Covenant");
            tempest.AddNext(ref key, "Covenant Antenna");
            tempest.AddNext(ref key, "Fuel Storage");
            tempest.AddNext(ref key, "Engine Cart");
            tempest.AddNext(ref key, "Missile Cart");
            #endregion
            #region Structure (MCC)
            tempest.AddSubcategory(ref key,
                "Bridge",
                "Platform, Covenant",
                "Catwalk, Straight",
                "Catwalk, Short",
                "Catwalk, Bend, Left",
                "Catwalk, Bend, Right",
                "Catwalk, Angled",
                "Catwalk, Large");
            tempest.AddSubcategory(ref key, "Bunker, Overlook", "Gunners Nest");
            tempest.AddSubcategory(ref key,
                "Cover, Small",
                "Block, Large",
                "Blocker, Hallway",
                "Column, Stone",
                "Tombstone",
                "Cover, Large, Stone",
                "Cover, Large",
                "Walkway Cover",
                "Walkway Cover, Short",
                "Cover, Large, Human",
                "I-Beam");
            tempest.AddSubcategory(ref key,
                "Wall (MCC)",
                "Door (MCC)",
                "Door, Human",
                "Door A, Forerunner",
                "Door B, Forerunner",
                "Door C, Forerunner",
                "Door D, Forerunner",
                "Door E, Forerunner",
                "Door F, Forerunner",
                "Door G, Forerunner",
                "Door H, Forerunner",
                "Wall, Small, Forerunner",
                "Wall, Large, Forerunner");
            tempest.AddNext(ref key, "Tree, Dead");
            #endregion
            #region Hidden Misc
            tempest.AddNext(ref key, "Generator");
            tempest.AddNext(ref key, "Vending Machine");
            tempest.AddNext(ref key, "Dinghy");
            #endregion
            tempest.AddNext(ref key, "Target Designator");// Other (MCC)
            tempest.AddNext(ref key, "Pelican, Hovering");
            tempest.AddNext(ref key, "Phantom, Hovering");
            #endregion

            #region Ridgeline
            TwoWayDictionary<int, string> ridgeline = new TwoWayDictionary<int, string>();
            maps[Map.Ridgeline] = ridgeline;
            key = 0x1F00;
            #region Gadgets
            ridgeline.AddSubcategory(ref key, "Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            ridgeline.AddNext(ref key, "Health Station");
            ridgeline.AddSubcategory(ref key, "Camo Powerup", "Overshield", "Custom Powerup");
            ridgeline.AddSubcategory(ref key,
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Vehicle",
                "Gravity Lift");
            ridgeline.AddSubcategory(ref key,
                "FX:Colorblind",
                "FX:Next Gen",
                "FX:Juicy",
                "FX:Nova",
                "FX:Olde Timey",
                "FX:Pen And Ink");
            ridgeline.AddSubcategory(ref key, "Receiver Node", "Sender Node", "Two-Way Node");
            ridgeline.AddSubcategory(ref key,
                "Die",
                "Golf Ball",
                "Golf Club",
                "Kill Ball",
                "Soccer Ball",
                "Tin Cup");
            ridgeline.AddSubcategory(ref key,
                "Light, Red",
                "Light, Blue",
                "Light, Green",
                "Light, Orange",
                "Light, Purple",
                "Light, Yellow",
                "Light, White",
                "Light, Red, Flashing",
                "Light, Yellow, Flashing");
            #endregion
            #region Spawning
            ridgeline.AddNext(ref key, "Initial Spawn");
            ridgeline.AddNext(ref key, "Respawn Point");
            ridgeline.AddNext(ref key, "Initial Loadout Camera");
            ridgeline.AddNext(ref key, "Respawn Zone");
            ridgeline.AddNext(ref key, "Respawn Zone, Weak");
            ridgeline.AddNext(ref key, "Respawn Zone, Anti");
            ridgeline.AddSubcategory(ref key, "Safe Boundary", "Soft Safe Boundary");
            ridgeline.AddSubcategory(ref key, "Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            ridgeline.AddNext(ref key, "Flag Stand");
            ridgeline.AddNext(ref key, "Capture Plate");
            ridgeline.AddNext(ref key, "Hill Marker");
            #endregion
            #region Scenery
            ridgeline.AddSubcategory(ref key,
                "Barricade, Small",
                "Barricade, Large",
                "Jersey Barrier",
                "Jersey Barrier, Short",
                "Covenant Barrier",
                "Portable Shield");
            ridgeline.AddSubcategory(ref key,
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open",
                "Crate, Fully Open");
            ridgeline.AddSubcategory(ref key, "Pallet", "Pallet, Large", "Pallet, Metal");
            ridgeline.AddSubcategory(ref key,
                "Sandbag Wall",
                "Sandbag, Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            #endregion
            #region Vehicles
            ridgeline.AddNext(ref key, "Banshee");
            ridgeline.AddNext(ref key, "Falcon");
            ridgeline.AddNext(ref key, "Ghost");
            ridgeline.AddNext(ref key, "Mongoose");
            ridgeline.AddNext(ref key, "Revenant");
            ridgeline.AddNext(ref key, "Scorpion");
            ridgeline.AddSubcategory(ref key, "Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            ridgeline.AddNext(ref key, "Wraith");
            ridgeline.AddNext(ref key, "Shade Turret");
            #endregion
            #region Structure
            #region Building Blocks
            ridgeline.AddSubcategory(ref key,
                "Block, 1x1",
                "Block, 1x1, Flat",
                "Block, 1x1, Short",
                "Block, 1x1, Tall",
                "Block, 1x1, Tall And Thin",
                "Block, 1x2",
                "Block, 1x4",
                "Block, 2x1, Flat",
                "Block, 2x2",
                "Block, 2x2, Flat",
                "Block, 2x2, Short",
                "Block, 2x2, Tall",
                "Block, 2x3",
                "Block, 2x4",
                "Block, 3x1, Flat",
                "Block, 3x3",
                "Block, 3x3, Flat",
                "Block, 3x3, Short",
                "Block, 3x3, Tall",
                "Block, 3x4",
                "Block, 4x4",
                "Block, 4x4, Flat",
                "Block, 4x4, Short",
                "Block, 4x4, Tall",
                "Block, 5x1, Short",
                "Block, 5x5, Flat");
            #endregion
            #region Bridges And Platforms
            ridgeline.AddSubcategory(ref key,
                "Bridge, Small",
                "Bridge, Medium",
                "Bridge, Large",
                "Bridge, XLarge",
                "Bridge, Diagonal",
                "Bridge, Diag, Small",
                "Corner, 45 Degrees",
                "Corner, 2x2",
                "Corner, 4x4",
                "Landing Pad",
                "Platform, Ramped",
                "Platform, Large",
                "Platform, XL",
                "Platform, Y",
                "Platform, Y, Large",
                "Sniper Nest",
                "Walkway, Large");
            #endregion
            #region Buildings
            ridgeline.AddSubcategory(ref key,
                "Bunker, Small",
                "Bunker, Small, Covered",
                "Bunker, Box",
                "Bunker, Ramp",
                "Tower, 2 Story",
                "Tower, 3 Story",
                "Tower, Tall",
                "Room, Double",
                "Bunker, Overlook",
                "Gunner's Nest");
            #endregion
            #region Decorative
            ridgeline.AddSubcategory(ref key,
                "Antenna, Small",
                "Brace",
                "Column",
                "Cover",
                "Cover, Crenellation",
                "Railing, Small",
                "Railing, Medium",
                "Railing, Long",
                "Teleporter Frame",
                "Strut",
                "Large Walkway Cover",
                "Cover, Small");
            #endregion
            #region Doors, Windows, And Walls
            ridgeline.AddSubcategory(ref key,
                "Door",
                "Door, Double",
                "Window",
                "Window, Double",
                "Wall",
                "Wall, Double",
                "Wall, Corner",
                "Wall, Curved",
                "Tunnel, Short",
                "Tunnel, Long");
            #endregion
            #region Inclines
            ridgeline.AddSubcategory(ref key,
                "Ramp, 1x2",
                "Ramp, 1x2, Shallow",
                "Ramp, 2x2",
                "Ramp, 2x2, Steep",
                "Ramp, Circular, Small",
                "Ramp, Bridge, Small",
                "Ramp, Bridge, Medium",
                "Ramp, Bridge, Large",
                "Ramp, XL");
            #endregion
            #region Natural
            ridgeline.AddSubcategory(ref key,
                "Rock, Small",
                "Rock, Flat",
                "Rock, Medium 1",
                "Rock, Medium 2",
                "Rock, Spire 1",
                "Rock, Spire 2",
                "Rock, Seastack",
                "Rock, Arch");
            #endregion
            ridgeline.AddNext(ref key, "Grid");
            ridgeline.AddNext(ref key, "Tree, Dead");
            #endregion
            #region Hidden Structure Blocks
            ridgeline.AddNext(ref key, "Block, 2x2, Invisible");
            ridgeline.AddNext(ref key, "Block, 1x1, Invisible");
            ridgeline.AddNext(ref key, "Block, 2x2x2, Invisible");
            ridgeline.AddNext(ref key, "Block, 4x4x2, Invisible");
            ridgeline.AddNext(ref key, "Block, 4x4x4, Invisible");
            ridgeline.AddNext(ref key, "Block, 2x1, Flat, Invisible");
            ridgeline.AddNext(ref key, "Block, 1x1, Flat, Invisible");
            ridgeline.AddNext(ref key, "Block, 1x1, Small, Invisible");
            ridgeline.AddNext(ref key, "Block, 2x2, Flat, Invisible");
            ridgeline.AddNext(ref key, "Block, 4x2, Flat, Invisible");
            ridgeline.AddNext(ref key, "Block, 4x4, Flat, Invisible");
            #endregion
            ridgeline.AddNext(ref key, "Health Cabinet");
            #endregion

            #region Breakneck
            TwoWayDictionary<int, string> breakneck = new TwoWayDictionary<int, string>();
            maps[Map.Breakneck] = breakneck;
            key = 0x1F00;

            #region Gadgets
            breakneck.AddSubcategory(ref key, "Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            breakneck.AddNext(ref key, "Health Station");
            breakneck.AddSubcategory(ref key, "Camo Powerup", "Overshield", "Custom Powerup");
            breakneck.AddSubcategory(ref key,
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Man, Human",
                "Cannon, Vehicle",
                "Gravity Lift");
            breakneck.AddSubcategory(ref key,
                "FX:Colorblind",
                "FX:Next Gen",
                "FX:Juicy",
                "FX:Nova",
                "FX:Olde Timey",
                "FX:Pen And Ink");
            breakneck.AddSubcategory(ref key, "Receiver Node", "Sender Node", "Two-Way Node");
            breakneck.AddSubcategory(ref key,
                "Die",
                "Golf Ball",
                "Golf Club",
                "Kill Ball",
                "Soccer Ball",
                "Tin Cup");
            breakneck.AddSubcategory(ref key,
                "Light, Red",
                "Light, Blue",
                "Light, Green",
                "Light, Orange",
                "Light, Purple",
                "Light, Yellow",
                "Light, White",
                "Light, Red, Flashing",
                "Light, Yellow, Flashing");
            #endregion
            #region Spawning
            breakneck.AddNext(ref key, "Initial Spawn");
            breakneck.AddNext(ref key, "Respawn Point");
            breakneck.AddNext(ref key, "Initial Loadout Camera");
            breakneck.AddNext(ref key, "Respawn Zone");
            breakneck.AddNext(ref key, "Respawn Zone, Weak");
            breakneck.AddNext(ref key, "Respawn Zone, Anti");
            breakneck.AddSubcategory(ref key, "Safe Boundary", "Soft Safe Boundary");
            breakneck.AddSubcategory(ref key, "Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            breakneck.AddNext(ref key, "Flag Stand");
            breakneck.AddNext(ref key, "Capture Plate");
            breakneck.AddNext(ref key, "Hill Marker");
            #endregion
            #region Scenery
            breakneck.AddSubcategory(ref key,
                "Barricade, Small",
                "Barricade, Large",
                "Jersey Barrier",
                "Jersey Barrier, Short",
                "Covenant Barrier",
                "Portable Shield");
            breakneck.AddSubcategory(ref key,
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open",
                "Crate, Fully Open");
            breakneck.AddSubcategory(ref key,
                "Sandbag Wall",
                "Sandbag, Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            breakneck.AddNext(ref key, "Street Cone");
            breakneck.AddSubcategory(ref key, "Pallet", "Pallet, Large", "Pallet, Metal");
            #endregion
            #region Vehicles
            breakneck.AddNext(ref key, "Banshee");
            breakneck.AddNext(ref key, "Ghost");
            breakneck.AddNext(ref key, "Mongoose");
            breakneck.AddNext(ref key, "Revenant");
            breakneck.AddSubcategory(ref key, "Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            breakneck.AddNext(ref key, "Wraith");
            breakneck.AddNext(ref key, "Falcon");
            breakneck.AddNext(ref key, "Scorpion");
            #endregion
            #region Structure
            #region Building Blocks
            breakneck.AddSubcategory(ref key,
                "Block, 1x1",
                "Block, 1x1, Flat",
                "Block, 1x1, Short",
                "Block, 1x1, Tall",
                "Block, 1x1, Tall And Thin",
                "Block, 1x2",
                "Block, 1x4",
                "Block, 2x1, Flat",
                "Block, 2x2",
                "Block, 2x2, Flat",
                "Block, 2x2, Short",
                "Block, 2x2, Tall");
            #endregion
            #region Bridges And Platforms
            breakneck.AddSubcategory(ref key,
                "Bridge, Small",
                "Bridge, Medium",
                "Bridge, Large",
                "Bridge, XLarge",
                "Bridge, Diagonal",
                "Bridge, Diag, Small",
                "Corner, 45 Degrees",
                "Corner, 2x2",
                "Corner, 4x4");
            #endregion
            #region Buildings
            breakneck.AddSubcategory(ref key,
                "Bunker, Small",
                "Bunker, Small, Covered");
            #endregion
            #region Decorative
            breakneck.AddSubcategory(ref key,
                "Antenna, Small",
                "Column",
                "Railing, Small",
                "Railing, Medium",
                "Railing, Long",
                "Teleporter Frame",
                "Strut",
                "Cover, Large",
                "I-Beam");
            #endregion
            #region Doors, Windows, And Walls
            breakneck.AddSubcategory(ref key,
                "Wall",
                "Wall, Double",
                "Wall, Corner",
                "Wall, Curved",
                "Door, Human");
            #endregion
            #region Inclines
            breakneck.AddSubcategory(ref key,
                "Ramp, 1x2",
                "Ramp, 1x2, Shallow",
                "Ramp, 2x2",
                "Ramp, 2x2, Steep",
                "Ramp, Circular, Small",
                "Ramp, Circular, Large",
                "Ramp, Bridge, Small",
                "Ramp, Bridge, Medium",
                "Ramp, Bridge, Large");
            #endregion
            breakneck.AddNext(ref key, "Grid");
            #endregion
            #region Hidden Structure Blocks
            breakneck.AddNext(ref key, "Block, 2x2, Invisible");
            breakneck.AddNext(ref key, "Block, 1x1, Invisible");
            breakneck.AddNext(ref key, "Block, 2x2x2, Invisible");
            breakneck.AddNext(ref key, "Block, 4x4x2, Invisible");
            breakneck.AddNext(ref key, "Block, 4x4x4, Invisible");
            breakneck.AddNext(ref key, "Block, 2x1, Flat, Invisible");
            breakneck.AddNext(ref key, "Block, 1x1, Flat, Invisible");
            breakneck.AddNext(ref key, "Block, 1x1, Small, Invisible");
            breakneck.AddNext(ref key, "Block, 2x2, Flat, Invisible");
            breakneck.AddNext(ref key, "Block, 4x2, Flat, Invisible");
            breakneck.AddNext(ref key, "Block, 4x4, Flat, Invisible");
            #endregion
            breakneck.AddNext(ref key, "Health Cabinet");
            #region Vehicles (MCC)
            breakneck.AddSubcategory(ref key, "Falcon, Nose Gun", "Falcon, Grenadier", "Falcon, Transport");
            breakneck.AddNext(ref key, "Warthog, Transport");
            breakneck.AddNext(ref key, "Cart, Electric");
            breakneck.AddNext(ref key, "Forklift");
            breakneck.AddNext(ref key, "Pickup");
            breakneck.AddNext(ref key, "Truck Cab");
            breakneck.AddNext(ref key, "Van, Oni");
            breakneck.AddNext(ref key, "Shade, Fuel Rod");
            #endregion
            #region Gadgets (MCC)
            breakneck.AddNext(ref key, "One Way Shield 1");
            breakneck.AddNext(ref key, "One Way Shield 2");
            breakneck.AddNext(ref key, "One Way Shield 3");
            breakneck.AddNext(ref key, "One Way Shield 4");
            breakneck.AddNext(ref key, "One Way Shield 5");
            breakneck.AddNext(ref key, "Shield Wall, Small");
            breakneck.AddNext(ref key, "Shield Wall, Medium");
            breakneck.AddNext(ref key, "Shield Wall, Large");
            breakneck.AddNext(ref key, "Shield Wall, X-Large");
            breakneck.AddNext(ref key, "Ammo Cabinet");
            breakneck.AddNext(ref key, "Spnkr Ammo");
            breakneck.AddNext(ref key, "Sniper Ammo");
            #endregion
            #region Scenery (MCC)
            breakneck.AddNext(ref key, "Heavy Barrier");
            breakneck.AddSubcategory(ref key, "Phantom", "Spirit", "Pelican", "Drop Pod, Elite", "Anti Air Gun");
            breakneck.AddSubcategory(ref key, "Cargo Truck, Destroyed", "Falcon, Destroyed", "Warthog, Destroyed");
            breakneck.AddNext(ref key, "Folding Chair");
            breakneck.AddNext(ref key, "Dumpster");
            breakneck.AddNext(ref key, "Dumpster, Tall");
            breakneck.AddNext(ref key, "Equipment Case");
            breakneck.AddNext(ref key, "Monitor");
            breakneck.AddNext(ref key, "Plasma Storage");
            breakneck.AddNext(ref key, "Camping Stool, Covenant");
            breakneck.AddNext(ref key, "Covenant Antenna");
            breakneck.AddNext(ref key, "Fuel Storage");
            breakneck.AddNext(ref key, "Engine Cart");
            breakneck.AddNext(ref key, "Missile Cart");
            #endregion
            #region Structure (MCC)
            breakneck.AddSubcategory(ref key, "Wall (MCC)", "Door (MCC)");
            #endregion
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

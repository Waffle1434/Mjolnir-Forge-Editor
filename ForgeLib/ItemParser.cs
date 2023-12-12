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

        public class MapPalette : TwoWayDictionary<int, string> {
            public int key;

            public MapPalette(int key) => this.key = key;

            void IncrementKey() => key = (key + (1 << 8)) & 0xFF00;

            public void AddNext(string name) {
                base[key] = name;
                IncrementKey();
            }
            public void AddSubcategory(params string[] names) {
                foreach (string value in names) base[key++] = value;
                IncrementKey();
            }
        }

        

        static MapPalette universalTypes = new MapPalette(0);
        static Dictionary<Map, MapPalette> maps = new Dictionary<Map, MapPalette>();

        static ItemParser() {
            #region Universal
            #region Weapons Human
            universalTypes.AddNext("Assault Rifle");
            universalTypes.AddNext("DMR");
            universalTypes.AddNext("Grenade Launcher");
            universalTypes.AddNext("Magnum");
            universalTypes.AddNext("Rocket Launcher");
            universalTypes.AddNext("Shotgun");
            universalTypes.AddNext("Sniper Rifle");
            universalTypes.AddNext("Spartan Laser");
            universalTypes.AddNext("Frag Grenade");
            universalTypes.AddNext("Mounted Machinegun");
            #endregion
            #region Weapons Covenant
            universalTypes.AddNext("Concussion Rifle");
            universalTypes.AddNext("Energy Sword");
            universalTypes.AddNext("Fuel Rod Gun");
            universalTypes.AddNext("Gravity Hammer");
            universalTypes.AddNext("Focus Rifle");
            universalTypes.AddNext("Needle Rifle");
            universalTypes.AddNext("Needler");
            universalTypes.AddNext("Plasma Launcher");
            universalTypes.AddNext("Plasma Pistol");
            universalTypes.AddNext("Plasma Repeater");
            universalTypes.AddNext("Plasma Rifle");
            universalTypes.AddNext("Spiker");
            universalTypes.AddNext("Plasma Grenade");
            universalTypes.AddNext("Plasma Turret");
            #endregion
            #region Armor Abilities
            universalTypes.AddNext("Active Camouflage");
            universalTypes.AddNext("Armor Lock");
            universalTypes.AddNext("Drop Shield");
            universalTypes.AddNext("Evade");
            universalTypes.AddNext("Hologram");
            universalTypes.AddNext("Jet Pack");
            universalTypes.AddNext("Sprint");
            #endregion
            #endregion

            #region Forge World
            MapPalette forgeWorld = new MapPalette(universalTypes.key);
            maps[Map.Forge_World] = forgeWorld;
            #region Vehicles
            forgeWorld.AddNext("Banshee");
            forgeWorld.AddNext("Falcon");
            forgeWorld.AddNext("Ghost");
            forgeWorld.AddNext("Mongoose");
            forgeWorld.AddNext("Revenant");
            forgeWorld.AddNext("Scorpion");
            forgeWorld.AddNext("Shade Turret");
            forgeWorld.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            forgeWorld.AddNext("Wraith");
            #endregion
            #region Gadgets
            forgeWorld.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            forgeWorld.AddNext("Health Station");
            forgeWorld.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            forgeWorld.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Vehicle",
                "Gravity Lift");
            forgeWorld.AddNext("One Way Shield 2");
            forgeWorld.AddNext("One Way Shield 3");
            forgeWorld.AddNext("One Way Shield 4");
            forgeWorld.AddSubcategory(
                "FX:Colorblind",
                "FX:Next Gen",
                "FX:Juicy",
                "FX:Nova",
                "FX:Olde Timey",
                "FX:Pen And Ink",
                "FX:Purple",
                "FX:Green",
                "FX:Orange");
            forgeWorld.AddNext("Shield Door, Small");
            forgeWorld.AddNext("Shield Door, Medium");
            forgeWorld.AddNext("Shield Door, Large");
            forgeWorld.AddSubcategory("Receiver Node", "Sender Node", "Two-Way Node");
            forgeWorld.AddSubcategory("Die", "Golf Ball", "Golf Club", "Kill Ball", "Soccer Ball", "Tin Cup");
            forgeWorld.AddSubcategory(
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
            forgeWorld.AddNext("Initial Spawn");
            forgeWorld.AddNext("Respawn Point");
            forgeWorld.AddNext("Initial Loadout Camera");
            forgeWorld.AddNext("Respawn Zone");
            forgeWorld.AddNext("Respawn Zone, Weak");
            forgeWorld.AddNext("Respawn Zone, Anti");
            forgeWorld.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            forgeWorld.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            forgeWorld.AddNext("Flag Stand");
            forgeWorld.AddNext("Capture Plate");
            forgeWorld.AddNext("Hill Marker");
            #endregion
            #region Scenery
            forgeWorld.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Covenant Barrier",
                "Portable Shield");
            forgeWorld.AddNext("Camping Stool");
            forgeWorld.AddSubcategory(
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open");
            forgeWorld.AddSubcategory(
                "Sandbag Wall",
                "Sandbag, Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            forgeWorld.AddNext("Street Cone");
            #endregion
            #region Structure
            #region Building Blocks
            forgeWorld.AddSubcategory(
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
            forgeWorld.AddSubcategory(
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
            forgeWorld.AddSubcategory(
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
            forgeWorld.AddSubcategory(
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
            forgeWorld.AddSubcategory(
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
            forgeWorld.AddSubcategory(
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
            forgeWorld.AddSubcategory(
                "Rock, Small",
                "Rock, Flat",
                "Rock, Medium 1",
                "Rock, Medium 2",
                "Rock, Spire 1",
                "Rock, Spire 2",
                "Rock, Seastack",
                "Rock, Arch");
            #endregion
            forgeWorld.AddNext("Grid");
            #endregion
            #region Hidden Structure Blocks
            forgeWorld.AddNext("Block, 2x2, Invisible");
            forgeWorld.AddNext("Block, 1x1, Invisible");
            forgeWorld.AddNext("Block, 2x2x2, Invisible");
            forgeWorld.AddNext("Block, 4x4x2, Invisible");
            forgeWorld.AddNext("Block, 4x4x4, Invisible");
            forgeWorld.AddNext("Block, 2x1, Flat, Invisible");
            forgeWorld.AddNext("Block, 1x1, Flat, Invisible");
            forgeWorld.AddNext("Block, 1x1, Small, Invisible");
            forgeWorld.AddNext("Block, 2x2, Flat, Invisible");
            forgeWorld.AddNext("Block, 4x2, Flat, Invisible");
            forgeWorld.AddNext("Block, 4x4, Flat, Invisible");
            #endregion
            #region Vehicles (MCC)
            forgeWorld.AddSubcategory("Falcon, Nose Gun", "Falcon, Grenadier", "Falcon, Transport");
            forgeWorld.AddNext("Warthog, Transport");
            forgeWorld.AddNext("Sabre");
            forgeWorld.AddNext("Seraph");
            forgeWorld.AddNext("Cart, Electric");
            forgeWorld.AddNext("Forklift");
            forgeWorld.AddNext("Pickup");
            forgeWorld.AddNext("Truck Cab");
            forgeWorld.AddNext("Van, Oni");
            forgeWorld.AddNext("Shade, Fuel Rod");
            #endregion
            #region Gadgets (MCC)
            forgeWorld.AddSubcategory(
                "Cannon, Man, Forerunner",
                "Cannon, Man, Heavy, Forerunner",
                "Cannon, Man, Light, Forerunner",
                "Gravity Lift, Forerunner",
                "Gravity Lift, Tall, Forerunner",
                "Cannon, Man, Human");
            forgeWorld.AddNext("One Way Shield 1");
            forgeWorld.AddNext("One Way Shield 5");
            forgeWorld.AddNext("Shield Wall, Small");
            forgeWorld.AddNext("Shield Wall, Medium");
            forgeWorld.AddNext("Shield Wall, Large");
            forgeWorld.AddNext("Shield Wall, X-Large");
            forgeWorld.AddNext("One Way Shield 2");
            forgeWorld.AddNext("One Way Shield 3");
            forgeWorld.AddNext("One Way Shield 4");
            forgeWorld.AddNext("Shield Door, Small");
            forgeWorld.AddNext("Shield Door, Small 1");
            forgeWorld.AddNext("Shield Door, Large");
            forgeWorld.AddNext("Shield Door, Large 1");
            forgeWorld.AddNext("Ammo Cabinet");
            forgeWorld.AddNext("Spnkr Ammo");
            forgeWorld.AddNext("Sniper Ammo");
            #endregion
            #region Scenery (MCC)
            forgeWorld.AddSubcategory("Jersey Barrier", "Jersey Barrier, Short", "Heavy Barrier");
            forgeWorld.AddSubcategory(
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Fully Open",
                "Crate, Forerunner, Small",
                "Crate, Forerunner, Large");
            forgeWorld.AddSubcategory("Pallet", "Pallet, Large", "Pallet, Metal");
            forgeWorld.AddSubcategory("Driftwood 1", "Driftwood 2", "Driftwood 3");
            forgeWorld.AddSubcategory("Phantom", "Spirit", "Pelican", "Drop Pod, Elite", "Anti Air Gun");
            forgeWorld.AddSubcategory("Cargo Truck, Destroyed", "Falcon, Destroyed", "Warthog, Destroyed");
            forgeWorld.AddNext("Folding Chair");
            forgeWorld.AddNext("Dumpster");
            forgeWorld.AddNext("Dumpster, Tall");
            forgeWorld.AddNext("Equipment Case");
            forgeWorld.AddNext("Monitor");
            forgeWorld.AddNext("Plasma Storage");
            forgeWorld.AddNext("Camping Stool, Covenant");
            forgeWorld.AddNext("Covenant Antenna");
            forgeWorld.AddNext("Fuel Storage");
            forgeWorld.AddNext("Engine Cart");
            forgeWorld.AddNext("Missile Cart");
            #endregion
            #region Structure (MCC)
            forgeWorld.AddSubcategory(
                "Bridge",
                "Platform, Covenant",
                "Catwalk, Straight",
                "Catwalk, Short",
                "Catwalk, Bend, Left",
                "Catwalk, Bend, Right",
                "Catwalk, Angled",
                "Catwalk, Large");
            forgeWorld.AddSubcategory("Bunker, Overlook", "Gunners Nest");
            forgeWorld.AddSubcategory(
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
            forgeWorld.AddSubcategory(
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
            forgeWorld.AddSubcategory("Rock, Spire 3", "Tree, Dead");
            #endregion
            #region Hidden Misc
            forgeWorld.AddNext("Generator");
            forgeWorld.AddNext("Vending Machine");
            forgeWorld.AddNext("Dinghy");
            #endregion
            forgeWorld.AddNext("Target Designator");// Other (MCC)
            forgeWorld.AddNext("Pelican, Hovering");
            forgeWorld.AddNext("Phantom, Hovering");
            #endregion

            #region Creep World
            MapPalette creepWorld = new MapPalette(universalTypes.key);
            maps[Map.Creep_Forge_World] = creepWorld;
            #region Vehicles
            creepWorld.AddNext("Banshee");
            //creepWorld.AddNext("Falcon");
            creepWorld.AddSubcategory("Falcon", "Falcon, Coaxial Gun / GL", "Falcon, MG", "Falcon, Rockets", "Falcon, Coaxial MG / GL", "Falcon, Cannon");
            creepWorld.AddNext("Ghost");
            creepWorld.AddNext("Mongoose");
            creepWorld.AddNext("Revenant");
            //creepWorld.AddNext("Scorpion");
            creepWorld.AddSubcategory("Scorpion", "Scorpion, Rockets");
            creepWorld.AddNext("Shade Turret");
            creepWorld.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            creepWorld.AddNext("Wraith");
            creepWorld.AddSubcategory("Pickup, Rockets", "Truck, Tank Turret");
            #endregion
            #region Gadgets
            creepWorld.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            creepWorld.AddNext("Health Station");
            creepWorld.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            creepWorld.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Vehicle",
                "Gravity Lift");
            creepWorld.AddNext("One Way Shield 2");
            creepWorld.AddNext("One Way Shield 3");
            creepWorld.AddNext("One Way Shield 4");
            creepWorld.AddSubcategory(
                "FX:Colorblind",
                "FX:Next Gen",
                "FX:Juicy",
                "FX:Nova",
                "FX:Olde Timey",
                "FX:Pen And Ink",
                "FX:Purple",
                "FX:Green",
                "FX:Orange");
            creepWorld.AddNext("Shield Door, Small");
            creepWorld.AddNext("Shield Door, Medium");
            creepWorld.AddNext("Shield Door, Large");
            creepWorld.AddSubcategory("Receiver Node", "Sender Node", "Two-Way Node");
            creepWorld.AddSubcategory("Die", "Golf Ball", "Golf Club", "Kill Ball", "Soccer Ball", "Tin Cup");
            creepWorld.AddSubcategory(
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
            creepWorld.AddNext("Initial Spawn");
            creepWorld.AddNext("Respawn Point");
            creepWorld.AddNext("Initial Loadout Camera");
            creepWorld.AddNext("Respawn Zone");
            creepWorld.AddNext("Respawn Zone, Weak");
            creepWorld.AddNext("Respawn Zone, Anti");
            creepWorld.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            creepWorld.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            creepWorld.AddNext("Flag Stand");
            creepWorld.AddNext("Capture Plate");
            creepWorld.AddNext("Hill Marker");
            #endregion
            #region Scenery
            creepWorld.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Covenant Barrier",
                "Portable Shield");
            creepWorld.AddNext("Camping Stool");
            creepWorld.AddSubcategory(
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open");
            creepWorld.AddSubcategory(
                "Sandbag Wall",
                "Sandbag, Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            creepWorld.AddNext("Street Cone");
            #endregion
            #region Structure
            #region Building Blocks
            creepWorld.AddSubcategory(
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
            creepWorld.AddSubcategory(
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
            creepWorld.AddSubcategory(
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
            creepWorld.AddSubcategory(
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
            creepWorld.AddSubcategory(
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
            creepWorld.AddSubcategory(
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
            creepWorld.AddSubcategory(
                "Rock, Small",
                "Rock, Flat",
                "Rock, Medium 1",
                "Rock, Medium 2",
                "Rock, Spire 1",
                "Rock, Spire 2",
                "Rock, Seastack",
                "Rock, Arch");
            #endregion
            creepWorld.AddNext("Grid");
            #endregion
            #region Hidden Structure Blocks
            creepWorld.AddNext("Block, 2x2, Invisible");
            creepWorld.AddNext("Block, 1x1, Invisible");
            creepWorld.AddNext("Block, 2x2x2, Invisible");
            creepWorld.AddNext("Block, 4x4x2, Invisible");
            creepWorld.AddNext("Block, 4x4x4, Invisible");
            creepWorld.AddNext("Block, 2x1, Flat, Invisible");
            creepWorld.AddNext("Block, 1x1, Flat, Invisible");
            creepWorld.AddNext("Block, 1x1, Small, Invisible");
            creepWorld.AddNext("Block, 2x2, Flat, Invisible");
            creepWorld.AddNext("Block, 4x2, Flat, Invisible");
            creepWorld.AddNext("Block, 4x4, Flat, Invisible");
            #endregion
            #region Vehicles (MCC)
            creepWorld.AddSubcategory("Falcon, Nose Gun", "Falcon, Grenadier", "Falcon, Transport");
            creepWorld.AddNext("Warthog, Transport");
            creepWorld.AddNext("Sabre");
            creepWorld.AddNext("Seraph");
            creepWorld.AddNext("Cart, Electric");
            creepWorld.AddNext("Forklift");
            creepWorld.AddNext("Pickup");
            creepWorld.AddNext("Truck Cab");
            creepWorld.AddNext("Van, Oni");
            creepWorld.AddNext("Shade, Fuel Rod");
            #endregion
            #region Gadgets (MCC)
            creepWorld.AddSubcategory(
                "Cannon, Man, Forerunner",
                "Cannon, Man, Heavy, Forerunner",
                "Cannon, Man, Light, Forerunner",
                "Gravity Lift, Forerunner",
                "Gravity Lift, Tall, Forerunner",
                "Cannon, Man, Human");
            creepWorld.AddNext("One Way Shield 1");
            creepWorld.AddNext("One Way Shield 5");
            creepWorld.AddNext("Shield Wall, Small");
            creepWorld.AddNext("Shield Wall, Medium");
            creepWorld.AddNext("Shield Wall, Large");
            creepWorld.AddNext("Shield Wall, X-Large");
            creepWorld.AddNext("One Way Shield 2");
            creepWorld.AddNext("One Way Shield 3");
            creepWorld.AddNext("One Way Shield 4");
            creepWorld.AddNext("Shield Door, Small");
            creepWorld.AddNext("Shield Door, Small 1");
            creepWorld.AddNext("Shield Door, Large");
            creepWorld.AddNext("Shield Door, Large 1");
            creepWorld.AddNext("Ammo Cabinet");
            creepWorld.AddNext("Spnkr Ammo");
            creepWorld.AddNext("Sniper Ammo");
            #endregion
            #region Scenery (MCC)
            creepWorld.AddSubcategory("Jersey Barrier", "Jersey Barrier, Short", "Heavy Barrier");
            creepWorld.AddSubcategory(
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Fully Open",
                "Crate, Forerunner, Small",
                "Crate, Forerunner, Large");
            creepWorld.AddSubcategory("Pallet", "Pallet, Large", "Pallet, Metal");
            creepWorld.AddSubcategory("Driftwood 1", "Driftwood 2", "Driftwood 3");
            creepWorld.AddSubcategory("Phantom", "Spirit", "Pelican", "Drop Pod, Elite", "Anti Air Gun");
            creepWorld.AddSubcategory("Cargo Truck, Destroyed", "Falcon, Destroyed", "Warthog, Destroyed");
            creepWorld.AddNext("Folding Chair");
            creepWorld.AddNext("Dumpster");
            creepWorld.AddNext("Dumpster, Tall");
            creepWorld.AddNext("Equipment Case");
            creepWorld.AddNext("Monitor");
            creepWorld.AddNext("Plasma Storage");
            creepWorld.AddNext("Camping Stool, Covenant");
            creepWorld.AddNext("Covenant Antenna");
            creepWorld.AddNext("Fuel Storage");
            creepWorld.AddNext("Engine Cart");
            creepWorld.AddNext("Missile Cart");
            #endregion
            #region Structure (MCC)
            creepWorld.AddSubcategory(
                "Bridge",
                "Platform, Covenant",
                "Catwalk, Straight",
                "Catwalk, Short",
                "Catwalk, Bend, Left",
                "Catwalk, Bend, Right",
                "Catwalk, Angled",
                "Catwalk, Large");
            creepWorld.AddSubcategory("Bunker, Overlook", "Gunners Nest");
            creepWorld.AddSubcategory(
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
            creepWorld.AddSubcategory(
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
            creepWorld.AddSubcategory("Rock, Spire 3", "Tree, Dead");
            #endregion
            #region Hidden Misc
            creepWorld.AddNext("Generator");
            creepWorld.AddNext("Vending Machine");
            creepWorld.AddNext("Dinghy");
            #endregion
            #region Other (MCC)
            creepWorld.AddNext("Target Designator");// Other (MCC)
            //creepWorld.AddNext("Pelican, Hovering");
            //creepWorld.AddNext("Phantom, Hovering");
            creepWorld.AddNext("Location Name Marker");
            #endregion
            #endregion

            #region Creep World Night
            MapPalette creepWorldNight = new MapPalette(universalTypes.key);
            maps[Map.Creep_Forge_World_Night] = creepWorldNight;
            #region Vehicles
            creepWorldNight.AddNext("Banshee");
            //creepWorldNight.AddNext("Falcon");
            creepWorldNight.AddSubcategory("Falcon", "Falcon, Coaxial Gun / GL", "Falcon, MG", "Falcon, Rockets", "Falcon, Coaxial MG / GL", "Falcon, Cannon");
            creepWorldNight.AddNext("Ghost");
            creepWorldNight.AddNext("Mongoose");
            creepWorldNight.AddNext("Revenant");
            //creepWorldNight.AddNext("Scorpion");
            creepWorldNight.AddSubcategory("Scorpion", "Scorpion, Rockets");
            creepWorldNight.AddNext("Shade Turret");
            creepWorldNight.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            creepWorldNight.AddNext("Wraith");
            creepWorldNight.AddSubcategory("Pickup, Rockets", "Truck, Tank Turret");
            #endregion
            #region Gadgets
            creepWorldNight.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            creepWorldNight.AddNext("Health Station");
            creepWorldNight.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            creepWorldNight.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Vehicle",
                "Gravity Lift");
            creepWorldNight.AddNext("One Way Shield 2");
            creepWorldNight.AddNext("One Way Shield 3");
            creepWorldNight.AddNext("One Way Shield 4");
            creepWorldNight.AddSubcategory(
                "FX:Colorblind",
                "FX:Next Gen",
                "FX:Juicy",
                "FX:Nova",
                "FX:Olde Timey",
                "FX:Pen And Ink",
                "FX:Purple",
                "FX:Green",
                "FX:Orange");
            creepWorldNight.AddNext("Shield Door, Small");
            creepWorldNight.AddNext("Shield Door, Medium");
            creepWorldNight.AddNext("Shield Door, Large");
            creepWorldNight.AddSubcategory("Receiver Node", "Sender Node", "Two-Way Node");
            creepWorldNight.AddSubcategory("Die", "Golf Ball", "Golf Club", "Kill Ball", "Soccer Ball", "Tin Cup");
            creepWorldNight.AddSubcategory(
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
            creepWorldNight.AddNext("Initial Spawn");
            creepWorldNight.AddNext("Respawn Point");
            creepWorldNight.AddNext("Initial Loadout Camera");
            creepWorldNight.AddNext("Respawn Zone");
            creepWorldNight.AddNext("Respawn Zone, Weak");
            creepWorldNight.AddNext("Respawn Zone, Anti");
            creepWorldNight.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            creepWorldNight.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            creepWorldNight.AddNext("Flag Stand");
            creepWorldNight.AddNext("Capture Plate");
            creepWorldNight.AddNext("Hill Marker");
            #endregion
            #region Scenery
            creepWorldNight.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Covenant Barrier",
                "Portable Shield");
            creepWorldNight.AddNext("Camping Stool");
            creepWorldNight.AddSubcategory(
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open");
            creepWorldNight.AddSubcategory(
                "Sandbag Wall",
                "Sandbag, Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            creepWorldNight.AddNext("Street Cone");
            #endregion
            #region Structure
            #region Building Blocks
            creepWorldNight.AddSubcategory(
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
            creepWorldNight.AddSubcategory(
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
            creepWorldNight.AddSubcategory(
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
            creepWorldNight.AddSubcategory(
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
            creepWorldNight.AddSubcategory(
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
            creepWorldNight.AddSubcategory(
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
            creepWorldNight.AddSubcategory(
                "Rock, Small",
                "Rock, Flat",
                "Rock, Medium 1",
                "Rock, Medium 2",
                "Rock, Spire 1",
                "Rock, Spire 2",
                "Rock, Seastack",
                "Rock, Arch");
            #endregion
            creepWorldNight.AddNext("Grid");
            #endregion
            #region Hidden Structure Blocks
            creepWorldNight.AddNext("Block, 2x2, Invisible");
            creepWorldNight.AddNext("Block, 1x1, Invisible");
            creepWorldNight.AddNext("Block, 2x2x2, Invisible");
            creepWorldNight.AddNext("Block, 4x4x2, Invisible");
            creepWorldNight.AddNext("Block, 4x4x4, Invisible");
            creepWorldNight.AddNext("Block, 2x1, Flat, Invisible");
            creepWorldNight.AddNext("Block, 1x1, Flat, Invisible");
            creepWorldNight.AddNext("Block, 1x1, Small, Invisible");
            creepWorldNight.AddNext("Block, 2x2, Flat, Invisible");
            creepWorldNight.AddNext("Block, 4x2, Flat, Invisible");
            creepWorldNight.AddNext("Block, 4x4, Flat, Invisible");
            #endregion
            #region Vehicles (MCC)
            creepWorldNight.AddSubcategory("Falcon, Nose Gun", "Falcon, Grenadier", "Falcon, Transport");
            creepWorldNight.AddNext("Warthog, Transport");
            creepWorldNight.AddNext("Sabre");
            creepWorldNight.AddNext("Seraph");
            creepWorldNight.AddNext("Cart, Electric");
            creepWorldNight.AddNext("Forklift");
            creepWorldNight.AddNext("Pickup");
            creepWorldNight.AddNext("Truck Cab");
            creepWorldNight.AddNext("Van, Oni");
            creepWorldNight.AddNext("Shade, Fuel Rod");
            #endregion
            #region Gadgets (MCC)
            creepWorldNight.AddSubcategory(
                "Cannon, Man, Forerunner",
                "Cannon, Man, Heavy, Forerunner",
                "Cannon, Man, Light, Forerunner",
                "Gravity Lift, Forerunner",
                "Gravity Lift, Tall, Forerunner",
                "Cannon, Man, Human");
            creepWorldNight.AddNext("One Way Shield 1");
            creepWorldNight.AddNext("One Way Shield 5");
            creepWorldNight.AddNext("Shield Wall, Small");
            creepWorldNight.AddNext("Shield Wall, Medium");
            creepWorldNight.AddNext("Shield Wall, Large");
            creepWorldNight.AddNext("Shield Wall, X-Large");
            creepWorldNight.AddNext("One Way Shield 2");
            creepWorldNight.AddNext("One Way Shield 3");
            creepWorldNight.AddNext("One Way Shield 4");
            creepWorldNight.AddNext("Shield Door, Small");
            creepWorldNight.AddNext("Shield Door, Small 1");
            creepWorldNight.AddNext("Shield Door, Large");
            creepWorldNight.AddNext("Shield Door, Large 1");
            creepWorldNight.AddNext("Ammo Cabinet");
            creepWorldNight.AddNext("Spnkr Ammo");
            creepWorldNight.AddNext("Sniper Ammo");
            #endregion
            #region Scenery (MCC)
            creepWorldNight.AddSubcategory("Jersey Barrier", "Jersey Barrier, Short", "Heavy Barrier");
            creepWorldNight.AddSubcategory(
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Fully Open",
                "Crate, Forerunner, Small",
                "Crate, Forerunner, Large");
            creepWorldNight.AddSubcategory("Pallet", "Pallet, Large", "Pallet, Metal");
            creepWorldNight.AddSubcategory("Driftwood 1", "Driftwood 2", "Driftwood 3");
            creepWorldNight.AddSubcategory("Phantom", "Spirit", "Pelican", "Drop Pod, Elite", "Anti Air Gun");
            creepWorldNight.AddSubcategory("Cargo Truck, Destroyed", "Falcon, Destroyed", "Warthog, Destroyed");
            creepWorldNight.AddNext("Folding Chair");
            creepWorldNight.AddNext("Dumpster");
            creepWorldNight.AddNext("Dumpster, Tall");
            creepWorldNight.AddNext("Equipment Case");
            creepWorldNight.AddNext("Monitor");
            creepWorldNight.AddNext("Plasma Storage");
            creepWorldNight.AddNext("Camping Stool, Covenant");
            creepWorldNight.AddNext("Covenant Antenna");
            creepWorldNight.AddNext("Fuel Storage");
            creepWorldNight.AddNext("Engine Cart");
            creepWorldNight.AddNext("Missile Cart");
            #endregion
            #region Structure (MCC)
            creepWorldNight.AddSubcategory(
                "Bridge",
                "Platform, Covenant",
                "Catwalk, Straight",
                "Catwalk, Short",
                "Catwalk, Bend, Left",
                "Catwalk, Bend, Right",
                "Catwalk, Angled",
                "Catwalk, Large");
            creepWorldNight.AddSubcategory("Bunker, Overlook", "Gunners Nest");
            creepWorldNight.AddSubcategory(
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
            creepWorldNight.AddSubcategory(
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
            creepWorldNight.AddSubcategory("Rock, Spire 3", "Tree, Dead");
            #endregion
            #region Hidden Misc
            creepWorldNight.AddNext("Generator");
            creepWorldNight.AddNext("Vending Machine");
            creepWorldNight.AddNext("Dinghy");
            #endregion
            #region Other (MCC)
            creepWorldNight.AddNext("Target Designator");// Other (MCC)
            //creepWorldNight.AddNext("Pelican, Hovering");
            //creepWorldNight.AddNext("Phantom, Hovering");
            creepWorldNight.AddNext("Location Name Marker");
            #endregion
            #endregion

            #region Tempest
            MapPalette tempest = new MapPalette(universalTypes.key);
            maps[Map.Tempest] = tempest;
            #region Vehicles
            tempest.AddNext("Banshee");
            tempest.AddNext("Ghost");
            tempest.AddNext("Mongoose");
            tempest.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            tempest.AddNext("Wraith");
            tempest.AddNext("Scorpion");
            #endregion
            #region Gadgets
            // exact same as FW?!
            tempest.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            tempest.AddNext("Health Station");
            tempest.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            tempest.AddSubcategory(
                "Cannon, Man, Forerunner",
                "Cannon, Man, Heavy, Forerunner",
                "Cannon, Man, Light, Forerunner",
                "Cannon, Vehicle",
                "Gravity Lift");
            tempest.AddNext("One Way Shield 2");
            tempest.AddNext("One Way Shield 3");
            tempest.AddNext("One Way Shield 4");
            tempest.AddSubcategory(
                "FX:Colorblind",
                "FX:Next Gen",
                "FX:Juicy",
                "FX:Nova",
                "FX:Olde Timey",
                "FX:Pen And Ink",
                "FX:Purple",
                "FX:Green",
                "FX:Orange");
            tempest.AddNext("Shield Door, Small");
            tempest.AddNext("Shield Door, Medium");
            tempest.AddNext("Shield Door, Large");
            tempest.AddSubcategory("Receiver Node", "Sender Node", "Two-Way Node");
            tempest.AddSubcategory("Die", "Golf Ball", "Golf Club", "Kill Ball", "Soccer Ball", "Tin Cup");
            tempest.AddSubcategory(
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
            tempest.AddNext("Initial Spawn");
            tempest.AddNext("Respawn Point");
            tempest.AddNext("Initial Loadout Camera");
            tempest.AddNext("Respawn Zone");
            tempest.AddNext("Respawn Zone, Weak");
            tempest.AddNext("Respawn Zone, Anti");
            tempest.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            tempest.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            // same as FW
            tempest.AddNext("Flag Stand");
            tempest.AddNext("Capture Plate");
            tempest.AddNext("Hill Marker");
            #endregion
            #region Scenery
            tempest.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Covenant Barrier");
            tempest.AddSubcategory(
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open");
            tempest.AddSubcategory(
                "Sandbag Wall",
                "Sandbag, Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            tempest.AddNext("Street Cone");
            tempest.AddSubcategory("Driftwood 1", "Driftwood 2", "Driftwood 3");
            #endregion
            #region Structure
            #region Building Blocks
            //FW
            tempest.AddSubcategory(
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
            tempest.AddSubcategory(
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
            tempest.AddSubcategory(
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
            tempest.AddSubcategory(
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
            tempest.AddSubcategory(
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
            tempest.AddSubcategory(
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
            tempest.AddSubcategory(
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
            tempest.AddNext("Grid");
            #endregion
            #region Hidden Structure Blocks
            tempest.AddNext("Block, 2x2, Invisible");
            tempest.AddNext("Block, 1x1, Invisible");
            tempest.AddNext("Block, 2x2x2, Invisible");
            tempest.AddNext("Block, 4x4x2, Invisible");
            tempest.AddNext("Block, 4x4x4, Invisible");
            tempest.AddNext("Block, 2x1, Flat, Invisible");
            tempest.AddNext("Block, 1x1, Flat, Invisible");
            tempest.AddNext("Block, 1x1, Small, Invisible");
            tempest.AddNext("Block, 2x2, Flat, Invisible");
            tempest.AddNext("Block, 4x2, Flat, Invisible");
            tempest.AddNext("Block, 4x4, Flat, Invisible");
            tempest.AddNext("Destination Delta(?!)");
            #endregion
            #region Vehicles (MCC)
            //FW
            tempest.AddSubcategory("Falcon, Nose Gun", "Falcon, Grenadier", "Falcon, Transport");
            tempest.AddNext("Warthog, Transport");
            tempest.AddNext("Sabre");
            tempest.AddNext("Seraph");
            tempest.AddNext("Cart, Electric");
            tempest.AddNext("Forklift");
            tempest.AddNext("Pickup");
            tempest.AddNext("Truck Cab");
            tempest.AddNext("Van, Oni");
            tempest.AddNext("Shade, Fuel Rod");
            #endregion
            #region Gadgets (MCC)
            tempest.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Gravity Lift, Forerunner",
                "Gravity Lift, Tall, Forerunner",
                "Cannon, Man, Human");
            tempest.AddNext("One Way Shield 1");
            tempest.AddNext("One Way Shield 5");
            tempest.AddNext("Shield Wall, Small");
            tempest.AddNext("Shield Wall, Medium");
            tempest.AddNext("Shield Wall, Large");
            tempest.AddNext("Shield Wall, X-Large");
            tempest.AddNext("One Way Shield 2");
            tempest.AddNext("One Way Shield 3");
            tempest.AddNext("One Way Shield 4");
            tempest.AddNext("Shield Door, Small");
            tempest.AddNext("Shield Door, Small 1");
            tempest.AddNext("Shield Door, Large");
            tempest.AddNext("Shield Door, Large 1");
            tempest.AddNext("Ammo Cabinet");
            tempest.AddNext("Spnkr Ammo");
            tempest.AddNext("Sniper Ammo");
            #endregion
            #region Scenery (MCC)
            tempest.AddSubcategory("Jersey Barrier", "Jersey Barrier, Short", "Heavy Barrier");
            tempest.AddSubcategory(
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Fully Open",
                "Crate, Forerunner, Small",
                "Crate, Forerunner, Large");
            tempest.AddSubcategory("Pallet", "Pallet, Large", "Pallet, Metal");
            tempest.AddSubcategory("Phantom", "Spirit", "Pelican", "Drop Pod, Elite", "Anti Air Gun");
            tempest.AddSubcategory("Cargo Truck, Destroyed", "Falcon, Destroyed", "Warthog, Destroyed");
            tempest.AddNext("Folding Chair");
            tempest.AddNext("Dumpster");
            tempest.AddNext("Dumpster, Tall");
            tempest.AddNext("Equipment Case");
            tempest.AddNext("Monitor");
            tempest.AddNext("Plasma Storage");
            tempest.AddNext("Camping Stool, Covenant");
            tempest.AddNext("Covenant Antenna");
            tempest.AddNext("Fuel Storage");
            tempest.AddNext("Engine Cart");
            tempest.AddNext("Missile Cart");
            #endregion
            #region Structure (MCC)
            tempest.AddSubcategory(
                "Bridge",
                "Platform, Covenant",
                "Catwalk, Straight",
                "Catwalk, Short",
                "Catwalk, Bend, Left",
                "Catwalk, Bend, Right",
                "Catwalk, Angled",
                "Catwalk, Large");
            tempest.AddSubcategory("Bunker, Overlook", "Gunners Nest");
            tempest.AddSubcategory(
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
            tempest.AddSubcategory(
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
            tempest.AddNext("Tree, Dead");
            #endregion
            #region Hidden Misc
            tempest.AddNext("Generator");
            tempest.AddNext("Vending Machine");
            tempest.AddNext("Dinghy");
            #endregion
            tempest.AddNext("Target Designator");// Other (MCC)
            tempest.AddNext("Pelican, Hovering");
            tempest.AddNext("Phantom, Hovering");
            #endregion

            #region Ridgeline
            MapPalette ridgeline = new MapPalette(universalTypes.key);
            maps[Map.Ridgeline] = ridgeline;
            #region Gadgets
            ridgeline.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            ridgeline.AddNext("Health Station");
            ridgeline.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            ridgeline.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Vehicle",
                "Gravity Lift");
            ridgeline.AddSubcategory(
                "FX:Colorblind",
                "FX:Next Gen",
                "FX:Juicy",
                "FX:Nova",
                "FX:Olde Timey",
                "FX:Pen And Ink");
            ridgeline.AddSubcategory("Receiver Node", "Sender Node", "Two-Way Node");
            ridgeline.AddSubcategory(
                "Die",
                "Golf Ball",
                "Golf Club",
                "Kill Ball",
                "Soccer Ball",
                "Tin Cup");
            ridgeline.AddSubcategory(
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
            ridgeline.AddNext("Initial Spawn");
            ridgeline.AddNext("Respawn Point");
            ridgeline.AddNext("Initial Loadout Camera");
            ridgeline.AddNext("Respawn Zone");
            ridgeline.AddNext("Respawn Zone, Weak");
            ridgeline.AddNext("Respawn Zone, Anti");
            ridgeline.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            ridgeline.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            ridgeline.AddNext("Flag Stand");
            ridgeline.AddNext("Capture Plate");
            ridgeline.AddNext("Hill Marker");
            #endregion
            #region Scenery
            ridgeline.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Jersey Barrier",
                "Jersey Barrier, Short",
                "Covenant Barrier",
                "Portable Shield");
            ridgeline.AddSubcategory(
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open",
                "Crate, Fully Open");
            ridgeline.AddSubcategory("Pallet", "Pallet, Large", "Pallet, Metal");
            ridgeline.AddSubcategory(
                "Sandbag Wall",
                "Sandbag, Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            #endregion
            #region Vehicles
            ridgeline.AddNext("Banshee");
            ridgeline.AddNext("Falcon");
            ridgeline.AddNext("Ghost");
            ridgeline.AddNext("Mongoose");
            ridgeline.AddNext("Revenant");
            ridgeline.AddNext("Scorpion");
            ridgeline.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            ridgeline.AddNext("Wraith");
            ridgeline.AddNext("Shade Turret");
            #endregion
            #region Structure
            #region Building Blocks
            ridgeline.AddSubcategory(
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
            ridgeline.AddSubcategory(
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
            ridgeline.AddSubcategory(
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
            ridgeline.AddSubcategory(
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
            ridgeline.AddSubcategory(
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
            ridgeline.AddSubcategory(
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
            ridgeline.AddSubcategory(
                "Rock, Small",
                "Rock, Flat",
                "Rock, Medium 1",
                "Rock, Medium 2",
                "Rock, Spire 1",
                "Rock, Spire 2",
                "Rock, Seastack",
                "Rock, Arch");
            #endregion
            ridgeline.AddNext("Grid");
            ridgeline.AddNext("Tree, Dead");
            #endregion
            #region Hidden Structure Blocks
            ridgeline.AddNext("Block, 2x2, Invisible");
            ridgeline.AddNext("Block, 1x1, Invisible");
            ridgeline.AddNext("Block, 2x2x2, Invisible");
            ridgeline.AddNext("Block, 4x4x2, Invisible");
            ridgeline.AddNext("Block, 4x4x4, Invisible");
            ridgeline.AddNext("Block, 2x1, Flat, Invisible");
            ridgeline.AddNext("Block, 1x1, Flat, Invisible");
            ridgeline.AddNext("Block, 1x1, Small, Invisible");
            ridgeline.AddNext("Block, 2x2, Flat, Invisible");
            ridgeline.AddNext("Block, 4x2, Flat, Invisible");
            ridgeline.AddNext("Block, 4x4, Flat, Invisible");
            #endregion
            ridgeline.AddNext("Health Cabinet");
            #endregion

            #region Breakneck
            MapPalette breakneck = new MapPalette(universalTypes.key);
            maps[Map.Breakneck] = breakneck;

            #region Gadgets
            breakneck.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            breakneck.AddNext("Health Station");
            breakneck.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            breakneck.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Man, Human",
                "Cannon, Vehicle",
                "Gravity Lift");
            breakneck.AddSubcategory(
                "FX:Colorblind",
                "FX:Next Gen",
                "FX:Juicy",
                "FX:Nova",
                "FX:Olde Timey",
                "FX:Pen And Ink");
            breakneck.AddSubcategory("Receiver Node", "Sender Node", "Two-Way Node");
            breakneck.AddSubcategory(
                "Die",
                "Golf Ball",
                "Golf Club",
                "Kill Ball",
                "Soccer Ball",
                "Tin Cup");
            breakneck.AddSubcategory(
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
            breakneck.AddNext("Initial Spawn");
            breakneck.AddNext("Respawn Point");
            breakneck.AddNext("Initial Loadout Camera");
            breakneck.AddNext("Respawn Zone");
            breakneck.AddNext("Respawn Zone, Weak");
            breakneck.AddNext("Respawn Zone, Anti");
            breakneck.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            breakneck.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            breakneck.AddNext("Flag Stand");
            breakneck.AddNext("Capture Plate");
            breakneck.AddNext("Hill Marker");
            #endregion
            #region Scenery
            breakneck.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Jersey Barrier",
                "Jersey Barrier, Short",
                "Covenant Barrier",
                "Portable Shield");
            breakneck.AddSubcategory(
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open",
                "Crate, Fully Open");
            breakneck.AddSubcategory(
                "Sandbag Wall",
                "Sandbag, Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            breakneck.AddNext("Street Cone");
            breakneck.AddSubcategory("Pallet", "Pallet, Large", "Pallet, Metal");
            #endregion
            #region Vehicles
            breakneck.AddNext("Banshee");
            breakneck.AddNext("Ghost");
            breakneck.AddNext("Mongoose");
            breakneck.AddNext("Revenant");
            breakneck.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            breakneck.AddNext("Wraith");
            breakneck.AddNext("Falcon");
            breakneck.AddNext("Scorpion");
            #endregion
            #region Structure
            #region Building Blocks
            breakneck.AddSubcategory(
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
            breakneck.AddSubcategory(
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
            breakneck.AddSubcategory(
                "Bunker, Small",
                "Bunker, Small, Covered");
            #endregion
            #region Decorative
            breakneck.AddSubcategory(
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
            breakneck.AddSubcategory(
                "Wall",
                "Wall, Double",
                "Wall, Corner",
                "Wall, Curved",
                "Door, Human");
            #endregion
            #region Inclines
            breakneck.AddSubcategory(
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
            breakneck.AddNext("Grid");
            #endregion
            #region Hidden Structure Blocks
            breakneck.AddNext("Block, 2x2, Invisible");
            breakneck.AddNext("Block, 1x1, Invisible");
            breakneck.AddNext("Block, 2x2x2, Invisible");
            breakneck.AddNext("Block, 4x4x2, Invisible");
            breakneck.AddNext("Block, 4x4x4, Invisible");
            breakneck.AddNext("Block, 2x1, Flat, Invisible");
            breakneck.AddNext("Block, 1x1, Flat, Invisible");
            breakneck.AddNext("Block, 1x1, Small, Invisible");
            breakneck.AddNext("Block, 2x2, Flat, Invisible");
            breakneck.AddNext("Block, 4x2, Flat, Invisible");
            breakneck.AddNext("Block, 4x4, Flat, Invisible");
            #endregion
            breakneck.AddNext("Health Cabinet");
            #region Vehicles (MCC)
            breakneck.AddSubcategory("Falcon, Nose Gun", "Falcon, Grenadier", "Falcon, Transport");
            breakneck.AddNext("Warthog, Transport");
            breakneck.AddNext("Cart, Electric");
            breakneck.AddNext("Forklift");
            breakneck.AddNext("Pickup");
            breakneck.AddNext("Truck Cab");
            breakneck.AddNext("Van, Oni");
            breakneck.AddNext("Shade, Fuel Rod");
            #endregion
            #region Gadgets (MCC)
            breakneck.AddNext("One Way Shield 1");
            breakneck.AddNext("One Way Shield 2");
            breakneck.AddNext("One Way Shield 3");
            breakneck.AddNext("One Way Shield 4");
            breakneck.AddNext("One Way Shield 5");
            breakneck.AddNext("Shield Wall, Small");
            breakneck.AddNext("Shield Wall, Medium");
            breakneck.AddNext("Shield Wall, Large");
            breakneck.AddNext("Shield Wall, X-Large");
            breakneck.AddNext("Ammo Cabinet");
            breakneck.AddNext("Spnkr Ammo");
            breakneck.AddNext("Sniper Ammo");
            #endregion
            #region Scenery (MCC)
            breakneck.AddNext("Heavy Barrier");
            breakneck.AddSubcategory("Phantom", "Spirit", "Pelican", "Drop Pod, Elite", "Anti Air Gun");
            breakneck.AddSubcategory("Cargo Truck, Destroyed", "Falcon, Destroyed", "Warthog, Destroyed");
            breakneck.AddNext("Folding Chair");
            breakneck.AddNext("Dumpster");
            breakneck.AddNext("Dumpster, Tall");
            breakneck.AddNext("Equipment Case");
            breakneck.AddNext("Monitor");
            breakneck.AddNext("Plasma Storage");
            breakneck.AddNext("Camping Stool, Covenant");
            breakneck.AddNext("Covenant Antenna");
            breakneck.AddNext("Fuel Storage");
            breakneck.AddNext("Engine Cart");
            breakneck.AddNext("Missile Cart");
            #endregion
            #region Structure (MCC)
            breakneck.AddSubcategory("Wall (MCC)", "Door (MCC)");
            #endregion
            #endregion

            #region Highlands
            MapPalette highlands = new MapPalette(universalTypes.key);
            maps[Map.Highlands] = highlands;

            #region Vehicles
            highlands.AddNext("Banshee");
            highlands.AddNext("Falcon");
            highlands.AddNext("Ghost");
            highlands.AddNext("Mongoose");
            highlands.AddNext("Revenant");
            highlands.AddNext("Scorpion");
            highlands.AddNext("Shade Turret");
            highlands.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            highlands.AddNext("Wraith");
            #endregion
            #region Gadgets
            highlands.AddSubcategory("Fusion Coil", "Landmine");
            highlands.AddNext("Health Station");
            highlands.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            highlands.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Vehicle",
                "Gravity Lift");
            highlands.AddSubcategory("Receiver Node", "Sender Node", "Two-Way Node");
            highlands.AddSubcategory("Die", "Golf Ball", "Golf Club", "Kill Ball", "Soccer Ball", "Tin Cup");
            highlands.AddSubcategory("Light, Red", "Light, Blue", "Light, Green", "Light, Orange");
            #endregion
            #region Spawning
            highlands.AddNext("Initial Spawn");
            highlands.AddNext("Respawn Point");
            highlands.AddNext("Initial Loadout Camera");
            highlands.AddNext("Respawn Zone");
            highlands.AddNext("Respawn Zone, Weak");
            highlands.AddNext("Respawn Zone, Anti");
            highlands.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            highlands.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            highlands.AddNext("Flag Stand");
            highlands.AddNext("Capture Plate");
            highlands.AddNext("Hill Marker");
            #endregion
            #region Scenery
            highlands.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Jersey Barrier",
                "Jersey Barrier, Short",
                "Covenant Barrier",
                "Portable Shield");
            highlands.AddNext("Camping Stool");
            highlands.AddNext("Folding Chair");
            highlands.AddSubcategory(
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open",
                "Crate, Fully Open");
            highlands.AddNext("Dumpster, Tall");
            highlands.AddSubcategory(
                "Sandbag Wall",
                "Sandbag Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            highlands.AddNext("Street Cone");
            highlands.AddSubcategory("Pallet", "Pallet, Large", "Pallet, Metal");
            #endregion
            #region Hidden Structure Blocks
            highlands.AddNext("Block, 2x2, Invisible");
            highlands.AddNext("Block, 1x1, Invisible");
            highlands.AddNext("Block, 2x2x2, Invisible");
            highlands.AddNext("Block, 4x4x2, Invisible");
            highlands.AddNext("Block, 4x4x4, Invisible");
            highlands.AddNext("Block, 2x1, Flat, Invisible");
            highlands.AddNext("Block, 1x1, Flat, Invisible");
            highlands.AddNext("Block, 1x1, Small, Invisible");
            highlands.AddNext("Block, 2x2, Flat, Invisible");
            highlands.AddNext("Block, 4x2, Flat, Invisible");
            highlands.AddNext("Block, 4x4, Flat, Invisible");
            #endregion
            #endregion

            #region Reflection
            MapPalette reflection = new MapPalette(universalTypes.key);
            maps[Map.Reflection] = reflection;

            #region Gadgets
            reflection.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            reflection.AddNext("Health Station");
            reflection.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            reflection.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Vehicle",
                "Gravity Lift");
            reflection.AddSubcategory(
                "FX:Colorblind",
                "FX:Next Gen",
                "FX:Juicy",
                "FX:Nova",
                "FX:Olde Timey",
                "FX:Pen And Ink");
            reflection.AddSubcategory("Receiver Node", "Sender Node", "Two-Way Node");
            reflection.AddSubcategory("Die", "Golf Ball", "Golf Club", "Kill Ball", "Soccer Ball");
            reflection.AddSubcategory(
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
            reflection.AddNext("Initial Spawn");
            reflection.AddNext("Respawn Point");
            reflection.AddNext("Initial Loadout Camera");
            reflection.AddNext("Respawn Zone");
            reflection.AddNext("Respawn Zone, Weak");
            reflection.AddNext("Respawn Zone, Anti");
            reflection.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            reflection.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            reflection.AddNext("Flag Stand");
            reflection.AddNext("Capture Plate");
            reflection.AddNext("Hill Marker");
            #endregion
            #region Scenery
            reflection.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Jersey Barrier",
                "Jersey Barrier, Short",
                "Covenant Barrier",
                "Portable Shield");
            reflection.AddNext("Camping Stool");
            reflection.AddNext("Folding Chair");
            reflection.AddSubcategory(
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open",
                "Crate, Fully Open");
            reflection.AddNext("Dumpster, Tall");
            reflection.AddSubcategory(
                "Sandbag Wall",
                "Sandbag Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            reflection.AddNext("Street Cone");
            reflection.AddSubcategory("Pallet", "Pallet, Large", "Pallet, Metal");
            #endregion
            #region Hidden Structure Blocks
            reflection.AddNext("Block, 2x2, Invisible");
            reflection.AddNext("Block, 1x1, Invisible");
            reflection.AddNext("Block, 2x2x2, Invisible");
            reflection.AddNext("Block, 4x4x2, Invisible");
            reflection.AddNext("Block, 4x4x4, Invisible");
            reflection.AddNext("Block, 2x1, Flat, Invisible");
            reflection.AddNext("Block, 1x1, Flat, Invisible");
            reflection.AddNext("Block, 1x1, Small, Invisible");
            reflection.AddNext("Block, 2x2, Flat, Invisible");
            reflection.AddNext("Block, 4x2, Flat, Invisible");
            reflection.AddNext("Block, 4x4, Flat, Invisible");
            #endregion
            #endregion

            #region Sword Base
            MapPalette sword_base = new MapPalette(universalTypes.key);
            maps[Map.Sword_Base] = sword_base;

            #region Gadgets
            sword_base.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            sword_base.AddNext("Health Station");
            sword_base.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            sword_base.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Vehicle",
                "Gravity Lift");
            sword_base.AddSubcategory(
                "FX:Colorblind",
                "FX:Next Gen",
                "FX:Juicy",
                "FX:Nova",
                "FX:Olde Timey",
                "FX:Pen And Ink");
            sword_base.AddSubcategory("Receiver Node", "Sender Node", "Two-Way Node");
            sword_base.AddSubcategory("Die", "Golf Ball", "Golf Club", "Kill Ball", "Soccer Ball");
            sword_base.AddSubcategory(
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
            sword_base.AddNext("Initial Spawn");
            sword_base.AddNext("Respawn Point");
            sword_base.AddNext("Initial Loadout Camera");
            sword_base.AddNext("Respawn Zone");
            sword_base.AddNext("Respawn Zone, Weak");
            sword_base.AddNext("Respawn Zone, Anti");
            sword_base.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            sword_base.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            sword_base.AddNext("Flag Stand");
            sword_base.AddNext("Capture Plate");
            sword_base.AddNext("Hill Marker");
            #endregion
            #region Scenery
            sword_base.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Jersey Barrier",
                "Jersey Barrier, Short",
                "Covenant Barrier",
                "Portable Shield");
            sword_base.AddNext("Camping Stool");
            sword_base.AddNext("Folding Chair");
            sword_base.AddSubcategory(
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open",
                "Crate, Fully Open");
            sword_base.AddSubcategory(
                "Sandbag Wall",
                "Sandbag Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            sword_base.AddNext("Street Cone");
            sword_base.AddSubcategory("Pallet", "Pallet, Large", "Pallet, Metal");
            #endregion
            #region Hidden Structure Blocks
            sword_base.AddNext("Block, 2x2, Invisible");
            sword_base.AddNext("Block, 1x1, Invisible");
            sword_base.AddNext("Block, 2x2x2, Invisible");
            sword_base.AddNext("Block, 4x4x2, Invisible");
            sword_base.AddNext("Block, 4x4x4, Invisible");
            sword_base.AddNext("Block, 2x1, Flat, Invisible");
            sword_base.AddNext("Block, 1x1, Flat, Invisible");
            sword_base.AddNext("Block, 1x1, Small, Invisible");
            sword_base.AddNext("Block, 2x2, Flat, Invisible");
            sword_base.AddNext("Block, 4x2, Flat, Invisible");
            sword_base.AddNext("Block, 4x4, Flat, Invisible");
            #endregion
            #endregion
        }

        public static bool TryTypeToName(int type, Map map, out string name) {
            if (universalTypes.TryGetValue(type, out name)) return true;
            if (maps.TryGetValue(map, out MapPalette mapTypes) && mapTypes.TryGetValue(type, out name)) return true;

            return false;
        }

        public static bool TryNameToType(string name, Map map, out int type) {
            if (universalTypes.TryGetValue(name, out type)) return true;
            if (maps.TryGetValue(map, out MapPalette mapTypes) && mapTypes.TryGetValue(name, out type)) return true;

            return false;
        }
    }
}

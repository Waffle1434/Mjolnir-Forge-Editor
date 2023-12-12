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

            public void AddItem(string name) {
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
            universalTypes.AddItem("Assault Rifle");
            universalTypes.AddItem("DMR");
            universalTypes.AddItem("Grenade Launcher");
            universalTypes.AddItem("Magnum");
            universalTypes.AddItem("Rocket Launcher");
            universalTypes.AddItem("Shotgun");
            universalTypes.AddItem("Sniper Rifle");
            universalTypes.AddItem("Spartan Laser");
            universalTypes.AddItem("Frag Grenade");
            universalTypes.AddItem("Mounted Machinegun");
            #endregion
            #region Weapons Covenant
            universalTypes.AddItem("Concussion Rifle");
            universalTypes.AddItem("Energy Sword");
            universalTypes.AddItem("Fuel Rod Gun");
            universalTypes.AddItem("Gravity Hammer");
            universalTypes.AddItem("Focus Rifle");
            universalTypes.AddItem("Needle Rifle");
            universalTypes.AddItem("Needler");
            universalTypes.AddItem("Plasma Launcher");
            universalTypes.AddItem("Plasma Pistol");
            universalTypes.AddItem("Plasma Repeater");
            universalTypes.AddItem("Plasma Rifle");
            universalTypes.AddItem("Spiker");
            universalTypes.AddItem("Plasma Grenade");
            universalTypes.AddItem("Plasma Turret");
            #endregion
            #region Armor Abilities
            universalTypes.AddItem("Active Camouflage");
            universalTypes.AddItem("Armor Lock");
            universalTypes.AddItem("Drop Shield");
            universalTypes.AddItem("Evade");
            universalTypes.AddItem("Hologram");
            universalTypes.AddItem("Jet Pack");
            universalTypes.AddItem("Sprint");
            #endregion
            #endregion

            #region Forge World
            MapPalette forgeWorld = new MapPalette(universalTypes.key);
            maps[Map.Forge_World] = forgeWorld;
            #region Vehicles
            forgeWorld.AddItem("Banshee");
            forgeWorld.AddItem("Falcon");
            forgeWorld.AddItem("Ghost");
            forgeWorld.AddItem("Mongoose");
            forgeWorld.AddItem("Revenant");
            forgeWorld.AddItem("Scorpion");
            forgeWorld.AddItem("Shade Turret");
            forgeWorld.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            forgeWorld.AddItem("Wraith");
            #endregion
            #region Gadgets
            forgeWorld.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            forgeWorld.AddItem("Health Station");
            forgeWorld.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            forgeWorld.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Vehicle",
                "Gravity Lift");
            forgeWorld.AddItem("One Way Shield 2");
            forgeWorld.AddItem("One Way Shield 3");
            forgeWorld.AddItem("One Way Shield 4");
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
            forgeWorld.AddItem("Shield Door, Small");
            forgeWorld.AddItem("Shield Door, Medium");
            forgeWorld.AddItem("Shield Door, Large");
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
            forgeWorld.AddItem("Initial Spawn");
            forgeWorld.AddItem("Respawn Point");
            forgeWorld.AddItem("Initial Loadout Camera");
            forgeWorld.AddItem("Respawn Zone");
            forgeWorld.AddItem("Respawn Zone, Weak");
            forgeWorld.AddItem("Respawn Zone, Anti");
            forgeWorld.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            forgeWorld.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            forgeWorld.AddItem("Flag Stand");
            forgeWorld.AddItem("Capture Plate");
            forgeWorld.AddItem("Hill Marker");
            #endregion
            #region Scenery
            forgeWorld.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Covenant Barrier",
                "Portable Shield");
            forgeWorld.AddItem("Camping Stool");
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
            forgeWorld.AddItem("Street Cone");
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
            forgeWorld.AddItem("Grid");
            #endregion
            #region Hidden Structure Blocks
            forgeWorld.AddItem("Block, 2x2, Invisible");
            forgeWorld.AddItem("Block, 1x1, Invisible");
            forgeWorld.AddItem("Block, 2x2x2, Invisible");
            forgeWorld.AddItem("Block, 4x4x2, Invisible");
            forgeWorld.AddItem("Block, 4x4x4, Invisible");
            forgeWorld.AddItem("Block, 2x1, Flat, Invisible");
            forgeWorld.AddItem("Block, 1x1, Flat, Invisible");
            forgeWorld.AddItem("Block, 1x1, Small, Invisible");
            forgeWorld.AddItem("Block, 2x2, Flat, Invisible");
            forgeWorld.AddItem("Block, 4x2, Flat, Invisible");
            forgeWorld.AddItem("Block, 4x4, Flat, Invisible");
            #endregion
            #region Vehicles (MCC)
            forgeWorld.AddSubcategory("Falcon, Nose Gun", "Falcon, Grenadier", "Falcon, Transport");
            forgeWorld.AddItem("Warthog, Transport");
            forgeWorld.AddItem("Sabre");
            forgeWorld.AddItem("Seraph");
            forgeWorld.AddItem("Cart, Electric");
            forgeWorld.AddItem("Forklift");
            forgeWorld.AddItem("Pickup");
            forgeWorld.AddItem("Truck Cab");
            forgeWorld.AddItem("Van, Oni");
            forgeWorld.AddItem("Shade, Fuel Rod");
            #endregion
            #region Gadgets (MCC)
            forgeWorld.AddSubcategory(
                "Cannon, Man, Forerunner",
                "Cannon, Man, Heavy, Forerunner",
                "Cannon, Man, Light, Forerunner",
                "Gravity Lift, Forerunner",
                "Gravity Lift, Tall, Forerunner",
                "Cannon, Man, Human");
            forgeWorld.AddItem("One Way Shield 1");
            forgeWorld.AddItem("One Way Shield 5");
            forgeWorld.AddItem("Shield Wall, Small");
            forgeWorld.AddItem("Shield Wall, Medium");
            forgeWorld.AddItem("Shield Wall, Large");
            forgeWorld.AddItem("Shield Wall, X-Large");
            forgeWorld.AddItem("One Way Shield 2");
            forgeWorld.AddItem("One Way Shield 3");
            forgeWorld.AddItem("One Way Shield 4");
            forgeWorld.AddItem("Shield Door, Small");
            forgeWorld.AddItem("Shield Door, Small 1");
            forgeWorld.AddItem("Shield Door, Large");
            forgeWorld.AddItem("Shield Door, Large 1");
            forgeWorld.AddItem("Ammo Cabinet");
            forgeWorld.AddItem("Spnkr Ammo");
            forgeWorld.AddItem("Sniper Ammo");
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
            forgeWorld.AddItem("Folding Chair");
            forgeWorld.AddItem("Dumpster");
            forgeWorld.AddItem("Dumpster, Tall");
            forgeWorld.AddItem("Equipment Case");
            forgeWorld.AddItem("Monitor");
            forgeWorld.AddItem("Plasma Storage");
            forgeWorld.AddItem("Camping Stool, Covenant");
            forgeWorld.AddItem("Covenant Antenna");
            forgeWorld.AddItem("Fuel Storage");
            forgeWorld.AddItem("Engine Cart");
            forgeWorld.AddItem("Missile Cart");
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
            forgeWorld.AddItem("Generator");
            forgeWorld.AddItem("Vending Machine");
            forgeWorld.AddItem("Dinghy");
            #endregion
            forgeWorld.AddItem("Target Designator");// Other (MCC)
            forgeWorld.AddItem("Pelican, Hovering");
            forgeWorld.AddItem("Phantom, Hovering");
            #endregion

            #region Creep World
            MapPalette creepWorld = new MapPalette(universalTypes.key);
            maps[Map.Creep_Forge_World] = creepWorld;
            #region Vehicles
            creepWorld.AddItem("Banshee");
            //creepWorld.AddNext("Falcon");
            creepWorld.AddSubcategory("Falcon", "Falcon, Coaxial Gun / GL", "Falcon, MG", "Falcon, Rockets", "Falcon, Coaxial MG / GL", "Falcon, Cannon");
            creepWorld.AddItem("Ghost");
            creepWorld.AddItem("Mongoose");
            creepWorld.AddItem("Revenant");
            //creepWorld.AddNext("Scorpion");
            creepWorld.AddSubcategory("Scorpion", "Scorpion, Rockets");
            creepWorld.AddItem("Shade Turret");
            creepWorld.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            creepWorld.AddItem("Wraith");
            creepWorld.AddSubcategory("Pickup, Rockets", "Truck, Tank Turret");
            #endregion
            #region Gadgets
            creepWorld.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            creepWorld.AddItem("Health Station");
            creepWorld.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            creepWorld.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Vehicle",
                "Gravity Lift");
            creepWorld.AddItem("One Way Shield 2");
            creepWorld.AddItem("One Way Shield 3");
            creepWorld.AddItem("One Way Shield 4");
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
            creepWorld.AddItem("Shield Door, Small");
            creepWorld.AddItem("Shield Door, Medium");
            creepWorld.AddItem("Shield Door, Large");
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
            creepWorld.AddItem("Initial Spawn");
            creepWorld.AddItem("Respawn Point");
            creepWorld.AddItem("Initial Loadout Camera");
            creepWorld.AddItem("Respawn Zone");
            creepWorld.AddItem("Respawn Zone, Weak");
            creepWorld.AddItem("Respawn Zone, Anti");
            creepWorld.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            creepWorld.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            creepWorld.AddItem("Flag Stand");
            creepWorld.AddItem("Capture Plate");
            creepWorld.AddItem("Hill Marker");
            #endregion
            #region Scenery
            creepWorld.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Covenant Barrier",
                "Portable Shield");
            creepWorld.AddItem("Camping Stool");
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
            creepWorld.AddItem("Street Cone");
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
            creepWorld.AddItem("Grid");
            #endregion
            #region Hidden Structure Blocks
            creepWorld.AddItem("Block, 2x2, Invisible");
            creepWorld.AddItem("Block, 1x1, Invisible");
            creepWorld.AddItem("Block, 2x2x2, Invisible");
            creepWorld.AddItem("Block, 4x4x2, Invisible");
            creepWorld.AddItem("Block, 4x4x4, Invisible");
            creepWorld.AddItem("Block, 2x1, Flat, Invisible");
            creepWorld.AddItem("Block, 1x1, Flat, Invisible");
            creepWorld.AddItem("Block, 1x1, Small, Invisible");
            creepWorld.AddItem("Block, 2x2, Flat, Invisible");
            creepWorld.AddItem("Block, 4x2, Flat, Invisible");
            creepWorld.AddItem("Block, 4x4, Flat, Invisible");
            #endregion
            #region Vehicles (MCC)
            creepWorld.AddSubcategory("Falcon, Nose Gun", "Falcon, Grenadier", "Falcon, Transport");
            creepWorld.AddItem("Warthog, Transport");
            creepWorld.AddItem("Sabre");
            creepWorld.AddItem("Seraph");
            creepWorld.AddItem("Cart, Electric");
            creepWorld.AddItem("Forklift");
            creepWorld.AddItem("Pickup");
            creepWorld.AddItem("Truck Cab");
            creepWorld.AddItem("Van, Oni");
            creepWorld.AddItem("Shade, Fuel Rod");
            #endregion
            #region Gadgets (MCC)
            creepWorld.AddSubcategory(
                "Cannon, Man, Forerunner",
                "Cannon, Man, Heavy, Forerunner",
                "Cannon, Man, Light, Forerunner",
                "Gravity Lift, Forerunner",
                "Gravity Lift, Tall, Forerunner",
                "Cannon, Man, Human");
            creepWorld.AddItem("One Way Shield 1");
            creepWorld.AddItem("One Way Shield 5");
            creepWorld.AddItem("Shield Wall, Small");
            creepWorld.AddItem("Shield Wall, Medium");
            creepWorld.AddItem("Shield Wall, Large");
            creepWorld.AddItem("Shield Wall, X-Large");
            creepWorld.AddItem("One Way Shield 2");
            creepWorld.AddItem("One Way Shield 3");
            creepWorld.AddItem("One Way Shield 4");
            creepWorld.AddItem("Shield Door, Small");
            creepWorld.AddItem("Shield Door, Small 1");
            creepWorld.AddItem("Shield Door, Large");
            creepWorld.AddItem("Shield Door, Large 1");
            creepWorld.AddItem("Ammo Cabinet");
            creepWorld.AddItem("Spnkr Ammo");
            creepWorld.AddItem("Sniper Ammo");
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
            creepWorld.AddItem("Folding Chair");
            creepWorld.AddItem("Dumpster");
            creepWorld.AddItem("Dumpster, Tall");
            creepWorld.AddItem("Equipment Case");
            creepWorld.AddItem("Monitor");
            creepWorld.AddItem("Plasma Storage");
            creepWorld.AddItem("Camping Stool, Covenant");
            creepWorld.AddItem("Covenant Antenna");
            creepWorld.AddItem("Fuel Storage");
            creepWorld.AddItem("Engine Cart");
            creepWorld.AddItem("Missile Cart");
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
            creepWorld.AddItem("Generator");
            creepWorld.AddItem("Vending Machine");
            creepWorld.AddItem("Dinghy");
            #endregion
            #region Other (MCC)
            creepWorld.AddItem("Target Designator");// Other (MCC)
            //creepWorld.AddNext("Pelican, Hovering");
            //creepWorld.AddNext("Phantom, Hovering");
            creepWorld.AddItem("Location Name Marker");
            #endregion
            #endregion

            #region Creep World Night
            MapPalette creepWorldNight = new MapPalette(universalTypes.key);
            maps[Map.Creep_Forge_World_Night] = creepWorldNight;
            #region Vehicles
            creepWorldNight.AddItem("Banshee");
            //creepWorldNight.AddNext("Falcon");
            creepWorldNight.AddSubcategory("Falcon", "Falcon, Coaxial Gun / GL", "Falcon, MG", "Falcon, Rockets", "Falcon, Coaxial MG / GL", "Falcon, Cannon");
            creepWorldNight.AddItem("Ghost");
            creepWorldNight.AddItem("Mongoose");
            creepWorldNight.AddItem("Revenant");
            //creepWorldNight.AddNext("Scorpion");
            creepWorldNight.AddSubcategory("Scorpion", "Scorpion, Rockets");
            creepWorldNight.AddItem("Shade Turret");
            creepWorldNight.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            creepWorldNight.AddItem("Wraith");
            creepWorldNight.AddSubcategory("Pickup, Rockets", "Truck, Tank Turret");
            #endregion
            #region Gadgets
            creepWorldNight.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            creepWorldNight.AddItem("Health Station");
            creepWorldNight.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            creepWorldNight.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Vehicle",
                "Gravity Lift");
            creepWorldNight.AddItem("One Way Shield 2");
            creepWorldNight.AddItem("One Way Shield 3");
            creepWorldNight.AddItem("One Way Shield 4");
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
            creepWorldNight.AddItem("Shield Door, Small");
            creepWorldNight.AddItem("Shield Door, Medium");
            creepWorldNight.AddItem("Shield Door, Large");
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
            creepWorldNight.AddItem("Initial Spawn");
            creepWorldNight.AddItem("Respawn Point");
            creepWorldNight.AddItem("Initial Loadout Camera");
            creepWorldNight.AddItem("Respawn Zone");
            creepWorldNight.AddItem("Respawn Zone, Weak");
            creepWorldNight.AddItem("Respawn Zone, Anti");
            creepWorldNight.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            creepWorldNight.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            creepWorldNight.AddItem("Flag Stand");
            creepWorldNight.AddItem("Capture Plate");
            creepWorldNight.AddItem("Hill Marker");
            #endregion
            #region Scenery
            creepWorldNight.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Covenant Barrier",
                "Portable Shield");
            creepWorldNight.AddItem("Camping Stool");
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
            creepWorldNight.AddItem("Street Cone");
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
            creepWorldNight.AddItem("Grid");
            #endregion
            #region Hidden Structure Blocks
            creepWorldNight.AddItem("Block, 2x2, Invisible");
            creepWorldNight.AddItem("Block, 1x1, Invisible");
            creepWorldNight.AddItem("Block, 2x2x2, Invisible");
            creepWorldNight.AddItem("Block, 4x4x2, Invisible");
            creepWorldNight.AddItem("Block, 4x4x4, Invisible");
            creepWorldNight.AddItem("Block, 2x1, Flat, Invisible");
            creepWorldNight.AddItem("Block, 1x1, Flat, Invisible");
            creepWorldNight.AddItem("Block, 1x1, Small, Invisible");
            creepWorldNight.AddItem("Block, 2x2, Flat, Invisible");
            creepWorldNight.AddItem("Block, 4x2, Flat, Invisible");
            creepWorldNight.AddItem("Block, 4x4, Flat, Invisible");
            #endregion
            #region Vehicles (MCC)
            creepWorldNight.AddSubcategory("Falcon, Nose Gun", "Falcon, Grenadier", "Falcon, Transport");
            creepWorldNight.AddItem("Warthog, Transport");
            creepWorldNight.AddItem("Sabre");
            creepWorldNight.AddItem("Seraph");
            creepWorldNight.AddItem("Cart, Electric");
            creepWorldNight.AddItem("Forklift");
            creepWorldNight.AddItem("Pickup");
            creepWorldNight.AddItem("Truck Cab");
            creepWorldNight.AddItem("Van, Oni");
            creepWorldNight.AddItem("Shade, Fuel Rod");
            #endregion
            #region Gadgets (MCC)
            creepWorldNight.AddSubcategory(
                "Cannon, Man, Forerunner",
                "Cannon, Man, Heavy, Forerunner",
                "Cannon, Man, Light, Forerunner",
                "Gravity Lift, Forerunner",
                "Gravity Lift, Tall, Forerunner",
                "Cannon, Man, Human");
            creepWorldNight.AddItem("One Way Shield 1");
            creepWorldNight.AddItem("One Way Shield 5");
            creepWorldNight.AddItem("Shield Wall, Small");
            creepWorldNight.AddItem("Shield Wall, Medium");
            creepWorldNight.AddItem("Shield Wall, Large");
            creepWorldNight.AddItem("Shield Wall, X-Large");
            creepWorldNight.AddItem("One Way Shield 2");
            creepWorldNight.AddItem("One Way Shield 3");
            creepWorldNight.AddItem("One Way Shield 4");
            creepWorldNight.AddItem("Shield Door, Small");
            creepWorldNight.AddItem("Shield Door, Small 1");
            creepWorldNight.AddItem("Shield Door, Large");
            creepWorldNight.AddItem("Shield Door, Large 1");
            creepWorldNight.AddItem("Ammo Cabinet");
            creepWorldNight.AddItem("Spnkr Ammo");
            creepWorldNight.AddItem("Sniper Ammo");
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
            creepWorldNight.AddItem("Folding Chair");
            creepWorldNight.AddItem("Dumpster");
            creepWorldNight.AddItem("Dumpster, Tall");
            creepWorldNight.AddItem("Equipment Case");
            creepWorldNight.AddItem("Monitor");
            creepWorldNight.AddItem("Plasma Storage");
            creepWorldNight.AddItem("Camping Stool, Covenant");
            creepWorldNight.AddItem("Covenant Antenna");
            creepWorldNight.AddItem("Fuel Storage");
            creepWorldNight.AddItem("Engine Cart");
            creepWorldNight.AddItem("Missile Cart");
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
            creepWorldNight.AddItem("Generator");
            creepWorldNight.AddItem("Vending Machine");
            creepWorldNight.AddItem("Dinghy");
            #endregion
            #region Other (MCC)
            creepWorldNight.AddItem("Target Designator");// Other (MCC)
            //creepWorldNight.AddNext("Pelican, Hovering");
            //creepWorldNight.AddNext("Phantom, Hovering");
            creepWorldNight.AddItem("Location Name Marker");
            #endregion
            #endregion

            #region Tempest
            MapPalette tempest = new MapPalette(universalTypes.key);
            maps[Map.Tempest] = tempest;
            #region Vehicles
            tempest.AddItem("Banshee");
            tempest.AddItem("Ghost");
            tempest.AddItem("Mongoose");
            tempest.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            tempest.AddItem("Wraith");
            tempest.AddItem("Scorpion");
            #endregion
            #region Gadgets
            // exact same as FW?!
            tempest.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            tempest.AddItem("Health Station");
            tempest.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            tempest.AddSubcategory(
                "Cannon, Man, Forerunner",
                "Cannon, Man, Heavy, Forerunner",
                "Cannon, Man, Light, Forerunner",
                "Cannon, Vehicle",
                "Gravity Lift");
            tempest.AddItem("One Way Shield 2");
            tempest.AddItem("One Way Shield 3");
            tempest.AddItem("One Way Shield 4");
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
            tempest.AddItem("Shield Door, Small");
            tempest.AddItem("Shield Door, Medium");
            tempest.AddItem("Shield Door, Large");
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
            tempest.AddItem("Initial Spawn");
            tempest.AddItem("Respawn Point");
            tempest.AddItem("Initial Loadout Camera");
            tempest.AddItem("Respawn Zone");
            tempest.AddItem("Respawn Zone, Weak");
            tempest.AddItem("Respawn Zone, Anti");
            tempest.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            tempest.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            // same as FW
            tempest.AddItem("Flag Stand");
            tempest.AddItem("Capture Plate");
            tempest.AddItem("Hill Marker");
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
            tempest.AddItem("Street Cone");
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
            tempest.AddItem("Grid");
            #endregion
            #region Hidden Structure Blocks
            tempest.AddItem("Block, 2x2, Invisible");
            tempest.AddItem("Block, 1x1, Invisible");
            tempest.AddItem("Block, 2x2x2, Invisible");
            tempest.AddItem("Block, 4x4x2, Invisible");
            tempest.AddItem("Block, 4x4x4, Invisible");
            tempest.AddItem("Block, 2x1, Flat, Invisible");
            tempest.AddItem("Block, 1x1, Flat, Invisible");
            tempest.AddItem("Block, 1x1, Small, Invisible");
            tempest.AddItem("Block, 2x2, Flat, Invisible");
            tempest.AddItem("Block, 4x2, Flat, Invisible");
            tempest.AddItem("Block, 4x4, Flat, Invisible");
            tempest.AddItem("Destination Delta(?!)");
            #endregion
            #region Vehicles (MCC)
            //FW
            tempest.AddSubcategory("Falcon, Nose Gun", "Falcon, Grenadier", "Falcon, Transport");
            tempest.AddItem("Warthog, Transport");
            tempest.AddItem("Sabre");
            tempest.AddItem("Seraph");
            tempest.AddItem("Cart, Electric");
            tempest.AddItem("Forklift");
            tempest.AddItem("Pickup");
            tempest.AddItem("Truck Cab");
            tempest.AddItem("Van, Oni");
            tempest.AddItem("Shade, Fuel Rod");
            #endregion
            #region Gadgets (MCC)
            tempest.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Gravity Lift, Forerunner",
                "Gravity Lift, Tall, Forerunner",
                "Cannon, Man, Human");
            tempest.AddItem("One Way Shield 1");
            tempest.AddItem("One Way Shield 5");
            tempest.AddItem("Shield Wall, Small");
            tempest.AddItem("Shield Wall, Medium");
            tempest.AddItem("Shield Wall, Large");
            tempest.AddItem("Shield Wall, X-Large");
            tempest.AddItem("One Way Shield 2");
            tempest.AddItem("One Way Shield 3");
            tempest.AddItem("One Way Shield 4");
            tempest.AddItem("Shield Door, Small");
            tempest.AddItem("Shield Door, Small 1");
            tempest.AddItem("Shield Door, Large");
            tempest.AddItem("Shield Door, Large 1");
            tempest.AddItem("Ammo Cabinet");
            tempest.AddItem("Spnkr Ammo");
            tempest.AddItem("Sniper Ammo");
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
            tempest.AddItem("Folding Chair");
            tempest.AddItem("Dumpster");
            tempest.AddItem("Dumpster, Tall");
            tempest.AddItem("Equipment Case");
            tempest.AddItem("Monitor");
            tempest.AddItem("Plasma Storage");
            tempest.AddItem("Camping Stool, Covenant");
            tempest.AddItem("Covenant Antenna");
            tempest.AddItem("Fuel Storage");
            tempest.AddItem("Engine Cart");
            tempest.AddItem("Missile Cart");
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
            tempest.AddItem("Tree, Dead");
            #endregion
            #region Hidden Misc
            tempest.AddItem("Generator");
            tempest.AddItem("Vending Machine");
            tempest.AddItem("Dinghy");
            #endregion
            tempest.AddItem("Target Designator");// Other (MCC)
            tempest.AddItem("Pelican, Hovering");
            tempest.AddItem("Phantom, Hovering");
            #endregion

            #region Ridgeline
            MapPalette ridgeline = new MapPalette(universalTypes.key);
            maps[Map.Ridgeline] = ridgeline;
            #region Gadgets
            ridgeline.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            ridgeline.AddItem("Health Station");
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
            ridgeline.AddItem("Initial Spawn");
            ridgeline.AddItem("Respawn Point");
            ridgeline.AddItem("Initial Loadout Camera");
            ridgeline.AddItem("Respawn Zone");
            ridgeline.AddItem("Respawn Zone, Weak");
            ridgeline.AddItem("Respawn Zone, Anti");
            ridgeline.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            ridgeline.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            ridgeline.AddItem("Flag Stand");
            ridgeline.AddItem("Capture Plate");
            ridgeline.AddItem("Hill Marker");
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
            ridgeline.AddItem("Banshee");
            ridgeline.AddItem("Falcon");
            ridgeline.AddItem("Ghost");
            ridgeline.AddItem("Mongoose");
            ridgeline.AddItem("Revenant");
            ridgeline.AddItem("Scorpion");
            ridgeline.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            ridgeline.AddItem("Wraith");
            ridgeline.AddItem("Shade Turret");
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
            ridgeline.AddItem("Grid");
            ridgeline.AddItem("Tree, Dead");
            #endregion
            #region Hidden Structure Blocks
            ridgeline.AddItem("Block, 2x2, Invisible");
            ridgeline.AddItem("Block, 1x1, Invisible");
            ridgeline.AddItem("Block, 2x2x2, Invisible");
            ridgeline.AddItem("Block, 4x4x2, Invisible");
            ridgeline.AddItem("Block, 4x4x4, Invisible");
            ridgeline.AddItem("Block, 2x1, Flat, Invisible");
            ridgeline.AddItem("Block, 1x1, Flat, Invisible");
            ridgeline.AddItem("Block, 1x1, Small, Invisible");
            ridgeline.AddItem("Block, 2x2, Flat, Invisible");
            ridgeline.AddItem("Block, 4x2, Flat, Invisible");
            ridgeline.AddItem("Block, 4x4, Flat, Invisible");
            #endregion
            ridgeline.AddItem("Health Cabinet");
            #endregion

            #region Breakneck
            MapPalette breakneck = new MapPalette(universalTypes.key);
            maps[Map.Breakneck] = breakneck;

            #region Gadgets
            breakneck.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            breakneck.AddItem("Health Station");
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
            breakneck.AddItem("Initial Spawn");
            breakneck.AddItem("Respawn Point");
            breakneck.AddItem("Initial Loadout Camera");
            breakneck.AddItem("Respawn Zone");
            breakneck.AddItem("Respawn Zone, Weak");
            breakneck.AddItem("Respawn Zone, Anti");
            breakneck.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            breakneck.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            breakneck.AddItem("Flag Stand");
            breakneck.AddItem("Capture Plate");
            breakneck.AddItem("Hill Marker");
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
            breakneck.AddItem("Street Cone");
            breakneck.AddSubcategory("Pallet", "Pallet, Large", "Pallet, Metal");
            #endregion
            #region Vehicles
            breakneck.AddItem("Banshee");
            breakneck.AddItem("Ghost");
            breakneck.AddItem("Mongoose");
            breakneck.AddItem("Revenant");
            breakneck.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            breakneck.AddItem("Wraith");
            breakneck.AddItem("Falcon");
            breakneck.AddItem("Scorpion");
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
            breakneck.AddItem("Grid");
            #endregion
            #region Hidden Structure Blocks
            breakneck.AddItem("Block, 2x2, Invisible");
            breakneck.AddItem("Block, 1x1, Invisible");
            breakneck.AddItem("Block, 2x2x2, Invisible");
            breakneck.AddItem("Block, 4x4x2, Invisible");
            breakneck.AddItem("Block, 4x4x4, Invisible");
            breakneck.AddItem("Block, 2x1, Flat, Invisible");
            breakneck.AddItem("Block, 1x1, Flat, Invisible");
            breakneck.AddItem("Block, 1x1, Small, Invisible");
            breakneck.AddItem("Block, 2x2, Flat, Invisible");
            breakneck.AddItem("Block, 4x2, Flat, Invisible");
            breakneck.AddItem("Block, 4x4, Flat, Invisible");
            #endregion
            breakneck.AddItem("Health Cabinet");
            #region Vehicles (MCC)
            breakneck.AddSubcategory("Falcon, Nose Gun", "Falcon, Grenadier", "Falcon, Transport");
            breakneck.AddItem("Warthog, Transport");
            breakneck.AddItem("Cart, Electric");
            breakneck.AddItem("Forklift");
            breakneck.AddItem("Pickup");
            breakneck.AddItem("Truck Cab");
            breakneck.AddItem("Van, Oni");
            breakneck.AddItem("Shade, Fuel Rod");
            #endregion
            #region Gadgets (MCC)
            breakneck.AddItem("One Way Shield 1");
            breakneck.AddItem("One Way Shield 2");
            breakneck.AddItem("One Way Shield 3");
            breakneck.AddItem("One Way Shield 4");
            breakneck.AddItem("One Way Shield 5");
            breakneck.AddItem("Shield Wall, Small");
            breakneck.AddItem("Shield Wall, Medium");
            breakneck.AddItem("Shield Wall, Large");
            breakneck.AddItem("Shield Wall, X-Large");
            breakneck.AddItem("Ammo Cabinet");
            breakneck.AddItem("Spnkr Ammo");
            breakneck.AddItem("Sniper Ammo");
            #endregion
            #region Scenery (MCC)
            breakneck.AddItem("Heavy Barrier");
            breakneck.AddSubcategory("Phantom", "Spirit", "Pelican", "Drop Pod, Elite", "Anti Air Gun");
            breakneck.AddSubcategory("Cargo Truck, Destroyed", "Falcon, Destroyed", "Warthog, Destroyed");
            breakneck.AddItem("Folding Chair");
            breakneck.AddItem("Dumpster");
            breakneck.AddItem("Dumpster, Tall");
            breakneck.AddItem("Equipment Case");
            breakneck.AddItem("Monitor");
            breakneck.AddItem("Plasma Storage");
            breakneck.AddItem("Camping Stool, Covenant");
            breakneck.AddItem("Covenant Antenna");
            breakneck.AddItem("Fuel Storage");
            breakneck.AddItem("Engine Cart");
            breakneck.AddItem("Missile Cart");
            #endregion
            #region Structure (MCC)
            breakneck.AddSubcategory("Wall (MCC)", "Door (MCC)");
            #endregion
            #endregion

            #region Highlands
            MapPalette highlands = new MapPalette(universalTypes.key);
            maps[Map.Highlands] = highlands;

            #region Vehicles
            highlands.AddItem("Banshee");
            highlands.AddItem("Falcon");
            highlands.AddItem("Ghost");
            highlands.AddItem("Mongoose");
            highlands.AddItem("Revenant");
            highlands.AddItem("Scorpion");
            highlands.AddItem("Shade Turret");
            highlands.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            highlands.AddItem("Wraith");
            #endregion
            #region Gadgets
            highlands.AddSubcategory("Fusion Coil", "Landmine");
            highlands.AddItem("Health Station");
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
            highlands.AddItem("Initial Spawn");
            highlands.AddItem("Respawn Point");
            highlands.AddItem("Initial Loadout Camera");
            highlands.AddItem("Respawn Zone");
            highlands.AddItem("Respawn Zone, Weak");
            highlands.AddItem("Respawn Zone, Anti");
            highlands.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            highlands.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            highlands.AddItem("Flag Stand");
            highlands.AddItem("Capture Plate");
            highlands.AddItem("Hill Marker");
            #endregion
            #region Scenery
            highlands.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Jersey Barrier",
                "Jersey Barrier, Short",
                "Covenant Barrier",
                "Portable Shield");
            highlands.AddItem("Camping Stool");
            highlands.AddItem("Folding Chair");
            highlands.AddSubcategory(
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open",
                "Crate, Fully Open");
            highlands.AddItem("Dumpster, Tall");
            highlands.AddSubcategory(
                "Sandbag Wall",
                "Sandbag Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            highlands.AddItem("Street Cone");
            highlands.AddSubcategory("Pallet", "Pallet, Large", "Pallet, Metal");
            #endregion
            #region Hidden Structure Blocks
            highlands.AddItem("Block, 2x2, Invisible");
            highlands.AddItem("Block, 1x1, Invisible");
            highlands.AddItem("Block, 2x2x2, Invisible");
            highlands.AddItem("Block, 4x4x2, Invisible");
            highlands.AddItem("Block, 4x4x4, Invisible");
            highlands.AddItem("Block, 2x1, Flat, Invisible");
            highlands.AddItem("Block, 1x1, Flat, Invisible");
            highlands.AddItem("Block, 1x1, Small, Invisible");
            highlands.AddItem("Block, 2x2, Flat, Invisible");
            highlands.AddItem("Block, 4x2, Flat, Invisible");
            highlands.AddItem("Block, 4x4, Flat, Invisible");
            #endregion
            #endregion

            #region Reflection
            MapPalette reflection = new MapPalette(universalTypes.key);
            maps[Map.Reflection] = reflection;

            #region Gadgets
            reflection.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            reflection.AddItem("Health Station");
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
            reflection.AddItem("Initial Spawn");
            reflection.AddItem("Respawn Point");
            reflection.AddItem("Initial Loadout Camera");
            reflection.AddItem("Respawn Zone");
            reflection.AddItem("Respawn Zone, Weak");
            reflection.AddItem("Respawn Zone, Anti");
            reflection.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            reflection.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            reflection.AddItem("Flag Stand");
            reflection.AddItem("Capture Plate");
            reflection.AddItem("Hill Marker");
            #endregion
            #region Scenery
            reflection.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Jersey Barrier",
                "Jersey Barrier, Short",
                "Covenant Barrier",
                "Portable Shield");
            reflection.AddItem("Camping Stool");
            reflection.AddItem("Folding Chair");
            reflection.AddSubcategory(
                "Crate, Small, Closed",
                "Crate, Metal, Multi",
                "Crate, Metal, Single",
                "Crate, Heavy Duty",
                "Crate, Heavy, Small",
                "Covenant Crate",
                "Crate, Half Open",
                "Crate, Fully Open");
            reflection.AddItem("Dumpster, Tall");
            reflection.AddSubcategory(
                "Sandbag Wall",
                "Sandbag Turret Wall",
                "Sandbag Corner, 45",
                "Sandbag Corner, 90",
                "Sandbag Endcap");
            reflection.AddItem("Street Cone");
            reflection.AddSubcategory("Pallet", "Pallet, Large", "Pallet, Metal");
            #endregion
            #region Hidden Structure Blocks
            reflection.AddItem("Block, 2x2, Invisible");
            reflection.AddItem("Block, 1x1, Invisible");
            reflection.AddItem("Block, 2x2x2, Invisible");
            reflection.AddItem("Block, 4x4x2, Invisible");
            reflection.AddItem("Block, 4x4x4, Invisible");
            reflection.AddItem("Block, 2x1, Flat, Invisible");
            reflection.AddItem("Block, 1x1, Flat, Invisible");
            reflection.AddItem("Block, 1x1, Small, Invisible");
            reflection.AddItem("Block, 2x2, Flat, Invisible");
            reflection.AddItem("Block, 4x2, Flat, Invisible");
            reflection.AddItem("Block, 4x4, Flat, Invisible");
            #endregion
            #endregion

            #region Sword Base
            MapPalette sword_base = new MapPalette(universalTypes.key);
            maps[Map.Sword_Base] = sword_base;

            #region Gadgets
            sword_base.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery", "Propane Tank");
            sword_base.AddItem("Health Station");
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
            sword_base.AddItem("Initial Spawn");
            sword_base.AddItem("Respawn Point");
            sword_base.AddItem("Initial Loadout Camera");
            sword_base.AddItem("Respawn Zone");
            sword_base.AddItem("Respawn Zone, Weak");
            sword_base.AddItem("Respawn Zone, Anti");
            sword_base.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            sword_base.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            sword_base.AddItem("Flag Stand");
            sword_base.AddItem("Capture Plate");
            sword_base.AddItem("Hill Marker");
            #endregion
            #region Scenery
            sword_base.AddSubcategory(
                "Barricade, Small",
                "Barricade, Large",
                "Jersey Barrier",
                "Jersey Barrier, Short",
                "Covenant Barrier",
                "Portable Shield");
            sword_base.AddItem("Camping Stool");
            sword_base.AddItem("Folding Chair");
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
            sword_base.AddItem("Street Cone");
            sword_base.AddSubcategory("Pallet", "Pallet, Large", "Pallet, Metal");
            #endregion
            #region Hidden Structure Blocks
            sword_base.AddItem("Block, 2x2, Invisible");
            sword_base.AddItem("Block, 1x1, Invisible");
            sword_base.AddItem("Block, 2x2x2, Invisible");
            sword_base.AddItem("Block, 4x4x2, Invisible");
            sword_base.AddItem("Block, 4x4x4, Invisible");
            sword_base.AddItem("Block, 2x1, Flat, Invisible");
            sword_base.AddItem("Block, 1x1, Flat, Invisible");
            sword_base.AddItem("Block, 1x1, Small, Invisible");
            sword_base.AddItem("Block, 2x2, Flat, Invisible");
            sword_base.AddItem("Block, 4x2, Flat, Invisible");
            sword_base.AddItem("Block, 4x4, Flat, Invisible");
            #endregion
            #endregion

            #region Spire
            MapPalette spire = new MapPalette(universalTypes.key);
            maps[Map.Spire] = spire;

            #region Vehicles
            spire.AddItem("Banshee");
            spire.AddItem("Falcon");
            spire.AddItem("Ghost");
            spire.AddItem("Mongoose");
            spire.AddItem("Revenant");
            spire.AddItem("Scorpion");
            spire.AddSubcategory("Warthog, Default", "Warthog, Gauss", "Warthog, Rocket");
            spire.AddItem("Wraith");
            #endregion
            #region Gadgets
            spire.AddSubcategory("Fusion Coil", "Landmine", "Plasma Battery");
            spire.AddItem("Health Station");
            spire.AddSubcategory("Camo Powerup", "Overshield", "Custom Powerup");
            spire.AddSubcategory(
                "Cannon, Man",
                "Cannon, Man, Heavy",
                "Cannon, Man, Light",
                "Cannon, Vehicle",
                "Gravity Lift");
            spire.AddSubcategory("Receiver Node", "Sender Node", "Two-Way Node");
            spire.AddItem("Golf Club");
            spire.AddSubcategory("Light, Red", "Light, Blue", "Light, Green", "Light, Orange");
            #endregion
            #region Spawning
            spire.AddItem("Initial Spawn");
            spire.AddItem("Respawn Point");
            spire.AddItem("Initial Loadout Camera");
            spire.AddItem("Respawn Zone");
            spire.AddItem("Respawn Zone, Weak");
            spire.AddItem("Respawn Zone, Anti");
            spire.AddSubcategory("Safe Boundary", "Soft Safe Boundary");
            spire.AddSubcategory("Kill Boundary", "Soft Kill Boundary");
            #endregion
            #region Objectives
            spire.AddItem("Flag Stand");
            spire.AddItem("Capture Plate");
            spire.AddItem("Hill Marker");
            #endregion
            #region Scenery
            spire.AddItem("Ramp, Stunt");
            spire.AddItem("Covenant Barrier");
            spire.AddItem("Shield Door, Small");
            spire.AddItem("Shield Door, Medium");
            spire.AddItem("Shield Door, Large");
            spire.AddItem("One Way Shield 2");
            spire.AddItem("One Way Shield 3");
            spire.AddItem("One Way Shield 4");
            spire.AddItem("Shield Wall, Small");
            spire.AddItem("Shield Wall, Medium");
            spire.AddItem("Shield Wall, Large");
            spire.AddItem("Shield Wall, X-Large");
            #endregion
            #region Hidden Structure Blocks
            spire.AddItem("Block, 2x2, Invisible");
            spire.AddItem("Block, 1x1, Invisible");
            spire.AddItem("Block, 2x2x2, Invisible");
            spire.AddItem("Block, 4x4x2, Invisible");
            spire.AddItem("Block, 4x4x4, Invisible");
            spire.AddItem("Block, 2x1, Flat, Invisible");
            spire.AddItem("Block, 1x1, Flat, Invisible");
            spire.AddItem("Block, 1x1, Small, Invisible");
            spire.AddItem("Block, 2x2, Flat, Invisible");
            spire.AddItem("Block, 4x2, Flat, Invisible");
            spire.AddItem("Block, 4x4, Flat, Invisible");
            #endregion
            #region Hidden Structure Blocks
            spire.AddItem("Spire Cannon, Base");
            spire.AddItem("Spire Cannon, Cliff");
            spire.AddItem("Spire Cannon, Gun");
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

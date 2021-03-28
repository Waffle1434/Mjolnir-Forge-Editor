using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgeAssistant
{
    public class ItemParser
    {
        public string GetItemFromCode(string code, string MapName)
        {
			//Universal Codes
			switch (code)
			{
				case "0000":
					{
						return "Assault Rifle";
					}
				case "0100":
					{
						return "DMR";
					}
				case "0200":
					{
						return "Grenade Launcher";
					}
				case "0300":
					{
						return "Magnum";
					}
				case "0400":
					{
						return "Rocket Launcher";
					}
				case "0500":
					{
						return "Shotgun";
					}
				case "0600":
					{
						return "Sniper Rifle";
					}
				case "0700":
					{
						return "Spartan Laser";
					}
				case "0800":
					{
						return "Frag Grenade";
					}
				case "0900":
					{
						return "Mounted Machinegun";
					}
				case "0A00":
					{
						return "Concussion Rifle";
					}
				case "0B00":
					{
						return "Energy Sword";
					}
				case "0C00":
					{
						return "Fuel Rod Gun";
					}
				case "0D00":
					{
						return "Gravity Hammer";
					}
				case "0E00":
					{
						return "Focus Rifle";
					}
				case "0F00":
					{
						return "Needle Rifle";
					}
				case "1000":
					{
						return "Needler";
					}
				case "1100":
					{
						return "Plasma Launcher";
					}
				case "1200":
					{
						return "Plasma Pistol";
					}
				case "1300":
					{
						return "Plasma Repeater";
					}
				case "1400":
					{
						return "Plasma Rifle";
					}
				case "1500":
					{
						return "Spiker";
					}
				case "1600":
					{
						return "Plasma Grenade";
					}
				case "1700":
					{
						return "Plasma Turret";
					}
				case "1800":
					{
						return "Active Camouflage";
					}
				case "1900":
					{
						return "Armor Lock";
					}
				case "1A00":
					{
						return "Drop Shield";
					}
				case "1B00":
					{
						return "Evade";
					}
				case "1C00":
					{
						return "Hologram";
					}
				case "1D00":
					{
						return "Jet Pack";
					}
				case "1E00":
					{
						return "Sprint";
					}
			}

			if (MapName == "forge_halo")
            {
                switch (code)
                {
					case "1F00":
						{
							return "Banshee";
						}
					case "2000":
						{
							return "Falcon";
						}
					case "2100":
						{
							return "Ghost";
						}
					case "2200":
						{
							return "Mongoose";
						}
					case "2300":
						{
							return "Revenant";
						}
					case "2400":
						{
							return "Scorpion";
						}
					case "2500":
						{
							return "Shade Turret";
						}
					case "2600":
						{
							return "Warthog, Default";
						}
					case "2601":
						{
							return "Warthog, Gauss";
						}
					case "2602":
						{
							return "Warthog, Rocket";
						}
					case "2700":
						{
							return "Wraith";
						}
					case "2800":
						{
							return "Fusion Coil";
						}
					case "2801":
						{
							return "Landmine";
						}
					case "2802":
						{
							return "Plasma Battery";
						}
					case "2803":
						{
							return "Propane Tank";
						}
					case "2900":
						{
							return "Health Station";
						}
					case "2A00":
						{
							return "Camo Powerup";
						}
					case "2A01":
						{
							return "Overshield";
						}
					case "2A02":
						{
							return "Custom Powerup";
						}
					case "2B00":
						{
							return "Cannon, Man";
						}
					case "2B01":
						{
							return "Cannon, Man, Heavy";
						}
					case "2B02":
						{
							return "Cannon, Man, Light";
						}
					case "2B03":
						{
							return "Cannon, Vehicle";
						}
					case "2B04":
						{
							return "Gravity Lift";
						}
					case "2C00":
						{
							return "One Way Shield 2";
						}
					case "2D00":
						{
							return "One Way Shield 3";
						}
					case "2E00":
						{
							return "One Way Shield 4";
						}
					case "2F00":
						{
							return "FX:Colorblind";
						}
					case "2F01":
						{
							return "FX:Next Gen";
						}
					case "2F02":
						{
							return "FX:Juicy";
						}
					case "2F03":
						{
							return "FX:Nova";
						}
					case "2F04":
						{
							return "FX:Olde Timey";
						}
					case "2F05":
						{
							return "FX:Pen And Ink";
						}
					case "2F06":
						{
							return "FX:Purple";
						}
					case "2F07":
						{
							return "FX:Green";
						}
					case "2F08":
						{
							return "FX:Orange";
						}
					case "3000":
						{
							return "Shield Door, Small";
						}
					case "3100":
						{
							return "Shield Door, Medium";
						}
					case "3200":
						{
							return "Shield Door, Large";
						}
					case "3300":
						{
							return "Receiver Node";
						}
					case "3301":
						{
							return "Sender Node";
						}
					case "3302":
						{
							return "Two-Way Node";
						}
					case "3400":
						{
							return "Die";
						}
					case "3401":
						{
							return "Golf Ball";
						}
					case "3402":
						{
							return "Golf Club";
						}
					case "3403":
						{
							return "Kill Ball";
						}
					case "3404":
						{
							return "Soccer Ball";
						}
					case "3405":
						{
							return "Tin Cup";
						}
					case "3500":
						{
							return "Light, Red";
						}
					case "3501":
						{
							return "Light, Blue";
						}
					case "3502":
						{
							return "Light, Green";
						}
					case "3503":
						{
							return "Light, Orange";
						}
					case "3504":
						{
							return "Light, Purple";
						}
					case "3505":
						{
							return "Light, Yellow";
						}
					case "3506":
						{
							return "Light, White";
						}
					case "3507":
						{
							return "Light, Red, Flashing";
						}
					case "3508":
						{
							return "Light, Yellow, Flashing";
						}
					case "3600":
						{
							return "Initial Spawn";
						}
					case "3700":
						{
							return "Respawn Point";
						}
					case "3800":
						{
							return "Initial Loadout Camera";
						}
					case "3900":
						{
							return "Respawn Zone";
						}
					case "3A00":
						{
							return "Respawn Zone, Weak";
						}
					case "3B00":
						{
							return "Respawn Zone, Anti";
						}
					case "3C00":
						{
							return "Safe Boundary";
						}
					case "3C01":
						{
							return "Soft Safe Boundary";
						}
					case "3D00":
						{
							return "Kill Boundary";
						}
					case "3D01":
						{
							return "Soft Kill Boundary";
						}
					case "3E00":
						{
							return "Flag Stand";
						}
					case "3F00":
						{
							return "Capture Plate";
						}
					case "4000":
						{
							return "Hill Marker";
						}
					case "4100":
						{
							return "Barricade, Small";
						}
					case "4101":
						{
							return "Barricade, Large";
						}
					case "4102":
						{
							return "Covenant Barrier";
						}
					case "4103":
						{
							return "Portable Shield";
						}
					case "4200":
						{
							return "Camping Stool";
						}
					case "4300":
						{
							return "Crate, Heavy Duty";
						}
					case "4301":
						{
							return "Crate, Heavy, Small";
						}
					case "4302":
						{
							return "Covenant Crate";
						}
					case "4303":
						{
							return "Crate, Half Open";
						}
					case "4400":
						{
							return "Sandbag Wall";
						}
					case "4401":
						{
							return "Sandbag, Turret Wall";
						}
					case "4402":
						{
							return "Sandbag Corner, 45";
						}
					case "4403":
						{
							return "Sandbag Corner, 90";
						}
					case "4404":
						{
							return "Sandbag Endcap";
						}
					case "4500":
						{
							return "Street Cone";
						}
					case "4600":
						{
							return "Block, 1x1";
						}
					case "4601":
						{
							return "Block, 1x1, Flat";
						}
					case "4602":
						{
							return "Block, 1x1, Short";
						}
					case "4603":
						{
							return "Block, 1x1, Tall";
						}
					case "4604":
						{
							return "Block, 1x1, Tall And Thin";
						}
					case "4605":
						{
							return "Block, 1x2";
						}
					case "4606":
						{
							return "Block, 1x4";
						}
					case "4607":
						{
							return "Block, 2x1, Flat";
						}
					case "4608":
						{
							return "Block, 2x2";
						}
					case "4609":
						{
							return "Block, 2x2, Flat";
						}
					case "460A":
						{
							return "Block, 2x2, Short";
						}
					case "460B":
						{
							return "Block, 2x2, Tall";
						}
					case "460C":
						{
							return "Block, 2x3";
						}
					case "460D":
						{
							return "Block, 2x4";
						}
					case "460E":
						{
							return "Block, 3x1, Flat";
						}
					case "460F":
						{
							return "Block, 3x3";
						}
					case "4610":
						{
							return "Block, 3x3, Flat";
						}
					case "4611":
						{
							return "Block, 3x3, Short";
						}
					case "4612":
						{
							return "Block, 3x3, Tall";
						}
					case "4613":
						{
							return "Block, 3x4";
						}
					case "4614":
						{
							return "Block, 4x4";
						}
					case "4615":
						{
							return "Block, 4x4, Flat";
						}
					case "4616":
						{
							return "Block, 4x4, Short";
						}
					case "4617":
						{
							return "Block, 4x4, Tall";
						}
					case "4618":
						{
							return "Block, 5x1, Short";
						}
					case "4619":
						{
							return "Block, 5x5, Flat";
						}
					case "4700":
						{
							return "Bridge, Small";
						}
					case "4701":
						{
							return "Bridge, Medium";
						}
					case "4702":
						{
							return "Bridge, Large";
						}
					case "4703":
						{
							return "Bridge, XLarge";
						}
					case "4704":
						{
							return "Bridge, Diagonal";
						}
					case "4705":
						{
							return "Bridge, Diag, Small";
						}
					case "4706":
						{
							return "Dish";
						}
					case "4707":
						{
							return "Dish, Open";
						}
					case "4708":
						{
							return "Corner, 45 Degrees";
						}
					case "4709":
						{
							return "Corner, 2x2";
						}
					case "470A":
						{
							return "Corner, 4x4";
						}
					case "470B":
						{
							return "Landing Pad";
						}
					case "470C":
						{
							return "Platform, Ramped";
						}
					case "470D":
						{
							return "Platform, Large";
						}
					case "470E":
						{
							return "Platform, XL";
						}
					case "470F":
						{
							return "Platform, XXL";
						}
					case "4710":
						{
							return "Platform, Y";
						}
					case "4711":
						{
							return "Platform, Y, Large";
						}
					case "4712":
						{
							return "Sniper Nest";
						}
					case "4713":
						{
							return "Staircase";
						}
					case "4714":
						{
							return "Walkway, Large";
						}
					case "4800":
						{
							return "Bunker, Small";
						}
					case "4801":
						{
							return "Bunker, Small, Covered";
						}
					case "4802":
						{
							return "Bunker, Box";
						}
					case "4803":
						{
							return "Bunker, Round";
						}
					case "4804":
						{
							return "Bunker, Ramp";
						}
					case "4805":
						{
							return "Pyramid";
						}
					case "4806":
						{
							return "Tower, 2 Story";
						}
					case "4807":
						{
							return "Tower, 3 Story";
						}
					case "4808":
						{
							return "Tower, Tall";
						}
					case "4809":
						{
							return "Room, Double";
						}
					case "480A":
						{
							return "Room, Triple";
						}
					case "4900":
						{
							return "Antenna, Small";
						}
					case "4901":
						{
							return "Antenna, Satellite";
						}
					case "4902":
						{
							return "Brace";
						}
					case "4903":
						{
							return "Brace, Large";
						}
					case "4904":
						{
							return "Brace, Tunnel";
						}
					case "4905":
						{
							return "Column";
						}
					case "4906":
						{
							return "Cover";
						}
					case "4907":
						{
							return "Cover, Crenellation";
						}
					case "4908":
						{
							return "Cover, Glass";
						}
					case "4909":
						{
							return "Glass Sail";
						}
					case "490A":
						{
							return "Railing, Small";
						}
					case "490B":
						{
							return "Railing, Medium";
						}
					case "490C":
						{
							return "Railing, Long";
						}
					case "490D":
						{
							return "Teleporter Frame";
						}
					case "490E":
						{
							return "Strut";
						}
					case "490F":
						{
							return "Large Walkway Cover";
						}
					case "4A00":
						{
							return "Door";
						}
					case "4A01":
						{
							return "Door, Double";
						}
					case "4A02":
						{
							return "Window";
						}
					case "4A03":
						{
							return "Window, Double";
						}
					case "4A04":
						{
							return "Wall";
						}
					case "4A05":
						{
							return "Wall, Double";
						}
					case "4A06":
						{
							return "Wall, Corner";
						}
					case "4A07":
						{
							return "Wall, Curved";
						}
					case "4A08":
						{
							return "Wall, Coliseum";
						}
					case "4A09":
						{
							return "Window, Colesium";
						}
					case "4A0A":
						{
							return "Tunnel, Short";
						}
					case "4A0B":
						{
							return "Tunnel, Long";
						}
					case "4B00":
						{
							return "Bank, 1x1";
						}
					case "4B01":
						{
							return "Bank, 1x2";
						}
					case "4B02":
						{
							return "Bank, 2x1";
						}
					case "4B03":
						{
							return "Bank, 2x2";
						}
					case "4B04":
						{
							return "Ramp, 1x2";
						}
					case "4B05":
						{
							return "Ramp, 1x2, Shallow";
						}
					case "4B06":
						{
							return "Ramp, 2x2";
						}
					case "4B07":
						{
							return "Ramp, 2x2, Steep";
						}
					case "4B08":
						{
							return "Ramp, Circular, Small";
						}
					case "4B09":
						{
							return "Ramp, Circular, Large";
						}
					case "4B0A":
						{
							return "Ramp, Bridge, Small";
						}
					case "4B0B":
						{
							return "Ramp, Bridge, Medium";
						}
					case "4B0C":
						{
							return "Ramp, Bridge, Large";
						}
					case "4B0D":
						{
							return "Ramp, XL";
						}
					case "4B0E":
						{
							return "Ramp, Stunt";
						}
					case "4C00":
						{
							return "Rock, Small";
						}
					case "4C01":
						{
							return "Rock, Flat";
						}
					case "4C02":
						{
							return "Rock, Medium 1";
						}
					case "4C03":
						{
							return "Rock, Medium 2";
						}
					case "4C04":
						{
							return "Rock, Spire 1";
						}
					case "4C05":
						{
							return "Rock, Spire 2";
						}
					case "4C06":
						{
							return "Rock, Seastack";
						}
					case "4C07":
						{
							return "Rock, Arch";
						}
					case "4D00":
						{
							return "Grid";
						}
					case "5900":
						{
							return "Falcon, Nose Gun";
						}
					case "5901":
						{
							return "Falcon, Grenadier";
						}
					case "5902":
						{
							return "Falcon, Transport";
						}
					case "5A00":
						{
							return "Warthog, Transport";
						}
					case "5B00":
						{
							return "Sabre";
						}
					case "5C00":
						{
							return "Seraph";
						}
					case "5D00":
						{
							return "Cart, Electric";
						}
					case "5E00":
						{
							return "Forklift";
						}
					case "5F00":
						{
							return "Pickup";
						}
					case "6000":
						{
							return "Truck Cab";
						}
					case "6100":
						{
							return "Van, Oni";
						}
					case "6200":
						{
							return "Shade, Fuel Rod";
						}
					case "6300":
						{
							return "Cannon, Man";
						}
					case "6301":
						{
							return "Cannon, Man, Heavy";
						}
					case "6302":
						{
							return "Cannon, Man, Light";
						}
					case "6303":
						{
							return "Gravity Lift, Forerunner";
						}
					case "6304":
						{
							return "Gravity Lift, Tall, Forerunner";
						}
					case "6305":
						{
							return "Cannon, Man, Human";
						}
					case "6400":
						{
							return "One Way Shield 1";
						}
					case "6500":
						{
							return "One Way Shield 5";
						}
					case "6600":
						{
							return "Shield Wall, Small";
						}
					case "6700":
						{
							return "Shield Wall, Medium";
						}
					case "6800":
						{
							return "Shield Wall, Large";
						}
					case "6900":
						{
							return "Shield Wall, X-Large";
						}
					case "6A00":
						{
							return "One Way Shield 2";
						}
					case "6B00":
						{
							return "One Way Shield 3";
						}
					case "6C00":
						{
							return "One Way Shield 4";
						}
					case "6D00":
						{
							return "Shield Door, Small";
						}
					case "6E00":
						{
							return "Shield Door, Small 1";
						}
					case "6F00":
						{
							return "Shield Door, Large";
						}
					case "7000":
						{
							return "Shield Door, Large 1";
						}
					case "7100":
						{
							return "Ammo Cabinet";
						}
					case "7200":
						{
							return "Spnkr Ammo";
						}
					case "7300":
						{
							return "Sniper Ammo";
						}
					case "7400":
						{
							return "Jersey Barrier";
						}
					case "7401":
						{
							return "Jersey Barrier, Short";
						}
					case "7402":
						{
							return "Heavy Barrier";
						}
					case "7500":
						{
							return "Small, Closed";
						}
					case "7501":
						{
							return "Crate, Metal, Multi";
						}
					case "7502":
						{
							return "Crate, Metal, Single";
						}
					case "7503":
						{
							return "Crate, Fully Open";
						}
					case "7504":
						{
							return "Crate, Forerunner, Small";
						}
					case "7505":
						{
							return "Crate, Forerunner, Large";
						}
					case "7600":
						{
							return "Pallet";
						}
					case "7601":
						{
							return "Pallet, Large";
						}
					case "7602":
						{
							return "Pallet, Metal";
						}
					case "7700":
						{
							return "Driftwood 1";
						}
					case "7701":
						{
							return "Driftwood 2";
						}
					case "7702":
						{
							return "Driftwood 3";
						}
					case "7800":
						{
							return "Phantom";
						}
					case "7801":
						{
							return "Spirit";
						}
					case "7802":
						{
							return "Pelican";
						}
					case "7803":
						{
							return "Drop Pod, Elite";
						}
					case "7804":
						{
							return "Anti Air Gun";
						}
					case "7900":
						{
							return "Cargo Truck, Destroyed";
						}
					case "7901":
						{
							return "Falcon, Destroyed";
						}
					case "7902":
						{
							return "Warthog, Destroyed";
						}
					case "7A00":
						{
							return "Folding Chair";
						}
					case "7B00":
						{
							return "Dumpster";
						}
					case "7C00":
						{
							return "Dumpster, Tall";
						}
					case "7D00":
						{
							return "Equipment Case";
						}
					case "7E00":
						{
							return "Monitor";
						}
					case "7F00":
						{
							return "Plasma Storage";
						}
					case "8000":
						{
							return "Camping Stool, Covenant";
						}
					case "8100":
						{
							return "Covenant Antenna";
						}
					case "8200":
						{
							return "Fuel Storage";
						}
					case "8300":
						{
							return "Engine Cart";
						}
					case "8400":
						{
							return "Missile Cart";
						}
					case "8500":
						{
							return "Bridge";
						}
					case "8501":
						{
							return "Platform, Covenant";
						}
					case "8502":
						{
							return "Catwalk, Straight";
						}
					case "8503":
						{
							return "Catwalk, Short";
						}
					case "8504":
						{
							return "Catwalk, Bend, Left";
						}
					case "8505":
						{
							return "Catwalk, Bend, Right";
						}
					case "8506":
						{
							return "Catwalk, Angled";
						}
					case "8507":
						{
							return "Catwalk, Large";
						}
					case "8600":
						{
							return "Bunker, Overlook";
						}
					case "8601":
						{
							return "Gunners Nest";
						}
					case "8700":
						{
							return "Cover, Small";
						}
					case "8701":
						{
							return "Block, Large";
						}
					case "8702":
						{
							return "Blocker, Hallway";
						}
					case "8703":
						{
							return "Column, Stone";
						}
					case "8704":
						{
							return "Tombstone";
						}
					case "8705":
						{
							return "Cover, Large, Stone";
						}
					case "8706":
						{
							return "Cover, Large";
						}
					case "8707":
						{
							return "Walkway Cover";
						}
					case "8708":
						{
							return "Walkway Cover, Short";
						}
					case "8709":
						{
							return "Cover, Large, Human";
						}
					case "870A":
						{
							return "I-Beam";
						}
					case "8800":
						{
							return "Wall";
						}
					case "8801":
						{
							return "Door";
						}
					case "8802":
						{
							return "Door, Human";
						}
					case "8803":
						{
							return "Door A, Forerunner";
						}
					case "8804":
						{
							return "Door B, Forerunner";
						}
					case "8805":
						{
							return "Door C, Forerunner";
						}
					case "8806":
						{
							return "Door D, Forerunner";
						}
					case "8807":
						{
							return "Door E, Forerunner";
						}
					case "8808":
						{
							return "Door F, Forerunner";
						}
					case "8809":
						{
							return "Door G, Forerunner";
						}
					case "880A":
						{
							return "Door H, Forerunner";
						}
					case "880B":
						{
							return "Wall, Small, Forerunner";
						}
					case "880C":
						{
							return "Wall, Large, Forerunner";
						}
					case "8900":
						{
							return "Rock, Spire 3";
						}
					case "8901":
						{
							return "Tree, Dead";
						}
					case "8D00":
						{
							return "Target Designator";
						}
				}
			}
            return code;
        }
    }
}

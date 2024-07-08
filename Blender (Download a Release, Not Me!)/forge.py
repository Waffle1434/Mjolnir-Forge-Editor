import bpy, enum, time, blf, textwrap, urllib.request, webbrowser
from bpy.types import Operator
from bpy.props import *
from mathutils import *
from ctypes import *
from os.path import exists
from math import *
from threading import Thread

check_update = True
vMjolnir = "0.9.9.0"
print("Mjolnir v" + vMjolnir)

maxObjectCount = 650
anvilScaling = True
propSceneName = 'Props'
shapesCollName = 'Shapes'
mapPalette = 'Forge World Palette'
dllPath = bpy.path.abspath("//") + "ForgeBridge.dll"
githubURL = "https://github.com/Waffle1434/Mjolnir-Forge-Editor/releases/latest"
persist_vars = bpy.app.driver_namespace
defaultGtLabel = ('NO_LABEL', "No Label", "Default")

class float3(Structure):
    _fields_ = [ ('x', c_float), ('y', c_float), ('z', c_float) ]
    def cross(self, other): return float3(self.y * other.z - self.z * other.y, self.z * other.x - self.x * other.z, self.x * other.y - self.y * other.x)
    def fromVector(vec): return float3(vec.x,vec.y,vec.z)
    def toVector(self): return Vector((self.x,self.y,self.z))
    def __str__(self): return "(%.2f,%.2f,%.2f)"%(self.x,self.y,self.z)
class ForgeObject(Structure):
    _fields_ = [
        ('show', c_ushort),
        ('itemCategory', c_ushort),
        ('idExt', c_uint),
        ('position', float3),
        ('forward', float3),
        ('up', float3),
        ('spawnRelativeToMapIndex', c_ushort),
        ('itemVariant', c_ubyte),
        ('pad1', c_ubyte),
        ('width', c_float),
        ('length', c_float),
        ('top', c_float),
        ('bottom', c_float),
        ('shape', c_ubyte),
        ('spawnSequence', c_byte),
        ('spawnTime', c_ubyte),
        ('cachedType', c_ubyte),
        ('gtLabelIndex', c_ushort),
        ('flags', c_ubyte),
        ('team', c_ubyte),
        ('otherInfoA', c_ubyte),
        ('otherInfoB', c_ubyte),
        ('color', c_ubyte),
        ('pad2', c_ubyte),
    ]
class ForgeObjectFlags(enum.IntFlag):
    PhysicsNormal = 0b00000000,
    PhysicsFixed  = 0b01000000,
    PhysicsPhased = 0b11000000,
    PhysicsMask   = 0b11000000,
    GameSpecific  = 0b00100000,
    Asymmetric    = 0b00001000,
    Symmetric     = 0b00000100,
    SymmetryMask  = 0b00001100,
    HideAtStart   = 0b00000010

def InverseDict(dict):
    invDict = {}
    for key, val in dict.items():
        if val not in invDict: invDict[val] = key
    return invDict

class TwoWayDict:    
    def __init__(self, A):
        self.AtoB = A
        self.BtoA = InverseDict(A)

map_object_types = {
    "Forge World"   : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'sprint_spartan', 7936: 'banshee', 8192: 'falcon', 8448: 'ghost', 8704: 'mongoose', 8960: 'revenant', 9216: 'scorpion', 9472: 'shade', 9728: 'warthog_default', 9729: 'warthog_gauss', 9730: 'warthog_rocket', 9984: 'wraith', 10240: 'exp_fusioncoil', 10241: 'exp_landmine', 10242: 'exp_battery', 10243: 'exp_propanetank', 10496: 'health_station', 10752: 'powerup_blue', 10753: 'powerup_red', 10754: 'powerup_yellow', 11008: 'mancannon_normal', 11009: 'mancannon_heavy', 11010: 'mancannon_light', 11011: 'mancannon_vehicle', 11012: 'mancannon_gravlift', 11264: 'door_oneway_small', 11520: 'door_oneway_medium', 11776: 'door_oneway_large', 12032: 'sfx_1', 12033: 'sfx_2', 12034: 'sfx_3', 12035: 'sfx_4', 12036: 'sfx_5', 12037: 'sfx_6', 12038: 'purple', 12039: 'green', 12040: 'orange', 12288: 'door_shield_small', 12544: 'door_shield_medium', 12800: 'door_shield_large', 13056: 'teleporter_reciever', 13057: 'teleporter_sender', 13058: 'teleporter_2way', 13312: 'toys_die', 13313: 'toys_golfball', 13314: 'toys_golfclub', 13315: 'toys_killball', 13316: 'toys_soccerball', 13317: 'toys_tincup', 13568: 'ff_light_red', 13569: 'ff_light_blue', 13570: 'ff_light_green', 13571: 'ff_light_orange', 13572: 'ff_light_purple', 13573: 'ff_light_yellow', 13574: 'ff_light_white', 13575: 'ff_light_red_flash', 13576: 'ff_light_yellow_flash', 13824: 'spawning_initial', 14080: 'spawning_respawn', 14336: 'spawning_camera', 14592: 'respawn_zone', 14848: 'respawn_zone_weak', 15104: 'respawn_zone_anti', 15360: 'spawning_safe', 15361: 'spawning_safe_soft', 15616: 'spawning_kill', 15617: 'spawning_kill_soft', 15872: 'obj_flag_base', 16128: 'obj_plate', 16384: 'obj_hill_marker', 16640: 'ff_scn_barricade_small', 16641: 'ff_scn_barricade_large', 16642: 'ff_scn_covenantbarrier', 16643: 'ff_scn_cov_shield', 16896: 'ff_scn_camping_stool', 17152: 'ff_scn_space_crate', 17153: 'ff_scn_space_crate_small', 17154: 'ff_scn_cov_crate', 17155: 'ff_scn_cov_crate_a', 17408: 'ff_scn_sandbag_wall', 17409: 'ff_scn_sandbag_turret', 17410: 'ff_scn_sandbag_45', 17411: 'ff_scn_sandbag_90', 17412: 'ff_scn_sandbag_end', 17664: 'ff_scn_street_cone', 17920: 'block_1x1', 17921: 'block_1x1_flat', 17922: 'block_1x1_short', 17923: 'block_1x1_tall', 17924: 'block_1x1_tall_thin', 17925: 'block_1x2', 17926: 'block_1x4', 17927: 'block_2x1_flat', 17928: 'block_2x2', 17929: 'block_2x2_flat', 17930: 'block_2x2_short', 17931: 'block_2x2_tall', 17932: 'block_2x3', 17933: 'block_2x4', 17934: 'block_3x1_flat', 17935: 'block_3x3', 17936: 'block_3x3_flat', 17937: 'block_3x3_short', 17938: 'block_3x3_tall', 17939: 'block_3x4', 17940: 'block_4x4', 17941: 'block_4x4_flat', 17942: 'block_4x4_short', 17943: 'block_4x4_tall', 17944: 'block_5x1_flat', 17945: 'block_5x5_flat', 18176: 'bridge_2x1', 18177: 'bridge_2x2', 18178: 'bridge_2x3', 18179: 'bridge_2x5', 18180: 'bridge_diag', 18181: 'bridge_diag_short', 18182: 'dish', 18183: 'dish_door', 18184: 'corner_45', 18185: 'plat_2x2_corner', 18186: 'plat_4x4_corner', 18187: 'plat_landing_pad', 18188: 'plat_ramped', 18189: 'plat_4x4_tower', 18190: 'cylinder_small', 18191: 'cylinder_large', 18192: 'plat_y', 18193: 'plat_y_large', 18194: 'sniper_nest', 18195: 'stair_case', 18196: 'walkway', 18432: 'bunker_low', 18433: 'bunker_covered', 18434: 'bunker_full', 18435: 'bunker_gulch', 18436: 'bunker_stairs', 18437: 'pyramid', 18438: 'tower', 18439: 'tower_3_story', 18440: 'tower_tall', 18441: 'room_double', 18442: 'room_triple', 18688: 'antenna', 18689: 'antenna_dish', 18690: 'brace', 18691: 'brace_large', 18692: 'brace_tunnel', 18693: 'ff_column', 18694: 'crenelation', 18695: 'crenelation_notched', 18696: 'cover_glass', 18697: 'wtf_is_this_thing', 18698: 'railing_short', 18699: 'railing_medium', 18700: 'railing_long', 18701: 'teleporter_frame', 18702: 'ff_strut', 18703: 'walkway_cover', 18944: 'door', 18945: 'door_double', 18946: 'window', 18947: 'window_double', 18948: 'wall', 18949: 'wall_double', 18950: 'wall_corner', 18951: 'wall_curved', 18952: 'wall_coliseum', 18953: 'window_coliseum', 18954: 'tunnel_short', 18955: 'tunnel_long', 19200: 'bank_1x1', 19201: 'bank_1x2', 19202: 'bank_2x1', 19203: 'bank_2x2', 19204: 'ramp_1x2', 19205: 'ramp_1x2_shallow', 19206: 'ramp_2x2', 19207: 'ramp_2x2_steep', 19208: 'ramp_circular_small', 19209: 'ramp_circular_large', 19210: 'ramp_bridge_small', 19211: 'ramp_bridge_medium', 19212: 'ramp_bridge_large', 19213: 'ramp_4x2', 19214: 'ramp_stunt', 19456: 'rock_small', 19457: 'rock_flat', 19458: 'rock_med1', 19459: 'rock_med2', 19460: 'rock_spire1', 19461: 'rock_spire2', 19462: 'rock_seastack', 19463: 'rock_arch', 19712: 'ff_grid', 19968: 'block_2x2_invis', 20224: 'block_1x1_invis', 20480: 'block_3x3_invis', 20736: 'block_4x4_invis', 20992: 'block_5x5_flat_invis', 21248: 'block_3x3_flat_invis', 21504: 'block_2x2_flat_invis', 21760: 'block_1x1_flat_invis', 22016: 'block_4x4_flat_invis', 22272: 'block_4x4_tall_invis', 22528: 'block_5x1_flat_invis', 22784: 'ff_thorage_falcon_default', 22785: 'ff_thorage_falcon_grenade', 22786: 'ff_thorage_falcon_no_guns', 23040: 'ff_thorage_warthog_troop', 23296: 'ff_thorage_sabre', 23552: 'ff_thorage_seraph', 23808: 'ff_thorage_electric_cart', 24064: 'ff_thorage_forklift', 24320: 'ff_thorage_pickup', 24576: 'ff_thorage_truck_cab', 24832: 'ff_thorage_oni_van', 25088: 'ff_thorage_shade_flak_cannon', 25344: 'mancannon_normal_forerunner', 25345: 'mancannon_heavy_forerunner', 25346: 'mancannon_light_forerunner', 25347: 'ff_cex_gravlift_forerunner', 25348: 'ff_cex_gravlift_tall_forerunner', 25349: 'ff_cex_mancannon_human', 25600: 'door_oneway_xsmall', 25856: 'door_oneway_xlarge', 26112: 'ff_scn_laserwall_small', 26368: 'ff_scn_laserwall_medium', 26624: 'ff_scn_laserwall_large', 26880: 'ff_scn_laserwall_xlarge', 27136: 'door_oneway_small_mcc', 27392: 'door_oneway_medium_mcc', 27648: 'door_oneway_large_mcc', 27904: 'door_shield_small_mcc', 28160: 'door_shield_small1_mcc', 28416: 'door_shield_large_mcc', 28672: 'door_shield_large1_mcc', 28928: 'ff_thorage_ammo_box', 29184: 'ff_thorage_rocket_ammo', 29440: 'ff_thorage_sniper_ammo', 29696: 'ff_scn_jerseybarrier', 29697: 'ff_scn_jerseybarrier_short', 29698: 'dlc_invasion_heavy_shield', 29952: 'ff_scn_crate_small_closed', 29953: 'ff_scn_crate_metal', 29954: 'ff_scn_crate_metal_single', 29955: 'ff_scn_cov_crate_b', 29956: 'ff_cex_crate_forerunner_sm', 29957: 'ff_cex_crate_forerunner_lg', 30208: 'ff_scn_pallet', 30209: 'ff_scn_pallet_large', 30210: 'ff_scn_pallet_metal', 30464: 'dlc_medium_driftwood_a', 30465: 'dlc_medium_driftwood_b', 30466: 'dlc_medium_driftwood_c', 30720: 'ff_thorage_phantom', 30721: 'ff_thorage_spirit', 30722: 'ff_thorage_pelican', 30723: 'ff_thorage_drop_pod_elite', 30724: 'ff_thorage_anti_air_cannon', 30976: 'ff_thorage_cargo_truck_destroyed', 30977: 'ff_thorage_falcon_destroyed', 30978: 'ff_thorage_warthog_destroyed', 31232: 'ff_scn_chair_folding', 31488: 'ff_scn_dumpster', 31744: 'ff_scn_dumpster_tall', 32000: 'ff_thorage_equipment_case', 32256: 'ff_thorage_monitor_flush', 32512: 'ff_scn_cov_storage', 32768: 'ff_scn_cov_stool', 33024: 'ff_scn_cov_antenna', 33280: 'ff_thorage_fuel_storage', 33536: 'dlc_slayer_cart_a', 33792: 'dlc_slayer_cart_b', 34048: 'dlc_bridge_down', 34049: 'ff_cex_platform_cov', 34050: 'ff_cex_catwalk_straight', 34051: 'ff_cex_catwalk_short', 34052: 'ff_cex_catwalk_bend_left', 34053: 'ff_cex_catwalk_bend_right', 34054: 'ff_cex_catwalk_angled', 34055: 'ff_cex_catwalk_lg', 34304: 'ff_cex_bunker', 34305: 'ff_cex_gunner_nest', 34560: 'ff_cex_cover_sm', 34561: 'ff_cex_large_block', 34562: 'ff_cex_hallway_blocker', 34563: 'ff_cex_stone_column', 34564: 'ff_cex_tombstone', 34565: 'ff_cex_stone_block', 34566: 'ff_cex_cover_lg', 34567: 'ff_thorage_cover_sm', 34568: 'ff_cex_walkway_cover_short', 34569: 'ff_thorage_cover_lg', 34570: 'ff_cex_i_beam', 34816: 'wall_mcc', 34817: 'door_mcc', 34818: 'ff_cex_door_human', 34819: 'ff_cex_door_forerunner_a', 34820: 'ff_cex_door_forerunner_b', 34821: 'ff_thorage_door_c', 34822: 'ff_thorage_door_d', 34823: 'ff_thorage_door_e', 34824: 'ff_thorage_door_f', 34825: 'ff_thorage_door_g', 34826: 'ff_thorage_door_h', 34827: 'ff_cex_wall_sm_forerunner', 34828: 'ff_cex_wall_lg_forerunner', 35072: 'dlc_medium_stone_b', 35073: 'ff_cex_tree_life', 35328: 'ff_thorage_generator', 35584: 'ff_thorage_vending_machine', 35840: 'ff_thorage_dinghy', 36096: 'ff_thorage_airstrike', 36352: 'ff_thorage_pelican_flyable', 36608: 'ff_thorage_phantom_flyable', 36864: 'obj_location_name'}),
    "Boardwalk"     : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'sprint_spartan', 7936: 'ghost', 8192: 'mongoose', 8448: 'exp_fusioncoil', 8449: 'exp_landmine', 8450: 'exp_battery', 8451: 'exp_propanetank', 8704: 'health_station', 8960: 'powerup_blue', 8961: 'powerup_red', 8962: 'powerup_yellow', 9216: 'mancannon_normal', 9217: 'mancannon_heavy', 9218: 'mancannon_light', 9219: 'mancannon_vehicle', 9220: 'mancannon_gravlift', 9472: 'sfx_1', 9473: 'sfx_2', 9474: 'sfx_3', 9475: 'sfx_4', 9476: 'sfx_5', 9477: 'sfx_6', 9728: 'teleporter_reciever', 9729: 'teleporter_sender', 9730: 'teleporter_2way', 9984: 'toys_die', 9985: 'toys_golfball', 9986: 'toys_golfclub', 9987: 'toys_killball', 9988: 'toys_soccerball', 10240: 'ff_light_red', 10241: 'ff_light_blue', 10242: 'ff_light_green', 10243: 'ff_light_orange', 10244: 'ff_light_purple', 10245: 'ff_light_yellow', 10246: 'ff_light_white', 10247: 'ff_light_red_flash', 10248: 'ff_light_yellow_flash', 10496: 'spawning_initial', 10752: 'spawning_respawn', 11008: 'spawning_camera', 11264: 'respawn_zone', 11520: 'respawn_zone_weak', 11776: 'respawn_zone_anti', 12032: 'spawning_safe', 12033: 'spawning_safe_soft', 12288: 'spawning_kill', 12289: 'spawning_kill_soft', 12544: 'obj_flag_base', 12800: 'obj_plate', 13056: 'obj_hill_marker', 13312: 'ff_scn_barricade_small', 13313: 'ff_scn_barricade_large', 13314: 'ff_scn_jerseybarrier', 13315: 'ff_scn_jerseybarrier_short', 13316: 'ff_scn_covenantbarrier', 13317: 'ff_scn_cov_shield', 13568: 'ff_scn_camping_stool', 13824: 'ff_scn_chair_folding', 14080: 'ff_scn_crate_small_closed', 14081: 'ff_scn_crate_metal', 14082: 'ff_scn_crate_metal_single', 14083: 'ff_scn_space_crate', 14084: 'ff_scn_space_crate_small', 14085: 'ff_scn_cov_crate', 14086: 'ff_scn_cov_crate_a', 14087: 'ff_scn_cov_crate_b', 14336: 'ff_scn_dumpster', 14592: 'ff_scn_dumpster_tall', 14848: 'ff_scn_cov_storage', 15104: 'ff_scn_sandbag_wall', 15105: 'ff_scn_sandbag_turret', 15106: 'ff_scn_sandbag_45', 15107: 'ff_scn_sandbag_90', 15108: 'ff_scn_sandbag_end', 15360: 'ff_scn_street_cone', 15616: 'ff_scn_pallet', 15617: 'ff_scn_pallet_large', 15618: 'ff_scn_pallet_metal', 15872: 'block_2x2_invis', 16128: 'block_1x1_invis', 16384: 'block_3x3_invis', 16640: 'block_4x4_invis', 16896: 'block_5x5_flat_invis', 17152: 'block_3x3_flat_invis', 17408: 'block_2x2_flat_invis', 17664: 'block_1x1_flat_invis', 17920: 'block_4x4_flat_invis', 18176: 'block_4x4_tall_invis', 18432: 'block_5x1_flat_invis'}),
    "Boneyard"      : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'sprint_spartan', 7936: 'banshee', 8192: 'ghost', 8448: 'mongoose', 8704: 'scorpion', 8960: 'warthog_default', 8961: 'warthog_gauss', 8962: 'warthog_rocket', 9216: 'wraith', 9472: 'exp_fusioncoil', 9473: 'exp_landmine', 9474: 'exp_battery', 9728: 'health_station', 9984: 'powerup_blue', 9985: 'powerup_red', 9986: 'powerup_yellow', 10240: 'mancannon_normal', 10241: 'mancannon_heavy', 10242: 'mancannon_light', 10243: 'mancannon_vehicle', 10244: 'mancannon_gravlift', 10496: 'teleporter_reciever', 10497: 'teleporter_sender', 10498: 'teleporter_2way', 10752: 'toys_golfclub', 11008: 'ff_light_red', 11009: 'ff_light_blue', 11010: 'ff_light_green', 11011: 'ff_light_orange', 11264: 'spawning_initial', 11520: 'spawning_respawn', 11776: 'spawning_camera', 12032: 'respawn_zone', 12288: 'respawn_zone_weak', 12544: 'respawn_zone_anti', 12800: 'spawning_safe', 12801: 'spawning_safe_soft', 13056: 'spawning_kill', 13057: 'spawning_kill_soft', 13312: 'obj_flag_base', 13568: 'obj_plate', 13824: 'obj_hill_marker', 14080: 'ramp_stunt', 14336: 'door_shield_small', 14592: 'door_shield_medium', 14848: 'door_shield_large', 15104: 'door_oneway_xsmall', 15360: 'door_oneway_small', 15616: 'door_oneway_medium', 15872: 'door_oneway_large', 16128: 'door_oneway_xlarge', 16384: 'ff_scn_laserwall_small', 16640: 'ff_scn_laserwall_medium', 16896: 'ff_scn_laserwall_large', 17152: 'ff_scn_laserwall_xlarge', 17408: 'block_2x2_invis', 17664: 'block_1x1_invis', 17920: 'block_3x3_invis', 18176: 'block_4x4_invis', 18432: 'block_5x5_flat_invis', 18688: 'block_3x3_flat_invis', 18944: 'block_2x2_flat_invis', 19200: 'block_1x1_flat_invis', 19456: 'block_4x4_flat_invis', 19712: 'block_4x4_tall_invis', 19968: 'block_5x1_flat_invis'}),
    "Countdown"     : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'arm_sprint', 7936: 'exp_fusioncoil', 7937: 'exp_landmine', 7938: 'exp_battery', 7939: 'exp_propanetank', 8192: 'health_station', 8448: 'powerup_blue', 8449: 'powerup_red', 8450: 'powerup_yellow', 8704: 'mancannon_normal', 8705: 'mancannon_heavy', 8706: 'mancannon_light', 8707: 'mancannon_vehicle', 8708: 'mancannon_gravlift', 8960: 'sfx_1', 8961: 'sfx_2', 8962: 'sfx_3', 8963: 'sfx_4', 8964: 'sfx_5', 8965: 'sfx_6', 9216: 'teleporter_reciever', 9217: 'teleporter_sender', 9218: 'teleporter_2way', 9472: 'toys_die', 9473: 'toys_golfball', 9474: 'toys_golfclub', 9475: 'toys_killball', 9476: 'toys_soccerball', 9728: 'ff_light_red', 9729: 'ff_light_blue', 9730: 'ff_light_green', 9731: 'ff_light_orange', 9732: 'ff_light_purple', 9733: 'ff_light_yellow', 9734: 'ff_light_white', 9735: 'ff_light_red_flash', 9736: 'ff_light_yellow_flash', 9984: 'spawning_initial', 10240: 'spawning_respawn', 10496: 'spawning_camera', 10752: 'respawn_zone', 11008: 'respawn_zone_weak', 11264: 'respawn_zone_anti', 11520: 'spawning_safe', 11521: 'spawning_safe_soft', 11776: 'spawning_kill', 11777: 'spawning_kill_soft', 12032: 'obj_flag_base', 12288: 'obj_plate', 12544: 'obj_hill_marker', 12800: 'ff_scn_barricade_small', 12801: 'ff_scn_barricade_large', 12802: 'ff_scn_jerseybarrier', 12803: 'ff_scn_jerseybarrier_short', 12804: 'ff_scn_covenantbarrier', 12805: 'ff_scn_cov_shield', 13056: 'ff_scn_camping_stool', 13312: 'ff_scn_chair_folding', 13568: 'ff_scn_crate_small_closed', 13569: 'ff_scn_crate_metal', 13570: 'ff_scn_crate_metal_single', 13571: 'ff_scn_space_crate', 13572: 'ff_scn_space_crate_small', 13573: 'ff_scn_cov_crate', 13574: 'ff_scn_cov_crate_a', 13575: 'ff_scn_cov_crate_b', 13824: 'ff_scn_dumpster', 14080: 'ff_scn_dumpster_tall', 14336: 'ff_scn_sandbag_wall', 14337: 'ff_scn_sandbag_turret', 14338: 'ff_scn_sandbag_45', 14339: 'ff_scn_sandbag_90', 14340: 'ff_scn_sandbag_end', 14592: 'ff_scn_street_cone', 14848: 'ff_scn_pallet', 14849: 'ff_scn_pallet_large', 14850: 'ff_scn_pallet_metal', 15104: 'block_2x2_invis', 15360: 'block_1x1_invis', 15616: 'block_3x3_invis', 15872: 'block_4x4_invis', 16128: 'block_5x5_flat_invis', 16384: 'block_3x3_flat_invis', 16640: 'block_2x2_flat_invis', 16896: 'block_1x1_flat_invis', 17152: 'block_4x4_flat_invis', 17408: 'block_4x4_tall_invis', 17664: 'block_5x1_flat_invis', 17920: 'ff_cex_i_beam', 18176: 'ff_thorage_cover_lg', 18432: 'ff_cex_door_human'}),
    "Powerhouse"    : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'sprint_spartan', 7936: 'ghost', 8192: 'mongoose', 8448: 'exp_fusioncoil', 8449: 'exp_landmine', 8450: 'exp_battery', 8451: 'exp_propanetank', 8704: 'health_station', 8960: 'powerup_blue', 8961: 'powerup_red', 8962: 'powerup_yellow', 9216: 'mancannon_normal', 9217: 'mancannon_heavy', 9218: 'mancannon_light', 9219: 'mancannon_vehicle', 9220: 'mancannon_gravlift', 9472: 'sfx_1', 9473: 'sfx_2', 9474: 'sfx_3', 9475: 'sfx_4', 9476: 'sfx_5', 9477: 'sfx_6', 9728: 'teleporter_reciever', 9729: 'teleporter_sender', 9730: 'teleporter_2way', 9984: 'toys_die', 9985: 'toys_golfball', 9986: 'toys_golfclub', 9987: 'toys_killball', 9988: 'toys_soccerball', 10240: 'ff_light_red', 10241: 'ff_light_blue', 10242: 'ff_light_green', 10243: 'ff_light_orange', 10244: 'ff_light_purple', 10245: 'ff_light_yellow', 10246: 'ff_light_white', 10247: 'ff_light_red_flash', 10248: 'ff_light_yellow_flash', 10496: 'spawning_initial', 10752: 'spawning_respawn', 11008: 'spawning_camera', 11264: 'respawn_zone', 11520: 'respawn_zone_weak', 11776: 'respawn_zone_anti', 12032: 'spawning_safe', 12033: 'spawning_safe_soft', 12288: 'spawning_kill', 12289: 'spawning_kill_soft', 12544: 'obj_flag_base', 12800: 'obj_plate', 13056: 'obj_hill_marker', 13312: 'ff_scn_barricade_small', 13313: 'ff_scn_barricade_large', 13314: 'ff_scn_jerseybarrier', 13315: 'ff_scn_jerseybarrier_short', 13316: 'ff_scn_covenantbarrier', 13317: 'ff_scn_cov_shield', 13568: 'ff_scn_camping_stool', 13824: 'ff_scn_cov_stool', 14080: 'ff_scn_chair_folding', 14336: 'ff_scn_cov_antenna', 14592: 'ff_scn_crate_small_closed', 14593: 'ff_scn_crate_metal', 14594: 'ff_scn_crate_metal_single', 14595: 'ff_scn_space_crate', 14596: 'ff_scn_space_crate_small', 14597: 'ff_scn_cov_crate', 14598: 'ff_scn_cov_crate_a', 14599: 'ff_scn_cov_crate_b', 14848: 'ff_scn_dumpster', 15104: 'ff_scn_dumpster_tall', 15360: 'ff_scn_cov_storage', 15616: 'ff_scn_sandbag_wall', 15617: 'ff_scn_sandbag_turret', 15618: 'ff_scn_sandbag_45', 15619: 'ff_scn_sandbag_90', 15620: 'ff_scn_sandbag_end', 15872: 'ff_scn_street_cone', 16128: 'ff_scn_pallet', 16129: 'ff_scn_pallet_large', 16130: 'ff_scn_pallet_metal', 16384: 'block_2x2_invis', 16640: 'block_1x1_invis', 16896: 'block_3x3_invis', 17152: 'block_4x4_invis', 17408: 'block_5x5_flat_invis', 17664: 'block_3x3_flat_invis', 17920: 'block_2x2_flat_invis', 18176: 'block_1x1_flat_invis', 18432: 'block_4x4_flat_invis', 18688: 'block_4x4_tall_invis', 18944: 'block_5x1_flat_invis'}),
    "Reflection"    : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'sprint_spartan', 7936: 'exp_fusioncoil', 7937: 'exp_landmine', 7938: 'exp_battery', 7939: 'exp_propanetank', 8192: 'health_station', 8448: 'powerup_blue', 8449: 'powerup_red', 8450: 'powerup_yellow', 8704: 'mancannon_normal', 8705: 'mancannon_heavy', 8706: 'mancannon_light', 8707: 'mancannon_vehicle', 8708: 'mancannon_gravlift', 8960: 'sfx_1', 8961: 'sfx_2', 8962: 'sfx_3', 8963: 'sfx_4', 8964: 'sfx_5', 8965: 'sfx_6', 9216: 'teleporter_reciever', 9217: 'teleporter_sender', 9218: 'teleporter_2way', 9472: 'toys_die', 9473: 'toys_golfball', 9474: 'toys_golfclub', 9475: 'toys_killball', 9476: 'toys_soccerball', 9728: 'ff_light_red', 9729: 'ff_light_blue', 9730: 'ff_light_green', 9731: 'ff_light_orange', 9732: 'ff_light_purple', 9733: 'ff_light_yellow', 9734: 'ff_light_white', 9735: 'ff_light_red_flash', 9736: 'ff_light_yellow_flash', 9984: 'spawning_initial', 10240: 'spawning_respawn', 10496: 'spawning_camera', 10752: 'respawn_zone', 11008: 'respawn_zone_weak', 11264: 'respawn_zone_anti', 11520: 'spawning_safe', 11521: 'spawning_safe_soft', 11776: 'spawning_kill', 11777: 'spawning_kill_soft', 12032: 'obj_flag_base', 12288: 'obj_plate', 12544: 'obj_hill_marker', 12800: 'ff_scn_barricade_small', 12801: 'ff_scn_barricade_large', 12802: 'ff_scn_jerseybarrier', 12803: 'ff_scn_jerseybarrier_short', 12804: 'ff_scn_covenantbarrier', 12805: 'ff_scn_cov_shield', 13056: 'ff_scn_camping_stool', 13312: 'ff_scn_chair_folding', 13568: 'ff_scn_crate_small_closed', 13569: 'ff_scn_crate_metal', 13570: 'ff_scn_crate_metal_single', 13571: 'ff_scn_space_crate', 13572: 'ff_scn_space_crate_small', 13573: 'ff_scn_cov_crate', 13574: 'ff_scn_cov_crate_a', 13575: 'ff_scn_cov_crate_b', 13824: 'ff_scn_dumpster_tall', 14080: 'ff_scn_sandbag_wall', 14081: 'ff_scn_sandbag_turret', 14082: 'ff_scn_sandbag_45', 14083: 'ff_scn_sandbag_90', 14084: 'ff_scn_sandbag_end', 14336: 'ff_scn_street_cone', 14592: 'ff_scn_pallet', 14593: 'ff_scn_pallet_large', 14594: 'ff_scn_pallet_metal', 14848: 'block_2x2_invis', 15104: 'block_1x1_invis', 15360: 'block_3x3_invis', 15616: 'block_4x4_invis', 15872: 'block_5x5_flat_invis', 16128: 'block_3x3_flat_invis', 16384: 'block_2x2_flat_invis', 16640: 'block_1x1_flat_invis', 16896: 'block_4x4_flat_invis', 17152: 'block_4x4_tall_invis', 17408: 'block_5x1_flat_invis'}),
    "Spire"         : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'arm_sprint', 7936: 'banshee', 8192: 'falcon', 8448: 'ghost', 8704: 'mongoose', 8960: 'revenant', 9216: 'scorpion', 9472: 'warthog_default', 9473: 'warthog_gauss', 9474: 'warthog_rocket', 9728: 'wraith', 9984: 'exp_fusioncoil', 9985: 'exp_landmine', 9986: 'exp_battery', 10240: 'health_station', 10496: 'powerup_blue', 10497: 'powerup_red', 10498: 'powerup_yellow', 10752: 'mancannon_normal', 10753: 'mancannon_heavy', 10754: 'mancannon_light', 10755: 'mancannon_vehicle', 10756: 'mancannon_gravlift', 11008: 'teleporter_reciever', 11009: 'teleporter_sender', 11010: 'teleporter_2way', 11264: 'toys_golfclub', 11520: 'ff_light_red', 11521: 'ff_light_blue', 11522: 'ff_light_green', 11523: 'ff_light_orange', 11776: 'spawning_initial', 12032: 'spawning_respawn', 12288: 'spawning_camera', 12544: 'respawn_zone', 12800: 'respawn_zone_weak', 13056: 'respawn_zone_anti', 13312: 'spawning_safe', 13313: 'spawning_safe_soft', 13568: 'spawning_kill', 13569: 'spawning_kill_soft', 13824: 'obj_flag_base', 14080: 'obj_plate', 14336: 'obj_hill_marker', 14592: 'ramp_stunt', 14848: 'ff_scn_covenantbarrier', 15104: 'door_shield_small', 15360: 'door_shield_medium', 15616: 'door_shield_large', 15872: 'door_oneway_small', 16128: 'door_oneway_medium', 16384: 'door_oneway_large', 16640: 'ff_scn_laserwall_small', 16896: 'ff_scn_laserwall_medium', 17152: 'ff_scn_laserwall_large', 17408: 'ff_scn_laserwall_xlarge', 17664: 'block_2x2_invis', 17920: 'block_1x1_invis', 18176: 'block_3x3_invis', 18432: 'block_4x4_invis', 18688: 'block_5x5_flat_invis', 18944: 'block_3x3_flat_invis', 19200: 'block_2x2_flat_invis', 19456: 'block_1x1_flat_invis', 19712: 'block_4x4_flat_invis', 19968: 'block_4x4_tall_invis', 20224: 'block_5x1_flat_invis', 20480: 'spire_base_cannon', 20736: 'spire_cliff_cannon', 20992: 'spire_gun_cannon'}),
    "Sword Base"    : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'sprint_spartan', 7936: 'exp_fusioncoil', 7937: 'exp_landmine', 7938: 'exp_battery', 7939: 'exp_propanetank', 8192: 'health_station', 8448: 'powerup_blue', 8449: 'powerup_red', 8450: 'powerup_yellow', 8704: 'mancannon_normal', 8705: 'mancannon_heavy', 8706: 'mancannon_light', 8707: 'mancannon_vehicle', 8708: 'mancannon_gravlift', 8960: 'sfx_1', 8961: 'sfx_2', 8962: 'sfx_3', 8963: 'sfx_4', 8964: 'sfx_5', 8965: 'sfx_6', 9216: 'teleporter_reciever', 9217: 'teleporter_sender', 9218: 'teleporter_2way', 9472: 'toys_die', 9473: 'toys_golfball', 9474: 'toys_golfclub', 9475: 'toys_killball', 9476: 'toys_soccerball', 9728: 'ff_light_red', 9729: 'ff_light_blue', 9730: 'ff_light_green', 9731: 'ff_light_orange', 9732: 'ff_light_purple', 9733: 'ff_light_yellow', 9734: 'ff_light_white', 9735: 'ff_light_red_flash', 9736: 'ff_light_yellow_flash', 9984: 'spawning_initial', 10240: 'spawning_respawn', 10496: 'spawning_camera', 10752: 'respawn_zone', 11008: 'respawn_zone_weak', 11264: 'respawn_zone_anti', 11520: 'spawning_safe', 11521: 'spawning_safe_soft', 11776: 'spawning_kill', 11777: 'spawning_kill_soft', 12032: 'obj_flag_base', 12288: 'obj_plate', 12544: 'obj_hill_marker', 12800: 'ff_scn_barricade_small', 12801: 'ff_scn_barricade_large', 12802: 'ff_scn_jerseybarrier', 12803: 'ff_scn_jerseybarrier_short', 12804: 'ff_scn_covenantbarrier', 12805: 'ff_scn_cov_shield', 13056: 'ff_scn_camping_stool', 13312: 'ff_scn_chair_folding', 13568: 'ff_scn_crate_small_closed', 13569: 'ff_scn_crate_metal', 13570: 'ff_scn_crate_metal_single', 13571: 'ff_scn_space_crate', 13572: 'ff_scn_space_crate_small', 13573: 'ff_scn_cov_crate', 13574: 'ff_scn_cov_crate_a', 13575: 'ff_scn_cov_crate_b', 13824: 'ff_scn_sandbag_wall', 13825: 'ff_scn_sandbag_turret', 13826: 'ff_scn_sandbag_45', 13827: 'ff_scn_sandbag_90', 13828: 'ff_scn_sandbag_end', 14080: 'ff_scn_street_cone', 14336: 'ff_scn_pallet', 14337: 'ff_scn_pallet_large', 14338: 'ff_scn_pallet_metal', 14592: 'block_2x2_invis', 14848: 'block_1x1_invis', 15104: 'block_3x3_invis', 15360: 'block_4x4_invis', 15616: 'block_5x5_flat_invis', 15872: 'block_3x3_flat_invis', 16128: 'block_2x2_flat_invis', 16384: 'block_1x1_flat_invis', 16640: 'block_4x4_flat_invis', 16896: 'block_4x4_tall_invis', 17152: 'block_5x1_flat_invis'}),
    "Zealot"        : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'sprint_spartan', 7936: 'exp_fusioncoil', 7937: 'exp_landmine', 7938: 'exp_battery', 7939: 'exp_propanetank', 8192: 'health_station', 8448: 'powerup_blue', 8449: 'powerup_red', 8450: 'powerup_yellow', 8704: 'mancannon_normal', 8705: 'mancannon_heavy', 8706: 'mancannon_light', 8707: 'mancannon_vehicle', 8708: 'mancannon_gravlift', 8960: 'sfx_1', 8961: 'sfx_2', 8962: 'sfx_3', 8963: 'sfx_4', 8964: 'sfx_5', 8965: 'sfx_6', 9216: 'teleporter_reciever', 9217: 'teleporter_sender', 9218: 'teleporter_2way', 9472: 'toys_die', 9473: 'toys_golfball', 9474: 'toys_golfclub', 9475: 'toys_killball', 9476: 'toys_soccerball', 9728: 'ff_light_red', 9729: 'ff_light_blue', 9730: 'ff_light_green', 9731: 'ff_light_orange', 9732: 'ff_light_purple', 9733: 'ff_light_yellow', 9734: 'ff_light_white', 9735: 'ff_light_red_flash', 9736: 'ff_light_yellow_flash', 9984: 'spawning_initial', 10240: 'spawning_respawn', 10496: 'spawning_camera', 10752: 'respawn_zone', 11008: 'respawn_zone_weak', 11264: 'respawn_zone_anti', 11520: 'spawning_safe', 11521: 'spawning_safe_soft', 11776: 'spawning_kill', 11777: 'spawning_kill_soft', 12032: 'obj_flag_base', 12288: 'obj_plate', 12544: 'obj_hill_marker', 12800: 'ff_scn_barricade_small', 12801: 'ff_scn_barricade_large', 12802: 'ff_scn_covenantbarrier', 12803: 'ff_scn_cov_shield', 13056: 'ff_scn_cov_stool', 13312: 'ff_scn_cov_antenna', 13568: 'ff_scn_space_crate', 13569: 'ff_scn_space_crate_small', 13570: 'ff_scn_cov_crate', 13571: 'ff_scn_cov_crate_a', 13572: 'ff_scn_cov_crate_b', 13824: 'ff_scn_cov_storage', 14080: 'ff_scn_sandbag_wall', 14081: 'ff_scn_sandbag_turret', 14082: 'ff_scn_sandbag_45', 14083: 'ff_scn_sandbag_90', 14084: 'ff_scn_sandbag_end', 14336: 'block_2x2_invis', 14592: 'block_1x1_invis', 14848: 'block_3x3_invis', 15104: 'block_4x4_invis', 15360: 'block_5x5_flat_invis', 15616: 'block_3x3_flat_invis', 15872: 'block_2x2_flat_invis', 16128: 'block_1x1_flat_invis', 16384: 'block_4x4_flat_invis', 16640: 'block_4x4_tall_invis', 16896: 'block_5x1_flat_invis', 17152: 'ff_cex_platform_cov'}),
    "Anchor 9"      : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'sprint_spartan', 7936: 'exp_fusioncoil', 7937: 'exp_landmine', 8192: 'health_station', 8448: 'powerup_blue', 8449: 'powerup_red', 8450: 'powerup_yellow', 8704: 'mancannon_normal', 8705: 'mancannon_heavy', 8706: 'mancannon_light', 8707: 'mancannon_vehicle', 8708: 'mancannon_gravlift', 8960: 'sfx_1', 8961: 'sfx_2', 8962: 'sfx_3', 8963: 'sfx_4', 8964: 'sfx_5', 8965: 'sfx_6', 9216: 'teleporter_reciever', 9217: 'teleporter_sender', 9218: 'teleporter_2way', 9472: 'toys_die', 9473: 'toys_golfball', 9474: 'toys_golfclub', 9475: 'toys_killball', 9476: 'toys_soccerball', 9477: 'toys_tincup', 9728: 'ff_light_red', 9729: 'ff_light_blue', 9730: 'ff_light_green', 9731: 'ff_light_orange', 9732: 'ff_light_purple', 9733: 'ff_light_yellow', 9734: 'ff_light_white', 9735: 'ff_light_red_flash', 9736: 'ff_light_yellow_flash', 9984: 'spawning_initial', 10240: 'spawning_respawn', 10496: 'spawning_camera', 10752: 'respawn_zone', 11008: 'respawn_zone_weak', 11264: 'respawn_zone_anti', 11520: 'spawning_safe', 11521: 'spawning_safe_soft', 11776: 'spawning_kill', 11777: 'spawning_kill_soft', 12032: 'obj_flag_base', 12288: 'obj_plate', 12544: 'obj_hill_marker', 12800: 'dlc_slayer_cart_a', 13056: 'dlc_slayer_cart_b', 13312: 'wall', 13568: 'ff_scn_street_cone', 13824: 'door_shield_small', 13825: 'door_shield_small1', 13826: 'door_shield_large', 13827: 'door_shield_large1', 14080: 'low_grav_volume', 14336: 'sheild_door_large', 14592: 'sheild_door_small', 14848: 'block_2x2_invis', 15104: 'block_1x1_invis', 15360: 'block_3x3_invis', 15616: 'block_4x4_invis', 15872: 'block_5x5_flat_invis', 16128: 'block_3x3_flat_invis', 16384: 'block_2x2_flat_invis', 16640: 'block_1x1_flat_invis', 16896: 'block_4x4_flat_invis', 17152: 'block_4x4_tall_invis', 17408: 'block_5x1_flat_invis', 17664: 'destination_delta'}),
    "Breakpoint"    : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'sprint_spartan', 7936: 'banshee', 8192: 'falcon', 8448: 'ghost', 8704: 'mongoose', 8960: 'warthog_default', 8961: 'warthog_gauss', 8962: 'warthog_rocket', 9216: 'wraith', 9472: 'scorpion', 9728: 'exp_fusioncoil', 9729: 'exp_landmine', 9984: 'health_station', 10240: 'powerup_blue', 10241: 'powerup_red', 10242: 'powerup_yellow', 10496: 'mancannon_normal', 10497: 'mancannon_heavy', 10498: 'mancannon_light', 10499: 'mancannon_vehicle', 10500: 'mancannon_gravlift', 10752: 'sfx_1', 10753: 'sfx_2', 10754: 'sfx_3', 10755: 'sfx_4', 10756: 'sfx_5', 10757: 'sfx_6', 11008: 'teleporter_reciever', 11009: 'teleporter_sender', 11010: 'teleporter_2way', 11264: 'toys_die', 11265: 'toys_golfball', 11266: 'toys_golfclub', 11267: 'toys_killball', 11268: 'toys_soccerball', 11269: 'toys_tincup', 11520: 'ff_light_red', 11521: 'ff_light_blue', 11522: 'ff_light_green', 11523: 'ff_light_orange', 11524: 'ff_light_purple', 11525: 'ff_light_yellow', 11526: 'ff_light_white', 11527: 'ff_light_red_flash', 11528: 'ff_light_yellow_flash', 11776: 'spawning_initial', 12032: 'spawning_respawn', 12288: 'spawning_camera', 12544: 'respawn_zone', 12800: 'respawn_zone_weak', 13056: 'respawn_zone_anti', 13312: 'spawning_safe', 13313: 'spawning_safe_soft', 13568: 'spawning_kill', 13569: 'spawning_kill_soft', 13824: 'obj_flag_base', 14080: 'obj_plate', 14336: 'obj_hill_marker', 14592: 'ff_scn_barricade_small', 14593: 'ff_scn_barricade_large', 14594: 'ff_scn_covenantbarrier', 14595: 'ff_scn_cov_shield', 14596: 'dlc_invasion_heavy_shield', 14597: 'ff_scn_jerseybarrier', 14848: 'ff_scn_space_crate', 14849: 'ff_scn_space_crate_small', 14850: 'ff_scn_cov_crate', 14851: 'ff_scn_cov_crate_a', 15104: 'ff_scn_street_cone', 15360: 'dlc_bridge_down', 15616: 'door', 15617: 'door_oneway_xsmall', 15618: 'door_oneway_small', 15872: 'rock_flat', 16128: 'rock_med1', 16384: 'rock_small', 16640: 'block_2x2_invis', 16896: 'block_1x1_invis', 17152: 'block_3x3_invis', 17408: 'block_4x4_invis', 17664: 'block_5x5_flat_invis', 17920: 'block_3x3_flat_invis', 18176: 'block_2x2_flat_invis', 18432: 'block_1x1_flat_invis', 18688: 'block_4x4_flat_invis', 18944: 'block_4x4_tall_invis', 19200: 'block_5x1_flat_invis', 19456: 'destination_delta', 19712: 'destination_zulu'}),
    "Tempest"       : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'sprint_spartan', 7936: 'banshee', 8192: 'ghost', 8448: 'mongoose', 8704: 'warthog_default', 8705: 'warthog_gauss', 8706: 'warthog_rocket', 8960: 'wraith', 9216: 'scorpion', 9472: 'exp_fusioncoil', 9473: 'exp_landmine', 9474: 'exp_battery', 9475: 'exp_propanetank', 9728: 'health_station', 9984: 'powerup_blue', 9985: 'powerup_red', 9986: 'powerup_yellow', 10240: 'mancannon_normal', 10241: 'mancannon_heavy', 10242: 'mancannon_light', 10243: 'mancannon_vehicle', 10244: 'mancannon_gravlift', 10496: 'door_oneway_small', 10752: 'door_oneway_medium', 11008: 'door_oneway_large', 11264: 'sfx_1', 11265: 'sfx_2', 11266: 'sfx_3', 11267: 'sfx_4', 11268: 'sfx_5', 11269: 'sfx_6', 11270: 'purple', 11271: 'green', 11272: 'orange', 11520: 'door_shield_small', 11776: 'door_shield_medium', 12032: 'door_shield_large', 12288: 'teleporter_reciever', 12289: 'teleporter_sender', 12290: 'teleporter_2way', 12544: 'toys_die', 12545: 'toys_golfball', 12546: 'toys_golfclub', 12547: 'toys_killball', 12548: 'toys_soccerball', 12549: 'toys_tincup', 12800: 'ff_light_red', 12801: 'ff_light_blue', 12802: 'ff_light_green', 12803: 'ff_light_orange', 12804: 'ff_light_purple', 12805: 'ff_light_yellow', 12806: 'ff_light_white', 12807: 'ff_light_red_flash', 12808: 'ff_light_yellow_flash', 13056: 'spawning_initial', 13312: 'spawning_respawn', 13568: 'spawning_camera', 13824: 'respawn_zone', 14080: 'respawn_zone_weak', 14336: 'respawn_zone_anti', 14592: 'spawning_safe', 14593: 'spawning_safe_soft', 14848: 'spawning_kill', 14849: 'spawning_kill_soft', 15104: 'obj_flag_base', 15360: 'obj_plate', 15616: 'obj_hill_marker', 15872: 'ff_scn_barricade_small', 15873: 'ff_scn_barricade_large', 15874: 'ff_scn_covenantbarrier', 16128: 'ff_scn_space_crate', 16129: 'ff_scn_space_crate_small', 16130: 'ff_scn_cov_crate', 16131: 'ff_scn_cov_crate_a', 16384: 'ff_scn_sandbag_wall', 16385: 'ff_scn_sandbag_turret', 16386: 'ff_scn_sandbag_45', 16387: 'ff_scn_sandbag_90', 16388: 'ff_scn_sandbag_end', 16640: 'ff_scn_street_cone', 16896: 'dlc_medium_driftwood_a', 16897: 'dlc_medium_driftwood_b', 16898: 'dlc_medium_driftwood_c', 17152: 'block_1x1', 17153: 'block_1x1_flat', 17154: 'block_1x1_short', 17155: 'block_1x1_tall', 17156: 'block_1x1_tall_thin', 17157: 'block_1x2', 17158: 'block_1x4', 17159: 'block_2x1_flat', 17160: 'block_2x2', 17161: 'block_2x2_flat', 17162: 'block_2x2_short', 17163: 'block_2x2_tall', 17164: 'block_2x3', 17165: 'block_2x4', 17166: 'block_3x1_flat', 17167: 'block_3x3', 17168: 'block_3x3_flat', 17169: 'block_3x3_short', 17170: 'block_3x3_tall', 17171: 'block_3x4', 17172: 'block_4x4', 17173: 'block_4x4_flat', 17174: 'block_4x4_short', 17175: 'block_4x4_tall', 17176: 'block_5x1_flat', 17177: 'block_5x5_flat', 17408: 'bridge_2x1', 17409: 'bridge_2x2', 17410: 'bridge_2x3', 17411: 'bridge_2x5', 17412: 'bridge_diag', 17413: 'bridge_diag_short', 17414: 'dish', 17415: 'dish_door', 17416: 'corner_45', 17417: 'plat_2x2_corner', 17418: 'plat_4x4_corner', 17419: 'plat_landing_pad', 17420: 'plat_ramped', 17421: 'plat_4x4_tower', 17422: 'cylinder_small', 17423: 'cylinder_large', 17424: 'plat_y', 17425: 'plat_y_large', 17426: 'sniper_nest', 17427: 'stair_case', 17428: 'walkway', 17664: 'bunker_low', 17665: 'bunker_covered', 17666: 'bunker_full', 17667: 'bunker_gulch', 17668: 'bunker_stairs', 17669: 'pyramid', 17670: 'tower', 17671: 'tower_3_story', 17672: 'tower_tall', 17673: 'room_double', 17674: 'room_triple', 17920: 'antenna', 17921: 'antenna_dish', 17922: 'brace', 17923: 'brace_large', 17924: 'brace_tunnel', 17925: 'ff_column', 17926: 'crenelation', 17927: 'crenelation_notched', 17928: 'cover_glass', 17929: 'wtf_is_this_thing', 17930: 'railing_short', 17931: 'railing_medium', 17932: 'railing_long', 17933: 'teleporter_frame', 17934: 'ff_strut', 17935: 'walkway_cover', 18176: 'door', 18177: 'door_double', 18178: 'window', 18179: 'window_double', 18180: 'wall', 18181: 'wall_double', 18182: 'wall_corner', 18183: 'wall_curved', 18184: 'wall_coliseum', 18185: 'window_coliseum', 18186: 'tunnel_short', 18187: 'tunnel_long', 18432: 'bank_1x1', 18433: 'bank_1x2', 18434: 'bank_2x1', 18435: 'bank_2x2', 18436: 'ramp_1x2', 18437: 'ramp_1x2_shallow', 18438: 'ramp_2x2', 18439: 'ramp_2x2_steep', 18440: 'ramp_circular_small', 18441: 'ramp_circular_large', 18442: 'ramp_bridge_small', 18443: 'ramp_bridge_medium', 18444: 'ramp_bridge_large', 18445: 'ramp_4x2', 18446: 'ramp_stunt', 18688: 'rock_small', 18689: 'rock_flat', 18690: 'rock_med1', 18691: 'rock_med2', 18692: 'rock_spire1', 18693: 'rock_spire2', 18694: 'rock_arch', 18695: 'dlc_medium_stone_a', 18696: 'dlc_medium_stone_b', 18944: 'ff_grid', 19200: 'block_2x2_invis', 19456: 'block_1x1_invis', 19712: 'block_3x3_invis', 19968: 'block_4x4_invis', 20224: 'block_5x5_flat_invis', 20480: 'block_3x3_flat_invis', 20736: 'block_2x2_flat_invis', 20992: 'block_1x1_flat_invis', 21248: 'block_4x4_flat_invis', 21504: 'block_4x4_tall_invis', 21760: 'block_5x1_flat_invis', 22016: 'destination_delta', 22272: 'ff_thorage_falcon_default', 22273: 'ff_thorage_falcon_grenade', 22274: 'ff_thorage_falcon_no_guns', 22528: 'ff_thorage_warthog_troop', 22784: 'ff_thorage_sabre', 23040: 'ff_thorage_seraph', 23296: 'ff_thorage_electric_cart', 23552: 'ff_thorage_forklift', 23808: 'ff_thorage_pickup', 24064: 'ff_thorage_truck_cab', 24320: 'ff_thorage_oni_van', 24576: 'ff_thorage_shade_flak_cannon', 24832: 'mancannon_normal_forerunner', 24833: 'mancannon_heavy_forerunner', 24834: 'mancannon_light_forerunner', 24835: 'ff_cex_gravlift_forerunner', 24836: 'ff_cex_gravlift_tall_forerunner', 24837: 'ff_cex_mancannon_human', 25088: 'door_oneway_xsmall', 25344: 'door_oneway_xlarge', 25600: 'ff_scn_laserwall_small', 25856: 'ff_scn_laserwall_medium', 26112: 'ff_scn_laserwall_large', 26368: 'ff_scn_laserwall_xlarge', 26624: 'door_oneway_small_mcc', 26880: 'door_oneway_medium_mcc', 27136: 'door_oneway_large_mcc', 27392: 'door_shield_small_mcc', 27648: 'door_shield_small1_mcc', 27904: 'door_shield_large_mcc', 28160: 'door_shield_large1_mcc', 28416: 'ff_thorage_ammo_box', 28672: 'ff_thorage_rocket_ammo', 28928: 'ff_thorage_sniper_ammo', 29184: 'ff_scn_jerseybarrier', 29185: 'ff_scn_jerseybarrier_short', 29186: 'dlc_invasion_heavy_shield', 29440: 'ff_scn_crate_small_closed', 29441: 'ff_scn_crate_metal', 29442: 'ff_scn_crate_metal_single', 29443: 'ff_scn_cov_crate_b', 29444: 'ff_cex_crate_forerunner_sm', 29445: 'ff_cex_crate_forerunner_lg', 29696: 'ff_scn_pallet', 29697: 'ff_scn_pallet_large', 29698: 'ff_scn_pallet_metal', 29952: 'ff_thorage_phantom', 29953: 'ff_thorage_spirit', 29954: 'ff_thorage_pelican', 29955: 'ff_thorage_drop_pod_elite', 29956: 'ff_thorage_anti_air_cannon', 30208: 'ff_thorage_cargo_truck_destroyed', 30209: 'ff_thorage_falcon_destroyed', 30210: 'ff_thorage_warthog_destroyed', 30464: 'ff_scn_chair_folding', 30720: 'ff_scn_dumpster', 30976: 'ff_scn_dumpster_tall', 31232: 'ff_thorage_equipment_case', 31488: 'ff_thorage_monitor_flush', 31744: 'ff_scn_cov_storage', 32000: 'ff_scn_cov_stool', 32256: 'ff_scn_cov_antenna', 32512: 'ff_thorage_fuel_storage', 32768: 'dlc_slayer_cart_a', 33024: 'dlc_slayer_cart_b', 33280: 'dlc_bridge_down', 33281: 'ff_cex_platform_cov', 33282: 'ff_cex_catwalk_straight', 33283: 'ff_cex_catwalk_short', 33284: 'ff_cex_catwalk_bend_left', 33285: 'ff_cex_catwalk_bend_right', 33286: 'ff_cex_catwalk_angled', 33287: 'ff_cex_catwalk_lg', 33536: 'ff_cex_bunker', 33537: 'ff_cex_gunner_nest', 33792: 'ff_cex_cover_sm', 33793: 'ff_cex_large_block', 33794: 'ff_cex_hallway_blocker', 33795: 'ff_cex_stone_column', 33796: 'ff_cex_tombstone', 33797: 'ff_cex_stone_block', 33798: 'ff_cex_cover_lg', 33799: 'ff_thorage_cover_sm', 33800: 'ff_cex_walkway_cover_short', 33801: 'ff_thorage_cover_lg', 33802: 'ff_cex_i_beam', 34048: 'wall_mcc', 34049: 'door_mcc', 34050: 'ff_cex_door_human', 34051: 'ff_cex_door_forerunner_a', 34052: 'ff_cex_door_forerunner_b', 34053: 'ff_thorage_door_c', 34054: 'ff_thorage_door_d', 34055: 'ff_thorage_door_e', 34056: 'ff_thorage_door_f', 34057: 'ff_thorage_door_g', 34058: 'ff_thorage_door_h', 34059: 'ff_cex_wall_sm_forerunner', 34060: 'ff_cex_wall_lg_forerunner', 34304: 'ff_cex_tree_life', 34560: 'ff_thorage_generator', 34816: 'ff_thorage_vending_machine', 35072: 'ff_thorage_dinghy', 35328: 'ff_thorage_airstrike', 35584: 'ff_thorage_pelican_flyable', 35840: 'ff_thorage_phantom_flyable', 36096: 'falcon', 36352: 'revenant', 36608: 'shade', 36864: 'obj_location_name'}),
    "Condemned"     : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'sprint_spartan', 7936: 'exp_fusioncoil', 7937: 'exp_propanetank', 8192: 'health_station', 8448: 'powerup_blue', 8449: 'powerup_red', 8450: 'powerup_yellow', 8704: 'mancannon_normal', 8705: 'mancannon_heavy', 8706: 'mancannon_light', 8707: 'mancannon_vehicle', 8708: 'mancannon_gravlift', 8960: 'sfx_1', 8961: 'sfx_2', 8962: 'sfx_3', 8963: 'sfx_4', 8964: 'sfx_5', 8965: 'sfx_6', 9216: 'teleporter_reciever', 9217: 'teleporter_sender', 9218: 'teleporter_2way', 9472: 'toys_die', 9473: 'toys_golfball', 9474: 'toys_golfclub', 9475: 'toys_killball', 9476: 'toys_soccerball', 9477: 'toys_tincup', 9728: 'ff_light_red', 9729: 'ff_light_blue', 9730: 'ff_light_green', 9731: 'ff_light_orange', 9732: 'ff_light_purple', 9733: 'ff_light_yellow', 9734: 'ff_light_white', 9735: 'ff_light_red_flash', 9736: 'ff_light_yellow_flash', 9984: 'spawning_initial', 10240: 'spawning_respawn', 10496: 'spawning_camera', 10752: 'respawn_zone', 11008: 'respawn_zone_weak', 11264: 'respawn_zone_anti', 11520: 'spawning_safe', 11521: 'spawning_safe_soft', 11776: 'spawning_kill', 11777: 'spawning_kill_soft', 12032: 'obj_flag_base', 12288: 'obj_plate', 12544: 'obj_hill_marker', 12800: 'ff_scn_barricade_small', 12801: 'ff_scn_barricade_large', 12802: 'ff_scn_jerseybarrier', 12803: 'ff_scn_jerseybarrier_short', 12804: 'ff_scn_covenantbarrier', 12805: 'ff_scn_cov_shield', 13056: 'ff_scn_camping_stool', 13312: 'ff_scn_cov_stool', 13568: 'ff_scn_chair_folding', 13824: 'ff_scn_crate_small_closed', 13825: 'ff_scn_crate_metal', 13826: 'ff_scn_crate_metal_single', 13827: 'ff_scn_space_crate', 13828: 'ff_scn_space_crate_small', 13829: 'ff_scn_cov_crate', 13830: 'ff_scn_cov_crate_a', 13831: 'ff_scn_cov_crate_b', 14080: 'ff_scn_cov_storage', 14336: 'ff_scn_sandbag_wall', 14337: 'ff_scn_sandbag_turret', 14338: 'ff_scn_sandbag_45', 14339: 'ff_scn_sandbag_90', 14340: 'ff_scn_sandbag_end', 14592: 'ff_scn_street_cone', 14848: 'ff_scn_pallet', 14849: 'ff_scn_pallet_large', 14850: 'ff_scn_pallet_metal', 15104: 'block_2x2_invis', 15360: 'block_1x1_invis', 15616: 'block_3x3_invis', 15872: 'block_4x4_invis', 16128: 'block_5x5_flat_invis', 16384: 'block_3x3_flat_invis', 16640: 'block_2x2_flat_invis', 16896: 'block_1x1_flat_invis', 17152: 'block_4x4_flat_invis', 17408: 'block_4x4_tall_invis', 17664: 'block_5x1_flat_invis', 17920: 'wall', 17921: 'wall_double', 17922: 'wall_corner', 17923: 'wall_curved', 17924: 'wall_coliseum', 17925: 'door_shield_small', 17926: 'red', 17927: 'blue'}),
    "Highlands"     : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'sprint_spartan', 7936: 'banshee', 8192: 'falcon', 8448: 'ghost', 8704: 'mongoose', 8960: 'revenant', 9216: 'scorpion', 9472: 'shade', 9728: 'warthog_default', 9729: 'warthog_gauss', 9730: 'warthog_rocket', 9984: 'wraith', 10240: 'exp_fusioncoil', 10241: 'exp_landmine', 10496: 'health_station', 10752: 'powerup_blue', 10753: 'powerup_red', 10754: 'powerup_yellow', 11008: 'mancannon_normal', 11009: 'mancannon_heavy', 11010: 'mancannon_light', 11011: 'mancannon_vehicle', 11012: 'mancannon_gravlift', 11264: 'teleporter_reciever', 11265: 'teleporter_sender', 11266: 'teleporter_2way', 11520: 'toys_die', 11521: 'toys_golfball', 11522: 'toys_golfclub', 11523: 'toys_killball', 11524: 'toys_soccerball', 11525: 'toys_tincup', 11776: 'ff_light_red', 11777: 'ff_light_blue', 11778: 'ff_light_green', 11779: 'ff_light_orange', 12032: 'spawning_initial', 12288: 'spawning_respawn', 12544: 'spawning_camera', 12800: 'respawn_zone', 13056: 'respawn_zone_weak', 13312: 'respawn_zone_anti', 13568: 'spawning_safe', 13569: 'spawning_safe_soft', 13824: 'spawning_kill', 13825: 'spawning_kill_soft', 14080: 'obj_flag_base', 14336: 'obj_plate', 14592: 'obj_hill_marker', 14848: 'ff_scn_barricade_small', 14849: 'ff_scn_barricade_large', 14850: 'ff_scn_jerseybarrier', 14851: 'ff_scn_jerseybarrier_short', 14852: 'ff_scn_covenantbarrier', 14853: 'ff_scn_cov_shield', 15104: 'ff_scn_camping_stool', 15360: 'ff_scn_chair_folding', 15616: 'ff_scn_crate_small_closed', 15617: 'ff_scn_crate_metal', 15618: 'ff_scn_crate_metal_single', 15619: 'ff_scn_space_crate', 15620: 'ff_scn_space_crate_small', 15621: 'ff_scn_cov_crate', 15622: 'ff_scn_cov_crate_a', 15623: 'ff_scn_cov_crate_b', 15872: 'ff_scn_dumpster_tall', 16128: 'ff_scn_sandbag_wall', 16129: 'ff_scn_sandbag_turret', 16130: 'ff_scn_sandbag_45', 16131: 'ff_scn_sandbag_90', 16132: 'ff_scn_sandbag_end', 16384: 'ff_scn_street_cone', 16640: 'ff_scn_pallet', 16641: 'ff_scn_pallet_large', 16642: 'ff_scn_pallet_metal', 16896: 'block_2x2_invis', 17152: 'block_1x1_invis', 17408: 'block_3x3_invis', 17664: 'block_4x4_invis', 17920: 'block_5x5_flat_invis', 18176: 'block_3x3_flat_invis', 18432: 'block_2x2_flat_invis', 18688: 'block_1x1_flat_invis', 18944: 'block_4x4_flat_invis', 19200: 'block_4x4_tall_invis', 19456: 'block_5x1_flat_invis'}),
    "Battle Canyon" : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'arm_sprint', 7936: 'exp_fusioncoil', 7937: 'exp_landmine', 7938: 'exp_battery', 7939: 'exp_propanetank', 8192: 'health_station', 8448: 'powerup_blue', 8449: 'powerup_red', 8450: 'powerup_yellow', 8704: 'mancannon_normal', 8705: 'mancannon_heavy', 8706: 'mancannon_light', 8707: 'mancannon_vehicle', 8708: 'mancannon_gravlift', 8709: 'ff_cex_gravlift_forerunner', 8710: 'ff_cex_gravlift_tall_forerunner', 8960: 'sfx_1', 8961: 'sfx_2', 8962: 'sfx_3', 8963: 'sfx_4', 8964: 'sfx_5', 8965: 'sfx_6', 9216: 'teleporter_reciever', 9217: 'teleporter_sender', 9218: 'teleporter_2way', 9472: 'toys_die', 9473: 'toys_golfball', 9474: 'toys_golfclub', 9475: 'toys_killball', 9476: 'toys_soccerball', 9477: 'toys_tincup', 9728: 'ff_light_red', 9729: 'ff_light_blue', 9730: 'ff_light_green', 9731: 'ff_light_orange', 9732: 'ff_light_purple', 9733: 'ff_light_yellow', 9734: 'ff_light_white', 9735: 'ff_light_red_flash', 9736: 'ff_light_yellow_flash', 9984: 'spawning_initial', 10240: 'spawning_respawn', 10496: 'spawning_camera', 10752: 'respawn_zone', 11008: 'respawn_zone_weak', 11264: 'respawn_zone_anti', 11520: 'spawning_safe', 11521: 'spawning_safe_soft', 11776: 'spawning_kill', 11777: 'spawning_kill_soft', 12032: 'obj_flag_base', 12288: 'obj_plate', 12544: 'obj_hill_marker', 12800: 'ff_scn_barricade_small', 12801: 'ff_scn_barricade_large', 12802: 'ff_scn_jerseybarrier', 12803: 'ff_scn_jerseybarrier_short', 12804: 'ff_scn_covenantbarrier', 12805: 'ff_scn_cov_shield', 13056: 'ff_scn_camping_stool', 13312: 'ff_scn_chair_folding', 13568: 'ff_scn_crate_metal', 13569: 'ff_scn_crate_metal_single', 13570: 'ff_scn_space_crate', 13571: 'ff_scn_space_crate_small', 13572: 'ff_scn_cov_crate', 13573: 'ff_scn_cov_crate_a', 13574: 'ff_scn_cov_crate_b', 13824: 'ff_scn_sandbag_wall', 13825: 'ff_scn_sandbag_turret', 13826: 'ff_scn_sandbag_45', 13827: 'ff_scn_sandbag_90', 13828: 'ff_scn_sandbag_end', 14080: 'ff_scn_street_cone', 14336: 'ff_scn_pallet', 14337: 'ff_scn_pallet_large', 14338: 'ff_scn_pallet_metal', 14592: 'block_1x1', 14593: 'block_1x1_flat', 14594: 'block_1x1_short', 14595: 'block_1x1_tall', 14596: 'block_1x1_tall_thin', 14597: 'block_1x2', 14598: 'block_1x4', 14599: 'block_2x1_flat', 14600: 'block_2x2', 14601: 'block_2x2_flat', 14602: 'block_2x2_short', 14603: 'block_2x2_tall', 14604: 'block_2x3', 14605: 'block_2x4', 14606: 'block_3x1_flat', 14848: 'bridge_2x1', 14849: 'bridge_2x2', 14850: 'bridge_2x3', 14851: 'corner_45', 14852: 'plat_2x2_corner', 15104: 'bunker_low', 15105: 'bunker_covered', 15360: 'brace', 15361: 'ff_column', 15362: 'crenelation', 15363: 'crenelation_notched', 15364: 'railing_short', 15365: 'railing_medium', 15366: 'railing_long', 15367: 'teleporter_frame', 15616: 'ff_cex_door_forerunner1', 15617: 'ff_cex_door_forerunner2', 15872: 'ramp_1x2', 15873: 'ramp_1x2_shallow', 15874: 'ramp_2x2', 15875: 'ramp_bridge_small', 16128: 'rock_small', 16129: 'rock_flat', 16130: 'rock_med1', 16131: 'rock_spire2', 16132: 'ff_cex_rock_cluster', 16133: 'ff_cex_rock_blocker', 16384: 'block_2x2_invis', 16640: 'block_1x1_invis', 16896: 'block_3x3_invis', 17152: 'block_4x4_invis', 17408: 'block_5x5_flat_invis', 17664: 'block_3x3_flat_invis', 17920: 'block_2x2_flat_invis', 18176: 'block_1x1_flat_invis', 18432: 'block_4x4_flat_invis', 18688: 'block_4x4_tall_invis', 18944: 'block_5x1_flat_invis', 19200: 'ff_cex_health_cabinet'}),
    "Penance"       : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'arm_sprint', 7936: 'exp_fusioncoil', 7937: 'exp_landmine', 7938: 'exp_battery', 7939: 'exp_propanetank', 8192: 'health_station', 8448: 'powerup_blue', 8449: 'powerup_red', 8450: 'powerup_yellow', 8704: 'mancannon_normal', 8705: 'mancannon_heavy', 8706: 'mancannon_light', 8707: 'mancannon_vehicle', 8708: 'mancannon_gravlift', 8709: 'ff_cex_gravlift_cov', 8960: 'sfx_1', 8961: 'sfx_2', 8962: 'sfx_3', 8963: 'sfx_4', 8964: 'sfx_5', 8965: 'sfx_6', 9216: 'teleporter_reciever', 9217: 'teleporter_sender', 9218: 'teleporter_2way', 9472: 'toys_die', 9473: 'toys_golfball', 9474: 'toys_golfclub', 9475: 'toys_killball', 9476: 'toys_soccerball', 9477: 'toys_tincup', 9728: 'ff_light_red', 9729: 'ff_light_blue', 9730: 'ff_light_green', 9731: 'ff_light_orange', 9732: 'ff_light_purple', 9733: 'ff_light_yellow', 9734: 'ff_light_white', 9735: 'ff_light_red_flash', 9736: 'ff_light_yellow_flash', 9984: 'spawning_initial', 10240: 'spawning_respawn', 10496: 'spawning_camera', 10752: 'respawn_zone', 11008: 'respawn_zone_weak', 11264: 'respawn_zone_anti', 11520: 'spawning_safe', 11521: 'spawning_safe_soft', 11776: 'spawning_kill', 11777: 'spawning_kill_soft', 12032: 'obj_flag_base', 12288: 'obj_plate', 12544: 'obj_hill_marker', 12800: 'ff_scn_barricade_small', 12801: 'ff_scn_barricade_large', 12802: 'ff_scn_covenantbarrier', 12803: 'ff_scn_cov_shield', 13056: 'ff_scn_cov_stool', 13312: 'ff_scn_cov_antenna', 13568: 'ff_scn_space_crate', 13569: 'ff_scn_space_crate_small', 13570: 'ff_scn_cov_crate', 13571: 'ff_scn_cov_crate_a', 13572: 'ff_scn_cov_crate_b', 13824: 'ff_scn_cov_storage', 14080: 'ff_scn_sandbag_wall', 14081: 'ff_scn_sandbag_turret', 14082: 'ff_scn_sandbag_45', 14083: 'ff_scn_sandbag_90', 14084: 'ff_scn_sandbag_end', 14336: 'ff_cex_platform_cov', 14592: 'block_2x2_invis', 14848: 'block_1x1_invis', 15104: 'block_3x3_invis', 15360: 'block_4x4_invis', 15616: 'block_5x5_flat_invis', 15872: 'block_3x3_flat_invis', 16128: 'block_2x2_flat_invis', 16384: 'block_1x1_flat_invis', 16640: 'block_4x4_flat_invis', 16896: 'block_4x4_tall_invis', 17152: 'block_5x1_flat_invis', 17408: 'ff_cex_health_cabinet'}),
    "Ridgeline"     : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'arm_sprint', 7936: 'exp_fusioncoil', 7937: 'exp_landmine', 7938: 'exp_battery', 7939: 'exp_propanetank', 8192: 'health_station', 8448: 'powerup_blue', 8449: 'powerup_red', 8450: 'powerup_yellow', 8704: 'mancannon_normal', 8705: 'mancannon_heavy', 8706: 'mancannon_light', 8707: 'mancannon_vehicle', 8708: 'mancannon_gravlift', 8960: 'sfx_1', 8961: 'sfx_2', 8962: 'sfx_3', 8963: 'sfx_4', 8964: 'sfx_5', 8965: 'sfx_6', 9216: 'teleporter_reciever', 9217: 'teleporter_sender', 9218: 'teleporter_2way', 9472: 'toys_die', 9473: 'toys_golfball', 9474: 'toys_golfclub', 9475: 'toys_killball', 9476: 'toys_soccerball', 9477: 'toys_tincup', 9728: 'ff_light_red', 9729: 'ff_light_blue', 9730: 'ff_light_green', 9731: 'ff_light_orange', 9732: 'ff_light_purple', 9733: 'ff_light_yellow', 9734: 'ff_light_white', 9735: 'ff_light_red_flash', 9736: 'ff_light_yellow_flash', 9984: 'spawning_initial', 10240: 'spawning_respawn', 10496: 'spawning_camera', 10752: 'respawn_zone', 11008: 'respawn_zone_weak', 11264: 'respawn_zone_anti', 11520: 'spawning_safe', 11521: 'spawning_safe_soft', 11776: 'spawning_kill', 11777: 'spawning_kill_soft', 12032: 'obj_flag_base', 12288: 'obj_plate', 12544: 'obj_hill_marker', 12800: 'ff_scn_barricade_small', 12801: 'ff_scn_barricade_large', 12802: 'ff_scn_jerseybarrier', 12803: 'ff_scn_jerseybarrier_short', 12804: 'ff_scn_covenantbarrier', 12805: 'ff_scn_cov_shield', 13056: 'ff_scn_crate_small_closed', 13057: 'ff_scn_crate_metal', 13058: 'ff_scn_crate_metal_single', 13059: 'ff_scn_space_crate', 13060: 'ff_scn_space_crate_small', 13061: 'ff_scn_cov_crate', 13062: 'ff_scn_cov_crate_a', 13063: 'ff_scn_cov_crate_b', 13312: 'ff_scn_pallet', 13313: 'ff_scn_pallet_large', 13314: 'ff_scn_pallet_metal', 13568: 'ff_scn_sandbag_wall', 13569: 'ff_scn_sandbag_turret', 13570: 'ff_scn_sandbag_45', 13571: 'ff_scn_sandbag_90', 13572: 'ff_scn_sandbag_end', 13824: 'banshee', 14080: 'falcon', 14336: 'ghost', 14592: 'mongoose', 14848: 'revenant', 15104: 'scorpion', 15360: 'warthog_default', 15361: 'warthog_gauss', 15362: 'warthog_rocket', 15616: 'wraith', 15872: 'shade', 16128: 'block_1x1', 16129: 'block_1x1_flat', 16130: 'block_1x1_short', 16131: 'block_1x1_tall', 16132: 'block_1x1_tall_thin', 16133: 'block_1x2', 16134: 'block_1x4', 16135: 'block_2x1_flat', 16136: 'block_2x2', 16137: 'block_2x2_flat', 16138: 'block_2x2_short', 16139: 'block_2x2_tall', 16140: 'block_2x3', 16141: 'block_2x4', 16142: 'block_3x1_flat', 16143: 'block_3x3', 16144: 'block_3x3_flat', 16145: 'block_3x3_short', 16146: 'block_3x3_tall', 16147: 'block_3x4', 16148: 'block_4x4', 16149: 'block_4x4_flat', 16150: 'block_4x4_short', 16151: 'block_4x4_tall', 16152: 'block_5x1_flat', 16153: 'block_5x5_flat', 16384: 'bridge_2x1', 16385: 'bridge_2x2', 16386: 'bridge_2x3', 16387: 'bridge_2x5', 16388: 'bridge_diag', 16389: 'bridge_diag_short', 16390: 'corner_45', 16391: 'plat_2x2_corner', 16392: 'plat_4x4_corner', 16393: 'plat_landing_pad', 16394: 'plat_ramped', 16395: 'plat_4x4_tower', 16396: 'cylinder_small', 16397: 'plat_y', 16398: 'plat_y_large', 16399: 'sniper_nest', 16400: 'walkway', 16640: 'bunker_low', 16641: 'bunker_covered', 16642: 'bunker_full', 16643: 'bunker_stairs', 16644: 'tower', 16645: 'tower_3_story', 16646: 'tower_tall', 16647: 'room_double', 16648: 'ff_cex_bunker', 16649: 'ff_cex_gunner_nest', 16896: 'antenna', 16897: 'brace', 16898: 'ff_column', 16899: 'crenelation', 16900: 'crenelation_notched', 16901: 'railing_short', 16902: 'railing_medium', 16903: 'railing_long', 16904: 'teleporter_frame', 16905: 'ff_strut', 16906: 'walkway_cover', 16907: 'ff_cex_cover_sm', 17152: 'door', 17153: 'door_double', 17154: 'window', 17155: 'window_double', 17156: 'wall', 17157: 'wall_double', 17158: 'wall_corner', 17159: 'wall_curved', 17160: 'tunnel_short', 17161: 'tunnel_long', 17408: 'ramp_1x2', 17409: 'ramp_1x2_shallow', 17410: 'ramp_2x2', 17411: 'ramp_2x2_steep', 17412: 'ramp_circular_small', 17413: 'ramp_bridge_small', 17414: 'ramp_bridge_medium', 17415: 'ramp_bridge_large', 17416: 'ramp_4x2', 17664: 'ff_cex_rock_small', 17665: 'ff_cex_rock_flat', 17666: 'ff_cex_rock_med_a', 17667: 'ff_cex_rock_med_b', 17668: 'ff_cex_rock_spire_a', 17669: 'ff_cex_rock_spire_b', 17670: 'ff_cex_rock_seastack', 17671: 'ff_cex_rock_arch', 17920: 'ff_grid', 18176: 'ff_cex_tree_life', 18432: 'block_2x2_invis', 18688: 'block_1x1_invis', 18944: 'block_3x3_invis', 19200: 'block_4x4_invis', 19456: 'block_5x5_flat_invis', 19712: 'block_3x3_flat_invis', 19968: 'block_2x2_flat_invis', 20224: 'block_1x1_flat_invis', 20480: 'block_4x4_flat_invis', 20736: 'block_4x4_tall_invis', 20992: 'block_5x1_flat_invis', 21248: 'ff_cex_health_cabinet', 21504: 'ff_thorage_falcon_default', 21505: 'ff_thorage_falcon_grenade', 21506: 'ff_thorage_falcon_no_guns', 21760: 'ff_thorage_warthog_troop', 22016: 'ff_thorage_sabre', 22272: 'ff_thorage_seraph', 22528: 'ff_thorage_electric_cart', 22784: 'ff_thorage_forklift', 23040: 'ff_thorage_pickup', 23296: 'ff_thorage_truck_cab', 23552: 'ff_thorage_oni_van', 23808: 'ff_thorage_shade_flak_cannon'}),
    "Solitary"      : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'arm_sprint', 7936: 'exp_fusioncoil', 7937: 'exp_landmine', 7938: 'exp_battery', 7939: 'exp_propanetank', 8192: 'health_station', 8448: 'powerup_blue', 8449: 'powerup_red', 8450: 'powerup_yellow', 8704: 'mancannon_normal', 8705: 'mancannon_heavy', 8706: 'mancannon_light', 8707: 'mancannon_vehicle', 8708: 'mancannon_gravlift', 8960: 'sfx_1', 8961: 'sfx_2', 8962: 'sfx_3', 8963: 'sfx_4', 8964: 'sfx_5', 8965: 'sfx_6', 9216: 'teleporter_reciever', 9217: 'teleporter_sender', 9218: 'teleporter_2way', 9472: 'toys_die', 9473: 'toys_golfball', 9474: 'toys_golfclub', 9475: 'toys_killball', 9476: 'toys_soccerball', 9477: 'toys_tincup', 9728: 'ff_light_red', 9729: 'ff_light_blue', 9730: 'ff_light_green', 9731: 'ff_light_orange', 9732: 'ff_light_purple', 9733: 'ff_light_yellow', 9734: 'ff_light_white', 9735: 'ff_light_red_flash', 9736: 'ff_light_yellow_flash', 9984: 'spawning_initial', 10240: 'spawning_respawn', 10496: 'spawning_camera', 10752: 'respawn_zone', 11008: 'respawn_zone_weak', 11264: 'respawn_zone_anti', 11520: 'spawning_safe', 11521: 'spawning_safe_soft', 11776: 'spawning_kill', 11777: 'spawning_kill_soft', 12032: 'obj_flag_base', 12288: 'obj_plate', 12544: 'obj_hill_marker', 12800: 'ff_scn_barricade_small', 12801: 'ff_scn_barricade_large', 12802: 'ff_scn_jerseybarrier', 12803: 'ff_scn_jerseybarrier_short', 12804: 'ff_scn_covenantbarrier', 12805: 'ff_scn_cov_shield', 13056: 'ff_scn_camping_stool', 13312: 'ff_scn_chair_folding', 13568: 'ff_scn_crate_metal', 13569: 'ff_scn_space_crate', 13570: 'ff_scn_space_crate_small', 13571: 'ff_scn_cov_crate', 13572: 'ff_scn_cov_crate_a', 13573: 'ff_scn_cov_crate_b', 13574: 'ff_cex_crate_forerunner_sm', 13575: 'ff_cex_crate_forerunner_lg', 13824: 'ff_scn_dumpster', 14080: 'ff_scn_dumpster_tall', 14336: 'ff_scn_sandbag_wall', 14337: 'ff_scn_sandbag_turret', 14338: 'ff_scn_sandbag_45', 14339: 'ff_scn_sandbag_90', 14340: 'ff_scn_sandbag_end', 14592: 'ff_scn_street_cone', 14848: 'ff_scn_pallet', 14849: 'ff_scn_pallet_large', 14850: 'ff_scn_pallet_metal', 15104: 'block_1x1', 15105: 'block_1x1_flat', 15106: 'block_1x1_short', 15107: 'block_1x1_tall', 15108: 'block_1x1_tall_thin', 15109: 'block_1x2', 15110: 'block_1x4', 15111: 'block_2x1_flat', 15112: 'block_2x2', 15113: 'block_2x2_flat', 15114: 'block_2x2_short', 15115: 'block_2x2_tall', 15116: 'block_2x3', 15117: 'block_2x4', 15118: 'block_3x1_flat', 15119: 'block_3x3', 15120: 'block_3x3_flat', 15121: 'block_3x3_short', 15122: 'block_3x4', 15123: 'block_4x4', 15124: 'block_4x4_flat', 15125: 'block_4x4_short', 15126: 'block_5x1_flat', 15360: 'bridge_2x1', 15361: 'bridge_2x2', 15362: 'bridge_2x3', 15363: 'bridge_2x5', 15364: 'bridge_diag', 15365: 'bridge_diag_short', 15366: 'corner_45', 15367: 'plat_2x2_corner', 15368: 'plat_4x4_corner', 15369: 'plat_y', 15370: 'ff_cex_catwalk_straight', 15371: 'ff_cex_catwalk_short', 15372: 'ff_cex_catwalk_bend_left', 15373: 'ff_cex_catwalk_bend_right', 15616: 'antenna', 15617: 'brace', 15618: 'ff_column', 15619: 'crenelation', 15620: 'crenelation_notched', 15621: 'cover_glass', 15622: 'wtf_is_this_thing', 15623: 'railing_short', 15624: 'railing_medium', 15625: 'railing_long', 15626: 'teleporter_frame', 15627: 'ff_strut', 15628: 'ff_cex_cover_sm', 15872: 'door', 15873: 'door_double', 15874: 'window', 15875: 'window_double', 15876: 'wall', 15877: 'wall_double', 15878: 'wall_corner', 15879: 'wall_curved', 15880: 'tunnel_short', 15881: 'tunnel_long', 15882: 'ff_cex_door_forerunner_a', 15883: 'ff_cex_door_forerunner_b', 16128: 'bank_1x1', 16129: 'bank_1x2', 16130: 'bank_2x1', 16131: 'bank_2x2', 16132: 'ramp_1x2', 16133: 'ramp_1x2_shallow', 16134: 'ramp_2x2', 16135: 'ramp_2x2_steep', 16136: 'ramp_circular_small', 16137: 'ramp_bridge_small', 16138: 'ramp_bridge_medium', 16139: 'ramp_bridge_large', 16384: 'ff_grid', 16640: 'block_2x2_invis', 16896: 'block_1x1_invis', 17152: 'block_3x3_invis', 17408: 'block_4x4_invis', 17664: 'block_5x5_flat_invis', 17920: 'block_3x3_flat_invis', 18176: 'block_2x2_flat_invis', 18432: 'block_1x1_flat_invis', 18688: 'block_4x4_flat_invis', 18944: 'block_4x4_tall_invis', 19200: 'block_5x1_flat_invis', 19456: 'ff_cex_health_cabinet', 19712: 'ff_short_grav_lift', 19713: 'ff_tall_grav_lift'}),
    "High Noon"     : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'sprint_spartan', 7936: 'exp_fusioncoil', 7937: 'exp_landmine', 7938: 'exp_battery', 7939: 'exp_propanetank', 8192: 'health_station', 8448: 'powerup_blue', 8449: 'powerup_red', 8450: 'powerup_yellow', 8704: 'mancannon_normal', 8705: 'mancannon_heavy', 8706: 'mancannon_light', 8707: 'mancannon_vehicle', 8708: 'mancannon_gravlift', 8709: 'ff_cex_gravlift_forerunner', 8710: 'ff_cex_gravlift_tall_forerunner', 8960: 'sfx_1', 8961: 'sfx_2', 8962: 'sfx_3', 8963: 'sfx_4', 8964: 'sfx_5', 8965: 'sfx_6', 9216: 'teleporter_reciever', 9217: 'teleporter_sender', 9218: 'teleporter_2way', 9472: 'toys_die', 9473: 'toys_golfball', 9474: 'toys_golfclub', 9475: 'toys_killball', 9476: 'toys_soccerball', 9477: 'toys_tincup', 9728: 'ff_light_red', 9729: 'ff_light_blue', 9730: 'ff_light_green', 9731: 'ff_light_orange', 9732: 'ff_light_purple', 9733: 'ff_light_yellow', 9734: 'ff_light_white', 9735: 'ff_light_red_flash', 9736: 'ff_light_yellow_flash', 9984: 'spawning_initial', 10240: 'spawning_respawn', 10496: 'spawning_camera', 10752: 'respawn_zone', 11008: 'respawn_zone_weak', 11264: 'respawn_zone_anti', 11520: 'spawning_safe', 11521: 'spawning_safe_soft', 11776: 'spawning_kill', 11777: 'spawning_kill_soft', 12032: 'obj_flag_base', 12288: 'obj_plate', 12544: 'obj_hill_marker', 12800: 'ff_scn_barricade_small', 12801: 'ff_scn_barricade_large', 12802: 'ff_scn_jerseybarrier', 12803: 'ff_scn_jerseybarrier_short', 12804: 'ff_scn_covenantbarrier', 12805: 'ff_scn_cov_shield', 13056: 'ff_scn_camping_stool', 13312: 'ff_scn_chair_folding', 13568: 'ff_scn_crate_small_closed', 13569: 'ff_scn_crate_metal', 13570: 'ff_scn_crate_metal_single', 13571: 'ff_scn_space_crate', 13572: 'ff_scn_space_crate_small', 13573: 'ff_scn_cov_crate', 13574: 'ff_scn_cov_crate_a', 13575: 'ff_scn_cov_crate_b', 13824: 'ff_scn_sandbag_wall', 13825: 'ff_scn_sandbag_turret', 13826: 'ff_scn_sandbag_45', 13827: 'ff_scn_sandbag_90', 13828: 'ff_scn_sandbag_end', 14080: 'ff_scn_street_cone', 14336: 'ff_scn_pallet', 14337: 'ff_scn_pallet_large', 14338: 'ff_scn_pallet_metal', 14592: 'block_1x1', 14593: 'block_1x1_flat', 14594: 'block_1x1_short', 14595: 'block_1x1_tall', 14596: 'block_1x1_tall_thin', 14597: 'block_1x2', 14598: 'block_1x4', 14599: 'block_2x1_flat', 14600: 'block_2x2', 14601: 'block_2x2_flat', 14602: 'block_2x2_short', 14603: 'block_2x2_tall', 14604: 'block_2x3', 14605: 'block_2x4', 14606: 'block_3x1_flat', 14607: 'block_3x3', 14608: 'block_3x3_flat', 14609: 'block_3x3_short', 14610: 'block_3x3_tall', 14611: 'ff_cex_large_block', 14612: 'ff_cex_hallway_blocker', 14613: 'ff_cex_door_forerunner_a', 14614: 'ff_cex_door_forerunner_b', 14848: 'bridge_2x1', 14849: 'bridge_2x2', 14850: 'bridge_2x3', 14851: 'bridge_2x5', 14852: 'bridge_diag', 14853: 'bridge_diag_short', 14854: 'corner_45', 14855: 'plat_2x2_corner', 14856: 'plat_4x4_corner', 14857: 'plat_landing_pad', 14858: 'plat_ramped', 14859: 'ff_cex_ramp_forerunner', 14860: 'plat_y', 14861: 'plat_y_large', 14862: 'sniper_nest', 14863: 'ff_cex_catwalk_angled', 14864: 'ff_cex_catwalk_lg', 15104: 'bunker_low', 15105: 'bunker_covered', 15106: 'bunker_full', 15360: 'antenna', 15361: 'antenna_dish', 15362: 'brace', 15363: 'brace_large', 15364: 'brace_tunnel', 15365: 'ff_column', 15366: 'ff_cex_stone_column', 15367: 'crenelation', 15368: 'crenelation_notched', 15369: 'cover_glass', 15370: 'railing_short', 15371: 'railing_medium', 15372: 'railing_long', 15373: 'teleporter_frame', 15374: 'ff_strut', 15375: 'ff_cex_walkway_cover_short', 15376: 'ff_cex_tombstone', 15377: 'ff_cex_stone_block', 15616: 'door', 15617: 'door_double', 15618: 'window', 15619: 'window_double', 15620: 'wall', 15621: 'wall_double', 15622: 'wall_corner', 15623: 'wall_curved', 15624: 'tunnel_short', 15625: 'tunnel_long', 15626: 'ff_cex_wall_sm_forerunner', 15627: 'ff_cex_wall_lg_forerunner', 15628: 'ff_cex_door_forerunner1', 15629: 'ff_cex_door_forerunner2', 15630: 'ff_cex_cover_lg', 15872: 'bank_1x1', 15873: 'bank_1x2', 15874: 'bank_2x1', 15875: 'bank_2x2', 15876: 'ramp_1x2', 15877: 'ramp_1x2_shallow', 15878: 'ramp_2x2', 15879: 'ramp_2x2_steep', 15880: 'ramp_bridge_small', 15881: 'ramp_bridge_medium', 15882: 'ramp_bridge_large', 15883: 'ramp_stunt', 16128: 'ff_grid', 16384: 'block_2x2_invis', 16640: 'block_1x1_invis', 16896: 'block_3x3_invis', 17152: 'block_4x4_invis', 17408: 'block_5x5_flat_invis', 17664: 'block_3x3_flat_invis', 17920: 'block_2x2_flat_invis', 18176: 'block_1x1_flat_invis', 18432: 'block_4x4_flat_invis', 18688: 'block_4x4_tall_invis', 18944: 'block_5x1_flat_invis', 19200: 'ff_cex_health_cabinet'}),
    "Breakneck"     : TwoWayDict({0: 'assault_rifle', 256: 'dmr', 512: 'grenade_launcher', 768: 'magnum', 1024: 'rocket_launcher', 1280: 'shotgun', 1536: 'sniper_rifle', 1792: 'spartan_laser', 2048: 'wep_nade_frag', 2304: 'wep_machinegun', 2560: 'concussion_rifle', 2816: 'energy_sword', 3072: 'wep_flakcannon', 3328: 'gravity_hammer', 3584: 'focus_rifle', 3840: 'needle_rifle', 4096: 'needler', 4352: 'wep_plasmalauncher', 4608: 'plasma_pistol', 4864: 'plasma_repeater', 5120: 'plasma_rifle', 5376: 'spike_rifle', 5632: 'wep_nade_plasma', 5888: 'wep_plasmaturret', 6144: 'active_camoflage', 6400: 'armor_lockup', 6656: 'drop_shield', 6912: 'evade_elite', 7168: 'hologram', 7424: 'jet_pack', 7680: 'arm_sprint', 7936: 'exp_fusioncoil', 7937: 'exp_landmine', 7938: 'exp_battery', 7939: 'exp_propanetank', 8192: 'health_station', 8448: 'powerup_blue', 8449: 'powerup_red', 8450: 'powerup_yellow', 8704: 'mancannon_normal', 8705: 'mancannon_heavy', 8706: 'mancannon_light', 8707: 'mancannon_vehicle', 8708: 'ff_cex_mancannon_human', 8709: 'mancannon_gravlift', 8960: 'sfx_1', 8961: 'sfx_2', 8962: 'sfx_3', 8963: 'sfx_4', 8964: 'sfx_5', 8965: 'sfx_6', 9216: 'teleporter_reciever', 9217: 'teleporter_sender', 9218: 'teleporter_2way', 9472: 'toys_die', 9473: 'toys_golfball', 9474: 'toys_golfclub', 9475: 'toys_killball', 9476: 'toys_soccerball', 9477: 'toys_tincup', 9728: 'ff_light_red', 9729: 'ff_light_blue', 9730: 'ff_light_green', 9731: 'ff_light_orange', 9732: 'ff_light_purple', 9733: 'ff_light_yellow', 9734: 'ff_light_white', 9735: 'ff_light_red_flash', 9736: 'ff_light_yellow_flash', 9984: 'spawning_initial', 10240: 'spawning_respawn', 10496: 'spawning_camera', 10752: 'respawn_zone', 11008: 'respawn_zone_weak', 11264: 'respawn_zone_anti', 11520: 'spawning_safe', 11521: 'spawning_safe_soft', 11776: 'spawning_kill', 11777: 'spawning_kill_soft', 12032: 'obj_flag_base', 12288: 'obj_plate', 12544: 'obj_hill_marker', 12800: 'ff_scn_barricade_small', 12801: 'ff_scn_barricade_large', 12802: 'ff_scn_jerseybarrier', 12803: 'ff_scn_jerseybarrier_short', 12804: 'ff_scn_covenantbarrier', 12805: 'ff_scn_cov_shield', 13056: 'ff_scn_crate_small_closed', 13057: 'ff_scn_crate_metal', 13058: 'ff_scn_crate_metal_single', 13059: 'ff_scn_space_crate', 13060: 'ff_scn_space_crate_small', 13061: 'ff_scn_cov_crate', 13062: 'ff_scn_cov_crate_a', 13063: 'ff_scn_cov_crate_b', 13312: 'ff_scn_sandbag_wall', 13313: 'ff_scn_sandbag_turret', 13314: 'ff_scn_sandbag_45', 13315: 'ff_scn_sandbag_90', 13316: 'ff_scn_sandbag_end', 13568: 'ff_scn_street_cone', 13824: 'ff_scn_pallet', 13825: 'ff_scn_pallet_large', 13826: 'ff_scn_pallet_metal', 14080: 'banshee', 14336: 'ghost', 14592: 'mongoose', 14848: 'revenant', 15104: 'warthog_default', 15105: 'warthog_gauss', 15106: 'warthog_rocket', 15360: 'wraith', 15616: 'falcon', 15872: 'scorpion', 16128: 'block_1x1', 16129: 'block_1x1_flat', 16130: 'block_1x1_short', 16131: 'block_1x1_tall', 16132: 'block_1x1_tall_thin', 16133: 'block_1x2', 16134: 'block_1x4', 16135: 'block_2x1_flat', 16136: 'block_2x2', 16137: 'block_2x2_flat', 16138: 'block_2x2_short', 16139: 'block_2x2_tall', 16384: 'bridge_2x1', 16385: 'bridge_2x2', 16386: 'bridge_2x3', 16387: 'bridge_2x5', 16388: 'bridge_diag', 16389: 'bridge_diag_short', 16390: 'corner_45', 16391: 'plat_2x2_corner', 16392: 'plat_4x4_corner', 16640: 'bunker_low', 16641: 'bunker_covered', 16896: 'antenna', 16897: 'ff_column', 16898: 'railing_short', 16899: 'railing_medium', 16900: 'railing_long', 16901: 'teleporter_frame', 16902: 'ff_strut', 16903: 'ff_cex_cover_lg', 16904: 'ff_cex_i_beam', 17152: 'wall', 17153: 'wall_double', 17154: 'wall_corner', 17155: 'wall_curved', 17156: 'ff_cex_door_human', 17408: 'ramp_1x2', 17409: 'ramp_1x2_shallow', 17410: 'ramp_2x2', 17411: 'ramp_2x2_steep', 17412: 'ramp_circular_small', 17413: 'ramp_circular_large', 17414: 'ramp_bridge_small', 17415: 'ramp_bridge_medium', 17416: 'ramp_bridge_large', 17664: 'ff_grid', 17920: 'block_2x2_invis', 18176: 'block_1x1_invis', 18432: 'block_3x3_invis', 18688: 'block_4x4_invis', 18944: 'block_5x5_flat_invis', 19200: 'block_3x3_flat_invis', 19456: 'block_2x2_flat_invis', 19712: 'block_1x1_flat_invis', 19968: 'block_4x4_flat_invis', 20224: 'block_4x4_tall_invis', 20480: 'block_5x1_flat_invis', 20736: 'ff_cex_health_cabinet', 20992: 'ff_thorage_falcon_default', 20993: 'ff_thorage_falcon_grenade', 20994: 'ff_thorage_falcon_no_guns', 21248: 'ff_thorage_warthog_troop', 21504: 'ff_thorage_electric_cart', 21760: 'ff_thorage_forklift', 22016: 'ff_thorage_pickup', 22272: 'ff_thorage_truck_cab', 22528: 'ff_thorage_oni_van', 22784: 'ff_thorage_shade_flak_cannon', 23040: 'door_oneway_xsmall', 23296: 'door_oneway_small', 23552: 'door_oneway_medium', 23808: 'door_oneway_large', 24064: 'door_oneway_xlarge', 24320: 'ff_scn_laserwall_small', 24576: 'ff_scn_laserwall_medium', 24832: 'ff_scn_laserwall_large', 25088: 'ff_scn_laserwall_xlarge', 25344: 'ff_thorage_ammo_box', 25600: 'ff_thorage_rocket_ammo', 25856: 'ff_thorage_sniper_ammo', 26112: 'dlc_invasion_heavy_shield', 26368: 'ff_thorage_phantom', 26369: 'ff_thorage_spirit', 26370: 'ff_thorage_pelican', 26371: 'ff_thorage_drop_pod_elite', 26372: 'ff_thorage_anti_air_cannon', 26624: 'ff_thorage_cargo_truck_destroyed', 26625: 'ff_thorage_falcon_destroyed', 26626: 'ff_thorage_warthog_destroyed', 26880: 'ff_scn_chair_folding', 27136: 'ff_scn_dumpster', 27392: 'ff_scn_dumpster_tall', 27648: 'ff_thorage_equipment_case', 27904: 'ff_thorage_monitor_flush', 28160: 'ff_scn_cov_storage', 28416: 'ff_scn_cov_stool', 28672: 'ff_scn_cov_antenna', 28928: 'ff_thorage_fuel_storage', 29184: 'dlc_slayer_cart_a', 29440: 'dlc_slayer_cart_b', 29696: 'wall_mcc', 29697: 'door'}),
}

type_to_name = TwoWayDict({
    'assault_rifle'           : "Assault Rifle",
    'dmr'                     : "DMR",
    'grenade_launcher'        : "Grenade Launcher",
    'magnum'                  : "Magnum",
    'rocket_launcher'         : "Rocket Launcher", 
    'shotgun'                 : "Shotgun",
    'sniper_rifle'            : "Sniper Rifle",
    'spartan_laser'           : "Spartan Laser",
    'wep_nade_frag'           : "Frag Grenade",
    'wep_machinegun'          : "Mounted Machinegun",
    'concussion_rifle'        : "Concussion Rifle",
    'energy_sword'            : "Energy Sword",
    'wep_flakcannon'          : "Fuel Rod Gun",
    'gravity_hammer'          : "Gravity Hammer",
    'focus_rifle'             : "Focus Rifle",
    'needle_rifle'            : "Needle Rifle",
    'needler'                 : "Needler",
    'wep_plasmalauncher'      : "Plasma Launcher",
    'plasma_pistol'           : "Plasma Pistol",
    'plasma_repeater'         : "Plasma Repeater",
    'plasma_rifle'            : "Plasma Rifle",
    'spike_rifle'             : "Spiker",
    'wep_nade_plasma'         : "Plasma Grenade",
    'wep_plasmaturret'        : "Plasma Turret",
    'active_camoflage'        : "Active Camouflage",
    'armor_lockup'            : "Armor Lock",
    'drop_shield'             : "Drop Shield",
    'evade_elite'             : "Evade",
    'hologram'                : "Hologram",
    'jet_pack'                : "Jet Pack",
    'sprint_spartan'          : "Sprint",
    'arm_sprint'              : "Sprint",
    'banshee'                 : "Banshee",
    'falcon'                  : "Falcon",
    'ghost'                   : "Ghost",
    'mongoose'                : "Mongoose",
    'revenant'                : "Revenant",
    'scorpion'                : "Scorpion",
    'shade'                   : "Shade Turret",
    'warthog_default'         : "Warthog, Default",
    'warthog_gauss'           : "Warthog, Gauss",
    'warthog_rocket'          : "Warthog, Rocket",
    'wraith'                  : "Wraith",
    'exp_fusioncoil'          : "Fusion Coil",
    'exp_landmine'            : "Landmine",
    'exp_battery'             : "Plasma Battery",
    'exp_propanetank'         : "Propane Tank",
    'health_station'          : "Health Station",
    'powerup_blue'            : "Camo Powerup",
    'powerup_red'             : "Overshield",
    'powerup_yellow'          : "Custom Powerup",
    'mancannon_normal'        : "Cannon, Man",
    'mancannon_heavy'         : "Cannon, Man, Heavy",
    'mancannon_light'         : "Cannon, Man, Light",
    'mancannon_vehicle'       : "Cannon, Vehicle",
    'mancannon_gravlift'      : "Gravity Lift",
    'ff_cex_gravlift_cov'     : "Gravity Lift, Covenant",
    'ff_cex_gravlift_forerunner' : "Gravity Lift, Forerunner",
    'ff_cex_gravlift_tall_forerunner' : "Gravity Lift, Tall, Forerunner",
    'door_oneway_small'       : "One Way Shield 2",
    'door_oneway_medium'      : "One Way Shield 3",
    'door_oneway_large'       : "One Way Shield 4",
    'sfx_1'                   : "FX:Colorblind",
    'sfx_2'                   : "FX:Next Gen",
    'sfx_3'                   : "FX:Juicy",
    'sfx_4'                   : "FX:Nova",
    'sfx_5'                   : "FX:Olde Timey",
    'sfx_6'                   : "FX:Pen And Ink",
    'purple'                  : "FX:Purple",
    'green'                   : "FX:Green",
    'orange'                  : "FX:Orange",
    'door_shield_small'       : "Shield Door, Small",
    'door_shield_medium'      : "Shield Door, Medium",
    'door_shield_large'       : "Shield Door, Large",
    'teleporter_reciever'     : "Receiver Node",
    'teleporter_sender'       : "Sender Node",
    'teleporter_2way'         : "Two-Way Node",
    'toys_die'                : "Die",
    'toys_golfball'           : "Golf Ball",
    'toys_golfclub'           : "Golf Club",
    'toys_killball'           : "Kill Ball",
    'toys_soccerball'         : "Soccer Ball",
    'toys_tincup'             : "Tin Cup",
    'ff_light_red'            : "Light, Red",
    'ff_light_blue'           : "Light, Blue",
    'ff_light_green'          : "Light, Green",
    'ff_light_orange'         : "Light, Orange",
    'ff_light_purple'         : "Light, Purple",
    'ff_light_yellow'         : "Light, Yellow",
    'ff_light_white'          : "Light, White",
    'ff_light_red_flash'      : "Light, Red, Flashing",
    'ff_light_yellow_flash'   : "Light, Yellow, Flashing",
    'spawning_initial'        : "Initial Spawn",
    'spawning_respawn'        : "Respawn Point",
    'spawning_camera'         : "Initial Loadout Camera",
    'respawn_zone'            : "Respawn Zone",
    'respawn_zone_weak'       : "Respawn Zone, Weak",
    'respawn_zone_anti'       : "Respawn Zone, Anti",
    'spawning_safe'           : "Safe Boundary",
    'spawning_safe_soft'      : "Soft Safe Boundary",
    'spawning_kill'           : "Kill Boundary",
    'spawning_kill_soft'      : "Soft Kill Boundary",
    'obj_flag_base'           : "Flag Stand",
    'obj_plate'               : "Capture Plate",
    'obj_hill_marker'         : "Hill Marker",
    'ff_scn_barricade_small'  : "Barricade, Small",
    'ff_scn_barricade_large'  : "Barricade, Large",
    'ff_scn_covenantbarrier'  : "Covenant Barrier",
    'ff_scn_cov_shield'       : "Portable Shield",
    'ff_scn_camping_stool'    : "Camping Stool",
    'ff_scn_space_crate'      : "Crate, Heavy Duty",
    'ff_scn_space_crate_small': "Crate, Heavy, Small",
    'ff_scn_cov_crate'        : "Covenant Crate",
    'ff_scn_cov_crate_a'      : "Crate, Half Open",
    'ff_scn_sandbag_wall'     : "Sandbag Wall",
    'ff_scn_sandbag_turret'   : "Sandbag, Turret Wall",
    'ff_scn_sandbag_45'       : "Sandbag Corner, 45",
    'ff_scn_sandbag_90'       : "Sandbag Corner, 90",
    'ff_scn_sandbag_end'      : "Sandbag Endcap",
    'ff_scn_street_cone'      : "Street Cone",
    'block_1x1'               : "Block, 1x1",
    'block_1x1_flat'          : "Block, 1x1, Flat",
    'block_1x1_short'         : "Block, 1x1, Short",
    'block_1x1_tall'          : "Block, 1x1, Tall",
    'block_1x1_tall_thin'     : "Block, 1x1, Tall And Thin",
    'block_1x2'               : "Block, 1x2",
    'block_1x4'               : "Block, 1x4",
    'block_2x1_flat'          : "Block, 2x1, Flat",
    'block_2x2'               : "Block, 2x2",
    'block_2x2_flat'          : "Block, 2x2, Flat",
    'block_2x2_short'         : "Block, 2x2, Short",
    'block_2x2_tall'          : "Block, 2x2, Tall",
    'block_2x3'               : "Block, 2x3",
    'block_2x4'               : "Block, 2x4",
    'block_3x1_flat'          : "Block, 3x1, Flat",
    'block_3x3'               : "Block, 3x3",
    'block_3x3_flat'          : "Block, 3x3, Flat",
    'block_3x3_short'         : "Block, 3x3, Short",
    'block_3x3_tall'          : "Block, 3x3, Tall",
    'block_3x4'               : "Block, 3x4",
    'block_4x4'               : "Block, 4x4",
    'block_4x4_flat'          : "Block, 4x4, Flat",
    'block_4x4_short'         : "Block, 4x4, Short",
    'block_4x4_tall'          : "Block, 4x4, Tall",
    'block_5x1_flat'          : "Block, 5x1, Short",
    'block_5x5_flat'          : "Block, 5x5, Flat",
    'bridge_2x1'              : "Bridge, Small",
    'bridge_2x2'              : "Bridge, Medium",
    'bridge_2x3'              : "Bridge, Large",
    'bridge_2x5'              : "Bridge, XLarge",
    'bridge_diag'             : "Bridge, Diagonal",
    'bridge_diag_short'       : "Bridge, Diag, Small",
    'dish'                    : "Dish",
    'dish_door'               : "Dish, Open",
    'corner_45'               : "Corner, 45 Degrees",
    'plat_2x2_corner'         : "Corner, 2x2",
    'plat_4x4_corner'         : "Corner, 4x4",
    'plat_landing_pad'        : "Landing Pad",
    'plat_ramped'             : "Platform, Ramped",
    'plat_4x4_tower'          : "Platform, Large",
    'cylinder_small'          : "Platform, XL",
    'cylinder_large'          : "Platform, XXL",
    'plat_y'                  : "Platform, Y",
    'plat_y_large'            : "Platform, Y, Large",
    'sniper_nest'             : "Sniper Nest",
    'stair_case'              : "Staircase",
    'walkway'                 : "Walkway, Large",
    'bunker_low'              : "Bunker, Small",
    'bunker_covered'          : "Bunker, Small, Covered",
    'bunker_full'             : "Bunker, Box",
    'bunker_gulch'            : "Bunker, Round",
    'bunker_stairs'           : "Bunker, Ramp",
    'pyramid'                 : "Pyramid",
    'tower'                   : "Tower, 2 Story",
    'tower_3_story'           : "Tower, 3 Story",
    'tower_tall'              : "Tower, Tall",
    'room_double'             : "Room, Double",
    'room_triple'             : "Room, Triple",
    'antenna'                 : "Antenna, Small",
    'antenna_dish'            : "Antenna, Satellite",
    'brace'                   : "Brace",
    'brace_large'             : "Brace, Large",
    'brace_tunnel'            : "Brace, Tunnel",
    'ff_column'               : "Column",
    'crenelation'             : "Cover",
    'crenelation_notched'     : "Cover, Crenellation",
    'cover_glass'             : "Cover, Glass",
    'wtf_is_this_thing'       : "Glass Sail",
    'railing_short'           : "Railing, Small",
    'railing_medium'          : "Railing, Medium",
    'railing_long'            : "Railing, Long",
    'teleporter_frame'        : "Teleporter Frame",
    'ff_strut'                : "Strut",
    'walkway_cover'           : "Large Walkway Cover",
    'door'                    : "Door",
    'door_double'             : "Door, Double",
    'window'                  : "Window",
    'window_double'           : "Window, Double",
    'wall'                    : "Wall",
    'wall_double'             : "Wall, Double",
    'wall_corner'             : "Wall, Corner",
    'wall_curved'             : "Wall, Curved",
    'wall_coliseum'           : "Wall, Coliseum",
    'window_coliseum'         : "Window, Colesium",
    'tunnel_short'            : "Tunnel, Short",
    'tunnel_long'             : "Tunnel, Long",
    'bank_1x1'                : "Bank, 1x1",
    'bank_1x2'                : "Bank, 1x2",
    'bank_2x1'                : "Bank, 2x1",
    'bank_2x2'                : "Bank, 2x2",
    'ramp_1x2'                : "Ramp, 1x2",
    'ramp_1x2_shallow'        : "Ramp, 1x2, Shallow",
    'ramp_2x2'                : "Ramp, 2x2",
    'ramp_2x2_steep'          : "Ramp, 2x2, Steep",
    'ramp_circular_small'     : "Ramp, Circular, Small",
    'ramp_circular_large'     : "Ramp, Circular, Large",
    'ramp_bridge_small'       : "Ramp, Bridge, Small",
    'ramp_bridge_medium'      : "Ramp, Bridge, Medium",
    'ramp_bridge_large'       : "Ramp, Bridge, Large",
    'ramp_4x2'                : "Ramp, XL",
    'ramp_stunt'              : "Ramp, Stunt",
    'rock_small'              : "Rock, Small",
    'dlc_medium_stone_a'      : "Rock, Small 1",
    'rock_flat'               : "Rock, Flat",
    'rock_med1'               : "Rock, Medium 1",
    'rock_med2'               : "Rock, Medium 2",
    'rock_spire1'             : "Rock, Spire 1",
    'rock_spire2'             : "Rock, Spire 2",
    'rock_seastack'           : "Rock, Seastack",
    'rock_arch'               : "Rock, Arch",
    'ff_grid'                 : "Grid",
    'block_2x2_invis'         : "Block, 2x2, Invisible",
    'block_1x1_invis'         : "Block, 1x1, Invisible",
    'block_3x3_invis'         : "Block, 2x2x2, Invisible",
    'block_4x4_invis'         : "Block, 4x4x2, Invisible",
    'block_5x5_flat_invis'    : "Block, 4x4x4, Invisible",
    'block_3x3_flat_invis'    : "Block, 2x1, Flat, Invisible",
    'block_2x2_flat_invis'    : "Block, 1x1, Flat, Invisible",
    'block_1x1_flat_invis'    : "Block, 1x1, Small, Invisible",
    'block_4x4_flat_invis'    : "Block, 2x2, Flat, Invisible",
    'block_4x4_tall_invis'    : "Block, 4x2, Flat, Invisible",
    'block_5x1_flat_invis'    : "Block, 4x4, Flat, Invisible",
    'ff_thorage_falcon_default'       : "Falcon, Nose Gun",
    'ff_thorage_falcon_grenade'       : "Falcon, Grenadier",
    'ff_thorage_falcon_no_guns'       : "Falcon, Transport",
    'ff_thorage_warthog_troop'        : "Warthog, Transport",
    'ff_thorage_sabre'                : "Sabre",
    'ff_thorage_seraph'               : "Seraph",
    'ff_thorage_electric_cart'        : "Cart, Electric",
    'ff_thorage_forklift'             : "Forklift",
    'ff_thorage_pickup'               : "Pickup",
    'ff_thorage_truck_cab'            : "Truck Cab",
    'ff_thorage_oni_van'              : "Van, Oni",
    'ff_thorage_shade_flak_cannon'    : "Shade, Fuel Rod",
    'mancannon_normal_forerunner'     : "Cannon, Man, Forerunner",
    'mancannon_heavy_forerunner'      : "Cannon, Man, Heavy, Forerunner",
    'mancannon_light_forerunner'      : "Cannon, Man, Light, Forerunner",
    'ff_cex_mancannon_human'          : "Cannon, Man, Human",
    'door_oneway_xsmall'              : "One Way Shield 1",
    'door_oneway_xlarge'              : "One Way Shield 5",
    'ff_scn_laserwall_small'          : "Shield Wall, Small",
    'ff_scn_laserwall_medium'         : "Shield Wall, Medium",
    'ff_scn_laserwall_large'          : "Shield Wall, Large",
    'ff_scn_laserwall_xlarge'         : "Shield Wall, X-Large",
    'door_oneway_small_mcc'           : "One Way Shield 2 (MCC)",
    'door_oneway_medium_mcc'          : "One Way Shield 3 (MCC)",
    'door_oneway_large_mcc'           : "One Way Shield 4 (MCC)",
    'door_shield_small_mcc'           : "Shield Door, Small (MCC)",
    'door_shield_small1_mcc'          : "Shield Door, Small 1 (MCC)",
    'door_shield_large_mcc'           : "Shield Door, Large (MCC)",
    'door_shield_large1_mcc'          : "Shield Door, Large 1 (MCC)",
    'ff_thorage_ammo_box'             : "Ammo Cabinet",
    'ff_cex_health_cabinet'           : "Health Cabinet",
    'ff_thorage_rocket_ammo'          : "Spnkr Ammo",
    'ff_thorage_sniper_ammo'          : "Sniper Ammo",
    'ff_scn_jerseybarrier'            : "Jersey Barrier",
    'ff_scn_jerseybarrier_short'      : "Jersey Barrier, Short",
    'dlc_invasion_heavy_shield'       : "Heavy Barrier",
    'ff_scn_crate_small_closed'       : "Crate, Small, Closed",
    'ff_scn_crate_metal'              : "Crate, Metal, Multi",
    'ff_scn_crate_metal_single'       : "Crate, Metal, Single",
    'ff_scn_cov_crate_b'              : "Crate, Fully Open",
    'ff_cex_crate_forerunner_sm'      : "Crate, Forerunner, Small",
    'ff_cex_crate_forerunner_lg'      : "Crate, Forerunner, Large",
    'ff_scn_pallet'                   : "Pallet",
    'ff_scn_pallet_large'             : "Pallet, Large",
    'ff_scn_pallet_metal'             : "Pallet, Metal",
    'dlc_medium_driftwood_a'          : "Driftwood 1",
    'dlc_medium_driftwood_b'          : "Driftwood 2",
    'dlc_medium_driftwood_c'          : "Driftwood 3",
    'ff_thorage_phantom'              : "Phantom",
    'ff_thorage_spirit'               : "Spirit",
    'ff_thorage_pelican'              : "Pelican",
    'ff_thorage_drop_pod_elite'       : "Drop Pod, Elite",
    'ff_thorage_anti_air_cannon'      : "Anti Air Gun",
    'ff_thorage_cargo_truck_destroyed': "Cargo Truck, Destroyed",
    'ff_thorage_falcon_destroyed'     : "Falcon, Destroyed",
    'ff_thorage_warthog_destroyed'    : "Warthog, Destroyed",
    'ff_scn_chair_folding'            : "Folding Chair",
    'ff_scn_dumpster'                 : "Dumpster",
    'ff_scn_dumpster_tall'            : "Dumpster, Tall",
    'ff_thorage_equipment_case'       : "Equipment Case",
    'ff_thorage_monitor_flush'        : "Monitor",
    'ff_scn_cov_storage'              : "Plasma Storage",
    'ff_scn_cov_stool'                : "Camping Stool, Covenant",
    'ff_scn_cov_antenna'              : "Covenant Antenna",
    'ff_thorage_fuel_storage'         : "Fuel Storage",
    'dlc_slayer_cart_a'               : "Engine Cart",
    'dlc_slayer_cart_b'               : "Missile Cart",
    'dlc_bridge_down'                 : "Bridge",
    'ff_cex_platform_cov'             : "Platform, Covenant",
    'ff_cex_catwalk_straight'         : "Catwalk, Straight",
    'ff_cex_catwalk_short'            : "Catwalk, Short",
    'ff_cex_catwalk_bend_left'        : "Catwalk, Bend, Left",
    'ff_cex_catwalk_bend_right'       : "Catwalk, Bend, Right",
    'ff_cex_catwalk_angled'           : "Catwalk, Angled",
    'ff_cex_catwalk_lg'               : "Catwalk, Large",
    'ff_cex_bunker'                   : "Bunker, Overlook",
    'ff_cex_gunner_nest'              : "Gunners Nest",
    'ff_cex_cover_sm'                 : "Cover, Small",
    'ff_cex_large_block'              : "Block, Large",
    'ff_cex_hallway_blocker'          : "Blocker, Hallway",
    'ff_cex_stone_column'             : "Column, Stone",
    'ff_cex_tombstone'                : "Tombstone",
    'ff_cex_stone_block'              : "Cover, Large, Stone",
    'ff_cex_cover_lg'                 : "Cover, Large",
    'ff_thorage_cover_sm'             : "Walkway Cover",
    'ff_cex_walkway_cover_short'      : "Walkway Cover, Short",
    'ff_thorage_cover_lg'             : "Cover, Large, Human",
    'ff_cex_i_beam'                   : "I-Beam",
    'wall_mcc'                        : "Wall (MCC)",
    'door_mcc'                        : "Door (MCC)",
    'ff_cex_door_human'               : "Door, Human",
    'ff_cex_door_forerunner_a'        : "Door A, Forerunner",
    'ff_cex_door_forerunner_b'        : "Door B, Forerunner",
    'ff_thorage_door_c'               : "Door C, Forerunner",
    'ff_thorage_door_d'               : "Door D, Forerunner",
    'ff_thorage_door_e'               : "Door E, Forerunner",
    'ff_thorage_door_f'               : "Door F, Forerunner",
    'ff_thorage_door_g'               : "Door G, Forerunner",
    'ff_thorage_door_h'               : "Door H, Forerunner",
    'ff_cex_wall_sm_forerunner'       : "Wall, Small, Forerunner",
    'ff_cex_wall_lg_forerunner'       : "Wall, Large, Forerunner",
    'dlc_medium_stone_b'              : "Rock, Spire 3",
    'ff_cex_tree_life'                : "Tree, Dead",
    'ff_thorage_generator'            : "Generator",
    'ff_thorage_vending_machine'      : "Vending Machine",
    'ff_thorage_dinghy'               : "Dinghy",
    'ff_thorage_airstrike'            : "Target Designator",
    'ff_thorage_pelican_flyable'      : "Pelican, Hovering",
    'ff_thorage_phantom_flyable'      : "Phantom, Hovering",
    'spire_base_cannon'               : "Spire Cannon, Base",
    'spire_cliff_cannon'              : "Spire Cannon, Cliff",
    'spire_gun_cannon'                : "Spire Cannon, Gun",
    'door_shield_small1'              : "Shield Door, Small 1",
    'door_shield_large1'              : "Shield Door, Large 1",
    'low_grav_volume'                 : "Low Gravity Volume",
    'sheild_door_large'               : "Shield Door, Large (Anchor 9)",
    'sheild_door_small'               : "Shield Door, Small (Anchor 9)",
    'ff_cex_door_forerunner1'         : "Door, Forerunner 1",
    'ff_cex_door_forerunner2'         : "Door, Forerunner 2", 
    'ff_cex_rock_cluster'             : "Rock Cluster, Blocker",
    'ff_cex_rock_blocker'             : "Rock, Large, Blocker",
    'ff_cex_rock_small'               : "Small Rock",
    'ff_cex_rock_flat'                : "Flat Rock",
    'ff_cex_rock_med_a'               : "Medium Rock A",
    'ff_cex_rock_med_b'               : "Medium Rock B",
    'ff_cex_rock_spire_a'             : "Rock Spire A",
    'ff_cex_rock_spire_b'             : "Rock Spire B",
    'ff_cex_rock_seastack'            : "Rock Seastack",
    'ff_cex_rock_arch'                : "Rock Arch",
    'ff_short_grav_lift'              : "Gravity Lift, Short (Solitary)",
    'ff_tall_grav_lift'               : "Gravity Lift, Tall (Solitary)",
    'ff_cex_ramp_forerunner'          : "Platform, Ramped, Stone",
    'obj_location_name'               : "Location Name",
    'destination_delta'               : "Destination Delta",
    'destination_zulu'                : "Destination Zulu",
    'red'                             : "Red (Condemned)",
    'blue'                            : "Blue (Condemned)",
})

def GetObjectTypeName(map_name, i_type):
    map_dict = map_object_types.get(map_name)
    if not map_dict: return f"Unknown (0x{i_type:X})"

    obj_type = map_dict.AtoB.get(i_type)
    if not obj_type: return f"Unknown (0x{i_type:X})"

    return type_to_name.AtoB.get(obj_type, obj_type)
def GetObjectTypeId(map_name, name):
    obj_type = type_to_name.BtoA[name]
    map_dict = map_object_types.get(map_name)
    return map_dict.BtoA[obj_type]

flagToPhysEnum = {
    ForgeObjectFlags.PhysicsNormal: 'NORMAL',
    ForgeObjectFlags.PhysicsFixed: 'FIXED',
    ForgeObjectFlags.PhysicsPhased: 'PHASED',
}
flagToSymmetry = {
    ForgeObjectFlags.Symmetric: 'SYMMETRIC',
    ForgeObjectFlags.Asymmetric: 'ASYMMETRIC',
    ForgeObjectFlags.Symmetric + ForgeObjectFlags.Asymmetric: 'BOTH',
}
colorNumberToEnum      = { 0:'RED', 1:'BLUE', 2:'GREEN', 3:'ORANGE', 4:'PURPLE', 5:'YELLOW', 6:'BROWN', 7:'PINK', 8:'NEUTRAL', 255:'TEAM_COLOR' }
shapeNumberToEnum      = { 0:'NONE', 1:'NONE', 2:'CYLINDER', 3:'BOX' }
objectNameToShapeColor = { 'Kill Boundary':1, 'Soft Kill Boundary':1, 'Safe Boundary':2, 'Soft Safe Boundary':2 }
objectTypeToShapeColor = { 0x3D00:1, 0x3D01:1, 0x3C00:2, 0x3C01:2 }
physEnumToFlag    = InverseDict(flagToPhysEnum)
symmetryToFlag    = InverseDict(flagToSymmetry)
colorEnumToNumber = InverseDict(colorNumberToEnum)
shapeEnumToNumber = InverseDict(shapeNumberToEnum)
gtIndexToLabel    = {}
gtLabelToIndex    = {}

#teleporterTypes = ("Receiver Node","Sender Node","Two-Way Node")
teleporterTypes = (12,13,14)

def initGtLabels():
    global gtIndexToLabel
    gtIndexToLabel = {65535: 'NO_LABEL'}
    persist_vars['gtIndexToLabel'] = gtIndexToLabel
def initInvGtLabels():
    global gtLabelToIndex
    gtLabelToIndex = InverseDict(gtIndexToLabel)
def recursive_330x(i, scale):
    if i > 0: return recursive_330x(i-1, scale=scale + scale // 33 + scale // 228)
    return scale
def spawnSeqToScale(spawnSequence, convention='47X'):
    if spawnSequence == -10: return 0.01

    match convention:
        case '33X':
            scale = 0.1*spawnSequence
            if spawnSequence < -10:
                scale *= -2
                if spawnSequence > -81: scale += 8
                elif spawnSequence < -80: scale = 2*scale - 8
            scale += 1
        case '71X':
            scale = 0.1*spawnSequence
            if spawnSequence < -10:
                scale *= -4
                if spawnSequence > -81: scale += 6
                elif spawnSequence < -80: scale = 4*scale - 90
            scale += 1
        case '47X':
            scale = spawnSequence
            lthn10 = spawnSequence < -10
            gthn71 = spawnSequence > -71
            gthn41 = spawnSequence > -41
            if lthn10:
                scale = 2*(scale + 101)
                if gthn71: scale *= 3 if gthn41 else 2
            scale = 10*scale + 100
            if lthn10:
                scale += 1000
                if gthn71: scale -= 1800 if gthn41 else 600
            scale *= 0.01 # Convert Halo Reach 100 based scale to 1 based scale
        case '330X':
            scale = 100
            i = spawnSequence
            if spawnSequence < 0:
                i *= 5
                scale += i
                if spawnSequence <= -20:
                    i = spawnSequence + 201
                    if spawnSequence == -20: scale = 1
            
            if spawnSequence < -20 or spawnSequence > 0:
                scale = recursive_330x(i, scale=100)
            
            scale *= 0.01 # Convert Halo Reach 100 based scale to 1 based scale
    return scale

def wrapText(text):
    strList = []
    for line in text.split('\n'):
        strList += textwrap.TextWrapper(width=60).wrap(text=line)
    return strList
def wrapTextPanel(self, text, scale=0.75):
    layout = self.layout.column(align=True)
    layout.scale_y = scale
    for wrapped in wrapText(text): layout.label(text=wrapped)

def lockObject(obj, scale=True, loc=False, rot=False):
    for i_ax in range(0,3):
        obj.lock_scale[i_ax] = scale
        obj.lock_location[i_ax] = loc
        obj.lock_rotation[i_ax] = rot

def createForgeObject(context, itemName, i=None, data=None):
    blobj = bpy.data.objects.new(itemName if i is None else "%d - %s"%(i,itemName), data)
    context.collection.objects.link(blobj)
    #blobj.show_instancer_for_viewport = blobj.show_instancer_for_render = False
    blobj['isForgeObject'] = True
    blobj.forge.object = blobj.name
    lockObject(blobj)
    
    if itemName in bpy.data.collections:
        blobj.instance_collection = bpy.data.collections[itemName]
        blobj.instance_type = 'COLLECTION'
        blobj.empty_display_size = 0.0001
    else:
        blobj.empty_display_type = 'ARROWS'
        blobj.empty_display_size = 0.5
        #blobj.show_name = True
    return blobj

def importForgeObjects(context, self=None, createCollections=False):
    if context.scene.name == propSceneName: return False
    if not forge.TrySetConnect(True): return False

    t0 = time.time()

    forge.ReadMemory()
    map_name = forge.GetMapName()
    if map_name == "None":
        print(forge.GetLastError())
    c = forge.GetObjectCount()
    gt_c = forge.GetGtLabelCount()
    print("Map: %s, %d Objects, %d Gametype Labels"%(forge.GetMapName(), c, gt_c))

    initGtLabels()
    for i in range(0,gt_c):
        label = forge.GetGtLabel(i).upper()
        gtIndexToLabel[i] = label
    initInvGtLabels()
    persist_vars['gtIndexToLabel'] = gtIndexToLabel

    for i in range(0,maxObjectCount):
        fobj = forge.GetObjectPtr(i).contents
        if fobj.show == 0: continue
        itemName = GetObjectTypeName(map_name, fobj.itemCategory << 8 | fobj.itemVariant)

        if createCollections and itemName not in bpy.data.collections:
            collection = bpy.data.collections.new(itemName)
            bpy.data.scenes[propSceneName].collection.children.link(collection)

            blobj = bpy.data.objects.new(itemName, None)
            collection.objects.link(blobj)
            blobj.empty_display_type = 'ARROWS'
            blobj.empty_display_size = 0.5
        
        blobj = createForgeObject(context, itemName, i)
        blobj.forge.FromForgeObject(fobj, blobj)

        if itemName == "Initial Loadout Camera":# TODO: put in collection instead
            cam = bpy.data.objects.new("Initial Loadout Camera", bpy.data.cameras.new("Initial Loadout Camera"))
            context.collection.objects.link(cam)
            cam.parent = blobj
            cam.rotation_euler = Euler((radians(90),0,0))
            lockObject(cam, True, True, True)
    
    msg = "Imported %d objects in %.3fs"%(c,time.time() - t0)
    if self is None: print(msg)
    else: self.report({'INFO'}, msg)
    return True
def exportForgeObjects(context, self=None):
    if not forge.TrySetConnect(True): return False

    hitLimit = False
    t0 = time.time()
    forge.CacheCurrentMap()
    forge.ClearObjectList()
    map_name = forge.GetMapName()
    
    i = 0
    for inst in bpy.context.evaluated_depsgraph_get().object_instances:
        blobj = tryGetForgeObjectFromInstance(inst)
        if blobj != None:
            if i >= maxObjectCount: 
                hitLimit = True
                continue
            fobj = forge.GetObjectPtr(i).contents
            blobj.forge.ToForgeObject(fobj, blobj, map_name, inst)
            forge.AddObject(i)
            i += 1
    
    forge.WriteMemory()
    msg = "Exported %d objects in %.3fs"%(i,time.time() - t0)
    if self is None: print(msg)
    else: self.report({'WARNING' if hitLimit else 'INFO'}, msg)
    return True

def executeAndReport(self, context, method):
    error = False
    try:
        if method(context, self): return {'FINISHED'}
        else: error = True
    except:
        error = True
    finally:
        if error:
            msg = forge.GetLastError()
            print(msg)
            self.report({'ERROR'}, msg)
            return {'CANCELLED'}

class ImportForgeObjects(Operator):
    """Attempt to connect to MCC and import current forge objects"""
    bl_idname = 'forge.import'
    bl_label = "Import Forge Objects"
    bl_options = {'REGISTER', 'UNDO'}

    additive: BoolProperty(name="Additive", description="Add the forge objects to the existing scene loaded in Blender", default=True)

    def execute(self, context):
        if not self.additive:
            for o in context.scene.objects:
                if o.get('isForgeObject', False): bpy.data.objects.remove(o, do_unlink=True)
        
        return executeAndReport(self, context, importForgeObjects)
class ExportForgeObjects(Operator):
    """Export current forge objects into MCC's forge"""
    bl_idname = 'forge.export'
    bl_label = "Export Forge Objects"

    confirm: BoolProperty(name="I understand, I saved a backup", default=False)
    neverAsk: BoolProperty(name="Don't ask again", default=False, description="Don't talk to me or my son ever again")

    def invoke(self, context, event):
        if persist_vars.get('forge_show_warning',True): return context.window_manager.invoke_props_dialog(self)
        else:
            self.confirm=True
            self.neverAsk=True
            return self.execute(context)
    def draw(self, context):
        wrapTextPanel(self, "This modifies MCC's RAM, changes take effect after round restart. This can accidentally cause crashes or corrupt the map. Before saving ingame, restart the round and ensure objects haven't unexpectedly changed.")

        col = self.layout.column(align=True)
        col.prop(self, "confirm")
        row = col.row(align=True)
        row.enabled = self.confirm
        row.prop(self, "neverAsk")
    def execute(self, context):
        if not self.confirm:
            self.report({'ERROR'}, "Cancelled Export")
            return {'CANCELLED'}
        persist_vars['forge_show_warning'] = not self.neverAsk

        return executeAndReport(self, context, exportForgeObjects)

def importForgeMenu(self, context): self.layout.operator(ImportForgeObjects.bl_idname, text="Forge Objects", icon='ANTIALIASED')
def exportForgeMenu(self, context): self.layout.operator(ExportForgeObjects.bl_idname, text="Forge Objects", icon='ANTIALIASED')

class ForgeObjectProps(bpy.types.PropertyGroup):
    def GetLabelEnum(self, context):
        labelEnum = []
        for i, label in persist_vars['gtIndexToLabel'].items():
            if i == 65535: labelEnum.append(('NO_LABEL',"No Label","Default"))
            else: labelEnum.append((label,label,""))
        return labelEnum

    def UpdateColor(self, context):
        col = self.color
        if col == 'TEAM_COLOR': col = self.team
        self.colorIndex = colorEnumToNumber[col]

    def UpdateShape(self, context):
        shObj = bpy.data.objects.get(self.shapeObject, None)
        if self.shape == 'NONE':
            if shObj != None: bpy.data.objects.remove(shObj, do_unlink=True)
            return
        
        if shObj is None:
            shObj = bpy.data.objects.new("%s Shape" % self.object, None)
            self.shapeObject = shObj.name
            '''collChildren = context.scene.collection.children
            shapesCollection = collChildren.get(shapesCollName, None)
            if shapesCollection is None:
                shapesCollection = bpy.data.collections.new(shapesCollName)
                collChildren.link(shapesCollection)
            coll = shapesCollection if shapesCollection != None else context.collection'''
            coll = context.collection
            coll.objects.link(shObj)
            shObj.rotation_euler = Euler((0,0,radians(90)))
            shObj.show_instancer_for_viewport = shObj.show_instancer_for_render = False
            shObj.instance_type = 'COLLECTION'
            blObj = bpy.data.objects[self.object]
            shObj.parent = blObj
            lockObject(shObj, True, True, True)
            #shObj['colorIndex'] = objectTypeToShapeColor.get(blObj.forge.objectType, 0)
        
        coll = None
        if self.shape == 'BOX':
            coll = bpy.data.collections['Shape Box']
            shObj.scale = (self.width,self.length,self.top + self.bottom)
        elif self.shape == 'CYLINDER':
            coll = bpy.data.collections['Shape Cylinder']
            d = self.width * 2
            shObj.scale = (d,d,self.top + self.bottom)
        if shObj.instance_collection != coll: shObj.instance_collection = coll
        
        shObj.location = (0,0,(self.top-self.bottom)/2)
    
    def IsScaled(self, blobj): return anvilScaling and blobj.forge.gtLabel == "SCALE"
    def UpdateScale(self, blobj):
        if self.IsScaled(blobj):
            scale = spawnSeqToScale(blobj.forge.spawnSequence, bpy.context.scene.forge.scaleConvention)
            blobj.scale = (scale,scale,scale)
        else: blobj.scale = (1,1,1)
    def UpdateSpawnSeq(self, context):
        if anvilScaling:
            if context.selected_objects:
                for blobj in context.selected_objects: self.UpdateScale(blobj)
            else:
                self.UpdateScale(bpy.data.objects[self.object])

    teamEnum = [
        ('RED', "Red", ""),
        ('BLUE', "Blue", ""),
        ('GREEN', "Green", ""),
        ('ORANGE', "Orange", ""),
        ('PURPLE', "Purple", ""),
        ('YELLOW', "Yellow", ""),
        ('BROWN', "Brown", ""),
        ('PINK', "Pink", ""),
        ('NEUTRAL', "Neutral", "")
    ]
    colorEnum = teamEnum + [ ('TEAM_COLOR', "Team", "") ]

    object: StringProperty()
    shapeObject: StringProperty()
    
    objectType: IntProperty()
    cachedType: IntProperty()
    gtLabel: EnumProperty(name="Game Type Label", description="How the gametype interprets this object", items=GetLabelEnum, default=0, update=UpdateSpawnSeq)
    otherInfoA: IntProperty()
    otherInfoB: IntProperty()

    
    physics: EnumProperty(name="Physics", description="Physics mode", default='PHASED',
        items=[
            ('NORMAL', "Normal", "Affected by gravity and movable"),
            ('FIXED', "Fixed", "Unaffected by gravity"),
            ('PHASED', "Phased", "Unaffected by gravity and collisionless"),
        ]
    )
    team: EnumProperty(name="Team", description="Object team", items=teamEnum, default='NEUTRAL', update=UpdateColor)
    color: EnumProperty(name="Color", description="Object color", items=colorEnum, default='TEAM_COLOR', update=UpdateColor)
    colorIndex: IntProperty(default=8)
    spawnSequence: IntProperty(name="Spawn Sequence", description="Gamemode phase at which the object will spawn", min=-100, max=100, update=UpdateSpawnSeq)
    spawnTime: IntProperty(name="Spawn Time", description="Time in seconds before the object spawns or respawns", min=0, soft_max=180, max=255)# 0 is never
    gameSpecific: BoolProperty(name="Game Specific", description="Should object exclusively spawn for current gamemode")
    placeAtStart: BoolProperty(name="Place At Start", description="Should object spawn at start", default=True)
    symmetry: EnumProperty(name="Symmetry", description="Gamemode symmetry",
        items=[
            ('BOTH', "Both", "Present in symmetric and asymmetric gamemodes (default)"),
            ('SYMMETRIC', "Symmetric", "Present only in symmetric gamemodes"),
            ('ASYMMETRIC', "Asymmetric", "Present only in asymmetric gamemodes"),
        ],
        default='BOTH'
    )
    
    shape: EnumProperty(name="Shape", description="Area shape", default='NONE', update=UpdateShape,
        items=[('NONE', "None", ""), ('CYLINDER', "Cylinder", ""), ('BOX', "Box", "")]
    )
    width: FloatProperty(name="Width", description="", unit='LENGTH', min=0,max=100.9, update=UpdateShape)
    length: FloatProperty(name="Length", description="", unit='LENGTH', min=0,max=100.9, update=UpdateShape)
    top: FloatProperty(name="Top", description="Distance to top from center", unit='LENGTH', min=0,max=100.9, update=UpdateShape)
    bottom: FloatProperty(name="Bottom", description="Distance to bottom from center", unit='LENGTH', min=0,max=100.9, update=UpdateShape)

    def FromForgeObject(self, fobj, blobj):
        fwd = fobj.forward
        up = fobj.up
        right = fwd.cross(up)
        pos = fobj.position
        blobj.matrix_world = Matrix(((right.x,fwd.x,up.x,pos.x),(right.y,fwd.y,up.y,pos.y),(right.z,fwd.z,up.z,pos.z),(0,0,0,1)))

        flags = ForgeObjectFlags(fobj.flags)
        
        self.objectType = fobj.itemCategory << 8 | fobj.itemVariant
        self.cachedType = fobj.cachedType
        self.gtLabel = gtIndexToLabel[fobj.gtLabelIndex]
        self.otherInfoA = fobj.otherInfoA
        self.otherInfoB = fobj.otherInfoB

        self.physics = flagToPhysEnum[flags & ForgeObjectFlags.PhysicsMask]
        self.team = colorNumberToEnum[fobj.team]
        self.color = colorNumberToEnum[fobj.color]
        self.symmetry = flagToSymmetry[flags & ForgeObjectFlags.SymmetryMask]
        self.spawnSequence = fobj.spawnSequence
        self.spawnTime = fobj.spawnTime
        self.placeAtStart = not ForgeObjectFlags.HideAtStart in flags
        self.gameSpecific = ForgeObjectFlags.GameSpecific in flags
        self.shape = shapeNumberToEnum[fobj.shape]
        self.width = fobj.width
        self.length = fobj.length
        self.top = fobj.top
        self.bottom = fobj.bottom
    
    def ToForgeObject(self, fobj, blobj, map_name, inst=None):
        m = blobj.matrix_world if inst is None else inst.matrix_world
        fobj.forward = float3.fromVector(m.col[1].normalized())
        fobj.up = float3.fromVector(m.col[2].normalized())
        fobj.position = float3.fromVector(m.col[3])

        coll = blobj.instance_collection
        ty = GetObjectTypeId(map_name, coll.name_full) if coll != None else self.objectType # TODO: replace
        fobj.itemCategory = ty >> 8
        fobj.itemVariant = ty & 0x00FF

        fobj.cachedType = self.cachedType
        fobj.gtLabelIndex = gtLabelToIndex[self.gtLabel]
        fobj.otherInfoA = self.otherInfoA
        fobj.otherInfoB = self.otherInfoB

        fobj.show = 1
        fobj.team = colorEnumToNumber[self.team]
        fobj.color = colorEnumToNumber[self.color]
        fobj.flags = ForgeObjectFlags.HideAtStart*(not self.placeAtStart) + ForgeObjectFlags.GameSpecific*self.gameSpecific + physEnumToFlag[self.physics] + symmetryToFlag[self.symmetry]
        fobj.spawnSequence = self.spawnSequence
        fobj.spawnTime = self.spawnTime
        fobj.shape = shapeEnumToNumber[self.shape]
        fobj.width = self.width
        fobj.length = self.length
        fobj.top = self.top
        fobj.bottom = self.bottom

def getWidth(targetRegion):
  for region in bpy.context.area.regions:
    if region.type == targetRegion:
      return region.width
  return 500
def drawForgeObjectProperties(self, context, region):
    layout = self.layout
    width = getWidth(region)
    wide = width > 200
    layout.use_property_split = wide
    obj = context.selected_objects[0]
    fprops = obj.forge
    
    locRotLay = layout if wide else layout.column(align=True)
    locRotLay.prop(obj, 'location')
    locRotLay.prop(obj, 'rotation_euler', text="Rotation")
    
    if wide:
        layout.prop(obj, 'instance_collection', text="Object Type")
    else:
        col = layout.column(align=True)
        col.label(text="Object Type:")
        col.prop(obj, 'instance_collection', text="")
    
    col = layout.column(align=True)
    col.use_property_split = width > 160
    col.prop(fprops, 'physics')
    col.prop(fprops, 'team')
    col.prop(fprops, 'color')
    col.prop(fprops, 'symmetry')

    if fprops.cachedType == 1:
        col = layout.column(align=True)
        col.label(text="Weapon")
        col.prop(fprops, 'otherInfoA', text="Spare Clips")
    elif fprops.cachedType in teleporterTypes:
        col = layout.column(align=True)
        col.label(text="Teleporter")
        col.prop(fprops, 'otherInfoA', text="Channel")
        col.prop(fprops, 'otherInfoB', text="Flags")
    
    col = layout.column(align=True)
    if wide:
        col.prop(fprops, 'spawnSequence')
        col.prop(fprops, 'spawnTime')
    else:
        col.label(text="Spawn")
        col.prop(fprops, 'spawnSequence', text="Sequence")
        col.prop(fprops, 'spawnTime', text="Time")
    
    col = layout.column(align=True)
    col.prop(fprops, 'placeAtStart')
    col.prop(fprops, 'gameSpecific')
    
    shape = fprops.shape
    col = layout.column(align=True)
    col.prop(fprops, 'shape')
    col2 = col.column(align=True)
    col2.enabled = shape != 'NONE'
    col2.prop(fprops, 'width', text=('Width' if shape == 'BOX' else 'Radius'))
    if shape == 'BOX': col2.prop(fprops, 'length')
    '''lenRow = col2.row(align=True)
    lenRow.enabled = shape == 'BOX'
    lenRow.prop(fprops, 'length')'''
    col2.prop(fprops, 'top')
    col2.prop(fprops, 'bottom')

    col = layout.column(align=True)
    col.prop(fprops, 'gtLabel')
    if fprops.IsScaled(obj):
        col.prop(context.scene.forge, 'scaleConvention')
    
    #layout.operator('render.render')
def pollForgePanel(self, context):
    objs = context.selected_objects
    if objs: return objs[0].get('isForgeObject', False)
    else: return False
class ForgeObjectPanel(bpy.types.Panel):
    bl_label = "Forge Properties"
    bl_idname = 'SCENE_PT_forge_object'
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'
    bl_context = 'object'
    
    @classmethod
    def poll(self, context): return pollForgePanel(self, context)
    
    def draw(self, context): drawForgeObjectProperties(self, context, 'WINDOW')
class ForgeObjectPanel_Sidebar(bpy.types.Panel):
    bl_label = "Forge Properties"
    bl_idname = 'SCENE_PT_forge_object_sidebar'
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'Forge'
    bl_context = 'objectmode'
    
    @classmethod
    def poll(self, context): return pollForgePanel(self, context)
    
    def draw(self, context): drawForgeObjectProperties(self, context, 'UI')

class PasteOverload(Operator):
    """Duplicates selected objects (for forge compatibility)"""
    bl_idname = 'view3d.pastebuffer'
    bl_label = "Paste Forge Objects"
    
    autoselect: BoolProperty(default=True)
    active: BoolProperty(default=True)

    @classmethod
    def poll(cls, context): return context.active_object != None and context.active_object.get('isForgeObject',False)
    def execute(self, context): return bpy.ops.object.duplicate_move('INVOKE_DEFAULT')

class ConvertForge(Operator):
    """Set if object is exported to MCC"""
    bl_idname = 'object.convert_forge'
    bl_label = "Convert Forge Object"

    isForgeObject: BoolProperty()

    def execute(self, context):
        for obj in context.selected_objects: obj['isForgeObject'] = self.isForgeObject
        return {'FINISHED'}
def convertForgeMenuItem(self, context):
    isForge = context.active_object.get('isForgeObject', False)
    if isForge: op = self.layout.operator(ConvertForge.bl_idname, icon='REMOVE', text="Convert To Blender Object")
    else: op = self.layout.operator(ConvertForge.bl_idname, icon='ADD', text="Convert To Forge Object")
    op.isForgeObject = not isForge

def arrayModifier(context, type, count=3, offset=(1,0,0), rotation=(0,0,0), curveLen=5):
    srcObj = context.active_object
    loc = srcObj.location
    size = 1

    coll = srcObj.instance_collection
    if coll != None:
        name = coll.name

        for inst_o in coll.objects:
            s = inst_o.dimensions.y
            if s > size: size = s
    else: name = srcObj.name

    bpy.ops.mesh.primitive_plane_add(enter_editmode=False, align='WORLD', location=loc, size=1)
    arrObj = masterParent = context.object
    arrObj.name = "%s Array" % name
    arrObj.instance_type = 'FACES'
    arrObj.show_instancer_for_viewport = arrObj.show_instancer_for_render = False
    arrObj.select_set(True)
    
    bpy.ops.object.modifier_add(type='ARRAY')
    mod = arrObj.modifiers["Array"]
    mod.show_on_cage = True
    mod.use_relative_offset = False

    if type != 'CONST':
        parentObj = masterParent = bpy.data.objects.new("%s Array Holder" % name, None)
        context.collection.objects.link(parentObj)
        parentObj.location = loc
        parentObj.empty_display_size = 2
        arrObj.parent = parentObj
        arrObj.location = (0,0,0)
    if type == 'OBJECT':
        offsetObj = bpy.data.objects.new("%s Array Offset" % name, None)
        context.collection.objects.link(offsetObj)
        #offsetObj.empty_display_type = 'CUBE'
        offsetObj.empty_display_size = size# 0.5 * size + 0.05
        offsetObj.location = offset
        offsetObj.rotation_euler = rotation
        lockObject(offsetObj)
        offsetObj.parent = parentObj
        offsetObj.select_set(True)

        mod.use_constant_offset = False
        mod.use_object_offset = True
        mod.offset_object = offsetObj
    else:
        mod.use_constant_offset = True
        mod.constant_offset_displace = offset
    if type == 'CURVE':
        bpy.ops.object.modifier_add(type='CURVE')
        cMod = arrObj.modifiers["Curve"]
        cMod.show_on_cage = cMod.show_in_editmode = True
        
        bpy.ops.curve.primitive_bezier_curve_add(enter_editmode=False, align='WORLD', location=(0,0,0), radius=curveLen)
        curve = bpy.context.object
        curve.name = "%s Curve" % name
        curve.parent = parentObj
        curve.select_set(True)
        lockObject(curve)
        curveData = curve.data
        curveData.twist_mode = 'Z_UP'
        curveData.use_deform_bounds = curve.show_in_front = True

        mod.fit_type = 'FIT_CURVE'
        mod.curve = curve
        cMod.object = curve

        arrObj.select_set(True)
    else:
        mod.count = count

    masterParent.rotation_euler = srcObj.rotation_euler

    srcObj.parent = arrObj
    srcObj.location = (0,0,0)
    srcObj.rotation_euler = (0,0,0)
    srcObj.display_type = 'BOUNDS'
    srcObj.select_set(True)
    if type != 'CONST': parentObj.select_set(True)

class SetupArray(Operator):
    """Setup array (repeated object duplication) for forge object"""
    bl_idname = 'object.setup_array'
    bl_label = "Setup Array"
    bl_options = {'REGISTER', 'UNDO'}

    type: EnumProperty(name="Type", description="Array modifier mode", default='CONST',
        items=[
            ('CONST', "Linear", "Simple XYZ offset from the modifier panel", 'MOD_ARRAY', 0),
            ('OBJECT', "Object", "Advanced XYZ offset and rotation from an object", 'MOD_TINT', 1),
            ('CURVE', "Curve", "Place objects along curve", 'MOD_CURVE', 2),
        ]
    )
    count: IntProperty(name="Count", description="Number of duplicated objects", default=3, min=1, max=650)
    offset: FloatVectorProperty(name="Offset", default=(1,0,0))
    rotation: FloatVectorProperty(name="Rotation", default=(0,0,0))
    size: FloatProperty(name="Size", description="Size of curve", default=5, min=0)

    @classmethod
    def poll(cls, context): 
        o = context.active_object
        return o != None# and o.instance_type == 'COLLECTION' and o.parent is None
    def execute(self, context):
        rot = self.rotation
        arrayModifier(context, self.type, self.count, self.offset, (radians(rot[0]),radians(rot[1]),radians(rot[2])), self.size)
        return {'FINISHED'}
    def draw(self, context):
        layout = self.layout
        layout.use_property_split = True
        layout.prop(self, 'type')
        if self.type != 'CURVE': layout.prop(self, 'count')
        layout.prop(self, 'offset')
        if self.type == 'OBJECT': layout.prop(self, 'rotation')
        if self.type == 'CURVE': layout.prop(self, 'size')
def setupArrayMenuItem(self, context): self.layout.operator(SetupArray.bl_idname, icon='MOD_ARRAY')

def tryGetForgeObjectFromInstance(inst):
    o = inst.object
    if inst.is_instance:
        if o.is_instancer: return o
    elif o.get('isForgeObject',False):
        p = o.parent
        if p is None or p.instance_type == 'NONE' or p.instance_type == 'COLLECTION': return o
    else: return None

def draw_forgeObjectOverlay():
    for area in bpy.context.screen.areas:
        if area.type != 'VIEW_3D': continue
        for space in area.spaces:
            if space.type == 'VIEW_3D':
                if not space.overlay.show_overlays: return
                break
    
    objCount = 0
    for inst in bpy.context.evaluated_depsgraph_get().object_instances:
        if tryGetForgeObjectFromInstance(inst) != None: objCount += 1
    
    font_id = 0
    blf.position(font_id, 15, 15, 0)
    blf.size(font_id, 11)
    if objCount > maxObjectCount: blf.color(font_id, 1,0,0,1)
    elif objCount > 500: blf.color(font_id, 1,0.5,0,1)
    else: blf.color(font_id, 1,1,1,1)
    blf.draw(font_id, "%d / %d" % (objCount,maxObjectCount))

iconDict = {}
def fillIconDict(collection):
    global iconDict
    for coll in collection.children:
        if len(coll.objects) > 0:
            if iconDict.get(coll, None) is None: iconDict[coll] = coll.forge.icon
        else: fillIconDict(coll)

def getCollectionEnums(collection, list):
    global iconDict
    for coll in collection.children:
        if len(coll.objects) > 0:
            list.append((coll.name, coll.name, "", iconDict.get(coll, 'NONE'), len(list)))
        else: getCollectionEnums(coll, list)
    return list
def genObjectTypesEnum(self, context): return getCollectionEnums(bpy.data.collections[mapPalette], [])
class AddForgeObject(Operator):
    """Add forge object"""
    bl_idname = 'forge.add_object'
    bl_label = "Forge Object..."
    bl_property = 'objectType'

    objectType: EnumProperty(name="Object Type", items=genObjectTypesEnum)
    
    def invoke(self, context, event):
        fillIconDict(bpy.data.collections[mapPalette])
        context.window_manager.invoke_search_popup(self)
        return {'FINISHED'}
    def execute(self, context):
        blobj = createForgeObject(context, self.objectType)
        blobj.location = context.scene.cursor.location
        for obj in bpy.context.selected_objects: obj.select_set(False)
        blobj.select_set(True)
        context.view_layer.objects.active = blobj
        
        bpy.ops.ed.undo_push()
        return {'FINISHED'}
def addForgeObjectMenuItem(self, context):
    layout = self.layout
    layout.operator_context = 'INVOKE_DEFAULT'
    layout.operator(AddForgeObject.bl_idname, icon='ADD')
    
    layout.context_pointer_set('forgeColl', bpy.data.collections[mapPalette])
    layout.menu(AddForgeObjectMenu.bl_idname)

class ForgeCollectionProps(bpy.types.PropertyGroup):
    def iconsEnum(self, context):
        icons = bpy.types.UILayout.bl_rna.functions['prop'].parameters['icon'].enum_items.keys()
        icoEnum = []
        for i in range(0,len(icons)):
            ico = icons[i]
            icoEnum.append((ico,"",ico,ico,i))
            #if ico == 'NONE': print(i)
        return icoEnum
    icon: EnumProperty(name="Icon",description="Icon used in menus", items=iconsEnum, default=0)
class ForgeCollectionPanel(bpy.types.Panel):
    bl_label = "Forge Collection"
    bl_idname = 'SCENE_PT_forge_collection'
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'
    bl_context = 'collection'
    
    def draw(self, context):
        layout = self.layout
        layout.use_property_split = True
        collProps = context.collection.forge
        layout.prop(collProps, 'icon')

class ForgeSceneProps(bpy.types.PropertyGroup):
    def UpdateScaleConvention(self, context):
        for blobj in context.scene.objects: blobj.forge.UpdateScale(blobj)
    scaleConvention: EnumProperty(name="Scale Convention",description="Gametype object scaling convention, or how to interpret the spawn sequence number.",
        items=( ('33X', "33X", "Trusty's Old"), ('71X', "71X", "Rabid MidgetMan's"), ('47X', "47X*", "Anvil Editor Default (Trusty's New)"), ('330X', "330X", "Tx Titan Scale. Max scale of 327.34") ), default=2, update=UpdateScaleConvention)
class ForgeScenePanel(bpy.types.Panel):
    bl_label = "Forge Scene"
    bl_idname = 'SCENE_PT_forge_scene'
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'
    bl_context = 'scene'
    
    def draw(self, context):
        layout = self.layout
        layout.use_property_split = True
        sceneProps = context.scene.forge
        layout.prop(sceneProps, 'scaleConvention')

class AddForgeObjectMenu(bpy.types.Menu):
    bl_label = "Forge Object"
    bl_idname = 'VIEW3D_MT_add_forge_object'

    def draw(self, context):
        try: children = context.forgeColl.children
        except: return

        layout = self.layout
        for coll in children:
            name = coll.name
            if len(coll.objects) > 0:
                layout.operator_context = 'EXEC_DEFAULT'
                op = layout.operator(AddForgeObject.bl_idname, text=name, icon=coll.forge.icon)
                op.objectType = name
            elif len(coll.children) > 0:
                self.layout.context_pointer_set('forgeColl',coll)
                layout.menu(self.__class__.bl_idname, text=name, icon=coll.forge.icon)

class ErrorMessage(Operator):
    """Error message"""
    bl_idname = 'forge.error'
    bl_label = "Error"

    message: bpy.props.StringProperty()
    url: bpy.props.StringProperty()

    def invoke(self, context, event):
        print("Error: " + self.message + "\n")
        return context.window_manager.invoke_props_dialog(self)
    def draw(self, context):
        wrapTextPanel(self, self.message)

        if self.url != "":
            layout = self.layout.column(align=True)
            layout.operator_context = 'INVOKE_DEFAULT'
            op = layout.operator('wm.url_open', text="Open Webpage")
            op.url = self.url
    def execute(self, context):
        return {'CANCELLED'}
class ScaleObjects(Operator):
    """Scale multiple objects as one"""
    bl_idname = 'forge.scale_objects'
    bl_label = "Scale Forge Objects"
    bl_options = {'REGISTER', 'UNDO'}
    
    seq: IntProperty(name="Spawn Sequence (Scale)", default=0, min=-100, max=100)

    @classmethod
    def poll(cls, context): return context.selected_objects and 'SCALE' in gtLabelToIndex
    def execute(self, context):
        scale = spawnSeqToScale(self.seq, context.scene.forge.scaleConvention)
        for blobj in context.selected_objects:
            fobj = blobj.forge
            fobj.gtLabel = 'SCALE'
            fobj.spawnSequence = self.seq
        bpy.ops.transform.resize(value=(scale, scale, scale))
        return {'FINISHED'}

reg_classes = [
    ForgeObjectProps, ForgeCollectionProps, ForgeSceneProps,
    ImportForgeObjects, ExportForgeObjects, AddForgeObject, PasteOverload, ConvertForge, SetupArray, ErrorMessage, ScaleObjects,
    ForgeObjectPanel, ForgeObjectPanel_Sidebar, AddForgeObjectMenu, ForgeCollectionPanel, ForgeScenePanel
]
reg_objMenus = [convertForgeMenuItem, setupArrayMenuItem]
reg_addMenus = [addForgeObjectMenuItem]

def registerDrawEvent(event, item):
    id = event.bl_rna.name
    handles = persist_vars.get(id, [])
    event.append(item)
    handles.append(item)
    persist_vars[id] = handles
def removeDrawEvents(event):
    for item in persist_vars.get(event.bl_rna.name,[]):
        try: event.remove(item)
        except: pass

def tryGetLatestRelease():
    with urllib.request.urlopen(githubURL) as req:
        url = req.url
        latestVersion = (url[url.rindex("tag/")+4:]).strip('/ ')

        if (vMjolnir != latestVersion): webbrowser.open(url)

def register():
    for cls in reg_classes:
        try: bpy.utils.register_class(cls)
        except Exception as e: print(e)
    for item in reg_objMenus: registerDrawEvent(bpy.types.VIEW3D_MT_object_context_menu, item)
    for item in reg_addMenus: registerDrawEvent(bpy.types.VIEW3D_MT_add, item)
    registerDrawEvent(bpy.types.TOPBAR_MT_file_import, importForgeMenu)
    registerDrawEvent(bpy.types.TOPBAR_MT_file_export, exportForgeMenu)

    bpy.types.Object.forge = bpy.props.PointerProperty(type=ForgeObjectProps)
    bpy.types.Collection.forge = bpy.props.PointerProperty(type=ForgeCollectionProps)
    bpy.types.Scene.forge = bpy.props.PointerProperty(type=ForgeSceneProps)
    
    persist_vars['forgeObjectsOverlay_handle'] = bpy.types.SpaceView3D.draw_handler_add(draw_forgeObjectOverlay, (), 'WINDOW', 'POST_PIXEL')
    if persist_vars.get('gtIndexToLabel', None) == None: initGtLabels()
    initInvGtLabels()

    if check_update:
        thread = Thread(target=tryGetLatestRelease)
        thread.start()

    if not exists(dllPath):
        bpy.ops.forge.error('INVOKE_DEFAULT', 
            message="ForgeBridge.dll not found!\nPlease download the latest release from " + githubURL, 
            url=githubURL)
        return
    global forge
    forge = cdll.LoadLibrary(dllPath)
    try:
        print("ForgeBridge.dll Version %d" % forge.GetDllVersion())
        forge.TrySetConnect.restype = c_bool
        forge.GetLastError.restype = c_wchar_p
        forge.GetMapName.restype = c_wchar_p
        forge.GetGtLabel.restype = c_wchar_p
        forge.GetObjectPtr.restype = POINTER(ForgeObject)
    except Exception as ex:
        bpy.ops.forge.error('INVOKE_DEFAULT', 
            message="Outdated ForgeBridge.dll in use!\nPlease download the latest release from https://github.com/Waffle1434/Mjolnir-Forge-Editor/releases", 
            rl="https://github.com/Waffle1434/Mjolnir-Forge-Editor/releases")
        print(ex)

def unregister():
    try: forge.TrySetConnect(False)
    except: pass
    for cls in reg_classes:
        try: bpy.utils.unregister_class(cls)
        except: pass
    removeDrawEvents(bpy.types.VIEW3D_MT_object_context_menu)
    removeDrawEvents(bpy.types.VIEW3D_MT_add)
    removeDrawEvents(bpy.types.TOPBAR_MT_file_import)
    removeDrawEvents(bpy.types.TOPBAR_MT_file_export)
    bpy.types.SpaceView3D.draw_handler_remove(persist_vars['forgeObjectsOverlay_handle'], 'WINDOW')


try: unregister()
except: pass
register()
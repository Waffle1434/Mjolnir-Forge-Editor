using ForgeLib.Halo3;
using RGiesecke.DllExport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ForgeLib {
    public enum Game : byte {
        None,
        HaloReach,
        Halo3,
    }

    public enum Map {
        None,

        // Halo Reach
        // TODO: Attribute strings
        Boardwalk,    //50_panopticon
        Boneyard,     //70_boneyard
        Countdown,    //45_launch_station
        Powerhouse,   //30_settlement
        Reflection,   //52_ivory_tower
        Spire,        //35_island
        Sword_Base,   //20_sword_slayer
        Zealot,       //45_aftship
        Anchor_9,     //dlc_slayer
        Breakpoint,   //dlc_invasion
        Tempest,      //dlc_medium
        Condemned,    //condemned
        Highlands,    //trainingpreserve
        Battle_Canyon,//cex_beavercreek
        Penance,      //cex_damnation
        Ridgeline,    //cex_timberland
        Solitary,     //cex_prisoner
        High_Noon,    //cex_hangemhigh
        Breakneck,    //cex_headlong
        Forge_World,   //forge_halo

        // Halo 3
        Construct,
        Epitaph,
        Guardian,
        HighGround,
        Isolation,
        LastResort,
        Narrows,
        Sandtrap,
        Snowbound,
        ThePit,
        Valhalla,
        Foundry,
        RatsNest,
        Standoff,
        Avalanche,
        Blackout,
        GhostTown,
        ColdStorage,
        Assembly,
        Orbital,
        Sandbox,
        Citadel,
        Heretic,
        Longshore
    }

    public static class ForgeBridge {
        public const int HR_maxObjects = 650;

        public static ProcessMemory memory = new ProcessMemory();
        public static Game currentGame;
        public static Map currentMap;
        public static Dictionary<int, MccForgeObject> forgeObjects = new Dictionary<int, MccForgeObject>();
        static UIntPtr forgeObjectArrayPointer;

        static UIntPtr reachBase;
        static Dictionary<Map, UIntPtr> mapPlayerPositions = new Dictionary<Map, UIntPtr>();
        static UIntPtr mccPlayerMonitorPosition;
        //static unsafe float3* playerMonitorPosition;

        static H3_MapVariant h3_mvar = new H3_MapVariant();

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static int GetDllVersion() => 4;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static bool TrySetConnect(bool connect) {
            if (connect) {
                if (memory.Connected) return true;

                Process[] processes = null;
                foreach (string procName in new string[] { "MCC-Win64-Shipping", "MCCWinStore-Win64-Shipping" }) {
                    processes = Process.GetProcessesByName(procName);
                    if (processes.Length > 0) goto FoundProcess;
                }

                lastError = "Failed to find Master Chief Collection process.";
                return false;

            FoundProcess:
                if (!memory.OpenProcess(processes[0].Id)) {
                    lastError += "Failed to connect to process.\n";
                    return false;
                }
            }
            else {
                try {
                    memory.CloseProcess();
                }
                catch {
                    lastError = "Failed to close process.";
                    return false;
                }
            }

            return true;
        }

        public static string lastError;
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static string GetLastError() => lastError;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static Game GetGame() => currentGame;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static int GetObjectCount() => forgeObjects.Count;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static unsafe HR_ForgeObject* GetObjectPtr(int i) => MccForgeObject.GetPointer(i);
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static unsafe string ForgeObject_GetItemName(int i) => forgeObjects[i].data->ItemName;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static int ItemNameToType([MarshalAs(UnmanagedType.LPWStr)] string itemName) {
            if (ItemParser.TryNameToType(itemName, currentMap, out int type)) return type;
            return 0;
        }

        static void GetPointers() {
            Console.WriteLine("GetPointers");
            if (memory.TryGetModuleBaseAddress("haloreach.dll", out reachBase)) {
                currentGame = Game.HaloReach;

                UIntPtr forgeBase = memory.ReadPointer(reachBase + 0x23CC688);
                gtLabelsPointer = forgeBase + 0x7F4;
                forgeObjectArrayPointer = forgeBase + 0x19FC;

                mapPlayerPositions[Map.Forge_World] = reachBase + 0x306ABC0;
                mapPlayerPositions[Map.Tempest] = reachBase + 0x30DD280;
                mapPlayerPositions[Map.Spire] = reachBase + 0x310EAD0;
                mapPlayerPositions[Map.None] = default;
            }
            else if (memory.TryGetModuleBaseAddress("halo3.dll", out UIntPtr halo3Base)) {
                currentGame = Game.Halo3;

                UIntPtr addr = halo3Base + 0x1F89980;
                foreach (int offset in new int[] { 0x760, 0x368, 0xA00 })
                    addr = memory.ReadPointer(addr) + offset;

                unsafe {
                    fixed (H3_MapVariant* mvarPtr = &h3_mvar) {
                        if (memory.TryReadStruct(addr, mvarPtr)) {
                            Console.WriteLine("Reading Halo 3 MVAR");
                            Console.WriteLine(h3_mvar.data.DisplayName);
                            Console.WriteLine(h3_mvar.data.Description);
                            Console.WriteLine(h3_mvar.data.Author);

                            H3_ForgeObject* objPtr = h3_mvar.GetForgeObjects();
                            for (int i = 0; i < 640; i++) {
                                H3_ForgeObject obj = *objPtr;
                                objPtr++;
                            }
                        }
                        else {
                            Console.WriteLine("Fail!");
                        }
                    }
                }
            }
            else {
                currentGame = Game.None;
            }
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static void ReadMemory() {
            if (!memory.Connected) return;

            GetPointers();

            _CacheCurrentMap();
            if (currentGame == Game.HaloReach) {
                GetGtLabels();

                mapPlayerPositions.TryGetValue(currentMap, out mccPlayerMonitorPosition);

                GetForgeObjects();
            }
        }

        #region Map Name
        const int mapNameOffset = 0x2687753;
        static Map GetCurrentMap() {
            switch (currentGame) {
                case Game.HaloReach:
                    return MapUtil.FromIdString(memory.ReadString(reachBase + mapNameOffset));
                case Game.Halo3:
                    return MapUtil.FromH3_Enum(h3_mvar.data.e_map_id);// TODO: !!!
                default:
                    return Map.None;
            }
        }

        static void _CacheCurrentMap() {
            currentMap = GetCurrentMap();

            if (currentMap == Map.None) {
                lastError += $"\nMap: >{memory.ReadString(reachBase + mapNameOffset)}< ({reachBase:X} {reachBase + mapNameOffset:X})";
                currentMap = Map.Forge_World;
            }
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static void CacheCurrentMap() => _CacheCurrentMap();

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static string GetMapName() => MapUtil.ToString(currentMap);
        #endregion

        #region Labels
        static UIntPtr gtLabelsPointer;
        public static readonly List<string> gtLabels = new List<string>();

        static void GetGtLabels() {
            gtLabels.Clear();
            foreach (string label in memory.ReadString(gtLabelsPointer, 4096, false).Split('\0')) {
                if (string.IsNullOrEmpty(label)) break;
                gtLabels.Add(label);
            }
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static int GetGtLabelCount() => gtLabels.Count;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static string GetGtLabel(int i) => gtLabels[i];
        #endregion

        static void GetForgeObjects() {
            // TODO: read as one byte array (maxObjects * ForgeObject.size bytes)
            // TODO: less allocatey system that preallocates all MccForgeObjects
            for (int i = 0; i < HR_maxObjects; i++) {
                UIntPtr objPtr = forgeObjectArrayPointer + i * HR_ForgeObject.size;
                byte flag = memory.ReadByte(objPtr);
                bool isObject = flag == 1;

                if (flag > 1) {
                    lastError += "\nUnknown flag!";
                    throw new Exception(lastError);
                }

                if (isObject) {
                    if (!forgeObjects.TryGetValue(i, out MccForgeObject fobj)) {
                        fobj = new MccForgeObject(objPtr, i);
                        forgeObjects[i] = fobj;
                    }

                    fobj.ReadFromMemory();
                }
                else {
                    if (forgeObjects.TryGetValue(i, out MccForgeObject fobj)) {
                        fobj.ReadFromMemory();
                        forgeObjects.Remove(i);
                    }

                    unsafe {
                        MccForgeObject.GetPointer(i)->show = 0;
                    }
                }
            }
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static void WriteMemory() {
            if (!memory.Connected) return;

            GetPointers();

            unsafe {
                for (int i = 0; i < HR_maxObjects; i++) {
                    if (forgeObjects.TryGetValue(i, out MccForgeObject fobj)) {
                        fobj.WriteMemory();
                    }
                    else {
                        UIntPtr objPtr = forgeObjectArrayPointer + i * HR_ForgeObject.size;
                        fobj = new MccForgeObject(objPtr, i);
                        fobj.ReadFromMemory();
                        fobj.data->show = 0;
                        fobj.WriteMemory();
                    }
                }
            }
        }

        #region Teleport
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static bool TryGetMonitorPosition(out float3 position) {//[Out]
            position = default;

            if (mccPlayerMonitorPosition == default) return false;

            unsafe {
                fixed (float3* posPtr = &position) {
                    memory.TryReadStruct(mccPlayerMonitorPosition, posPtr);
                    return true;
                }
            }
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static bool TryTeleportMonitor(float3 position) {
            if (mccPlayerMonitorPosition == default) return false;

            unsafe {
                memory.TryWriteStruct(mccPlayerMonitorPosition, &position);
                return true;
            }
        }
        #endregion

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static void ClearObjectList() => forgeObjects.Clear();

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static unsafe void AddObject(int i) {
            if (i >= HR_maxObjects) return;

            UIntPtr objPtr = forgeObjectArrayPointer + i * HR_ForgeObject.size;
            MccForgeObject mccFobj = new MccForgeObject(objPtr, i);
            mccFobj.data->idExt = 0xFFFFFFFF;
            mccFobj.data->spawnRelativeToMapIndex = 0xFFFF;
            forgeObjects[i] = mccFobj;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static unsafe H3_MapVariant* GetH3_MVAR_Ptr() {
            fixed (H3_MapVariant* ptr = &h3_mvar) return ptr;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static H3_MapVariant GetH3_MVAR() => h3_mvar;
    }
}

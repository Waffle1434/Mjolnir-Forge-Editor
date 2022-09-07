using RGiesecke.DllExport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ForgeLib {
    public enum Map {
        None,
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
        Forge_World   //forge_halo
    }

    public static class ForgeBridge {
        public const int maxObjects = 650;

        public static ProcessMemory memory = new ProcessMemory();
        static UIntPtr reachBase;

        public static Map currentMap;
        public static Dictionary<int, MccForgeObject> forgeObjects = new Dictionary<int, MccForgeObject>();
        static UIntPtr forgeObjectArrayPointer;

        static Dictionary<Map, UIntPtr> mapPlayerPositions = new Dictionary<Map, UIntPtr>();
        static UIntPtr mccPlayerMonitorPosition;
        //static unsafe float3* playerMonitorPosition;

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
        public static int GetObjectCount() => forgeObjects.Count;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static unsafe ForgeObject* GetObjectPtr(int i) => MccForgeObject.GetPointer(i);
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static unsafe string ForgeObject_GetItemName(int i) => forgeObjects[i].data->ItemName;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static int ItemNameToType([MarshalAs(UnmanagedType.LPWStr)] string itemName) {
            if (ItemParser.TryNameToType(itemName, currentMap, out int type)) return type;
            return 0;
        }

        static void GetPointers() {
            reachBase = memory.ModuleBaseAddress("haloreach.dll");
            UIntPtr forgeBase = memory.ReadPointer(reachBase + 0x2514938);
            gtLabelsPointer = forgeBase + 0x7F4;
            forgeObjectArrayPointer = forgeBase + 0x19FC;

            mapPlayerPositions[Map.Forge_World] = reachBase + 0x306ABC0;
            mapPlayerPositions[Map.Tempest] = reachBase + 0x30DD280;
            mapPlayerPositions[Map.Spire] = reachBase + 0x310EAD0;
            mapPlayerPositions[Map.None] = default;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static void ReadMemory() {
            if (!memory.Connected) return;

            GetPointers();

            _CacheCurrentMap();
            GetGtLabels();

            //mapPlayerPositions.TryGetValue(currentMap, out mccPlayerMonitorPosition);

            GetForgeObjects();
        }

        #region Map Name
        const int mapNameOffset = 0x2766710;
        static Map GetCurrentMap() => MapUtil.FromId(memory.ReadString(reachBase + mapNameOffset));

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
            for (int i = 0; i < maxObjects; i++) {
                UIntPtr objPtr = forgeObjectArrayPointer + i * ForgeObject.size;
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
                for (int i = 0; i < maxObjects; i++) {
                    if (forgeObjects.TryGetValue(i, out MccForgeObject fobj)) {
                        fobj.WriteMemory();
                    }
                    else {
                        UIntPtr objPtr = forgeObjectArrayPointer + i * ForgeObject.size;
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
            if (i >= maxObjects) return;

            UIntPtr objPtr = forgeObjectArrayPointer + i * ForgeObject.size;
            MccForgeObject mccFobj = new MccForgeObject(objPtr, i);
            mccFobj.data->idExt = 0xFFFFFFFF;
            mccFobj.data->spawnRelativeToMapIndex = 0xFFFF;
            forgeObjects[i] = mccFobj;
        }
    }
}

using RGiesecke.DllExport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static ForgeLib.ProcessMemory;

namespace ForgeLib {
    public enum Map {
        None,
        Boardwalk,    // 50_panopticon
        Boneyard,     // 70_boneyard
        Countdown,    // 45_launch_station
        Powerhouse,   // 30_settlement
        Reflection,   // 52_ivory_tower
        Spire,        // 35_island
        Sword_Base,   // 20_sword_slayer
        Zealot,       // 45_aftship
        Anchor_9,     // dlc_slayer
        Breakpoint,   // dlc_invasion
        Tempest,      // dlc_medium
        Condemned,    // condemned
        Highlands,    // trainingpreserve
        Battle_Canyon,// cex_beavercreek
        Penance,      // cex_damnation
        Ridgeline,    // cex_timberland
        Solitary,     // cex_prisoner
        High_Noon,    // cex_hangemhigh
        Breakneck,    // cex_headlong
        Forge_World,  // forge_halo
        Creep_Forge_World,        // creep_forge_halo
        Creep_Forge_World_Night   // creep_forge_halo_night
    }

    public static class ForgeBridge {
        public const int maxObjects = 650;

        public static ProcessMemory memory = new ProcessMemory();
        static UIntPtr reachBase;
        static UIntPtr forgeObjectArrayPointer;
        static UIntPtr mapFilenamePointer;

        public static Map currentMap;
        public static Dictionary<int, MccForgeObject> forgeObjects = new Dictionary<int, MccForgeObject>();

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static int GetDllVersion() => 6;

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

        // Old MCC versions can be obtained through Steam DB https://steamdb.info/app/976730/depots/
        static void GetPointers() {
            reachBase = memory.ModuleBaseAddress("haloreach.dll");
            GetForgePointers();
            GetMapFilenamePointer();
        }
        static void GetForgePointers() {
            /* Steam MCC Halo Reach Disassembly - 29 June 2022
            haloreach.dll+155A5 - 48 89 05 0CB79700     - mov [haloreach.dll+990CB8],rax
            haloreach.dll+155AC - 89 35 C2723B02        - mov [haloreach.dll+23CC874],esi
            haloreach.dll+155B2 - 75 0A                 - jne haloreach.dll+155BE
            haloreach.dll+155B4 - C7 05 B6723B02 02000000 - mov [haloreach.dll+23CC874],00000002
            haloreach.dll+155BE - 48 8B 05 CB723B02     - mov rax,[haloreach.dll+23CC890] <---------------- [haloreach.dll+23CC890] + 0x20000 = forge pointer
            haloreach.dll+155C5 - 4C 8D 4D 20           - lea r9,[rbp+20]
            haloreach.dll+155C9 - 89 74 24 28           - mov [rsp+28],esi
            haloreach.dll+155CD - 4C 8D 45 28           - lea r8,[rbp+28]
            haloreach.dll+155D1 - BA 00000200           - mov edx,00020000
            haloreach.dll+155D6 - 48 89 45 28           - mov [rbp+28],rax
            haloreach.dll+155DA - 33 C9                 - xor ecx,ecx
            haloreach.dll+155DC - C7 45 20 00003801     - mov [rbp+20],01380000
            haloreach.dll+155E3 - C7 44 24 20 08000000  - mov [rsp+20],00000008
            haloreach.dll+155EB - E8 98FEFFFF           - call haloreach.dll+15488
            */

            AOB forge_ptr_aob = new AOB("48 89 05 ?? ?? ?? ?? 89 35 ?? ?? ?? ?? 75 0A C7 05 ?? ?? ?? ?? 02 00 00 00 48 8B 05 ?? ?? ?? ?? 4C 8D 4D 20 89 74 24 28 4C 8D 45 28 BA 00 00 02 00 48 89 45 28 33 C9 C7 45 20 00 00 38 01 C7 44 24 20 08 00 00 00 E8 ?? ?? ?? ??");
            UIntPtr start_address = reachBase;
            if (memory.FindArrayOfBytes(forge_ptr_aob, start_address, 0x40000, out byte[] bytes, out int i_bytes)) {
                const int address_offset = 28;
                const int next_offset = address_offset + 4;
                int mov_offset = BitConverter.ToInt32(bytes, i_bytes + address_offset);
                Trace.Assert(mov_offset >= 0, "offset might be signed, uncertain.");

                UIntPtr next_instruction = start_address + i_bytes + next_offset;
                UIntPtr abs_address = next_instruction + mov_offset;

                UIntPtr forgeBase = memory.ReadPointer(abs_address) + 0x20000;
                const int update_offset = 0x10; // Something was added inbetween?
                gtLabelsPointer = forgeBase + update_offset + 0x7F4;
                forgeObjectArrayPointer = forgeBase + update_offset + 0x19FC;
            }
            else {
                throw new Exception("Failed to find Forge pointer");
            }
        }
        static void GetMapFilenamePointer() {
            /* Steam MCC Halo Reach Disassembly - 29 June 2022
            haloreach.dll+302E6 - E8 596A7600           - call haloreach.dll+796D44 { ->->VCRUNTIME140.memset }
            haloreach.dll+302EB - 0F10 05 86E74502      - movups xmm0,[haloreach.dll+248EA78] <---------------- This + 0x8 is a pointer to a pointer for map info
            haloreach.dll+302F2 - 8B 15 B8F74D02        - mov edx,[haloreach.dll+250FAB0]
            haloreach.dll+302F8 - 48 8D 05 09748200     - lea rax,[haloreach.dll+857708]
            haloreach.dll+302FF - 48 89 45 7F           - mov [rbp+7F],rax
            haloreach.dll+30303 - 41 B0 01              - mov r8b,01
            haloreach.dll+30306 - F3 0F7F 05 7AE74502   - movdqu [haloreach.dll+248EA88],xmm0
            haloreach.dll+3030E - 89 35 84E74502        - mov [haloreach.dll+248EA98],esi
            */
            AOB map_info_aob = new AOB("E8 ?? ?? ?? ?? 0F 10 05 ?? ?? ?? ?? 8B 15 ?? ?? ?? ?? 48 8D 05 ?? ?? ?? ?? 48 89 45 7F 41 B0 01 F3 0F 7F 05 ?? ?? ?? ?? 89 ?? ?? ?? ?? ??");
            UIntPtr start_address = reachBase + 0x20000;
            if (memory.FindArrayOfBytes(map_info_aob, start_address, 0x20000, out byte[] bytes, out int i_bytes)) {
                const int call_size = 5, movups_size = 7;
                int movups_offset = BitConverter.ToInt32(bytes, i_bytes + call_size + 3);
                Trace.Assert(movups_offset >= 0, "offset might be signed, uncertain.");

                UIntPtr next_instruction = start_address + i_bytes + call_size + movups_size; // mov instruction after movups
                UIntPtr abs_address = next_instruction + movups_offset;

                mapFilenamePointer = memory.ReadPointer(abs_address + 8) + 0xD0;
            }
            else {
                throw new Exception("Failed to find map name pointer");
            }
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
        const int mapNameOffset = 0x025CDBC0; // 0x25CDBB8 + 0x8
        static Map GetCurrentMap() => MapUtil.FromFilename(memory.ReadString(mapFilenamePointer, 32));

        static void _CacheCurrentMap() {
            currentMap = GetCurrentMap();

            if (currentMap == Map.None) {
                lastError += $"\nUnknown Map";
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

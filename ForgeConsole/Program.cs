using ForgeLib;
using System;

namespace ForgeConsole {
    class Program {
        static void Main(string[] args) {
            if (!ForgeBridge.TrySetConnect(true)) {
                Console.WriteLine(ForgeBridge.GetLastError());
                return;
            }

            ForgeBridge.ReadMemory();

            //Console.WriteLine(System.Runtime.InteropServices.Marshal.SizeOf(typeof(float3)));

            Console.WriteLine($"Map: {ForgeBridge.GetMapName()}, {ForgeBridge.GetObjectCount()} Objects");

            if (ForgeBridge.TryGetMonitorPosition(out float3 pos)) {
                Console.WriteLine($"Player Position: {pos}");
                ForgeBridge.TryTeleportMonitor(new float3(pos.x, pos.y, pos.z + 10f));
            }

            unsafe {
                int c = ForgeBridge.GetObjectCount();
                for (int i = 0; i < c; i++) {
                    ForgeObject fobj = *ForgeBridge.GetObjectPtr(i);
                    Console.WriteLine(fobj);
                }
            }
        }
    }
}

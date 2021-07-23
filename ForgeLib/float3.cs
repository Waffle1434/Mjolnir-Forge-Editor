using System.Runtime.InteropServices;

namespace ForgeLib {
    //[StructLayout(LayoutKind.Explicit, Size = size)]
    public struct float3 {
        //public const int size = 12;

        //[FieldOffset(0)] public float x;
        //[FieldOffset(4)] public float y;
        //[FieldOffset(8)] public float z;
        public float x, y, z;

        public float3(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString() => $"({x:0.00}, {y:0.00}, {z:0.00})";
    }
}

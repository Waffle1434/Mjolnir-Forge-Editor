using System.Runtime.InteropServices;

namespace ForgeLib {
    public enum Color : byte { Red, Blue, Green, Orange, Purple, Yellow, Brown, Pink, Neutral, TeamColor = 255 }

    public enum Shape : byte { None, Cylinder = 2, Box }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct float3 {
        public float x, y, z;

        public float3(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString() => $"({x:0.00}, {y:0.00}, {z:0.00})";

        public const int size = 12;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Transform {
        public float3 position;
        public float3 forward;
        public float3 up;

        public const int size = 36;
        public override string ToString() => $"{position} {forward} {up}";
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ShapeData {
        public float width;
        public float length;
        public float top;
        public float bottom;

        public const int size = 16;

        public override string ToString() => $"({width}, {length}, {top}, {bottom})";
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Range {
        public float min, max;

        public const int size = 2;

        public override string ToString() => $"({min:0.00}, {max:0.00})";
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Bounds {
        public Range x, y, z;

        public const int size = 3 * Range.size;

        public override string ToString() => $"({x}, {y}, {z})";
    }
}

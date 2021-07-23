using System;

namespace ForgeAssistant {
    struct MemoryRegionResult {
        public UIntPtr CurrentBaseAddress { get; set; }
        public long RegionSize { get; set; }
        public UIntPtr RegionBase { get; set; }
    }
}

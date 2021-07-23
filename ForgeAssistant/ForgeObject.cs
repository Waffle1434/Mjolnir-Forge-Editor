using System;

namespace ForgeAssistant {
    public class ForgeObject {
        public string address;

        public int itemId, itemVariant, type;
        public byte spawnTime;
        public float x, y, z;
        public bool valid;

        public ForgeObject(string address) {
            this.address = address;
        }

        public string ItemName => ItemParser.GetItemNameFromType(type, Form1.MapName);

        float ReadFloat(string code) => Form1.MCCMemory.ReadFloat(code, "", false);

        public void ReadFromMemory() {
            // TODO: read as block?
            // TODO: read from cached byte array?
            // TODO: use IntPtrs instead of string addresses?
            itemId = Form1.MCCMemory.ReadByte(address + "+6");
            itemVariant = Form1.MCCMemory.ReadByte(address + "+32");
            type = itemId << 8 | itemVariant;
            //ItemParser.GetItemNameFromType(type, MapName)

            x = ReadFloat(address + "+C");
            y = ReadFloat(address + "+10");
            z = ReadFloat(address + "+14");

            spawnTime = (byte)Form1.MCCMemory.ReadByte(address + "+46");// TODO: prevent cast to int
            valid = Form1.MCCMemory.ReadByte(address + "+4A") != 0;

            #region Rotation
            /*WI.r11 = ReadFloat(_SpawnAddr + "+18");
            WI.r12 = ReadFloat(_SpawnAddr + "+1C");
            WI.r13 = ReadFloat(_SpawnAddr + "+20");
            //WI.r21 = ReadFloat(_SpawnAddr + "+1C");
            //WI.r22 = ReadFloat(_SpawnAddr + "+20");
            //WI.r23 = ReadFloat(_SpawnAddr + "+24");
            WI.r31 = ReadFloat(_SpawnAddr + "+24");
            WI.r32 = ReadFloat(_SpawnAddr + "+28");
            WI.r33 = ReadFloat(_SpawnAddr + "+2C");*/

            /*if (MapName == "forge_halo") {
                SharpDX.Matrix3x3 tm = new SharpDX.Matrix3x3();
                const long plus = 0x0307D198;
                long inc = 0x18 * i;
                string tmpBase = "haloreach.dll+" + (plus + inc).ToString("X8");
                tm.M11 = ReadFloat(tmpBase + ",0x12C");
                tm.M12 = ReadFloat(tmpBase + ",0x130");
                tm.M13 = ReadFloat(tmpBase + ",0x134");
                tm.M21 = ReadFloat(tmpBase + ",0x13C");
                tm.M22 = ReadFloat(tmpBase + ",0x140");
                tm.M23 = ReadFloat(tmpBase + ",0x144");
                tm.M31 = ReadFloat(tmpBase + ",0x14C");
                tm.M32 = ReadFloat(tmpBase + ",0x150");
                tm.M33 = ReadFloat(tmpBase + ",0x154");
                WI._matrix = tm;
            }*/
            #endregion

            // size = 0x4C = 76 bytes
            //0x00 - 4 bytes
            //0x04 - int? valid?
            //0x06 - byte - itemCategory?
            //0x08 - 4 bytes - FFFFFFFF
            //0x0C - float - x
            //0x10 - float - y
            //0x14 - float - z
            //0x18 - float - r11
            //0x1C - float - r12
            //0x20 - float - r13
            //0x24 - float - r31
            //0x28 - float - r32
            //0x2C - float - r33
            //0x30 - float - 0000FFFF?
            //0x32 - byte - itemVariant?
            //0x46 - byte - spawnTime
            //0x4A - byte - valid?

            #region Memory Dumps
            /*00 - 13
            01 - 0
            02 - 0
            03 - 0
            04 - 1
            05 - 0
            06 - 70
            07 - 0
            08 - 255
            09 - 255
            0A - 255
            0B - 255
            0C - 190
            0D - 125
            0E - 155
            0F - 66
            10 - 85
            11 - 242
            12 - 74
            13 - 67
            14 - 130
            15 - 102
            16 - 134
            17 - 66
            18 - 0
            19 - 130
            1A - 29
            1B - 59
            1C - 182
            1D - 15
            1E - 73
            1F - 57
            20 - 208
            21 - 255
            22 - 127
            23 - 191
            24 - 160
            25 - 255
            26 - 127
            27 - 63
            28 - 158
            29 - 137
            2A - 29
            2B - 59
            2C - 158
            2D - 137
            2E - 29
            2F - 59
            30 - 255
            31 - 255
            32 - 6
            33 - 0
            34 - 0
            35 - 0
            36 - 0
            37 - 0
            38 - 0
            39 - 0
            3A - 0
            3B - 0
            3C - 0
            3D - 0
            3E - 0
            3F - 0
            40 - 0
            41 - 0
            42 - 0
            43 - 0
            44 - 0
            45 - 0
            46 - 0
            47 - 0
            48 - 255
            49 - 255
            4A - 204
            4B - 8*/

            /*00 - 0
            01 - 0
            02 - 0
            03 - 0
            04 - 1
            05 - 0
            06 - 74
            07 - 0
            08 - 255
            09 - 255
            0A - 255
            0B - 255
            0C - 32
            0D - 182
            0E - 44
            0F - 66
            10 - 225
            11 - 245
            12 - 89
            13 - 67
            14 - 40
            15 - 102
            16 - 132
            17 - 66
            18 - 239
            19 - 100
            1A - 137
            1B - 188
            1C - 42
            1D - 52
            1E - 116
            1F - 191
            20 - 192
            21 - 99
            22 - 153
            23 - 62
            24 - 138
            25 - 246
            26 - 127
            27 - 191
            28 - 163
            29 - 211
            2A - 137
            2B - 60
            2C - 85
            2D - 131
            2E - 29
            2F - 187
            30 - 255
            31 - 255
            32 - 5
            33 - 0
            34 - 0
            35 - 0
            36 - 0
            37 - 0
            38 - 0
            39 - 0
            3A - 0
            3B - 0
            3C - 0
            3D - 0
            3E - 0
            3F - 0
            40 - 0
            41 - 0
            42 - 0
            43 - 0
            44 - 0
            45 - 0
            46 - 0
            47 - 0
            48 - 255
            49 - 255
            4A - 204
            4B - 8*/
            #endregion
        }

        public void WriteToMemory() {
            byte[] by = new byte[1];
            by[0] = (byte)itemId;
            Form1.MCCMemory.WriteBytes(address + "+6", by);
            by[0] = (byte)itemVariant;
            Form1.MCCMemory.WriteBytes(address + "+32", by);

            Form1.MCCMemory.WriteBytes(address + "+C", BitConverter.GetBytes(x));
            Form1.MCCMemory.WriteBytes(address + "+10", BitConverter.GetBytes(y));
            Form1.MCCMemory.WriteBytes(address + "+14", BitConverter.GetBytes(y));

            by[0] = spawnTime;
            Form1.MCCMemory.WriteBytes(address + "+46", by);

            //List<string> rotOffsets = new List<string> { "+18", "+1C", "+20", "+24", "+28", "+2C" };
        }
    }
}

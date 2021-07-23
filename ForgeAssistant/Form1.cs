using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
//using SharpDX;

namespace ForgeAssistant {
    public partial class Form1 : Form {
        // Pointers
        string BasePointer = "haloreach.dll+0232A4E8";
        string MapNamePointer = "haloreach.dll+872760";
        string TypePointer = "haloreach.dll+00B22FB0,0x71A8";

        public static Mem MCCMemory = new Mem();
        Process MCCProcess = new Process();
        public CopyPaste CP;
        public static string MapName = "";
        string MonitorX = "";
        List<string> types = new List<string>();
        Dictionary<string, string> MapPlayerOffset = new Dictionary<string, string>();
        public static List<string> Addresses = new List<string>();
        public static Dictionary<string, ForgeObject> forgeObjects = new Dictionary<string, ForgeObject>();
        List<string> PAddresses = new List<string>();
        List<string> AddressHeap = new List<string>();
        //long ModuleBase = 0;
        //long ModuleEnd = 0;
        public List<WorldItem> WorldItems = new List<WorldItem>();
        public struct RotationMatrix {
            public double r11;
            public double r12;
            public double r13;
            public double r21;
            public double r22;
            public double r23;
            public double r31;
            public double r32;
            public double r33;
        }
        public struct WorldItem {
            public string Base;
            public string PhysBase;
            public string Type;
            public Vector3 Position;
            public Vector3 Render;
            public float r11;
            public float r12;
            public float r13;
            public float r21;
            public float r22;
            public float r23;
            public float r31;
            public float r32;
            public float r33;
            public SharpDX.Matrix3x3 _matrix;
        }
        #region Initial Attachment
        private void AttachToProcess() {
            try {
                MCCProcess = Process.GetProcessesByName("MCC-Win64-Shipping")[0];
                MCCMemory.OpenProcess(MCCProcess.Id);
                MCCMemory.Main = this;
                //attachToProcessToolStripMenuItem.PerformClick();
            }
            catch {
                MessageBox.Show("Make sure Master Chief Collection is running!");
            }
        }
        private void DetachFromProcess() {
            try {
                MCCMemory.CloseProcess();
                MCCProcess = null;
            }
            catch {

            }
        }
        #endregion
        #region Internal Functions
        private void GetReachType() {
            if (WorldItems.Count > 0) {
                for (int i = 0; i < (WorldItems.Count * 10); i++) {
                    int incrementor = 0x100C + i * 0x14;
                    int value = MCCMemory.ReadInt(TypePointer + ",0x" + incrementor.ToString("X4"));
                    string addr = value.ToString("X8");
                    //valid value
                    if (value != -1 && value != 0) {
                        //MessageBox.Show(addr);
                        types.Add(addr);
                        continue;
                    }
                    //check at address +4
                    if (value == -1) {
                        incrementor += 4;
                        value = MCCMemory.ReadInt(TypePointer + ",0x" + incrementor.ToString("X4"));
                        addr = value.ToString("X8");
                        if (value != -1 && value != 0) {
                            //MessageBox.Show(addr);
                            types.Add(addr);
                            i += 1;
                        }
                    }
                    //MessageBox.Show(value.ToString());
                }
                for (int i = 0; i < WorldItems.Count; i++) {
                    itemList.Items[i].SubItems[1].Text = types[i];
                }
            }
        }
        private void GetMonitorX() {
            //ReadMapName();
            //0x17C
            //MonitorX = MCCMemory.ReadLong("haloreach.dll+03614370").ToString("X8");
            MonitorX = MapPlayerOffset[MapName];
        }
        private string ReadMapName() {
            //byte[] addr = MCCMemory.ReadBytes(MapNamePointer, 30);
            string addr = MCCMemory.ReadString(MapNamePointer).Split('.')[0];
            MapName = addr;
            return addr;
            //this.Text = addr;
            //if (addr != null)
            //{
            //    string map = System.Text.Encoding.Unicode.GetString(addr);
            //    MapName = map.Split(',')[0];
            //    return map.Split(',')[0];
            //}
            //else
            //{
            //    return "";
            //}
        }
        private float ReadFloat(string code) => MCCMemory.ReadFloat(code, "", false);
        private void GetMapObjects() {
            long baseAddress = MCCMemory.ReadLong(BasePointer) + 0x19F8;
            for (int i = 0; i < 650; i++) {
                int incrementor = i * 0x4C;
                //int typeincrementor = i * 0x14;
                string _SpawnAddr = (baseAddress + incrementor).ToString("X8");
                bool isValid = MCCMemory.ReadByte(_SpawnAddr + "+4A") != 0;

                if (isValid) {
                    if (!forgeObjects.TryGetValue(_SpawnAddr, out ForgeObject fobj)) {
                        //itemList.Items.Add(new ListViewItem(new string[3]));
                    }

                    fobj.ReadFromMemory();

                    /*ListViewItem.ListViewSubItemCollection subItems = itemList.Items[i].SubItems;
                    subItems[0].Text = i.ToString();
                    subItems[1].Text = fobj.ItemName;
                    subItems[2].Text = $"({fobj.x:0.0}, {fobj.y:0.0}, {fobj.z:0.0})";*/
                }
                else if (forgeObjects.ContainsKey(_SpawnAddr)) {
                    forgeObjects.Remove(_SpawnAddr);
                }

                if (isValid) {
                    if (!Addresses.Contains(_SpawnAddr)) {// TODO: hashset/dictionary?
                        //string typeAddress = (MCCMemory.ReadLong(TypePointer) + 0x1160 + typeincrementor).ToString("X8");
                        //string type = MCCMemory.Read2Byte(typeAddress).ToString("X2");
                        int itemID = MCCMemory.ReadByte(_SpawnAddr + "+6");
                        int itemVariant = MCCMemory.ReadByte(_SpawnAddr + "+32");
                        int type = itemID << 8 | itemVariant;
                        WorldItem WI = new WorldItem();
                        WI.Base = _SpawnAddr;
                        Addresses.Add(_SpawnAddr);
                        ListViewItem LVI = new ListViewItem();
                        LVI.SubItems[0].Text = i.ToString();
                        //this.Text = label9.Text.Split(' ')[2];
                        //this.Text = MapName;
                        LVI.SubItems.Add(ItemParser.GetItemNameFromType(type, MapName));
                        //LVI.SubItems.Add(type);

                        /*byte[] bytes = MCCMemory.ReadBytes(_SpawnAddr, 76);
                        for (int i1 = 0; i1 < bytes.Length; i1++) {
                            byte by = bytes[i1];
                            Console.WriteLine($"{i1.ToString("X2")} - {by}");
                        }*/

                        float X = ReadFloat(_SpawnAddr + "+C");
                        float Y = ReadFloat(_SpawnAddr + "+10");
                        float Z = ReadFloat(_SpawnAddr + "+14");
                        Vector3 loc = new Vector3(X, Y, Z);
                        WI.Position = loc;
                        WI.r11 = ReadFloat(_SpawnAddr + "+18");
                        WI.r12 = ReadFloat(_SpawnAddr + "+1C");
                        WI.r13 = ReadFloat(_SpawnAddr + "+20");
                        //WI.r21 = ReadFloat(_SpawnAddr + "+1C");
                        //WI.r22 = ReadFloat(_SpawnAddr + "+20");
                        //WI.r23 = ReadFloat(_SpawnAddr + "+24");
                        WI.r31 = ReadFloat(_SpawnAddr + "+24");
                        WI.r32 = ReadFloat(_SpawnAddr + "+28");
                        WI.r33 = ReadFloat(_SpawnAddr + "+2C");
                        if (MapName == "forge_halo") {
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
                        }
                        WorldItems.Add(WI);
                        LVI.SubItems.Add($"({loc.X:0.0}, {loc.Y:0.0}, {loc.Z:0.0})");
                        itemList.Items.Add(LVI);
                    }
                    else {
                        string tmpAddress = Addresses[i];
                        float X = ReadFloat(tmpAddress + "+C");
                        float Y = ReadFloat(tmpAddress + "+10");
                        float Z = ReadFloat(tmpAddress + "+14");
                        Vector3 loc = new Vector3(X, Y, Z);
                        WorldItem WI = new WorldItem();
                        WI.Base = tmpAddress;
                        WI.Position = loc;
                        WI.r11 = ReadFloat(tmpAddress + "+18");
                        WI.r12 = ReadFloat(tmpAddress + "+1C");
                        WI.r13 = ReadFloat(tmpAddress + "+20");
                        //WI.r21 = ReadFloat(tmpAddress + "+1C");
                        //WI.r22 = ReadFloat(tmpAddress + "+20");
                        //WI.r23 = ReadFloat(tmpAddress + "+24");
                        WI.r31 = ReadFloat(tmpAddress + "+24");
                        WI.r32 = ReadFloat(tmpAddress + "+28");
                        WI.r33 = ReadFloat(tmpAddress + "+2C");
                        //if (MapName == "forge_halo")
                        //{
                        //    SharpDX.Matrix3x3 tm = new SharpDX.Matrix3x3();
                        //    long plus = 0x0307D198;
                        //    long inc = 0x18 * i;
                        //    string tmpBase = "haloreach.dll+" + (plus + inc).ToString("X8");
                        //    tm.M11 = ReadFloat(tmpBase + ",0x12C");
                        //    tm.M12 = ReadFloat(tmpBase + ",0x130");
                        //    tm.M13 = ReadFloat(tmpBase + ",0x134");
                        //    tm.M21 = ReadFloat(tmpBase + ",0x13C");
                        //    tm.M22 = ReadFloat(tmpBase + ",0x140");
                        //    tm.M23 = ReadFloat(tmpBase + ",0x144");
                        //    tm.M31 = ReadFloat(tmpBase + ",0x14C");
                        //    tm.M32 = ReadFloat(tmpBase + ",0x150");
                        //    tm.M33 = ReadFloat(tmpBase + ",0x154");
                        //    WI._matrix = tm;
                        //}
                        WorldItems[i] = WI;
                        itemList.Items[i].SubItems[2].Text = $"({loc.X:0.0}, {loc.Y:0.0}, {loc.Z:0.0})";
                    }
                }
                else if (Addresses.Contains(_SpawnAddr)) {// No longer valid, remove address.
                    Addresses.RemoveAt(i);
                    itemList.Items.RemoveAt(i);
                    WorldItems.RemoveAt(i);
                    continue;
                }
            }
            //if (listView1.Items.Count == WorldItems.Count)
            //{
            //    GetReachType();
            //}
        }
        #endregion
        public Form1() {
            InitializeComponent();
        }
        public Writer writer;
        private void Form1_Load(object sender, EventArgs e) {
            writer = new Writer(this);
            lTotal.Location = new Point(lTotal.Location.X - 46, lTotal.Location.Y);
            lIndex.Location = new Point(lTotal.Location.X + 2, lIndex.Location.Y);
            lSelected.Location = new Point(lTotal.Location.X + 1, lSelected.Location.Y);
            editProgress.BringToFront();
            CheckForIllegalCrossThreadCalls = false;
            CP = new CopyPaste(this);
            //listView1.Columns[0].Width = 100;
            //listView1.Columns[1].Width = 60;
            //listView1.Columns[2].Width = 160;
            MapPlayerOffset["50_panopticon"] = "+036123A8,0x17C"; // Boardwalk
            MapPlayerOffset["70_boneyard"] = "+030505D0,0x1C0"; // Boneyard
            MapPlayerOffset["45_launch_station"] = "+0304B350,0x1C0"; // Countdown
            MapPlayerOffset["30_settlement"] = "+0304B530,0x1C0"; // Powerhouse
            MapPlayerOffset["52_ivory_tower"] = "+0304C110,0x1C0"; // Reflection
            MapPlayerOffset["35_island"] = "+0304ECF0,0x1C0"; // Spire
            MapPlayerOffset["20_sword_slayer"] = "+0304C930,0x1C0"; // Sword Base
            MapPlayerOffset["45_aftship"] = "+030B4728,0x1C0"; // Zealot
            MapPlayerOffset["dlc_slayer"] = "+0304BF30,0x1C0"; // Anchor 9
            MapPlayerOffset["dlc_invasion"] = "+02FBECC8,0x1C0"; // Breakpoint
            MapPlayerOffset["dlc_medium"] = "+0304B670,0x1C0"; // Tempest
            MapPlayerOffset["condemned"] = "+0304C6B0,0x1C0"; // Condemned
            MapPlayerOffset["trainingpreserve"] = "+0304F450,0x1C0"; // Highlands
            MapPlayerOffset["cex_beavercreek"] = "+0304B490,0x1C0"; // Battle Canyon
            MapPlayerOffset["cex_damnation"] = "+0304B5D0,0x1C0"; // Penance
            MapPlayerOffset["cex_timberland"] = "+0304F270,0x1C0"; // Ridgeline
            MapPlayerOffset["cex_prisoner"] = "+0304B850,0x1C0"; // Solitary
            MapPlayerOffset["cex_hangemhigh"] = "+03612388,0x17C"; // High Noon
            MapPlayerOffset["cex_headlong"] = "+0304EF50,0x1C0"; // Breakneck
            MapPlayerOffset["forge_halo"] = "+03612398,0x17C"; // Forge World
            //"haloreach.dll"+03614370
            //for (var i = 0; i < 20; i++)
            //{
            //    MapPlayerOffset.Add("+03614370"); // All Maps
            //}
            //listView1.Columns[3].Width = 120;
            //AttachToProcess();
            attachToProcessToolStripMenuItem.PerformClick();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            DetachFromProcess();
        }
        private void nMoveInc_ValueChanged(object sender, EventArgs e) {
            nMoveX.Increment = nMoveInc.Value;
            nMoveY.Increment = nMoveInc.Value;
            nMoveZ.Increment = nMoveInc.Value;
        }

        private void GetSpawnTime(string addrs) {
            int spawnTime = MCCMemory.ReadByte(addrs);
            this.nSpawnTime.Value = spawnTime;
        }
        private void SetSpawnTime(int amount, string addrs) {
            byte[] valueBytes = new byte[] { };
            valueBytes = BitConverter.GetBytes(amount);
            MCCMemory.WriteBytes(addrs, valueBytes);
        }
        private void nSpawnTime_ValueChanged(object sender, EventArgs e) {
            if (!itemList.Focused && itemList.SelectedItems.Count > 0) {
                //write spawn time
                SetSpawnTime((int)nSpawnTime.Value, WorldItems[itemList.SelectedItems[0].Index].Base + "+46");
            }
        }

        int index = -1;
        private void itemList_SelectedIndexChanged(object sender, EventArgs e) {
            //.

            contextMenu.Visible = false;
            if (itemList.SelectedItems.Count > 0) {
                lIndex.Text = "Index : " + itemList.SelectedItems[0].Index;
                lSelected.Text = "Selected : " + itemList.SelectedItems.Count;
                //this.Text = listView1.SelectedItems[0].Index.ToString();
                GetMonitorX();
                index = itemList.SelectedIndices[0];
                GetSpawnTime(WorldItems[index].Base + "+46");
                nX.Value = (decimal)WorldItems[index].Position.X;
                nY.Value = (decimal)WorldItems[index].Position.Y;
                nZ.Value = (decimal)WorldItems[index].Position.Z;

                DefaultSpawnX = (decimal)WorldItems[index].Position.X;
                DefaultSpawnY = (decimal)WorldItems[index].Position.Y;
                DefaultSpawnZ = (decimal)WorldItems[index].Position.Z;

                n11.Value = (decimal)WorldItems[index].r11;

                //label14.Text = (ReadFloat(PAddresses[0] + RenderOffsets[1])).ToString();
                n21.Value = (decimal)WorldItems[index].r12;
                n31.Value = (decimal)WorldItems[index].r13;
                //numericUpDown14.Value = (decimal)WorldItems[index].r21;
                //numericUpDown15.Value = (decimal)WorldItems[index].r22;
                //numericUpDown16.Value = (decimal)WorldItems[index].r23;
                n13.Value = (decimal)WorldItems[index].r31;
                n23.Value = (decimal)WorldItems[index].r32;
                n33.Value = (decimal)WorldItems[index].r33;
                Pitch.Value = 0;
                Yaw.Value = 0;
                Roll.Value = 0;
                if (MapName == "forge_halo") {
                    bTeleport.Enabled = true;
                }
                //button1.Enabled = true;
                GetRollPitchYaw(itemList.SelectedItems[0].Index);
            }
            else {
                index = -1;

                //button1.Enabled = false;
            }
            nMoveX.Value = 0;
            nMoveY.Value = 0;
            nMoveZ.Value = 0;
        }
        private void itemList_MouseUp(object sender, MouseEventArgs e) {
            if (itemList.SelectedItems.Count > 0 && itemList.SelectedIndices[0] > -1) {
                if (e.Button == MouseButtons.Right) {
                    contextMenu.Location = new Point(e.X, e.Y);
                    contextMenu.Visible = true;
                }
            }
            else {
                contextMenu.Visible = false;
            }
        }

        private void UpdateNumerics(object sender, EventArgs e) {
            if (!itemList.Focused && itemList.SelectedItems.Count > 0) {
                timer1.Enabled = false;
                NumericUpDown tmpNUD = sender as NumericUpDown;

                if (tmpNUD.Name.Contains("SX")) {
                    if (index > -1) {
                        if (!writer.BGW.IsBusy) {
                            writer.Amount = (float)tmpNUD.Value;
                            writer.Axis = "X";
                            writer.DoWork(1);
                        }
                    }
                }
                if (tmpNUD.Name.Contains("SY")) {
                    if (index > -1) {
                        if (!writer.BGW.IsBusy) {
                            writer.Amount = (float)tmpNUD.Value;
                            writer.Axis = "Y";
                            writer.DoWork(1);
                        }
                    }
                }
                if (tmpNUD.Name.Contains("SZ")) {
                    if (index > -1) {
                        if (!writer.BGW.IsBusy) {
                            writer.Amount = (float)tmpNUD.Value;
                            writer.Axis = "Z";
                            writer.DoWork(1);
                        }
                    }
                }
            }
        }
        decimal DefaultSpawnX = 0;
        decimal DefaultSpawnY = 0;
        decimal DefaultSpawnZ = 0;
        private void MoveEvent(object sender, EventArgs e) {
            if (itemList.SelectedItems.Count > 0) {
                NumericUpDown tmpMove = sender as NumericUpDown;
                string name = tmpMove.Name;
                switch (name[name.Length - 1]) {
                    case 'X': writer.Axis = "X"; break;
                    case 'Y': writer.Axis = "Y"; break;
                    case 'Z': writer.Axis = "Z"; break;
                }

                writer.Amount = (float)tmpMove.Value;
                if (!writer.BGW.IsBusy) writer.DoWork(0);
            }
        }
        //AOB Mask 00 00 80 3F 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? F0 00 70 42
        private void timer1_Tick(object sender, EventArgs e) {
            lMap.Text = "Map : " + ReadMapName();
            GetMapObjects();
            lTotal.Text = "Total : " + Addresses.Count.ToString();
        }
        private void attachToProcessToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!timer1.Enabled) {
                attachToProcessToolStripMenuItem.Text = "Detach from Process";
                AttachToProcess();
                timer1.Enabled = true;
                //this.Text = "Forge Assistant : " + ReadMapName();
                //if (ReadMapName() != "NO MAP LOADED")
                //{
                //    //ReadSpawnPoint();
                //    ReadItem();
                //    GetPlayerLocation();
                //}
            }
            else {
                attachToProcessToolStripMenuItem.Text = "Attach to Process";
                DetachFromProcess();
                timer1.Enabled = false;
            }
        }

        private void msToolStripMenuItem_Click(object sender, EventArgs e) {
            msToolStripMenuItem1.Checked = false;
            msToolStripMenuItem2.Checked = false;
            msToolStripMenuItem3.Checked = false;
            timer1.Interval = 100;
        }
        private void msToolStripMenuItem1_Click(object sender, EventArgs e) {
            msToolStripMenuItem.Checked = false;
            msToolStripMenuItem2.Checked = false;
            msToolStripMenuItem3.Checked = false;
            timer1.Interval = 250;
        }
        private void msToolStripMenuItem2_Click(object sender, EventArgs e) {
            msToolStripMenuItem.Checked = false;
            msToolStripMenuItem1.Checked = false;
            msToolStripMenuItem3.Checked = false;
            timer1.Interval = 500;
        }
        private void msToolStripMenuItem3_Click(object sender, EventArgs e) {
            msToolStripMenuItem.Checked = false;
            msToolStripMenuItem1.Checked = false;
            msToolStripMenuItem2.Checked = false;
            timer1.Interval = 1000;
        }
        private void keepInForegroundToolStripMenuItem_Click(object sender, EventArgs e) {
            if (keepInForegroundToolStripMenuItem.Checked) {
                this.TopMost = true;
            }
            else {
                this.TopMost = false;
            }
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            ABOUT ab = new ABOUT();
            ab.ShowDialog(this);
        }

        private void ManualEdit(bool enabled) {
            nX.Enabled = enabled;
            nY.Enabled = enabled;
            nZ.Enabled = enabled;
        }
        private void cbManualEdit_CheckedChanged(object sender, EventArgs e) => ManualEdit(cbManualEdit.Checked);

        private RotationMatrix Get3x3Matrix() {
            RotationMatrix RM = new RotationMatrix();
            double RadiansMultiplier = Math.PI / 180;
            double su = Math.Sin((double)Roll.Value * RadiansMultiplier * -1);
            double cu = Math.Cos((double)Roll.Value * RadiansMultiplier * -1);
            double sv = Math.Sin((double)Pitch.Value * RadiansMultiplier);
            double cv = Math.Cos((double)Pitch.Value * RadiansMultiplier);
            double sw = Math.Sin((double)Yaw.Value * RadiansMultiplier * -1);
            double cw = Math.Cos((double)Yaw.Value * RadiansMultiplier * -1);

            RM.r11 = cv * cw;
            RM.r12 = su * sv * cw - cu * sw;
            RM.r13 = su * sw + cu * sv * cw;
            RM.r21 = cv * sw;
            RM.r22 = cu * cw + su * sv * sw;
            RM.r23 = cu * sv * sw - su * cw;
            RM.r31 = -sv;
            RM.r32 = su * cv;
            RM.r33 = cu * cv;

            return RM;
        }
        public RotationMatrix Get3x3Matrix(float r, float p, float y) {
            RotationMatrix RM = new RotationMatrix();
            double RadiansMultiplier = Math.PI / 180;
            double su = Math.Sin((double)Roll.Value * RadiansMultiplier * -1);
            double cu = Math.Cos((double)Roll.Value * RadiansMultiplier * -1);
            double sv = Math.Sin((double)Pitch.Value * RadiansMultiplier);
            double cv = Math.Cos((double)Pitch.Value * RadiansMultiplier);
            double sw = Math.Sin((double)Yaw.Value * RadiansMultiplier * -1);
            double cw = Math.Cos((double)Yaw.Value * RadiansMultiplier * -1);

            RM.r11 = cv * cw;
            RM.r12 = su * sv * cw - cu * sw;
            RM.r13 = su * sw + cu * sv * cw;
            RM.r21 = cv * sw;
            RM.r22 = cu * cw + su * sv * sw;
            RM.r23 = cu * sv * sw - su * cw;
            RM.r31 = -sv;
            RM.r32 = su * cv;
            RM.r33 = cu * cv;

            return RM;
        }
        private SharpDX.Matrix3x3 SharpGet3x3Matrix(double r, double p, double y) {
            SharpDX.Matrix3x3 RM = new SharpDX.Matrix3x3();
            double RadiansMultiplier = Math.PI / 180;
            float su = (float)Math.Sin(r * RadiansMultiplier * -1);
            float cu = (float)Math.Cos(r * RadiansMultiplier * -1);
            float sv = (float)Math.Sin(p * RadiansMultiplier);
            //
            float cv = (float)Math.Cos(p * RadiansMultiplier);
            float sw = (float)Math.Sin(y * RadiansMultiplier * -1);
            //

            float cw = (float)Math.Cos(y * RadiansMultiplier * -1);

            RM.M11 = cv * cw;
            RM.M12 = su * sv * cw - cu * sw;
            RM.M13 = su * sw + cu * sv * cw;

            RM.M21 = cv * sw;
            RM.M22 = cu * cw + su * sv * sw;
            RM.M23 = cu * sv * sw - su * cw;

            RM.M31 = -sv;
            RM.M32 = su * cv;
            RM.M33 = cu * cv;

            return RM;
        }
        private SharpDX.Matrix3x2 sharp3x2(double r, double p, double y) {
            SharpDX.Matrix3x2 RM = new SharpDX.Matrix3x2();
            double RadiansMultiplier = Math.PI / 180;
            float su = (float)Math.Sin(r * RadiansMultiplier * -1);
            float cu = (float)Math.Cos(r * RadiansMultiplier * -1);
            float sv = (float)Math.Sin(p * RadiansMultiplier);
            float cv = (float)Math.Cos(p * RadiansMultiplier);
            float sw = (float)Math.Sin(y * RadiansMultiplier * -1);
            float cw = (float)Math.Cos(y * RadiansMultiplier * -1);

            RM.M11 = cv * cw;
            RM.M12 = su * sv * cw - cu * sw;
            RM.M21 = su * sw + cu * sv * cw;
            RM.M22 = -sv;
            RM.M31 = su * cv;
            RM.M32 = cu * cv;
            return RM;
        }
        public Vector3 GetRollPitchYaw(int idx) {
            double RadiansToDegrees = Math.PI / 180;

            float r11 = WorldItems[idx].r11;
            float r12 = WorldItems[idx].r12;
            float r13 = WorldItems[idx].r13;
            //float r21 = WorldItems[idx]._matrix.M21;
            //float r22 = WorldItems[idx]._matrix.M22;
            //float r23 = WorldItems[idx]._matrix.M23;
            float r31 = WorldItems[idx].r31;
            float r32 = WorldItems[idx].r32;
            float r33 = WorldItems[idx].r33;

            //float r21;

            //m21 = m11 + ((m31 + m12) - -m13 - (m32 + m33))
            //double r21 = r11 - ((r32 + r33) - (r12 + r13));

            //r11 = m11
            //r12 = m31
            //r13 = 


            //double r21 = r11 + ((r31 + r12) - -r13 - (r32+r33));

            //double r21 = (r13 / r32);
            //double corrector2 = Math.PI - corrector;

            //double pitch = (Math.Asin(WorldItems[idx].r31) * -1) / RadiansToDegrees;
            double pitch = Math.Atan2(-r31, Math.Sqrt(Math.Pow(r32, 2) + Math.Pow(r33, 2))) / RadiansToDegrees;
            double pitch2 = Math.PI - pitch;

            // double yaw = (Math.Atan2(-r21, r11)) / -RadiansToDegrees; ;
            double yaw = 0;// = Math.Asin(r12)/RadiansToDegrees;
            double roll;// = Math.Atan2(r32, r33) / RadiansToDegrees; ;
            if (r32 == 0) {
                yaw = Math.Asin(r12) / RadiansToDegrees;
            }
            if (r32 != 0 && r31 == 0) {
                double r21 = r13 / r32;
                yaw = Math.Atan2(r21, r11) / RadiansToDegrees;
            }
            if (r32 != 0 && r31 != 0) {
                double r21 = (r32 / r31) / 2;
                yaw = Math.Atan2(r21, r11) / RadiansToDegrees;
            }


            if (Math.Cos(pitch) > 0) {
                //roll = (Math.Atan2(r32 / Math.Cos(pitch), r33 / Math.Cos(pitch))) / RadiansToDegrees;
                //roll = Math.Atan2(-r31, Math.Sqrt(Math.Pow(r32, 2) + Math.Pow(r33, 2))) / RadiansToDegrees;
                //roll = Math.Atan2(r32, r33) / RadiansToDegrees;

                roll = Math.Asin(r12) / RadiansToDegrees;

                //roll = Math.Asin(r11) / RadiansToDegrees;
                //yaw = (Math.Atan2(r13 / Math.Cos(roll), r11 / Math.Cos(roll))) / RadiansToDegrees;
                //roll = Math.Atan2(r11, r12) / RadiansToDegrees;
                //yaw = (Math.Atan2(-r11, r11)) / -RadiansToDegrees;
                //roll = Math.Asin(r11) / RadiansToDegrees;
                //yaw = (Math.Acos(r31) / Math.Asin(r11)) / RadiansToDegrees;
            }
            else if (Math.Cos(pitch) < 0) {
                //roll = (Math.Atan2(r32 / Math.Cos(pitch2), r33 / Math.Cos(pitch2))) / RadiansToDegrees;
                //roll = Math.Atan2(r32, r33) / RadiansToDegrees;
                roll = Math.Asin(r12) / RadiansToDegrees;
                //yaw = (Math.Atan2(-r11, r11)) / RadiansToDegrees;
            }
            else {
                yaw = 0;
                roll = 0;
            }
            Vector3 tmpVec = new Vector3((float)pitch, (float)yaw, -(float)roll);
            //roll = (Math.Atan2(WorldItems[index].r32, WorldItems[index].r33) * -1) / RadiansToDegrees;
            Pitch.Value = (decimal)pitch;
            Yaw.Value = (decimal)yaw;
            Roll.Value = -(decimal)roll;
            return tmpVec;
            //Pitch.Value = (decimal)pitch;
            //Yaw.Value = (decimal)yaw;
            //Roll.Value = (decimal)roll;
        }
        public List<float> RotFloats = new List<float>();

        private void nRotInc_ValueChanged(object sender, EventArgs e) {
            if (nRotInc.Value != 0) {
                Pitch.Increment = nRotInc.Value;
                Yaw.Increment = nRotInc.Value;
                Roll.Increment = nRotInc.Value;
            }
        }
        private void nRot_ValueChanged(object sender, EventArgs e) {
            if (!itemList.Focused && itemList.SelectedItems.Count > 0) {
                RotationMatrix RM = Get3x3Matrix();
                n11.Value = (decimal)RM.r11;
                n21.Value = (decimal)RM.r12;
                n31.Value = (decimal)RM.r13;
                //numericUpDown14.Value = (decimal)RM.r21;
                //numericUpDown15.Value = (decimal)RM.r22;
                //numericUpDown16.Value = (decimal)RM.r23;
                n13.Value = (decimal)RM.r31;
                n23.Value = (decimal)RM.r32;
                n33.Value = (decimal)RM.r33;
                writer.RotFloats.Clear();
                writer.RotFloats.Add((float)n11.Value);
                writer.RotFloats.Add((float)n21.Value);
                writer.RotFloats.Add((float)n31.Value);
                writer.RotFloats.Add((float)n13.Value);
                writer.RotFloats.Add((float)n23.Value);
                writer.RotFloats.Add((float)n33.Value);
                Matrix3x2 tmpM = new Matrix3x2((float)RM.r11, (float)RM.r12, (float)RM.r13, (float)RM.r31, (float)RM.r32, (float)RM.r33);
                //MessageBox.Show(tmpM.ToString());
                Clipboard.SetText(tmpM.ToString());
                if (!writer.BGW.IsBusy) {
                    writer.DoWork(2);
                    //WriteSpawnRotation((float)numericUpDown12.Value, "+1C");
                    //WriteSpawnRotation((float)numericUpDown13.Value, "+20");

                    //WriteSpawnRotation((float)numericUpDown17.Value, "+24");
                    //WriteSpawnRotation((float)numericUpDown18.Value, "+28");
                    //WriteSpawnRotation((float)numericUpDown19.Value, "+2C");
                }
            }
        }

        private void _Refresh() {
            //timer1.Enabled = false;
            Addresses.Clear();
            PAddresses.Clear();
            AddressHeap.Clear();
            WorldItems.Clear();
            itemList.Items.Clear();
            //timer1.Enabled = true;
        }

        private void copyRotationToolStripMenuItem_Click(object sender, EventArgs e) {
            if (itemList.SelectedItems.Count > 0) {
                CP.Copy();
                pasteLocationToolStripMenuItem.Enabled = true;
                pasteRotationToolStripMenuItem.Enabled = true;
                pasteLocationAndRotationToolStripMenuItem.Enabled = true;
            }
            contextMenu.Visible = false;
        }

        private void pasteLocationToolStripMenuItem_Click(object sender, EventArgs e) {
            if (itemList.SelectedItems.Count > 0) {
                CP.PasteLocation();
            }
            contextMenu.Visible = false;
        }
        private void pasteRotationToolStripMenuItem_Click(object sender, EventArgs e) {
            if (itemList.SelectedItems.Count > 0) {
                CP.PasteRotation();
            }
            contextMenu.Visible = false;
        }
        private void pasteLocationAndRotationToolStripMenuItem_Click(object sender, EventArgs e) {
            if (itemList.SelectedItems.Count > 0) {
                CP.PasteLocationAndRotation();
            }
            contextMenu.Visible = false;
        }

        //public float Amount;
        //public List<float> Amounts = new List<float>();
        //public string Axis;

        /*private void ReadInGameList() {
            //7FF9F9CFED71
        }*/

        private void copymulti_Click(object sender, EventArgs e) {
            CP.CopyMulti();
            contextMenu.Visible = false;
            pastemulti.Enabled = true;
        }

        private void pasteLocationMultiToolStripMenuItem_Click(object sender, EventArgs e) {
            CP.PasteLocationMulti();
            contextMenu.Visible = false;
        }
        private void pasteRotationMultiToolStripMenuItem_Click(object sender, EventArgs e) {
            CP.PasteRotationMulti();
            contextMenu.Visible = false;
        }
        private void pasteLocationAndRotationMultiToolStripMenuItem_Click(object sender, EventArgs e) {
            CP.PasteRotLocMulti();
            contextMenu.Visible = false;
        }

        private string MatOutput(string matrix) {
            string tmpString = matrix.Replace("{ ", "").Replace(" {", "\n{").Replace(" }", "").Replace("0 ", "0\t       ").Replace("1 ", "1\t       ");
            return tmpString;
        }
        private SharpDX.Matrix3x2 Multiply3x3_3x2(SharpDX.Matrix3x2 m3x2, SharpDX.Matrix3x3 m3x3) {
            SharpDX.Matrix3x2 tmpMat = new SharpDX.Matrix3x2();
            tmpMat.M11 = m3x3.M11 * m3x2.M11 + m3x3.M12 * m3x2.M21 + m3x3.M13 * m3x2.M31;
            tmpMat.M12 = m3x3.M11 * m3x2.M12 + m3x3.M12 * m3x2.M22 + m3x3.M13 * m3x2.M32;
            //tmpMat.M12 = m3x3.M21 * m3x2.M12 + m3x3.M22 * m3x2.M22 + m3x3.M23 * m3x2.M32;

            //tmpMat.M12 *= -1;

            tmpMat.M21 = m3x3.M21 * m3x2.M11 + m3x3.M22 * m3x2.M21 + m3x3.M23 * m3x2.M31;
            tmpMat.M22 = m3x3.M21 * m3x2.M12 + m3x3.M22 * m3x2.M22 + m3x3.M23 * m3x2.M32;

            //tmpMat.M22 *= -1;

            tmpMat.M31 = m3x3.M31 * m3x2.M11 + m3x3.M32 * m3x2.M21 + m3x3.M33 * m3x2.M31;
            tmpMat.M32 = m3x3.M31 * m3x2.M12 + m3x3.M32 * m3x2.M22 + m3x3.M33 * m3x2.M32;
            //tmpMat.M32 = m3x3.M21 * m3x2.M11 + m3x3.M22 * m3x2.M21 + m3x3.M23 * m3x2.M31;

            return tmpMat;
        }
        private string CleanFloat(float tmp) {
            int stringLength = tmp.ToString().Length;
            string returnString = "";
            if (stringLength == 1) {
                returnString = " " + tmp.ToString() + ".0000000";
            }
            if (stringLength == 2) {
                returnString = tmp.ToString() + ".0000000";
            }
            if (stringLength == 3) {
                returnString = " " + tmp.ToString() + "000000";
            }
            if (stringLength == 4) {
                returnString = tmp.ToString() + "000000";
            }
            if (stringLength > 4) {
                if (tmp > 0) {
                    returnString = " " + tmp.ToString();
                }
                else {
                    returnString = tmp.ToString();
                }
            }
            return returnString;
        }
        private string FormatMatrix3x2(SharpDX.Matrix3x2 mat) {
            string M11 = CleanFloat(mat.M11);
            string M12 = CleanFloat(mat.M12);
            string M21 = CleanFloat(mat.M21);
            string M22 = CleanFloat(mat.M22);
            string M31 = CleanFloat(mat.M31);
            string M32 = CleanFloat(mat.M32);

            string output = "[M11 : " + M11 + "\tM12 : " + M12 + "]\n" + "[M21 : " + M21 + "\tM22 : " + M22 + "]\n" + "[M31 : " + M31 + "\tM32 : " + M32 + "]\n";
            return output;
        }
        private string FormatMatrix3x3(SharpDX.Matrix3x3 mat) {
            string M11 = CleanFloat(mat.M11);
            string M12 = CleanFloat(mat.M12);
            string M13 = CleanFloat(mat.M13);
            string M21 = CleanFloat(mat.M21);
            string M22 = CleanFloat(mat.M22);
            string M23 = CleanFloat(mat.M23);
            string M31 = CleanFloat(mat.M31);
            string M32 = CleanFloat(mat.M32);
            string M33 = CleanFloat(mat.M33);

            string output = "[M11 : " + M11 + "\tM12 : " + M12 + "\tM13 : " + M13 + "]\n" + "[M21 : " + M21 + "\tM22 : " + M22 + "\tM23 : " + M23 + "]\n" + "[M31 : " + M31 + "\tM32 : " + M32 + "\tM33 : " + M33 + "]\n";
            return output;
        }

        static List<List<float>> comb;
        static bool[] used;
        static List<SharpDX.Matrix3x2> GetCombinationSample(float[] arr) {
            //int[] arr = { 10, 50, 3, 1, 2 };
            used = new bool[arr.Length];
            List<string> done = new List<string>();
            //used.Fill(false);
            string tmpstring = "";
            comb = new List<List<float>>();
            List<float> c = new List<float>();
            GetComb(arr, 0, c);

            List<SharpDX.Matrix3x2> MatList = new List<SharpDX.Matrix3x2>();

            foreach (List<float> item in comb) {
                SharpDX.Matrix3x2 tmpMat = new SharpDX.Matrix3x2();
                List<float> MatFloats = new List<float>();
                foreach (float x in item) {
                    string cur = x + ",";
                    //Console.Write(x + ",");
                    MatFloats.Add(x);
                    tmpstring += cur;
                }
                tmpMat.M11 = MatFloats[0];
                tmpMat.M12 = MatFloats[1];
                tmpMat.M21 = MatFloats[2];
                tmpMat.M22 = MatFloats[3];
                tmpMat.M31 = MatFloats[4];
                tmpMat.M32 = MatFloats[5];
                MatList.Add(tmpMat);
                if (!done.Contains(tmpstring)) {
                    done.Add(tmpstring);
                    tmpstring = "";
                }
                tmpstring += "";
                Console.WriteLine("");
            }
            for (int i = 0; i < done.Count; i++) {
                tmpstring += done[i] + "\n";
            }
            return MatList;
        }
        static void GetComb(float[] arr, int colindex, List<float> c) {

            if (colindex >= arr.Length) {
                comb.Add(new List<float>(c));
                return;
            }
            for (int i = 0; i < arr.Length; i++) {
                if (!used[i]) {
                    used[i] = true;
                    c.Add(arr[i]);
                    GetComb(arr, colindex + 1, c);
                    c.RemoveAt(c.Count - 1);
                    used[i] = false;
                }
            }
        }
        List<float> MatrixVariables = new List<float>();
        //string combinations = "";

        private Vector2 GetOriginZ() {
            float xmax = -10000;
            float ymax = -10000;
            float xmin = 10000;
            float ymin = 10000;
            for (int i = 0; i < itemList.SelectedItems.Count; i++) {
                int tmpIndex = itemList.SelectedItems[i].Index;
                float tmpX = WorldItems[tmpIndex].Position.X;
                //MessageBox.Show(tmpX.ToString());
                float tmpY = WorldItems[tmpIndex].Position.Y;
                if (tmpX > xmax) {
                    xmax = tmpX;
                }
                if (tmpX < xmin) {
                    xmin = tmpX;
                }
                if (tmpY > ymax) {
                    ymax = tmpY;
                }
                if (tmpY < ymin) {
                    ymin = tmpY;
                }
            }
            //MessageBox.Show("X Max : " + xmax + "\n" + "Y Max : " + ymax);
            //MessageBox.Show("X Min : " + xmin + "\n" + "Y Min : " + ymin);
            float xDist = (xmax - xmin) / 2;
            float yDist = (ymax - ymin) / 2;
            Vector2 Origin = new Vector2(xmax - xDist, ymax - yDist);
            this.Text = Origin.ToString();
            return Origin;
        }
        private Vector2 GetOriginY() {
            float xmax = -10000;
            float zmax = -10000;
            float xmin = 10000;
            float zmin = 10000;
            for (int i = 0; i < itemList.SelectedItems.Count; i++) {
                int tmpIndex = itemList.SelectedItems[i].Index;
                float tmpX = WorldItems[tmpIndex].Position.X;
                //MessageBox.Show(tmpX.ToString());
                float tmpZ = WorldItems[tmpIndex].Position.Z;
                if (tmpX > xmax) {
                    xmax = tmpX;
                }
                if (tmpX < xmin) {
                    xmin = tmpX;
                }
                if (tmpZ > zmax) {
                    zmax = tmpZ;
                }
                if (tmpZ < zmin) {
                    zmin = tmpZ;
                }
            }
            //MessageBox.Show("X Max : " + xmax + "\n" + "Y Max : " + ymax);
            //MessageBox.Show("X Min : " + xmin + "\n" + "Y Min : " + ymin);
            float xDist = (xmax - xmin) / 2;
            float zDist = (zmax - zmin) / 2;
            Vector2 Origin = new Vector2(xmax - xDist, zmax - zDist);
            this.Text = Origin.ToString();
            return Origin;
        }
        private Vector2 GetOriginX() {
            float ymax = -10000;
            float zmax = -10000;
            float ymin = 10000;
            float zmin = 10000;
            for (int i = 0; i < itemList.SelectedItems.Count; i++) {
                int tmpIndex = itemList.SelectedItems[i].Index;
                float tmpY = WorldItems[tmpIndex].Position.Y;
                //MessageBox.Show(tmpX.ToString());
                float tmpZ = WorldItems[tmpIndex].Position.Z;
                if (tmpY > ymax) {
                    ymax = tmpY;
                }
                if (tmpY < ymin) {
                    ymin = tmpY;
                }
                if (tmpZ > zmax) {
                    zmax = tmpZ;
                }
                if (tmpZ < zmin) {
                    zmin = tmpZ;
                }
            }
            //MessageBox.Show("X Max : " + xmax + "\n" + "Y Max : " + ymax);
            //MessageBox.Show("X Min : " + xmin + "\n" + "Y Min : " + ymin);
            float yDist = (ymax - ymin) / 2;
            float zDist = (zmax - zmin) / 2;
            Vector2 Origin = new Vector2(ymax - yDist, zmax - zDist);
            this.Text = Origin.ToString();
            return Origin;
        }

        private void rotateAroundYawZToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!writer.BGW.IsBusy) {
                //get the origin between the greatest and lowest points
                contextMenu.Visible = false;
                writer.Origin = GetOriginZ();
                //input box for yaw
                RotationInput RI = new RotationInput();
                RI.Main = this;
                RI.type = "YAW";
                RI.ShowDialog();
            }
            contextMenu.Visible = false;
        }
        private void rotateAroundYpitchToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!writer.BGW.IsBusy) {
                //get the origin between the greatest and lowest points
                contextMenu.Visible = false;
                writer.Origin = GetOriginY();
                //input box for yaw
                RotationInput RI = new RotationInput();
                RI.Main = this;
                RI.type = "PITCH";
                RI.ShowDialog();
            }
            contextMenu.Visible = false;
        }
        private void rotateAroundXrollToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!writer.BGW.IsBusy) {
                //get the origin between the greatest and lowest points
                contextMenu.Visible = false;
                writer.Origin = GetOriginX();
                //input box for yaw
                RotationInput RI = new RotationInput();
                RI.Main = this;
                RI.type = "ROLL";
                RI.ShowDialog();
            }
            contextMenu.Visible = false;
        }

        private void bTeleport_Click(object sender, EventArgs e) {
            TeleportToObject();
            //IntPtr addrs;
            //MCCMemory.modules.TryGetValue("haloreach.dll", out addrs);
            ////ModuleBase = "0x" + addrs.ToString("X8");

            ////this.Text = "0x" + addrs.ToString("X8");
            //ProcessModule m = MCCProcess.Modules[179];
            //ModuleBase = (long)m.BaseAddress;
            //ModuleEnd = (m.ModuleMemorySize + (long)m.BaseAddress);
            ////progressBar2.Visible = true;
            //AOBScan(ModuleBase, ModuleEnd);
        }
        private void TeleportToObject() {
            float X = (float)nX.Value;
            float Y = (float)nY.Value;
            float Z = (float)nZ.Value + 0.5f;

            byte[] valueBytesX = new byte[] { };
            valueBytesX = BitConverter.GetBytes(X);
            byte[] valueBytesY = new byte[] { };
            valueBytesY = BitConverter.GetBytes(Y);
            byte[] valueBytesZ = new byte[] { };
            valueBytesZ = BitConverter.GetBytes(Z);
            //0x17C
            string MonitorY = MonitorX.Remove(MonitorX.Length - 2, 2);
            MonitorY = MonitorY + "80";
            string MonitorZ = MonitorX.Remove(MonitorX.Length - 2, 2);
            MonitorZ = MonitorZ + "84";
            //string tmpY = MonitorX.Remove(MonitorX.Length-1,1);
            //string tmpZ = MonitorX.Remove(MonitorX.Length - 1, 1);
            //tmpY = tmpY + "+4";
            //tmpZ = tmpZ + "+8";

            MCCMemory.WriteBytes(MonitorX, valueBytesX);
            MCCMemory.WriteBytes(MonitorY, valueBytesY);
            MCCMemory.WriteBytes(MonitorZ, valueBytesZ);
        }

        private void button2_Click(object sender, EventArgs e) {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(Application.StartupPath + "\\cases.txt");
            System.IO.StreamReader sr = new System.IO.StreamReader(Application.StartupPath + "\\MapItems\\forgeworld2.txt");
            string caseBegin = "case \"";
            foreach (ListViewItem item in itemList.Items) {
                caseBegin = "case \"" + item.SubItems[1].Text + "\":";
                string caseEnd = "\n{\n\treturn \"";
                caseEnd += sr.ReadLine() + "\";\n}";
                sw.WriteLine(caseBegin + caseEnd);
            }
            sr.Close();
            sw.Close();
        }
        private void button3_Click(object sender, EventArgs e) {
            //Apply Yaw Rotation
            // MessageBox.Show(GetRollPitchYaw(0).ToString());
            double angle = 45 * (Math.PI / 180);
            SharpDX.Matrix3x3 rotationMatrix = SharpDX.Matrix3x3.RotationYawPitchRoll((float)angle, (float)angle, (float)angle);
            WorldItem WI = WorldItems[0];
            SharpDX.Matrix3x2 ItemMatrix = new SharpDX.Matrix3x2(WI.r11, WI.r12, WI.r13, WI.r31, WI.r32, WI.r33);
            SharpDX.Matrix3x3 ItemMatrix3x3 = new SharpDX.Matrix3x3(WI.r11, WI.r12, WI.r13, WI.r31, WI.r32, WI.r33, 0, 0, 1);
            SharpDX.Matrix3x2 M3 = new SharpDX.Matrix3x2();
            //SharpDX.Matrix3x3 M3 = new SharpDX.Matrix3x3();
            //SharpDX.Matrix3x2 NewMatrix = new SharpDX.Matrix3x2();
            M3 = Multiply3x3_3x2(ItemMatrix, rotationMatrix);
            //M3 = (ItemMatrix3x3 * rotationMatrix);
            //NewMatrix = new SharpDX.Matrix3x2(M3.M11, WI.r31, M3.M21, WI.r32, M3.M31, WI.r33);
            SharpDX.Matrix3x3 asd = SharpGet3x3Matrix(30, 10, 30);
            Clipboard.SetText(FormatMatrix3x3(asd));

        }
    }
}

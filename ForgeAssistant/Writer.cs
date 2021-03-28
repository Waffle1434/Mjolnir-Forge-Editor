using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ForgeAssistant
{
    public class Writer
    {
        Form1 Main;
        public BackgroundWorker BGW = new BackgroundWorker();
        int workType = 0;
        public List<float> Amounts = new List<float>();
        public List<float> RotFloats = new List<float>();
        public float Amount;
        public string Axis;
        public Vector2 Origin;
        public float StructureRotationAngle;
        List<string> offsets = new List<string> { "+18", "+1C", "+20", "+24", "+28", "+2C" };
        public Writer(Form1 main)
        {
            Main = main;
            BGW.WorkerReportsProgress = true;
            BGW.DoWork += BGW_DoWork;
            BGW.ProgressChanged += BGW_ProgressChanged;
            BGW.RunWorkerCompleted += BGW_RunWorkerCompleted;
        }

        private void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Main.progressBar1.Value = 0;
            Main.progressBar1.Visible = false;
            Main.timer1.Enabled = true;
        }

        private void BGW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Main.progressBar1.Value = e.ProgressPercentage;
        }

        public void DoWork(int type)
        {
            Main.timer1.Enabled = false;
            workType = type;
            Main.progressBar1.Maximum = Main.listView1.SelectedItems.Count;
            Main.progressBar1.Visible = true;
            BGW.RunWorkerAsync();
        }
        private void BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            //do work
            switch (workType)
            {
                //Nudging
                case 0:
                    {
                        byte[] valueBytes = new byte[] { };

                        if (Main.listView1.SelectedItems.Count > 0)
                        {
                            switch (Axis)
                            {
                                case "X":
                                    for (var i = 0; i < Main.listView1.SelectedItems.Count; i++)
                                    {
                                        string tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                                        float tmp = Main.WorldItems[Main.listView1.SelectedItems[i].Index].Position.X;
                                        valueBytes = BitConverter.GetBytes(Amount + tmp);
                                        Main.MCCMemory.WriteBytes(tmpAddrs + "+C", valueBytes);
                                        BGW.ReportProgress(i);
                                    }
                                    break;
                                case "Y":
                                    for (var i = 0; i < Main.listView1.SelectedItems.Count; i++)
                                    {
                                        string tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                                        float tmp = Main.WorldItems[Main.listView1.SelectedItems[i].Index].Position.Y;
                                        valueBytes = BitConverter.GetBytes(Amount + tmp);
                                        Main.MCCMemory.WriteBytes(tmpAddrs + "+10", valueBytes);
                                        BGW.ReportProgress(i);
                                    }
                                    break;
                                case "Z":
                                    for (var i = 0; i < Main.listView1.SelectedItems.Count; i++)
                                    {
                                        string tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                                        float tmp = Main.WorldItems[Main.listView1.SelectedItems[i].Index].Position.Z;
                                        valueBytes = BitConverter.GetBytes(Amount + tmp);
                                        Main.MCCMemory.WriteBytes(tmpAddrs + "+14", valueBytes);
                                        BGW.ReportProgress(i);
                                    }
                                    break;
                            }
                        }
                        break;
                    }
                //Set Locations
                case 1:
                    {
                        byte[] valueBytes = new byte[] { };
                        valueBytes = BitConverter.GetBytes(Amount);

                        string tmpAddrs = "";
                        if (Main.listView1.SelectedItems.Count > 0)
                        {
                            switch (Axis)
                            {
                                case "X":
                                    for (var i = 0; i < Main.listView1.SelectedItems.Count; i++)
                                    {
                                        tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                                        Main.MCCMemory.WriteBytes(tmpAddrs + "+C", valueBytes);
                                        BGW.ReportProgress(i);
                                    }
                                    break;
                                case "Y":
                                    for (var i = 0; i < Main.listView1.SelectedItems.Count; i++)
                                    {
                                        tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                                        Main.MCCMemory.WriteBytes(tmpAddrs + "+10", valueBytes);
                                        BGW.ReportProgress(i);
                                    }
                                    break;
                                case "Z":
                                    for (var i = 0; i < Main.listView1.SelectedItems.Count; i++)
                                    {
                                        tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                                        Main.MCCMemory.WriteBytes(tmpAddrs + "+14", valueBytes);
                                        BGW.ReportProgress(i);
                                    }
                                    break;
                                case "ALL":
                                    byte[] valueBytesX = new byte[] { };
                                    valueBytesX = BitConverter.GetBytes(Amounts[0]);
                                    byte[] valueBytesY = new byte[] { };
                                    valueBytesY = BitConverter.GetBytes(Amounts[1]);
                                    byte[] valueBytesZ = new byte[] { };
                                    valueBytesZ = BitConverter.GetBytes(Amounts[2]);
                                    for (var i = 0; i < Main.listView1.SelectedItems.Count; i++)
                                    {
                                        tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                                        Main.MCCMemory.WriteBytes(tmpAddrs + "+C", valueBytesX);
                                        Main.MCCMemory.WriteBytes(tmpAddrs + "+10", valueBytesY);
                                        Main.MCCMemory.WriteBytes(tmpAddrs + "+14", valueBytesZ);
                                        BGW.ReportProgress(i);
                                    }
                                    break;
                            }
                        }
                        break;
                    }
                //Set Rotations
                case 2:
                    {
                        byte[] valueBytes = new byte[] { };

                        for (var i = 0; i < Main.listView1.SelectedItems.Count; i++)
                        {
                            string tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                            for (var j = 0; j < RotFloats.Count; j++)
                            {
                                valueBytes = BitConverter.GetBytes(RotFloats[j]);
                                Main.MCCMemory.WriteBytes(tmpAddrs + offsets[j], valueBytes);
                            }
                            BGW.ReportProgress(i);
                        }
                        break;
                    }
                //Set Location and Rotation
                case 3:
                    {
                        byte[] valueBytes = new byte[] { };
                        valueBytes = BitConverter.GetBytes(Amount);

                        string tmpAddrs = "";
                        if (Main.listView1.SelectedItems.Count > 0)
                        {
                            byte[] valueBytesX = new byte[] { };
                            valueBytesX = BitConverter.GetBytes(Amounts[0]);
                            byte[] valueBytesY = new byte[] { };
                            valueBytesY = BitConverter.GetBytes(Amounts[1]);
                            byte[] valueBytesZ = new byte[] { };
                            valueBytesZ = BitConverter.GetBytes(Amounts[2]);
                            for (var i = 0; i < Main.listView1.SelectedItems.Count; i++)
                            {
                                tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+C", valueBytesX);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+10", valueBytesY);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+14", valueBytesZ);
                                for (var j = 0; j < RotFloats.Count; j++)
                                {
                                    valueBytes = BitConverter.GetBytes(RotFloats[j]);
                                    Main.MCCMemory.WriteBytes(tmpAddrs + offsets[j], valueBytes);
                                }
                                BGW.ReportProgress(i);
                            }
                        }
                        break;
                    }
                //Paste Multi Location
                case 4:
                    {
                        byte[] valueBytesX = new byte[] { };
                        byte[] valueBytesY = new byte[] { };
                        byte[] valueBytesZ = new byte[] { };
                        for (var i = 0; i < Main.CP.CopiedLists.Count; i++)
                        {
                            if (Main.listView1.SelectedItems.Count > i)
                            {
                                string tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                                float x = Main.CP.CopiedLists.ElementAt(i)[6];
                                float y = Main.CP.CopiedLists[i][7];
                                float z = Main.CP.CopiedLists[i][8];
                                valueBytesX = BitConverter.GetBytes(x);
                                valueBytesY = BitConverter.GetBytes(y);
                                valueBytesZ = BitConverter.GetBytes(z);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+C", valueBytesX);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+10", valueBytesY);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+14", valueBytesZ);
                            }
                            BGW.ReportProgress(i);
                        }
                        break;
                    }
                //Paste Multi Rotation
                case 5:
                    {
                        byte[] valueBytesR11 = new byte[] { };
                        byte[] valueBytesR12 = new byte[] { };
                        byte[] valueBytesR13 = new byte[] { };
                        byte[] valueBytesR31 = new byte[] { };
                        byte[] valueBytesR32 = new byte[] { };
                        byte[] valueBytesR33 = new byte[] { };
                        for (var i = 0; i < Main.CP.CopiedLists.Count; i++)
                        {
                            if (Main.listView1.SelectedItems.Count > i)
                            {
                                string tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                                float r11 = Main.CP.CopiedLists[i][0];
                                float r12 = Main.CP.CopiedLists[i][1];
                                float r13 = Main.CP.CopiedLists[i][2];
                                float r31 = Main.CP.CopiedLists[i][3];
                                float r32 = Main.CP.CopiedLists[i][4];
                                float r33 = Main.CP.CopiedLists[i][5];
                                valueBytesR11 = BitConverter.GetBytes(r11);
                                valueBytesR12 = BitConverter.GetBytes(r12);
                                valueBytesR13 = BitConverter.GetBytes(r13);
                                valueBytesR31 = BitConverter.GetBytes(r31);
                                valueBytesR32 = BitConverter.GetBytes(r32);
                                valueBytesR33 = BitConverter.GetBytes(r33);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+18", valueBytesR11);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+1C", valueBytesR12);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+20", valueBytesR13);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+24", valueBytesR31);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+28", valueBytesR32);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+2C", valueBytesR33);
                            }
                            BGW.ReportProgress(i);
                        }
                        break;
                    }
                //Paste Multi Location and Rotation
                case 6:
                    {
                        byte[] valueBytesR11 = new byte[] { };
                        byte[] valueBytesR12 = new byte[] { };
                        byte[] valueBytesR13 = new byte[] { };
                        byte[] valueBytesR31 = new byte[] { };
                        byte[] valueBytesR32 = new byte[] { };
                        byte[] valueBytesR33 = new byte[] { };
                        byte[] valueBytesX = new byte[] { };
                        byte[] valueBytesY = new byte[] { };
                        byte[] valueBytesZ = new byte[] { };
                        for (var i = 0; i < Main.CP.CopiedLists.Count; i++)
                        {
                            if (Main.listView1.SelectedItems.Count > i)
                            {
                                string tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                                float r11 = Main.CP.CopiedLists[i][0];
                                float r12 = Main.CP.CopiedLists[i][1];
                                float r13 = Main.CP.CopiedLists[i][2];
                                float r31 = Main.CP.CopiedLists[i][3];
                                float r32 = Main.CP.CopiedLists[i][4];
                                float r33 = Main.CP.CopiedLists[i][5];
                                float x = Main.CP.CopiedLists[i][6];
                                float y = Main.CP.CopiedLists[i][7];
                                float z = Main.CP.CopiedLists[i][8];
                                valueBytesR11 = BitConverter.GetBytes(r11);
                                valueBytesR12 = BitConverter.GetBytes(r12);
                                valueBytesR13 = BitConverter.GetBytes(r13);
                                valueBytesR31 = BitConverter.GetBytes(r31);
                                valueBytesR32 = BitConverter.GetBytes(r32);
                                valueBytesR33 = BitConverter.GetBytes(r33);
                                valueBytesX = BitConverter.GetBytes(x);
                                valueBytesY = BitConverter.GetBytes(y);
                                valueBytesZ = BitConverter.GetBytes(z);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+C", valueBytesX);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+10", valueBytesY);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+14", valueBytesZ);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+18", valueBytesR11);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+1C", valueBytesR12);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+20", valueBytesR13);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+24", valueBytesR31);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+28", valueBytesR32);
                                Main.MCCMemory.WriteBytes(tmpAddrs + "+2C", valueBytesR33);
                            }
                            BGW.ReportProgress(i);
                        }
                        break;
                    }
                //Rotate Around Z Axis Origin (Yaw)
                case 7:
                    {
                        Matrix4x4 translation = new Matrix4x4();
                       
                        byte[] valueBytes = new byte[] { };
                        byte[] valueBytesX = new byte[] { };                        
                        byte[] valueBytesY = new byte[] { };
                        for (var i = 0; i < Main.listView1.SelectedItems.Count; i++)
                        {
                            string tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                            int tmpIndex = Main.listView1.SelectedItems[i].Index;
                            
                            Vector3 RPY = Main.GetRollPitchYaw(tmpIndex);
                            float angle = RPY.Y + StructureRotationAngle * ((float)Math.PI / 180);
                            translation = Matrix4x4.CreateRotationZ(angle, new Vector3(Origin.X, Origin.Y, 0));
                            Main.Get3x3Matrix(RPY.Z, RPY.X, RPY.Y);
                            List<float> WorldItemRotMat = new List<float>();
                            WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r11);
                            WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r13);
                            WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r12);
                            WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r31);
                            WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r32);
                            WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r33);
                            Matrix3x2 rotated = new Matrix3x2(WorldItemRotMat[0], WorldItemRotMat[1], WorldItemRotMat[2], WorldItemRotMat[3], WorldItemRotMat[4], WorldItemRotMat[5]);
                            Matrix3x2 rotation = Matrix3x2.CreateRotation(angle) * rotated;
                            //rotated.
                            //rotated = rotated * rotation;
                            RotFloats.Clear();
                            RotFloats.Add(rotation.M11);
                            RotFloats.Add(rotation.M21);
                            RotFloats.Add(rotation.M31);
                            RotFloats.Add(rotation.M12);
                            RotFloats.Add(rotation.M22);
                            RotFloats.Add(rotation.M32);
                            for (var j = 0; j < RotFloats.Count; j++)
                            {
                                valueBytes = BitConverter.GetBytes(RotFloats[j]);
                                Main.MCCMemory.WriteBytes(tmpAddrs + offsets[j], valueBytes);
                            }
                            float x = Main.WorldItems[tmpIndex].Position.X;
                            float y = Main.WorldItems[tmpIndex].Position.Y;
                            float z = Main.WorldItems[tmpIndex].Position.Z;
                            Vector3 coord = new Vector3(x,y,z);
                            coord = Vector3.Transform(coord, translation);
                            valueBytesX = BitConverter.GetBytes(coord.X);
                            valueBytesY = BitConverter.GetBytes(coord.Y);
                            Main.MCCMemory.WriteBytes(tmpAddrs + "+C", valueBytesX);
                            Main.MCCMemory.WriteBytes(tmpAddrs + "+10", valueBytesY);
                            BGW.ReportProgress(i);
                        }
                        break;
                    }
                //Rotate Around Y Axis Origin (Pitch)
                case 8:
                    {
                        Matrix4x4 translation = new Matrix4x4();
                        float angle = StructureRotationAngle * ((float)Math.PI / 180);
                        translation = Matrix4x4.CreateRotationY(angle, new Vector3(Origin.X, 0, Origin.Y));
                        byte[] valueBytes = new byte[] { };
                        byte[] valueBytesX = new byte[] { };
                        byte[] valueBytesY = new byte[] { };
                        for (var i = 0; i < Main.listView1.SelectedItems.Count; i++)
                        {
                            string tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                            int tmpIndex = Main.listView1.SelectedItems[i].Index;
                            //List<float> WorldItemRotMat = new List<float>();
                            //WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r11);
                            //WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r13);
                            //WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r12);
                            //WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r31);
                            //WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r32);
                            //WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r33);
                            //Matrix3x2 rotated = new Matrix3x2(WorldItemRotMat[0], WorldItemRotMat[1], WorldItemRotMat[2], WorldItemRotMat[3], WorldItemRotMat[4], WorldItemRotMat[5]);
                            //Matrix3x2 rotation = Matrix3x2.CreateRotation(angle) * rotated;
                            ////rotated.
                            ////rotated = rotated * rotation;
                            //RotFloats.Clear();
                            //RotFloats.Add(rotation.M11);
                            //RotFloats.Add(rotation.M21);
                            //RotFloats.Add(rotation.M31);
                            //RotFloats.Add(rotation.M12);
                            //RotFloats.Add(rotation.M22);
                            //RotFloats.Add(rotation.M32);
                            //for (var j = 0; j < RotFloats.Count; j++)
                            //{
                            //    valueBytes = BitConverter.GetBytes(RotFloats[j]);
                            //    Main.MCCMemory.WriteBytes(tmpAddrs + offsets[j], valueBytes);
                            //}
                            float x = Main.WorldItems[tmpIndex].Position.X;
                            float y = Main.WorldItems[tmpIndex].Position.Y;
                            float z = Main.WorldItems[tmpIndex].Position.Z;
                            Vector3 coord = new Vector3(x, y, z);
                            coord = Vector3.Transform(coord, translation);
                            valueBytesX = BitConverter.GetBytes(coord.X);
                            valueBytesY = BitConverter.GetBytes(coord.Z);
                            Main.MCCMemory.WriteBytes(tmpAddrs + "+C", valueBytesX);
                            Main.MCCMemory.WriteBytes(tmpAddrs + "+14", valueBytesY);
                            BGW.ReportProgress(i);
                        }
                        break;
                    }
                //Rotate Around X Axis Origin (Roll)
                case 9:
                    {
                        Matrix4x4 translation = new Matrix4x4();
                        float angle = StructureRotationAngle * ((float)Math.PI / 180);
                        translation = Matrix4x4.CreateRotationX(angle, new Vector3(0, Origin.X, Origin.Y));
                        byte[] valueBytes = new byte[] { };
                        byte[] valueBytesX = new byte[] { };
                        byte[] valueBytesY = new byte[] { };
                        for (var i = 0; i < Main.listView1.SelectedItems.Count; i++)
                        {
                            string tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                            int tmpIndex = Main.listView1.SelectedItems[i].Index;
                            //List<float> WorldItemRotMat = new List<float>();
                            //WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r11);
                            //WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r13);
                            //WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r12);
                            //WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r31);
                            //WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r32);
                            //WorldItemRotMat.Add(Main.WorldItems[tmpIndex].r33);
                            //Matrix3x2 rotated = new Matrix3x2(WorldItemRotMat[0], WorldItemRotMat[1], WorldItemRotMat[2], WorldItemRotMat[3], WorldItemRotMat[4], WorldItemRotMat[5]);
                            //Matrix3x2 rotation = Matrix3x2.CreateRotation(angle) * rotated;
                            ////rotated.
                            ////rotated = rotated * rotation;
                            //RotFloats.Clear();
                            //RotFloats.Add(rotation.M11);
                            //RotFloats.Add(rotation.M21);
                            //RotFloats.Add(rotation.M31);
                            //RotFloats.Add(rotation.M12);
                            //RotFloats.Add(rotation.M22);
                            //RotFloats.Add(rotation.M32);
                            //for (var j = 0; j < RotFloats.Count; j++)
                            //{
                            //    valueBytes = BitConverter.GetBytes(RotFloats[j]);
                            //    Main.MCCMemory.WriteBytes(tmpAddrs + offsets[j], valueBytes);
                            //}
                            float x = Main.WorldItems[tmpIndex].Position.X;
                            float y = Main.WorldItems[tmpIndex].Position.Y;
                            float z = Main.WorldItems[tmpIndex].Position.Z;
                            Vector3 coord = new Vector3(x, y, z);
                            coord = Vector3.Transform(coord, translation);
                            valueBytesX = BitConverter.GetBytes(coord.Y);
                            valueBytesY = BitConverter.GetBytes(coord.Z);
                            Main.MCCMemory.WriteBytes(tmpAddrs + "+10", valueBytesX);
                            Main.MCCMemory.WriteBytes(tmpAddrs + "+14", valueBytesY);
                            BGW.ReportProgress(i);
                        }
                        break;
                    }
            }
        }
    }
}

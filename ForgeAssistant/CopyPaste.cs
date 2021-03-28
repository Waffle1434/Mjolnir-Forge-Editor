using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForgeAssistant
{
    public class CopyPaste
    {
        Form1 Main;
        public List<List<float>> CopiedLists = new List<List<float>>();
        List<float> CopiedRotationLocation = new List<float>();
        public CopyPaste(Form1 Form)
        {
            Main = Form;
        }
        private void WriteNewSpawnLocationRotationMulti()
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
            for (var i = 0; i < CopiedLists.Count; i++)
            {
                if (Main.listView1.SelectedItems.Count > i)
                {
                    string tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                    float r11 = CopiedLists[i][0];
                    float r12 = CopiedLists[i][1];
                    float r13 = CopiedLists[i][2];
                    float r31 = CopiedLists[i][3];
                    float r32 = CopiedLists[i][4];
                    float r33 = CopiedLists[i][5];
                    float x = CopiedLists[i][6];
                    float y = CopiedLists[i][7];
                    float z = CopiedLists[i][8];
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
            }
        }
        private void WriteNewSpawnLocationMulti()
        {
            byte[] valueBytesX = new byte[] { };
            byte[] valueBytesY = new byte[] { };
            byte[] valueBytesZ = new byte[] { };
            for (var i = 0; i < CopiedLists.Count; i++)
            {
                if (Main.listView1.SelectedItems.Count > i)
                {
                    string tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                    float x = CopiedLists.ElementAt(i)[6];
                    float y = CopiedLists[i][7];
                    float z = CopiedLists[i][8];
                    valueBytesX = BitConverter.GetBytes(x);
                    valueBytesY = BitConverter.GetBytes(y);
                    valueBytesZ = BitConverter.GetBytes(z);
                    Main.MCCMemory.WriteBytes(tmpAddrs + "+C", valueBytesX);
                    Main.MCCMemory.WriteBytes(tmpAddrs + "+10", valueBytesY);
                    Main.MCCMemory.WriteBytes(tmpAddrs + "+14", valueBytesZ);
                }
            }
        }
        private void WriteNewSpawnRotationMulti()
        {
            byte[] valueBytesR11 = new byte[] { };
            byte[] valueBytesR12 = new byte[] { };
            byte[] valueBytesR13 = new byte[] { };
            byte[] valueBytesR31 = new byte[] { };
            byte[] valueBytesR32 = new byte[] { };
            byte[] valueBytesR33 = new byte[] { };
            for (var i = 0; i < CopiedLists.Count; i++)
            {
                if (Main.listView1.SelectedItems.Count > i)
                {
                    string tmpAddrs = Main.listView1.SelectedItems[i].SubItems[0].Text;
                    float r11 = CopiedLists[i][0];
                    float r12 = CopiedLists[i][1];
                    float r13 = CopiedLists[i][2];
                    float r31 = CopiedLists[i][3];
                    float r32 = CopiedLists[i][4];
                    float r33 = CopiedLists[i][5];
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
            }
        }
        public void PasteLocation()
        {
            //Main.timer1.Enabled = false;
            Main.writer.Axis = "ALL";
            Main.writer.Amounts.Clear();
            Main.writer.Amounts.Add(CopiedRotationLocation[6]);
            Main.writer.Amounts.Add(CopiedRotationLocation[7]);
            Main.writer.Amounts.Add(CopiedRotationLocation[8]);
            //Main.progressBar1.Maximum = Main.listView1.SelectedItems.Count;
            //Main.progressBar1.Visible = true;
            //Main.backgroundWorker2.RunWorkerAsync();
            Main.writer.DoWork(1);


            //Main.WriteNewSpawnLocation(CopiedRotationLocation[6], "X");
            //Main.WriteNewSpawnLocation(CopiedRotationLocation[7], "Y");
            //Main.WriteNewSpawnLocation(CopiedRotationLocation[8], "Z");
            //Main.numericUpDown1SX.Value = (decimal)CopiedRotationLocation[6];
            //Main.numericUpDown2SY.Value = (decimal)CopiedRotationLocation[7];
            //Main.numericUpDown3SZ.Value = (decimal)CopiedRotationLocation[8];
        }
        public void PasteRotation()
        {
            //Main.WriteSpawnRotation(CopiedRotationLocation[0], "+18");
            //Main.WriteSpawnRotation(CopiedRotationLocation[1], "+1C");
            //Main.WriteSpawnRotation(CopiedRotationLocation[2], "+20");
            //Main.WriteSpawnRotation(CopiedRotationLocation[3], "+24");
            //Main.WriteSpawnRotation(CopiedRotationLocation[4], "+28");
            //Main.WriteSpawnRotation(CopiedRotationLocation[5], "+2C");
            Main.writer.RotFloats.Clear();
            Main.writer.RotFloats.Add(CopiedRotationLocation[0]);
            Main.writer.RotFloats.Add(CopiedRotationLocation[1]);
            Main.writer.RotFloats.Add(CopiedRotationLocation[2]);
            Main.writer.RotFloats.Add(CopiedRotationLocation[3]);
            Main.writer.RotFloats.Add(CopiedRotationLocation[4]);
            Main.writer.RotFloats.Add(CopiedRotationLocation[5]);
           
            Main.writer.DoWork(2);
            //Main.numericUpDown11.Value = (decimal)CopiedRotationLocation[0];
            //Main.numericUpDown12.Value = (decimal)CopiedRotationLocation[1];
            //Main.numericUpDown13.Value = (decimal)CopiedRotationLocation[2];
            //Main.numericUpDown17.Value = (decimal)CopiedRotationLocation[3];
            //Main.numericUpDown18.Value = (decimal)CopiedRotationLocation[4];
            //Main.numericUpDown19.Value = (decimal)CopiedRotationLocation[5];
        }
        public void PasteLocationAndRotation()
        {
            Main.writer.RotFloats.Clear();
            Main.writer.RotFloats.Add(CopiedRotationLocation[0]);
            Main.writer.RotFloats.Add(CopiedRotationLocation[1]);
            Main.writer.RotFloats.Add(CopiedRotationLocation[2]);
            Main.writer.RotFloats.Add(CopiedRotationLocation[3]);
            Main.writer.RotFloats.Add(CopiedRotationLocation[4]);
            Main.writer.RotFloats.Add(CopiedRotationLocation[5]);
            Main.writer.Amounts.Clear();
            Main.writer.Amounts.Add(CopiedRotationLocation[6]);
            Main.writer.Amounts.Add(CopiedRotationLocation[7]);
            Main.writer.Amounts.Add(CopiedRotationLocation[8]);
            Main.writer.DoWork(3);
            //Main.WriteSpawnRotation();
            //Main.WriteNewSpawnLocation(CopiedRotationLocation[6], "X");
            //Main.WriteNewSpawnLocation(CopiedRotationLocation[7], "Y");
            //Main.WriteNewSpawnLocation(CopiedRotationLocation[8], "Z");
            //Main.numericUpDown11.Value = (decimal)CopiedRotationLocation[0];
            //Main.numericUpDown12.Value = (decimal)CopiedRotationLocation[1];
            //Main.numericUpDown13.Value = (decimal)CopiedRotationLocation[2];
            //Main.numericUpDown17.Value = (decimal)CopiedRotationLocation[3];
            //Main.numericUpDown18.Value = (decimal)CopiedRotationLocation[4];
            //Main.numericUpDown19.Value = (decimal)CopiedRotationLocation[5];
            //Main.numericUpDown1SX.Value = (decimal)CopiedRotationLocation[6];
            //Main.numericUpDown2SY.Value = (decimal)CopiedRotationLocation[7];
            //Main.numericUpDown3SZ.Value = (decimal)CopiedRotationLocation[8];
        }
        public void PasteLocationMulti()
        {
            //WriteNewSpawnLocationMulti();
            if (!Main.writer.BGW.IsBusy)
            {
                //Main.timer1.Enabled = false;
                //Main.progressBar1.Maximum = CopiedLists.Count;
                //Main.progressBar1.Visible = true;
                //Main.backgroundWorker3.RunWorkerAsync();
                Main.writer.DoWork(4);
            }            
            //Main.numericUpDown1SX.Value = (decimal)CopiedRotationLocation[6];
            //Main.numericUpDown2SY.Value = (decimal)CopiedRotationLocation[7];
            //Main.numericUpDown3SZ.Value = (decimal)CopiedRotationLocation[8];
        }
        public void PasteRotationMulti()
        {
            if (!Main.writer.BGW.IsBusy)
            {
                Main.writer.DoWork(5);
            }            
        }
        public void PasteRotLocMulti()
        {
            if (!Main.writer.BGW.IsBusy)
            {
                Main.writer.DoWork(6);
            }
        }
        public void Copy()
        {
            CopiedRotationLocation.Clear();
            int tmpIndex = Main.listView1.SelectedItems[0].Index;

            CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].r11);
            CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].r12);
            CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].r13);
            CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].r31);
            CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].r32);
            CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].r33);
            CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].Position.X);
            CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].Position.Y);
            CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].Position.Z);
        }
        public void CopyMulti()
        {
            CopiedLists.Clear();

            for (var i = 0; i < Main.listView1.SelectedItems.Count; i++)
            {
                CopiedRotationLocation = new List<float>();
                int tmpIndex = Main.listView1.SelectedItems[i].Index;
                CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].r11);
                CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].r12);
                CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].r13);
                CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].r31);
                CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].r32);
                CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].r33);
                CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].Position.X);
                CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].Position.Y);
                CopiedRotationLocation.Add(Main.WorldItems[tmpIndex].Position.Z);
                CopiedLists.Add(CopiedRotationLocation);
            }          
        }
    }
}

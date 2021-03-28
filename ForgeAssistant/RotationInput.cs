using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForgeAssistant
{
    public partial class RotationInput : Form
    {
        public RotationInput()
        {
            InitializeComponent();
        }
        public Form1 Main;
        public string type;
        private void button1_Click(object sender, EventArgs e)
        {
            //perform operation
            if (type == "YAW")
            {
                Form1.RotationMatrix RM = Get3x3MatrixYAW();
                Main.writer.RotFloats.Clear();
                Main.writer.RotFloats.Add((float)RM.r11);
                Main.writer.RotFloats.Add((float)RM.r12);
                Main.writer.RotFloats.Add((float)RM.r13);
                Main.writer.RotFloats.Add((float)RM.r31);
                Main.writer.RotFloats.Add((float)RM.r32);
                Main.writer.RotFloats.Add((float)RM.r33);
                if (!Main.writer.BGW.IsBusy)
                {
                    Main.writer.StructureRotationAngle = (float)numericUpDown1.Value;
                    Main.writer.DoWork(7);
                }
            }
            if (type == "PITCH")
            {
                Form1.RotationMatrix RM = Get3x3MatrixPITCH();
                Main.writer.RotFloats.Clear();
                Main.writer.RotFloats.Add((float)RM.r11);
                Main.writer.RotFloats.Add((float)RM.r12);
                Main.writer.RotFloats.Add((float)RM.r13);
                Main.writer.RotFloats.Add((float)RM.r31);
                Main.writer.RotFloats.Add((float)RM.r32);
                Main.writer.RotFloats.Add((float)RM.r33);
                if (!Main.writer.BGW.IsBusy)
                {
                    Main.writer.StructureRotationAngle = (float)numericUpDown1.Value;
                    Main.writer.DoWork(8);
                }
            }
            if (type == "ROLL")
            {
                Form1.RotationMatrix RM = Get3x3MatrixROLL();
                Main.writer.RotFloats.Clear();
                Main.writer.RotFloats.Add((float)RM.r11);
                Main.writer.RotFloats.Add((float)RM.r12);
                Main.writer.RotFloats.Add((float)RM.r13);
                Main.writer.RotFloats.Add((float)RM.r31);
                Main.writer.RotFloats.Add((float)RM.r32);
                Main.writer.RotFloats.Add((float)RM.r33);
                if (!Main.writer.BGW.IsBusy)
                {
                    Main.writer.StructureRotationAngle = (float)numericUpDown1.Value;
                    Main.writer.DoWork(9);
                }
            }
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //abort operation
            this.Close();
        }
        private Form1.RotationMatrix Get3x3MatrixYAW()
        {
            Form1.RotationMatrix RM = new Form1.RotationMatrix();
            double RadiansMultiplier = Math.PI / 180;
            double su = Math.Sin(0 * RadiansMultiplier);
            double cu = Math.Cos(0 * RadiansMultiplier);
            double sv = Math.Sin(0 * RadiansMultiplier);
            double cv = Math.Cos(0 * RadiansMultiplier);
            double sw = Math.Sin((double)numericUpDown1.Value * RadiansMultiplier);
            double cw = Math.Cos((double)numericUpDown1.Value * RadiansMultiplier);

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
        private Form1.RotationMatrix Get3x3MatrixPITCH()
        {
            Form1.RotationMatrix RM = new Form1.RotationMatrix();
            double RadiansMultiplier = Math.PI / 180;
            double su = Math.Sin(0 * RadiansMultiplier);
            double cu = Math.Cos(0 * RadiansMultiplier);
            double sv = Math.Sin((double)numericUpDown1.Value * RadiansMultiplier);
            double cv = Math.Cos((double)numericUpDown1.Value * RadiansMultiplier);
            double sw = Math.Sin(0 * RadiansMultiplier);
            double cw = Math.Cos(0 * RadiansMultiplier);

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
        private Form1.RotationMatrix Get3x3MatrixROLL()
        {
            Form1.RotationMatrix RM = new Form1.RotationMatrix();
            double RadiansMultiplier = Math.PI / 180;
            double su = Math.Sin((double)numericUpDown1.Value * RadiansMultiplier);
            double cu = Math.Cos((double)numericUpDown1.Value * RadiansMultiplier);
            double sv = Math.Sin(0 * RadiansMultiplier);
            double cv = Math.Cos(0 * RadiansMultiplier);
            double sw = Math.Sin(0 * RadiansMultiplier);
            double cw = Math.Cos(0 * RadiansMultiplier);

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
    }
}

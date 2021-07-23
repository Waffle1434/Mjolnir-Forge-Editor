using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForgeAssistant
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Thread thread = new Thread(Loop);
            thread.Name = "IPC";
            thread.Start();

            using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen("testmap5", 1024, MemoryMappedFileAccess.ReadWriteExecute)) {
                using (MemoryMappedViewStream stream = mmf.CreateViewStream()) {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write((byte)1);//mm = mmap.mmap(-1, 1024, "testmap5", mmap.ACCESS_READ)
                    writer.Write(1434);//int.from_bytes(mm[1:5], "little")
                    writer.Flush();
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static void Loop() {
            int port = 15536;
            UdpClient udpServer = new UdpClient(port);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Loopback, port);

            /*while (true) {
                Console.WriteLine($"Waiting on port {port}");
                var data = udpServer.Receive(ref remoteEP);
                Console.Write("receive data from " + remoteEP.ToString());
                udpServer.Send(new byte[] { 1 }, 1, remoteEP); // reply back
            }*/

            
        }
    }
}

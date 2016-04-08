using System;
using System.Collections.Generic;
using System.Text;
using NVNC;

namespace VNCTest
{
    class Program
    {
        static void Main(string[] args)
        {
            VncServer s = new VncServer("password", 5900, "SACHIN-VNC");
            try
            {
                s.Start();
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Console.ReadLine();
        }
    }
}

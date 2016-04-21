using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UDPClient
{
    public static class Logger
    {
        public static bool IsEnabled = true;

        public static bool IsConsoleOn = true;
        public static bool IsDebugOn = true;

        internal static void WriteLine(String Message)
        {
            if (!IsEnabled) { return; }
            if (IsConsoleOn) { Console.WriteLine(Message); }
            if (IsDebugOn) { Debug.WriteLine(Message); }
        }

        internal static void Write(String Message)
        {
            if (!IsEnabled) { return; }
            if (IsConsoleOn) { Console.Write(Message); }
            if (IsConsoleOn) { Debug.WriteLine(Message); }
        }
    }
}

using System;

namespace Firmware_Extractor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Arg0: " + args[0] + " Arg1: " + args[1]);
            Firmware.ProccesFw(args[0], args[1]);
            Console.WriteLine("Finish");
        }
    }
}

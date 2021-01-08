using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace GeneralLabTestCOM
{
    public class Program
    {

        static void Main(string[] args)
        {
            var mmFile = MemoryMappedFile.CreateNew(@"Global\SImmf", 1024);
            Mutex mutex = new Mutex(false, @"Global\SImutex");

            Type CartT = Type.GetTypeFromProgID("SIcart.Cart");
            object cart = Activator.CreateInstance(CartT);

            Type ManagerT = Type.GetTypeFromProgID("SImanager.Manager");
            object manager = Activator.CreateInstance(ManagerT);

            ManagerT.InvokeMember("Draw", System.Reflection.BindingFlags.InvokeMethod, null, manager, null);


            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) break;
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    CartT.InvokeMember("Move", System.Reflection.BindingFlags.InvokeMethod, null, cart, new object[] { 2 });

                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    CartT.InvokeMember("Move", System.Reflection.BindingFlags.InvokeMethod, null, cart, new object[] { -2 });
                }
            }
        }
    }
}
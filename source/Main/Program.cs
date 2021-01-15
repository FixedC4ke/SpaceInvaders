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
            Type ManagerT = Type.GetTypeFromProgID("SImanager.Manager");
            object manager = Activator.CreateInstance(ManagerT); //создание объекта-диспетчера

            Type CartT = Type.GetTypeFromProgID("SIcart.Cart");
            object cart = Activator.CreateInstance(CartT); //создание объекта-тачанки

            Type ShipT = Type.GetTypeFromProgID("SIship.Ship");
            object ship = Activator.CreateInstance(ShipT);

            int offset = (int)ManagerT.InvokeMember("Draw", System.Reflection.BindingFlags.InvokeMethod, null, manager, new object[] { "cart" }); //вывод тачанки на консоль
            CartT.GetProperty("Offset").SetValue(cart, offset);

            offset = (int)ManagerT.InvokeMember("Draw", System.Reflection.BindingFlags.InvokeMethod, null, manager, new object[] { "ship" });
            ShipT.GetProperty("Offset").SetValue(ship, offset);

            while (true) //обработка нажатия клавиш
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) break;
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    CartT.InvokeMember("Move", System.Reflection.BindingFlags.InvokeMethod, null, cart, new object[] { (short)2 });

                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    CartT.InvokeMember("Move", System.Reflection.BindingFlags.InvokeMethod, null, cart, new object[] { (short)-2 });
                }
            }
        }
    }
}
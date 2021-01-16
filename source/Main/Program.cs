using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace SpaceInvaders
{
    public class Program
    {
        private static readonly Type ManagerT = Type.GetTypeFromProgID("SImanager.Manager");
        private static readonly Type ShipT = Type.GetTypeFromProgID("SIship.Ship");
        private static readonly Type CartT = Type.GetTypeFromProgID("SIcart.Cart");
        private static object manager;
        static void Main(string[] args)
        {
            manager = Activator.CreateInstance(ManagerT); //создание объекта-диспетчера

            object cart = Activator.CreateInstance(CartT); //создание объекта-тачанки
            int offset = (int)ManagerT.InvokeMember("Draw", System.Reflection.BindingFlags.InvokeMethod, null, manager, new object[] { "cart" }); //вывод тачанки на консоль
            CartT.GetProperty("Offset").SetValue(cart, offset);

            GenerateLineOfShipsOf(10);

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


        static void GenerateLineOfShipsOf(int n)
        {
            for (int i = 0; i < n; i++)
            {
                object ship = Activator.CreateInstance(ShipT);
                int offset = (int)ManagerT.InvokeMember("Draw", System.Reflection.BindingFlags.InvokeMethod, null, manager, new object[] { "ship" + i.ToString() });
                ShipT.GetProperty("Offset").SetValue(ship, offset);
                ShipT.InvokeMember("Action", System.Reflection.BindingFlags.InvokeMethod, null, ship, new object[] { Console.BufferWidth - 5 });
            }
        }
    }
}
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Collections.Generic;
using Microsoft.VisualBasic;


namespace SpaceInvaders
{
    public class Program
    {
        private static readonly Type ManagerT = Type.GetTypeFromProgID("SImanager.Manager");
        private static readonly Type ShipT = Type.GetTypeFromProgID("SIship.Ship");
        private static readonly Type CartT = Type.GetTypeFromProgID("SIcart.Cart");
        private static readonly Type PatronT = Type.GetTypeFromProgID("SIpatron.Patron");
        private static readonly Type SettingsT = Type.GetTypeFromProgID("SettingsActiveX.pdf_Reader");
        private static Semaphore sem;
        private static object manager;

        public static double ShipSpeed { get; set; }

        [DllImport("SIConsoleAPI.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "InitializeConsole")]
        internal static extern int InitializeConsole();

        static void Main(string[] args)
        {
            InitializeConsole();
            manager = Activator.CreateInstance(ManagerT); //создание объекта-диспетчера

            object cart = Activator.CreateInstance(CartT); //создание объекта-тачанки


            int offset = (int)ManagerT.InvokeMember("Draw", System.Reflection.BindingFlags.InvokeMethod, null, manager, new object[] { "cart" }); //вывод тачанки на консоль
            CartT.GetProperty("Offset").SetValue(cart, offset);


            MSScriptControl.ScriptControl sc = new MSScriptControl.ScriptControl();
            sc.Language = "VBScript";
            sc.AddCode("Function LevelSel() LevelSel = InputBox(\"Введите номер уровня:\", \"ScriptControl\", 1) End Function");
            int level = int.Parse(sc.Run("LevelSel"));

            dynamic activeX = Activator.CreateInstance(SettingsT);
            Dictionary<string, string> check = (Dictionary<string, string>)SettingsT.InvokeMember("getSettings", System.Reflection.BindingFlags.InvokeMethod, null, activeX, new object[] { level }); //вывод тачанки на консоль

            check.TryGetValue("countEnemies", out string counten);
            GenerateLineOfShipsOf(Int32.Parse(counten));

            check.TryGetValue("speedCart", out string speed);
            ShipSpeed = double.Parse(speed);

            check.TryGetValue("frequencyShot", out string freq);
            CartT.GetProperty("RechargeTimerMs").SetValue(cart, double.Parse(freq));


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
                else if (key.Key == ConsoleKey.Spacebar)
                {
                    bool ready = (bool)CartT.InvokeMember("Shoot", System.Reflection.BindingFlags.InvokeMethod, null, cart, null);
                    if (ready)
                    {
                        object patron = Activator.CreateInstance(PatronT);
                        PatronT.GetField("Manager").SetValue(patron, manager);
                        ManagerT.InvokeMember("CreatePatron", System.Reflection.BindingFlags.InvokeMethod, null, manager, null);
                        int offset2 = (int)ManagerT.InvokeMember("Draw", System.Reflection.BindingFlags.InvokeMethod, null, manager, new object[] { "patron" });
                        PatronT.GetProperty("Offset").SetValue(patron, offset2);
                        PatronT.InvokeMember("Action", System.Reflection.BindingFlags.InvokeMethod, null, patron, null);
                    }

                }
            }
        }


        static void GenerateLineOfShipsOf(int n)
        {
            sem = new Semaphore(n, n); // семафор: первый параметр - какому числу объектов изначально доступен семафор, второй - максимальное число объектов, его использующих
            for (int i = 0; i < n; i++)
            {
                Thread thr = new Thread(new ThreadStart(CreateShip));
                thr.Start();
            }
            ManagerT.GetProperty("InitialShipCount").SetValue(manager, (int)n);
        }

        static void CreateShip()
        {
            object ship = Activator.CreateInstance(ShipT);
            sem.WaitOne();
            int offset = (int)ManagerT.InvokeMember("Draw", System.Reflection.BindingFlags.InvokeMethod, null, manager, new object[] { "ship" });
            sem.Release();
            ShipT.GetProperty("Offset").SetValue(ship, offset);
            ShipT.GetProperty("ShipSpeed").SetValue(ship, ShipSpeed);
            ShipT.InvokeMember("Action", System.Reflection.BindingFlags.InvokeMethod, null, ship, new object[] { Console.BufferWidth - 5 });
        }
    }
}
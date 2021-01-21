using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.Timers;

namespace SImanager
{
    [Guid("609CBC76-38A9-4DD1-A006-996167323C23")]
    public interface IManager
    {
        int Draw(string name);
        void CreatePatron();

        void CheckHit();
        bool CheckCartHit();
        void DestroyObject(int offset);
    }
    [Guid("21BA2D84-6D45-47F3-9ED8-F76F4CA6A93A")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Manager: IManager
    {
        private static readonly int length = 1024;
        private int used = 0;
        private short prevShipPosition = -10;
        private short prevShipY = 0;
        private static int entitySize = Marshal.SizeOf(typeof(Entity));
        private static MemoryMappedFile mmFile = MemoryMappedFile.CreateFromFile("test.mmf", System.IO.FileMode.Create, @"Global\SImmf", length);
        private static MemoryMappedViewAccessor acc = mmFile.CreateViewAccessor(0, length, MemoryMappedFileAccess.Read);
        private static Type BombT = Type.GetTypeFromProgID("SIbomb.Bomb");
        private static readonly Type ManagerT = Type.GetTypeFromProgID("SImanager.Manager");
        private static readonly Type ShipT = Type.GetTypeFromProgID("SIship.Ship");
        private static readonly Type CartT = Type.GetTypeFromProgID("SIcart.Cart");
        private static readonly Type PatronT = Type.GetTypeFromProgID("SIpatron.Patron");
        private static Dictionary<int, dynamic> objects = new Dictionary<int, dynamic>();
        private delegate int HPEvent(short x);
        private event HPEvent HPChanged;
        public int ShipCount { get; set; }
        private static Random shiprandom = new Random((int)DateTime.Now.Ticks);

        private short hp;
        private short CartHP
        {
            get { return hp; }
            set { HPChanged(value); hp = value; }
        }
        [DllImport("SIConsoleAPI.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "ShowHP")]
        internal static extern int ShowHP(short x);
        public Manager()
        {
            System.Timers.Timer tb = new System.Timers.Timer(5000);
            tb.Elapsed += CreateBomb;
            tb.AutoReset = true;
            tb.Enabled = true;
            HPChanged += ShowHP;
            CartHP = 3;
            Mutex mutex = new Mutex(false, @"Global\SImutex");
            entitySize = Marshal.SizeOf(typeof(Entity));
        }
        public static short XglobalCart;
        public static short YglobalCart;
        public static short[] globalShip = new short[2];

        public int Draw(string name)
        {
            Entity entity;
            if (name.Contains("cart"))
            {
                entity = new Entity()
                {
                    X = (short)(Console.WindowWidth / 2),
                    Y = (short)(Console.WindowHeight - 5),
                    TypeA = Marshal.StringToHGlobalAnsi(name)
                };
            }
            else if (name.Contains("ship"))
            {
                if (prevShipPosition < Console.WindowWidth) prevShipPosition += 10;
                else { prevShipPosition = -10; prevShipY += 3; }
                entity = new Entity()
                {
                    X = prevShipPosition,
                    Y = prevShipY,
                    TypeA = Marshal.StringToHGlobalAnsi(name)
                };
            }
            else if (name.Contains("patron"))
            {
                entity = new Entity()
                {
                    X = XglobalCart,
                    Y = YglobalCart,
                    TypeA = Marshal.StringToHGlobalAnsi(name)
                };
            }
            else if (name.Contains("bomb"))
            {
                entity = new Entity()
                {
                    X = globalShip[0],
                    Y = globalShip[1],
                    TypeA = Marshal.StringToHGlobalAnsi(name)
                };
            }
            else return -1;

            using (var mmFile = MemoryMappedFile.OpenExisting(
    @"Global\SImmf", MemoryMappedFileRights.Write))
            {
                Mutex mutex = Mutex.OpenExisting(@"Global\SImutex");
                mutex.WaitOne();
                using (var acc = mmFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite))
                {

                    Entity entity1;
                    int i;
                    for (i = 0; i < used; i += entitySize)
                    {
                        acc.Read(i, out entity1);
                        if (Marshal.PtrToStringAnsi(entity1.TypeA).Contains("del"))
                        {
                            acc.Write(i, ref entity);
                            mutex.ReleaseMutex();
                            return i;
                        }
                    }
                    if (used + entitySize < length)
                    {
                        acc.Write(used, ref entity);
                        used += entitySize;
                    }
                }
                mutex.ReleaseMutex();
            }
            return used - entitySize; //вернуть смещение отображенного объекта
        }


        public void DestroyObject(int offset)
        {
            using (var mmFile = MemoryMappedFile.OpenExisting(
    @"Global\SImmf", MemoryMappedFileRights.Write))
            {
                Mutex mutex = Mutex.OpenExisting(@"Global\SImutex");
                mutex.WaitOne();
                using (var acc = mmFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite))
                {
                    Entity entity;
                    acc.Read(offset, out entity);
                    entity.TypeA = Marshal.StringToHGlobalAnsi("del");
                    acc.Write(offset, ref entity);
                }
                mutex.ReleaseMutex();
            }
        }

        private void CreateBomb(Object source, ElapsedEventArgs e)
        {
            Entity entity;
            int curship = shiprandom.Next(0, ShipCount);
            int count = 0;
            for (int i = 0; i < used; i += entitySize)
            {
                acc.Read(i, out entity);
                string name = Marshal.PtrToStringAnsi(entity.TypeA);
                if (name.Contains("ship"))
                {
                    if (count == curship)
                    {
                        dynamic bomb = Activator.CreateInstance(BombT);
                        BombT.GetField("Manager").SetValue(bomb, this);

                        globalShip[0] = (short)(entity.X + 3);
                        globalShip[1] = entity.Y;
                        BombT.GetProperty("Offset").SetValue(bomb, Draw("bomb"));
                        BombT.InvokeMember("Action", System.Reflection.BindingFlags.InvokeMethod, null, bomb, null);
                        return;
                    }
                    else count++;
                }
            }
        }

        public void CreatePatron()
        {
            Entity entity;
            for (int i = 0; i<used; i += entitySize)
            {
                acc.Read(i, out entity);
                string name = Marshal.PtrToStringAnsi(entity.TypeA);
                if (name.Contains("cart"))
                {
                    XglobalCart = (short)(entity.X+4);
                    YglobalCart = (short)(entity.Y-2);
                }
            }
        }

        public void CheckHit()
        {
            Entity entity;
            short[] globalPatron = { -1, -1 };
            for (int i = 0; i < used; i += entitySize)
            {
                acc.Read(i, out entity);
                string name = Marshal.PtrToStringAnsi(entity.TypeA);
                if (name.Contains("patron"))
                {
                    if (entity.Y <= 0) { 
                        DestroyObject(i); return; 
                    }
                    globalPatron[0] = entity.X; globalPatron[1] = entity.Y;
                }
            }
            for (int i = 0; i < used; i += entitySize)
            {
                acc.Read(i, out entity);
                string name = Marshal.PtrToStringAnsi(entity.TypeA);
                if (name.Contains("ship"))
                {
                    if (globalPatron[0] >= entity.X && globalPatron[0] <= (short)(entity.X + 6) &&
                        globalPatron[1] >= entity.Y && globalPatron[1] <= (short)(entity.Y + 2))
                    {
                        DestroyObject(i);
                        ShipCount--;
                        return;
                    }
                }
            }
        }

        public bool CheckCartHit()
        {
            Entity entity;
            short[] globalBomb = { -1, -1 };
            for (int i = 0; i < used; i += entitySize)
            {
                acc.Read(i, out entity);
                string name = Marshal.PtrToStringAnsi(entity.TypeA);
                if (name.Contains("bomb"))
                {
                    if (entity.Y > Console.WindowHeight)
                    {
                        DestroyObject(i); return false;
                    }
                    globalBomb[0] = entity.X; globalBomb[1] = entity.Y;
                }
            }
            for (int i = 0; i < used; i += entitySize)
            {
                acc.Read(i, out entity);
                string name = Marshal.PtrToStringAnsi(entity.TypeA);
                if (name.Contains("cart"))
                {
                    if (globalBomb[0] >= entity.X && globalBomb[0] <= (short)(entity.X + 9) &&
                        globalBomb[1] >= entity.Y && globalBomb[1] <= (short)(entity.Y +3))
                    {
                        CartHP -= 1;
                        return true;
                    }
                }
            }
            return false;
        }

    }
}

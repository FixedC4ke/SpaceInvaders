using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Timers;

namespace SIcart
{

    [Guid("25D9880C-A39F-4BDD-BA0D-2D83BBB35346")]
    public interface ICart
    {
        void Move(short step);
        bool Shoot();
    }

    [Guid("41281675-0E7A-4A60-AFEB-F76E98C633C1")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Cart : ICart
    {
        [DllImport("SIConsoleAPI.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "DrawCart")]
        internal static extern int DrawCart(short x, short y, bool clean);
        public int Offset { get; set; }
        private static Mutex mutex;
        private bool readytoshoot = true;
        private static System.Timers.Timer shottimer = new System.Timers.Timer();
        public Cart()
        {
            mutex = Mutex.OpenExisting(@"Global\SImutex");
            shottimer.Elapsed += SetReady;
            shottimer.Interval = 2000;
            shottimer.AutoReset = false;
        }
        public void Move(short step) //изменение координат для тачанки
        {
            using (var mmFile = MemoryMappedFile.OpenExisting(
                @"Global\SImmf", MemoryMappedFileRights.ReadWrite))
            {
                mutex.WaitOne();
                using (var acc = mmFile.CreateViewAccessor(0, 1024, MemoryMappedFileAccess.ReadWrite))
                {
                    Entity entity;
                    acc.Read(Offset, out entity);
                    short[] prevpos = { entity.X, entity.Y };
                    entity.X += step;
                    acc.Write(Offset, ref entity);
                    DrawCart(prevpos[0], prevpos[1], true);
                    DrawCart(entity.X, entity.Y, false);
                }
                mutex.ReleaseMutex();
            }
        }
        public bool Shoot()
        {
            if (readytoshoot)
            {
                readytoshoot = false;
                shottimer.Start();
                return true;
            }
            return false;
        }
        private void SetReady(Object source, ElapsedEventArgs e)
        {
            readytoshoot = true;
        }
    }
}

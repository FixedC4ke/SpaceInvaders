using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Diagnostics;
using System.Timers;

namespace SIship
{
    [Guid("7042061C-5F90-4781-8E5B-C79750192636")]
    public interface IShip
    {
        void Action(int maxX);
    }

    [Guid("0BC8F8F4-E572-4632-8EE5-A21B80499E13")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Ship: IShip
    {
        public int Offset { get; set; }

        private bool movesToRight = true;
        private int maxX;
        private int entitySize = Marshal.SizeOf(typeof(Entity));
        private short step = 2;
        private static Mutex mutex;
        private static MemoryMappedFile mmFile;

        public Ship()
        {
            mutex = Mutex.OpenExisting(@"Global\SImutex");
            mmFile = MemoryMappedFile.OpenExisting(
                @"Global\SImmf", MemoryMappedFileRights.ReadWrite);
        }
        public void Move(Object x, ElapsedEventArgs e)
        {
                mutex.WaitOne();
                using (var acc = mmFile.CreateViewAccessor(Offset, entitySize, MemoryMappedFileAccess.ReadWrite))
                {
                    Entity entity;
                    acc.Read(0, out entity);
                    if (entity.X < 0 || entity.X > maxX) { movesToRight = !movesToRight; entity.Y += 3; }
                    if (movesToRight) entity.X += step;
                    else entity.X -= step;
                    acc.Write(0, ref entity);
                }
                mutex.ReleaseMutex();
        }
        public void Action(int maxX)
        {
            this.maxX = maxX;
            System.Timers.Timer t = new System.Timers.Timer(500);
            t.Elapsed += Move;
            t.AutoReset = true;
            t.Enabled = true;
        }
    }
}

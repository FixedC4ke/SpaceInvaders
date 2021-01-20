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

namespace SIbomb
{
    [Guid("79437C01-E9A7-44FD-BE5F-42510B4B4701")]
    public interface SIbomb
    {
        void Move(Object x, ElapsedEventArgs e);
        void Action();
    }

    [Guid("2EA11A3B-1277-4D69-88A8-2A6CEAC762BA")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Bomb : SIbomb
    {
        public int Offset { get; set; }
        private static Mutex mutex;

        public Bomb()
        {
            mutex = Mutex.OpenExisting(@"Global\SImutex");
        }

        public void Move(Object x, ElapsedEventArgs e)
        {
            using (var mmFile = MemoryMappedFile.OpenExisting(
              @"Global\SImmf", MemoryMappedFileRights.ReadWrite))
            {
                mutex.WaitOne();
                using (var acc = mmFile.CreateViewAccessor(0, 1024, MemoryMappedFileAccess.ReadWrite))
                {
                    Entity entity;
                    acc.Read(Offset, out entity);
                    entity.Y += 2;
                    acc.Write(Offset, ref entity);
                }
                mutex.ReleaseMutex();
            }
        }



        public void Action()
        {
            System.Timers.Timer t = new System.Timers.Timer(50);
            t.Elapsed += Move;
            t.AutoReset = true;
            t.Enabled = true;
        }
    }
}

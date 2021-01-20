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

namespace SIpatron
{
    [Guid("DF36483D-27C9-49E9-8F66-4D7D281D0572")]
    public interface SIpatron
    {
        void Move(Object x, ElapsedEventArgs e);
        void Action();
        void Stop();
    }

    [Guid("EDAA681F-C543-469C-9F5E-C09962298D76")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Patron: SIpatron
    {
        public int Offset { get; set; }
        private static Mutex mutex;
        private System.Timers.Timer t = new System.Timers.Timer(50);


        public Patron()
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
                    if (Marshal.PtrToStringAnsi(entity.TypeA).Contains("del"))
                    {
                        Stop();
                    }
                    else
                    {
                        entity.Y -= 2;
                        acc.Write(Offset, ref entity);
                    }
                }
                mutex.ReleaseMutex();
            }
        }



        public void Action()
        {
            t.Elapsed += Move;
            t.AutoReset = true;
            t.Enabled = true;
        }
        public void Stop()
        {
            t.Enabled = false;
        }
    }
}

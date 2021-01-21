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
        void Stop();
    }

    [Guid("2EA11A3B-1277-4D69-88A8-2A6CEAC762BA")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Bomb : SIbomb
    {
        public int Offset { get; set; }
        private static Mutex mutex;
        private System.Timers.Timer t = new System.Timers.Timer(50);
        [DllImport("SIConsoleAPI.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "DrawBomb")]
        internal static extern int DrawBomb(short x, short y, bool clean);

        public static object Manager;
        private static readonly Type ManagerT = Type.GetTypeFromProgID("SImanager.Manager");


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
                    if (Marshal.PtrToStringAnsi(entity.TypeA).Contains("del"))
                    {

                        Stop();
                        DrawBomb(entity.X, entity.Y, true);
                    }
                    else
                    {
                        short[] prevpos = { entity.X, entity.Y };
                        entity.Y += 2;
                        acc.Write(Offset, ref entity);
                        DrawBomb(prevpos[0], prevpos[1], true);
                        DrawBomb(entity.X, entity.Y, false);
                        bool hit = (bool)ManagerT.InvokeMember("CheckCartHit", System.Reflection.BindingFlags.InvokeMethod, null, Manager, null);
                        if (hit) ManagerT.InvokeMember("DestroyObject", System.Reflection.BindingFlags.InvokeMethod, null, Manager, new object[] { Offset });
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

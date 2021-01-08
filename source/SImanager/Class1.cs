using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO.MemoryMappedFiles;

namespace SImanager
{
    [Guid("609CBC76-38A9-4DD1-A006-996167323C23")]
    public interface IManager
    {
        int Draw();
    }
    [Guid("21BA2D84-6D45-47F3-9ED8-F76F4CA6A93A")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Manager
    {
        private readonly Console2 _console2;
        private readonly int length = 1024;
        private int used = 0;
        public int entitySize;
        public Manager()
        {
            _console2 = new Console2(110, 41, ConsoleColor.Black);
            Timer t = new Timer(Update, null, 0, 100);
            var mmFile = MemoryMappedFile.CreateNew(@"Global\SImmf", length);
            Mutex mutex = new Mutex(false, @"Global\SImutex");
            entitySize = Marshal.SizeOf(typeof(Entity));

        }
        public void Update(Object x)
        {
            _console2.Clear();

            using (var mmFile = MemoryMappedFile.OpenExisting(
                @"Global\SImmf", MemoryMappedFileRights.Read))
            {
                using (var acc = mmFile.CreateViewAccessor(0,length, MemoryMappedFileAccess.Read))
                {
                    Mutex mutex = Mutex.OpenExisting(@"Global\SImutex");
                    mutex.WaitOne();
                    for (int i = 0; i<used; i += entitySize)
                    {
                        Entity entity;
                        acc.Read(i, out entity);
                        ConsoleArea na = new ConsoleArea(10, 5);
                        na.SetDefaultBackground(ConsoleColor.White);
                        na.Write(Marshal.PtrToStringAnsi(entity.TypeA), 0, 0);
                        _console2.DrawArea(na, entity.X, entity.Y);
                    }
                    mutex.ReleaseMutex();
                }
            }
        }
        public int Draw()
        {
            ConsoleArea na = new ConsoleArea(10, 5);
            na.SetDefaultBackground(ConsoleColor.White);
            Entity cart = new Entity() { X = (short)(_console2.Width / 2), Y = (short)(_console2.Height - na.Height)
            ,TypeA=Marshal.StringToHGlobalAnsi("cart")};

            using (var mmFile = MemoryMappedFile.OpenExisting(
    @"Global\SImmf", MemoryMappedFileRights.Write))
            {
                using (var acc = mmFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Write))
                {
                    Mutex mutex = Mutex.OpenExisting(@"Global\SImutex");
                    mutex.WaitOne();
                    if (used + entitySize < length)
                    {
                        acc.Write(used, ref cart);
                        used += entitySize;
                    }
                    mutex.ReleaseMutex();
                }
            }
            return used-entitySize; //вернуть смещение отображенного объекта
        }
    }
    
}

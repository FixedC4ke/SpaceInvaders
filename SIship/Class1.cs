using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace SIship
{
    [Guid("7042061C-5F90-4781-8E5B-C79750192636")]
    public interface IShip
    {
        void Move(Object x);
    }

    [Guid("0BC8F8F4-E572-4632-8EE5-A21B80499E13")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Ship: IShip
    {
        public int Offset { get; set; }
        public Ship()
        {
            Timer t = new Timer(Move, null, 0, 500);
        }
        public void Move(Object x)
        {
            short step = 2;
            using (var mmFile = MemoryMappedFile.OpenExisting(
                @"Global\SImmf", MemoryMappedFileRights.ReadWrite))
            {
                using (var acc = mmFile.CreateViewAccessor(0, 1024, MemoryMappedFileAccess.ReadWrite))
                {
                    Mutex mutex = Mutex.OpenExisting(@"Global\SImutex");
                    mutex.WaitOne();
                    Entity entity;
                    acc.Read(Offset, out entity);
                    entity.X += step;
                    acc.Write(Offset, ref entity);
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}

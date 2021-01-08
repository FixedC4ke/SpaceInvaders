using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace SIcart
{
    [Guid("25D9880C-A39F-4BDD-BA0D-2D83BBB35346")]
    public interface ICart
    {
        void Move(int step);
    }

    [Guid("41281675-0E7A-4A60-AFEB-F76E98C633C1")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Cart: ICart
    {
        public int position;
        private object manager;
        public Cart()
        {
            using (var mmFile = MemoryMappedFile.OpenExisting(
                @"Global\SImmf", MemoryMappedFileRights.Write))
            {
                using (var acc = mmFile.CreateViewAccessor(0,0, MemoryMappedFileAccess.Write))
                {
                    int[] data = new int[1] { 10 };

                    Mutex mutex = Mutex.OpenExisting(@"Global\SImutex");
                    mutex.WaitOne();
                    acc.WriteArray(0, data, 0, data.Length);
                    mutex.ReleaseMutex();
                }
            }
        }
        public void Move(int step)
        {
            using (var mmFile = MemoryMappedFile.OpenExisting(
                @"Global\SImmf", MemoryMappedFileRights.ReadWrite))
            {
                using (var acc = mmFile.CreateViewAccessor(0,0, MemoryMappedFileAccess.ReadWrite))
                {
                    int[] data = new int[1];
                    Mutex mutex = Mutex.OpenExisting(@"Global\SImutex");
                    mutex.WaitOne();
                    acc.ReadArray(0, data, 0, data.Length);
                    data[0] += step;
                    acc.WriteArray(0, data, 0, data.Length);
                    mutex.ReleaseMutex();
                }
            }
        }
        public void MoveToRight()
        {

        }
    }
}

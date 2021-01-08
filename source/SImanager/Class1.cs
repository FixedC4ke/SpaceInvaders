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
        void Draw();
        void Update(Object x);
    }
    [Guid("21BA2D84-6D45-47F3-9ED8-F76F4CA6A93A")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Manager
    {
        private readonly Console2 _console2;
        private List<ConsoleArea> areas;
        public Manager()
        {
            _console2 = new Console2(110, 41, ConsoleColor.Black);
            Timer t = new Timer(Update, null, 0, 100);
            areas = new List<ConsoleArea>();
        }
        public void Update(Object x)
        {
            int pos;
            using (var mmFile = MemoryMappedFile.OpenExisting(
                @"Global\SImmf", MemoryMappedFileRights.Read))
            {
                using (var acc = mmFile.CreateViewAccessor(0,0, MemoryMappedFileAccess.Read))
                {
                    int[] data = new int[1];
                    Mutex mutex = Mutex.OpenExisting(@"Global\SImutex");
                    mutex.WaitOne();
                    acc.ReadArray(0, data, 0, data.Length);
                    mutex.ReleaseMutex();
                    pos = data[0];
                }
            }
            _console2.Clear();
            foreach (ConsoleArea area in areas)
            {
                _console2.DrawArea(area, (short)pos, 20);
            }
        }
        public void Draw()
        {
            ConsoleArea na = new ConsoleArea(10, 5);
            na.SetDefaultBackground(ConsoleColor.White);
            areas.Add(na); 
        }
    }
    
}

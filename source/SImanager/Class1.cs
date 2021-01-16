﻿using System;
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
    }
    [Guid("21BA2D84-6D45-47F3-9ED8-F76F4CA6A93A")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Manager
    {
        private readonly Console2 _console2;
        private static readonly int length = 1024;
        private int used = 0;
        private short prevShipPosition = -10;
        private short prevShipY = 0;
        private static int entitySize = Marshal.SizeOf(typeof(Entity));
        private static MemoryMappedFile mmFile = MemoryMappedFile.CreateFromFile("test.mmf", System.IO.FileMode.Create, @"Global\SImmf", length);
        private static MemoryMappedViewAccessor acc = mmFile.CreateViewAccessor(0, length, MemoryMappedFileAccess.Read);
        public Manager()
        {
            _console2 = new Console2(110, 41, ConsoleColor.Black);
            System.Timers.Timer t = new System.Timers.Timer(10);
            t.Elapsed += Update;
            t.AutoReset = true;
            t.Enabled = true;
            Mutex mutex = new Mutex(false, @"Global\SImutex");
            entitySize = Marshal.SizeOf(typeof(Entity));

        }
        public void Update(Object source, ElapsedEventArgs e)
        {
            _console2.Clear();
                    for (int i = 0; i<used; i += entitySize)
                    {
                        Entity entity;
                        acc.Read(i, out entity);
                        ConsoleArea na;
                        string name = Marshal.PtrToStringAnsi(entity.TypeA);

                        if (name.Contains("cart"))
                        {
                            na = new ConsoleArea(10, 5);
                            na.SetDefaultBackground(ConsoleColor.Green);
                        }
                        else if (name.Contains("ship"))
                        {
                            na = new ConsoleArea(6, 2);
                            na.SetDefaultBackground(ConsoleColor.White);
                        }
                        else return;

                        na.Write(name, 0, 0);
                        _console2.DrawArea(na, entity.X, entity.Y);
                    }
        }
        public int Draw(string name)
        {
            Entity entity;
            if (name.Contains("cart"))
            {
                entity = new Entity()
                {
                    X = (short)(_console2.Width / 2),
                    Y = (short)(_console2.Height - 5),
                    TypeA = Marshal.StringToHGlobalAnsi(name)
                };
            }
            else if (name.Contains("ship"))
            {
                if (prevShipPosition<_console2.Width) prevShipPosition += 10;
                else { prevShipPosition = -10; prevShipY += 3; }
                entity = new Entity()
                {
                    X = prevShipPosition,
                    Y = prevShipY,
                    TypeA = Marshal.StringToHGlobalAnsi(name)
                };
            }
            else return -1;

            using (var mmFile = MemoryMappedFile.OpenExisting(
    @"Global\SImmf", MemoryMappedFileRights.Write))
            {
                Mutex mutex = Mutex.OpenExisting(@"Global\SImutex");
                mutex.WaitOne();
                using (var acc = mmFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Write))
                {
                    if (used + entitySize < length)
                    {
                        acc.Write(used, ref entity);
                        used += entitySize;
                    }
                }
                mutex.ReleaseMutex();
            }
            return used-entitySize; //вернуть смещение отображенного объекта
        }
    }
    
}

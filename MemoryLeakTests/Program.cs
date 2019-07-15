using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace MemoryLeakTests
{
    class Program
    {
        public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumThreadWindows(int threadId, EnumWindowProc callback, IntPtr lParam);
        static void Main(string[] args)
        {
            RunWithoutGCAlloc(); // This will leaks
            RunWithGCAlloc(); // This will not leak
        }

        private static void RunWithoutGCAlloc()
        {
            var a = true;
            while (a)
            {
                foreach (var process in Process.GetProcesses())
                {
                    foreach (ProcessThread thread in process.Threads)
                    {
                        var list = new List<IntPtr>();
                        EnumThreadWindows(thread.Id, (wnd, param) =>
                        {
                            list.Add(wnd);
                            return true;
                        }, IntPtr.Zero);
                        thread.Dispose();
                    }

                    process.Dispose();
                }

                Thread.Sleep(1);
            }
        }

        private static void RunWithGCAlloc()
        {
            var a = true;
            while (a)
            {
                foreach (var process in Process.GetProcesses())
                {
                    foreach (ProcessThread thread in process.Threads)
                    {
                        var list = new List<IntPtr>();
                        var alloc = GCHandle.Alloc(list);
                        var lParam = GCHandle.ToIntPtr(alloc);
                        EnumThreadWindows(thread.Id, (wnd, param) =>
                        {
                            var list1 = GCHandle.FromIntPtr(param).Target as List<IntPtr>;
                            list1?.Add(wnd);
                            return true;
                        }, lParam);
                        thread.Dispose();
                        alloc.Free();
                    }

                    process.Dispose();
                }

                Thread.Sleep(1);
            }
        }
    }
}
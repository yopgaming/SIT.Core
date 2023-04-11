using SIT.Tarkov.Core;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace SIT.Core.Misc
{
    public static class GCHelpers
    {
        [DllImport("psapi.dll", EntryPoint = "EmptyWorkingSet")]
        private static extern bool EmptyWorkingSetCall(IntPtr hProcess);

        public static void EmptyWorkingSet()
        {
            EmptyWorkingSetCall(Process.GetCurrentProcess().Handle);
        }

        public static bool Emptying = false;

        public static void EnableGC()
        {
            if (GarbageCollector.GCMode == GarbageCollector.Mode.Disabled)
            {
                PatchConstants.Logger.LogDebug($"EnableGC():Enabled GC");
                //GarbageCollector.CollectIncremental(1000000);
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            }
        }

        public static void DisableGC()
        {

            if (GarbageCollector.GCMode == GarbageCollector.Mode.Enabled)
            {
                Collect(true);
                PatchConstants.Logger.LogDebug($"DisableGC():Disabled GC");
                GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
            }
        }

        public static void ClearGarbage(bool emptyTheSet = false)
        {
            PatchConstants.Logger.LogDebug($"ClearGarbage()");
            EnableGC();
            Collect(force: true);
            if (Emptying)
                return;

            Resources.UnloadUnusedAssets();
            if (emptyTheSet)
            {
                Emptying = true;
                RunHeapPreAllocation();
                Collect(force: true);
                EmptyWorkingSet();
            }
            Emptying = false;
            DisableGC();
        }

        public static void RunHeapPreAllocation()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int num = Math.Max(0, 128);
            UnityEngine.Debug.Log(num + " MBs");
            if (num > 0)
            {
                object[] array = new object[1024 * num];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new byte[1024];
                }
                array = null;
                stopwatch.Stop();
                PatchConstants.Logger.LogDebug($"Heap pre-allocation for {num} mBs took {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        public static void Collect(bool force = false)
        {
            Collect(2, GCCollectionMode.Optimized, isBlocking: true, compacting: false, force);
        }

        public static void Collect(int generation, GCCollectionMode gcMode, bool isBlocking, bool compacting, bool force)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(generation, gcMode, isBlocking, compacting);
        }
    }
}

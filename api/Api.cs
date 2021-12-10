using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace webHttpTest.api
{
    public class WorkThreadState
    {
        public CancellationTokenSource cts { get; set; }
        public int cpu { get; set; }
    }

    public class Api
    {
        public static void Work(int? duration, int? cpu)
        {
            cpu = cpu.HasValue ? cpu.Value : 0;
            duration = duration.HasValue ? duration.Value : 0;

            Debug.WriteLine($"Generating {cpu.Value} cpu for {duration.Value} milliseconds");

            // cpu
            var threads = new List<WorkThreadState>();

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                var threadState = new WorkThreadState { cts = new CancellationTokenSource(), cpu = cpu.Value }; 

                ThreadPool.QueueUserWorkItem(new WaitCallback(CPUKill), threadState);

                threads.Add(threadState);
            }

            Thread.Sleep(duration.Value);

            foreach (var t in threads)
            {
                t.cts.Cancel();
            }
        }

        static void CPUKill(object state)
        {
            var threadState = (WorkThreadState)state;

            Parallel.For(0, 1, new Action<int>((int i) =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                while (!threadState.cts.IsCancellationRequested)
                {
                    if (watch.ElapsedMilliseconds > threadState.cpu)
                    {
                        Thread.Sleep(100 - threadState.cpu);
                        watch.Reset();
                        watch.Start();
                    }
                }
            }));
        }
    }
}

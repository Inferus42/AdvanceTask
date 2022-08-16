using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvanceTask
{
    public static class Task1_2
    {
        public static void main()
        {
            TP();
            Task1();
        }

        public static void TP()
        {
            var handleA = new ManualResetEvent(false);
            var handleB = new ManualResetEvent(false);
            var handleB1 = new ManualResetEvent(false);
            var handleB2 = new ManualResetEvent(false);

            // worker A
            ThreadPool.QueueUserWorkItem(delegate
            {
                Go(1000, "заказчик А");
                handleB.Set();
            });

            // worker B
            ThreadPool.QueueUserWorkItem(delegate
            {
                Go(1500, "заказчик Б");
                handleA.Set();
            });

            //storage 1
            ThreadPool.QueueUserWorkItem(delegate
            {
                handleB.WaitOne();
                Go(500, "склад 1");
                handleB1.Set();
            });

            //storage 2
            ThreadPool.QueueUserWorkItem(delegate
            {
                handleB.WaitOne();
                Go(600, "склад 2");
                handleB2.Set();
            });

            WaitHandle.WaitAll(new WaitHandle[] { handleA, handleB1, handleB2 });

            Console.WriteLine("ThreadPool done");
        }

        public static void Task1()
        {
            Task taskA = Task.Run(() =>
            {
                Go(1000, "заказчик А");
            });

            Task taskB = Task.Run(() =>
            {
                Go(1500, "заказчик Б");
            });

            Task taskB1 = taskB.ContinueWith(t => Go(500, "склад 1"));
            Task taskB2 = taskB.ContinueWith(t => Go(600, "склад 2"));

            taskA.Wait();
            taskB.Wait();
            taskB1.Wait();
            taskB2.Wait();
            Console.WriteLine("Task done");
        }

        static void Go(int i, string s)
        {
            Console.WriteLine($"start {s}");
            Delay(i);
            Console.WriteLine($"done {s}");
        }

        static Task<DateTimeOffset> Delay(int millisecondsTimeout)
        {
            TaskCompletionSource<DateTimeOffset> tcs = null;
            Timer timer = null;

            timer = new Timer(delegate
            {
                timer.Dispose();
                tcs.TrySetResult(DateTimeOffset.UtcNow);
            }, null, Timeout.Infinite, Timeout.Infinite);

            tcs = new TaskCompletionSource<DateTimeOffset>(timer);
            timer.Change(millisecondsTimeout, Timeout.Infinite);
            return tcs.Task;
        }
    }
}

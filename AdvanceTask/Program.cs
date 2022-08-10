using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AdvanceTask
{
    class Program
    {

        static void Main()
        {
            Task1(); // and task 2  method Delay

            //TaskLoopMethod(); //task 3

            SynchContext(); //task 4

            Console.WriteLine($"done Main");
        }


        static void Task1()
        {
            Task taskA = new Task(() =>
            {
                Go(1000, "заказчик А");
            });

            Task taskB = new Task(() =>
            {
                Go(1500, "заказчик Б");
            });

            Task taskB1 = taskB.ContinueWith(t => Go(500, "склад 1"));
            Task taskB2 = taskB.ContinueWith(t => Go(600, "склад 2"));

            taskA.Start();
            taskB.Start();

            taskA.Wait();
            taskB.Wait();
            taskB1.Wait();
            taskB2.Wait();
            Console.WriteLine("done");

            static async void Go(int i, string s)
            {
                Console.WriteLine($"start {s}");
                await Delay(i);
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


        public class TaskLoop
        {
            public Action A { get; set; }
            public int Max { get; set; }
            int _state { get; set; }
            public Task Task { get; set; }

            private async Task Work()
            {
                await Task.Delay(100);
            }

            public void Run()
            {
                if (_state == 0)
                {

                    Task Task = new Task(() => { Work(); });

                    _state = 0;
                    Task.ContinueWith(_ => Run());
                }
                else if (_state == Max)
                {
                    return;
                }
            }

        }

        public static void TaskLoopMethod()
        {
            var taskLoop = new TaskLoop
            {
                A = () => Console.WriteLine($"After delay {Thread.CurrentThread.ManagedThreadId}"),
                Max = 5,
            };

            Console.WriteLine($"Hello world {Thread.CurrentThread.ManagedThreadId}");
            taskLoop.Run();
            taskLoop.Task.Wait();
        }



        static void SynchContext()
        {
            SingleThreadSynchronizationContext.Run(async delegate
            {
                await TestAsync();

            });
        }

        static async Task TestAsync()
        {
            var d = new Dictionary<int, int>();
            for (int i = 0; i < 100; i++)
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                int count;
                d[id] = d.TryGetValue(id, out count) ? count + 1 : 1;
                await Task.Yield();
            }
            foreach (var pair in d) Console.WriteLine(pair);
        }
    }

    public class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>>
          m_queue = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        public override void Post(SendOrPostCallback d, object state)
        {
            m_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
        }

        public void RunOnCurrentThread()
        {
            KeyValuePair<SendOrPostCallback, object> workItem;
            while (m_queue.TryTake(out workItem, Timeout.Infinite))
                workItem.Key(workItem.Value);
        }

        public void Complete()
        {
            m_queue.CompleteAdding();
        }

        public static void Run(Func<Task> func)
        {
            var prevCtx = SynchronizationContext.Current;
            try
            {
                var syncCtx = new SingleThreadSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(syncCtx);

                var t = func();
                t.ContinueWith(delegate { syncCtx.Complete(); }, TaskScheduler.Default);

                syncCtx.RunOnCurrentThread();
                t.GetAwaiter().GetResult();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvanceTask
{
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
    public static class TestSingleThreadSynchronizationContext
    {
        public static void SynchContext()
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
            Console.WriteLine("");
            foreach (var pair in d) Console.WriteLine($"TreadId: {pair.Key} TreadWorkCount:  {pair.Value}");
        }
    }
}

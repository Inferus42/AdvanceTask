using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvanceTask
{
    public class TaskLoop
    {
        public Action A { get; set; }
        public int Max { get; set; }
        int _state { get; set; }
        TaskCompletionSource<object> _tack = new TaskCompletionSource<object>();
        public Task Task => _tack.Task;

        private async Task Work()
        {
            await Task.Delay(100);
        }

        public void Run()
        {
            if (_state == Max)
            {

                _tack.SetResult("result");
                return;
            }
            _state++;
            Task localTask = Task.Delay(100);
            localTask = localTask.ContinueWith(_ => A(), TaskContinuationOptions.ExecuteSynchronously);
            localTask.ContinueWith(_ => Run(), TaskContinuationOptions.ExecuteSynchronously);

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
    }
}

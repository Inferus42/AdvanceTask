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
            Task1_2.main(); // and task 2 method Delay
            Console.WriteLine();

            TaskLoop.TaskLoopMethod(); //task 3 
            Console.WriteLine();

            TestSingleThreadSynchronizationContext.SynchContext(); //task 4
            Console.WriteLine();

            Console.WriteLine($"done Main");
        }



    }
}

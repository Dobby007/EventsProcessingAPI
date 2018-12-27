using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RandomDataGenerator
{
    class ObjectAllocator
    {
        private const int MaxFilesNumber = 32;
        private const int MaxReadAttempts = 64;
        private Thread _watcherThread;
        private ConcurrentBag<string> _reversedStrings = new ConcurrentBag<string>();
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsRunning => !(_cancellationTokenSource?.IsCancellationRequested ?? true);

        public void Start()
        {
            if (IsRunning)
                return;

            _cancellationTokenSource = new CancellationTokenSource();
            _watcherThread = new Thread(StartAllocation);
            _watcherThread.IsBackground = true;
            _watcherThread.Start(_cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _watcherThread = null;
            GC.Collect();
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private void StartAllocation(object state)
        {
            CancellationToken token = (CancellationToken)state;
            while (!token.IsCancellationRequested)
            {
                Parallel.For(0, MaxFilesNumber, (index) =>
                {
                    try
                    {
                        using (var writer = new StreamWriter(new FileStream("out" + index + ".txt", FileMode.Create, FileAccess.Write)))
                        {
                            foreach (var number in Enumerable.Range(0, byte.MaxValue))
                            {
                                string finalString = "";
                                foreach (var s in Enumerable.Repeat(number.ToString(), byte.MaxValue))
                                {
                                    finalString += s;
                                }
                                writer.WriteLine(finalString);
                            }
                        }

                        for (var i = 0; i < MaxReadAttempts; i++)
                        {
                            using (var reader = new StreamReader(new FileStream("out" + index + ".txt", FileMode.Open, FileAccess.Read)))
                            {
                                var fileContents = reader.ReadToEnd();
                                _reversedStrings.Add(Reverse(fileContents));
                            }

                            if (token.IsCancellationRequested)
                                return;
                        }

                    }
                    catch (OutOfMemoryException)
                    {
                        Console.WriteLine("Out of memory");
                        GC.Collect();
                    }
                }); 
                _reversedStrings.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private string Reverse(string str)
        {
            char[] chArray = str.ToCharArray();
            string resultStr = null;
            for (int i = chArray.Length; i > 0; i--)
            {
                resultStr += chArray[i - 1].ToString();
            }
            return resultStr;
        }
        
    }
}

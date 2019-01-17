using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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
        private Thread _allocThread;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly AllocationMode _allocationMode;

        public bool IsRunning => !(_cancellationTokenSource?.IsCancellationRequested ?? true) && !_allocationCompleted;
        private volatile bool _allocationCompleted = false;


        public ObjectAllocator(AllocationMode allocationMode)
        {
            _allocationMode = allocationMode;
        }


        public void Start()
        {
            if (IsRunning)
                return;

            _allocationCompleted = false;
            _cancellationTokenSource = new CancellationTokenSource();
            _allocThread = new Thread(StartAllocation);
            _allocThread.IsBackground = true;
            _allocThread.Start(_cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _allocThread = null;
            GC.Collect();
        }

        public void Wait()
        {
            _allocThread.Join();
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private void StartAllocation(object state)
        {
            CancellationToken token = (CancellationToken)state;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    RunAllocationCycle(token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Allocation completed");
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.ToString());
            }
            finally
            {
                _allocationCompleted = true;
            }
        }

        private void RunAllocationCycle(CancellationToken token)
        {
            var reversedStrings = new ConcurrentBag<string>();
            Parallel.For(0, MaxFilesNumber, new ParallelOptions { CancellationToken = token }, (index) =>
            {
                try
                {
                    Stream stream = _allocationMode == AllocationMode.Hard
                        ? new FileStream("out" + index + ".txt", FileMode.Create, FileAccess.Write)
                        : (Stream)new MemoryStream();

                    using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, stream is MemoryStream))
                    {
                        foreach (var number in Enumerable.Range(0, ushort.MaxValue))
                        {
                            string finalString = "";
                            foreach (var s in Enumerable.Repeat(number.ToString(), byte.MaxValue))
                            {
                                finalString += s;
                            }
                            writer.WriteLine(finalString);

                            if (token.IsCancellationRequested)
                                return;
                        }
                    }

                    if (_allocationMode == AllocationMode.Hard)
                        stream = new FileStream("out" + index + ".txt", FileMode.Open, FileAccess.Read);

                    for (var i = 0; i < MaxReadAttempts; i++)
                    {
                        using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
                        {
                            var fileContents = reader.ReadToEnd();
                            reversedStrings.Add(Reverse(fileContents));
                        }

                        if (token.IsCancellationRequested)
                            return;
                    }

                    if (_allocationMode == AllocationMode.Light)
                    {
                        //GC.Collect();
                    }
                    GC.Collect();
                }
                catch (OutOfMemoryException)
                {
                    Debug.WriteLine("Out of memory");
                    GC.Collect();
                }
            });
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

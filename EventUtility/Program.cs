using EventsDomain;
using EventsProcessingAPI;
using EventsProcessingAPI.Density;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace EventUtility
{
    class Program
    {
        private const string LoadingCompletedTemplate = "\nCount of events: {0}, count of buckets: {1}\nEvent range: {2}-{3}μs ({4}ms)";
        private const string ChoiceTemplate = "What do you want to do? Enter 'p' to show payloads, enter 'c' to show charts. \nPlease make your choice:  ";
        
        


        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: dotnet EventUtility.dll {pathToGeneratedFile}");
                Console.WriteLine("Example: dotnet EventUtility.dll ..\\..\\..\\..\\data\\events.bin");
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            try
            {
                Console.Write(ChoiceTemplate);
                bool showPayloads = string.Equals(Console.ReadKey().KeyChar.ToString(), "p", StringComparison.OrdinalIgnoreCase);
                Console.WriteLine();

                LoadEvents(args[0], showPayloads);
            }
            catch (Exception exc)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(exc.ToString());
            }
            finally
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }

        }

        private static void LoadEvents(string filePath, bool showPayloads)
        {
            var eventsApi = new ApiFacade
            {
                ProgressHandler = new ConsoleProgress()
            };

            
            var container = eventsApi.LoadEventsFromFileAsync(filePath, showPayloads ?
                LoadStrategyType.LoadEventsAndPayloads : LoadStrategyType.LoadEventsForChart)
                .GetAwaiter()
                .GetResult();


            var eventsTotalCount = container.Buckets.Sum(b => (long)b.Events.Length);
            Bucket lastBucket = container.GetLastBucket();
            long startTimestamp = container.FirstTimestamp,
                 endTimestamp = lastBucket?.GetAbsoluteTimeForEvent(lastBucket.GetLastEvent()) ?? 0;


            Console.WriteLine(
                LoadingCompletedTemplate,
                eventsTotalCount,
                container.Buckets.Length,
                startTimestamp / 10,
                endTimestamp / 10,
                (endTimestamp - startTimestamp) / 10000
            );

            var interactiveUserMode = new InteractiveUserMode(
                showPayloads ? InteractiveModeType.ShowPayloads : InteractiveModeType.ShowCharts,
                container
            );
            interactiveUserMode.Start();
        }

    }
}

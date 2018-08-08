using System;
using System.Diagnostics;

namespace ImportFootprint
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.MonitoringIsEnabled = true;

            var importer = new Importer();
            Console.WriteLine("Generating the test file");
            importer.GenerateFile();

            Console.WriteLine("Reading the test file");
            importer.Import();

            Console.WriteLine($"Took: {AppDomain.CurrentDomain.MonitoringTotalProcessorTime.TotalMilliseconds:#,###} ms");
            Console.WriteLine($"Allocated: {AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize / 1024:#,#} kb");
            Console.WriteLine($"Peak Working Set: {Process.GetCurrentProcess().PeakWorkingSet64 / 1024:#,#} kb");

            for (int index = 0; index <= GC.MaxGeneration; index++)
            {
                Console.WriteLine($"Gen {index} collections: {GC.CollectionCount(index)}");
            }
        }
    }
}

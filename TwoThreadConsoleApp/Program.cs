using System;
using System.Diagnostics;
using System.Text;

namespace TwoThreadConsoleApp
{
    internal class Program
    {
        static int  QueueSize = 1000;
        static string InputFile = "input.txt";
        static string OutputFile = "output.txt";

        private static void Main(string[] args)
        {
            Console.WriteLine("Press any key to start");
            Console.ReadKey();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // split task in two threads, one reader, one writer
            // output is in sync with input
            var multi = new TwoThread(QueueSize);

            multi.ProcessFileAsync(InputFile, OutputFile, processLine);

            multi.Dispose();
                        
            stopwatch.Stop();

            TimeSpan ts = stopwatch.Elapsed;

            var elapsedTime = string.Format("Runtime {0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            Console.WriteLine(elapsedTime);
            Console.WriteLine("Done. Press any key to exit");
            Console.ReadKey();

        }

        //simple example, return reverse line
        private static string processLine(string arg)
        {
            var sb = new StringBuilder();

            for (int i = arg.Length-1; i >=0 ; i--)
            {
                sb.Append(arg[i]);
            }

            return sb.ToString();
        }
    }
}
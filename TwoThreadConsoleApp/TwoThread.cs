using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace TwoThreadConsoleApp
{
    public class TwoThread : IDisposable
    {
        private int QueueSize = 10000;
        private BlockingCollection<string> _inputQueue;
        private BlockingCollection<string> _outputQueue;

        public TwoThread()
        {
            _inputQueue = new BlockingCollection<string>(QueueSize);
            _outputQueue = new BlockingCollection<string>(QueueSize);
        }

        public TwoThread(int queueSize)
        {
            QueueSize = queueSize;
            _inputQueue = new BlockingCollection<string>(QueueSize);
            _outputQueue = new BlockingCollection<string>(QueueSize);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inputQueue.Dispose();
                _outputQueue.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        internal Task CreateReader(string inputFilename)
        {
            var task = Task.Factory.StartNew(() =>
                ReadInput(inputFilename), TaskCreationOptions.LongRunning);

            return task;
        }

        internal Task CreateWriter(string outputFilename)
        {
            return Task.Factory.StartNew(() =>
                WriteOutput(outputFilename), TaskCreationOptions.LongRunning);
        }

        public void ProcessFileAsync(string inputFilename, string outputFilename,
            Func<string, string> processLine)
        {
            var reader = Task.Factory.StartNew(() =>
                ReadInput(inputFilename), TaskCreationOptions.LongRunning);

            var writer = Task.Factory.StartNew(() =>
                WriteOutput(outputFilename), TaskCreationOptions.LongRunning);

            foreach (var line in _inputQueue.GetConsumingEnumerable())
            {
                var newline = processLine(line);
                _outputQueue.Add(newline);
            }

            _outputQueue.CompleteAdding();
            Task.WaitAll(reader, writer);
        }

        private void ReadInput(string inputFilename)
        {
            foreach (var line in File.ReadLines(inputFilename))
            {
                _inputQueue.Add(line);
            }

            _inputQueue.CompleteAdding();
        }

        private void WriteOutput(string outputFilename)
        {
            using (var outputFile = new StreamWriter(outputFilename))
            {
                foreach (var line in _outputQueue.GetConsumingEnumerable())
                {
                    outputFile.WriteLine(line);
                }
            }
        }
    }
}
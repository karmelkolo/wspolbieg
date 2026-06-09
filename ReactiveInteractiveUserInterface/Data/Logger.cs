using System.Collections.Concurrent;

namespace TP.ConcurrentProgramming.Data
{
    internal class Logger : IDisposable
    {
        private readonly ConcurrentQueue<string> _logsToWrite = new();
        private readonly Task _writeTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private StreamWriter _logFile;

        public Logger()
        {
            _logFile = new StreamWriter($"{DateTime.Now:dd-mm-yyyy_HH-mm-ss}.txt", append: true);
            _writeTask = Task.Run(Logging);
        }

        public void Log(Data.IBall ball)
        {
            string message = $"[{DateTime.Now}] Ball: {ball.GetHashCode()} | Position: {ball.Position.x:F2}; {ball.Position.y:F2}";
            _logsToWrite.Enqueue(message);
        }

        private async Task Logging()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (_logsToWrite.TryDequeue(out string message))
                {
                    await _logFile.WriteLineAsync(message);
                }
                else
                {
                    await Task.Delay(20);
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _writeTask.Dispose();
        }
    }
}

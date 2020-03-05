using System;
using System.Threading;
using System.Threading.Tasks;
using Arrowgene.Logging;

namespace Arrowgene.Services.Tasks
{
    public abstract class PeriodicTask
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        protected readonly ILogger Logger;


        public PeriodicTask()
        {
            Logger = LogProvider.Logger(this);
        }

        public abstract string Name { get; }
        public abstract TimeSpan TimeSpan { get; }

        protected abstract void Execute();
        protected abstract bool RunAtStart { get; }

        public void Start()
        {
            if (_task != null)
            {
                Logger.Error($"Task {Name} already started");
                return;
            }


            _cancellationTokenSource = new CancellationTokenSource();
            _task = new Task(Run, _cancellationTokenSource.Token);
            _task.Start(TaskScheduler.Default);
        }

        public void Stop()
        {
            if (_task == null)
            {
                Logger.Error($"Task {Name} already stopped");
                return;
            }

            _cancellationTokenSource.Cancel();
            _task = null;
        }

        private async void Run()
        {
            Logger.Debug($"Task {Name} started");
            if (RunAtStart)
            {
                Logger.Trace($"Task {Name} run");
                Execute();
                Logger.Trace($"Task {Name} completed");
            }

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Logger.Debug($"Task {Name} canceled");
                }

                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Logger.Trace($"Task {Name} run");
                    Execute();
                    Logger.Trace($"Task {Name} completed");
                }
            }

            Logger.Debug($"Task {Name} ended");
        }
    }
}
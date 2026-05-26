//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {

            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            cancelToken?.Cancel();
            balls.Clear();

            cancelToken = new CancellationTokenSource();

            for (int i = 0; i < numberOfBalls; i++)
            {
                double diameter = 20;
                Vector startingPos = new Vector(random.NextDouble() * (borderWidth - diameter), random.NextDouble() * (borderHeight - diameter));
                Vector startingVel = new Vector(random.NextDouble() * 5, random.NextDouble() * 5);

                if (startingVel.x == 0 && startingVel.y == 0)
                {
                    startingVel = new(1, 1);
                }

                Ball newBall = new Ball(startingPos, startingVel, diameter);

                balls.Add(newBall);

                upperLayerHandler(newBall.Position, newBall);

                Task.Run(() => MoveBall(newBall, cancelToken.Token));
            }
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    cancelToken?.Cancel();
                    cancelToken?.Dispose();
                    cancelToken = null;
                    balls.Clear();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private bool Disposed = false;

        private readonly List<Ball> balls = new();
        private readonly Random random = new();
        private CancellationTokenSource? cancelToken;

        private readonly double borderWidth = 390;
        private readonly double borderHeight = 410;

        private async Task MoveBall(Ball ball, CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {
                ball.Move();
                await Task.Delay(16, cancel).ContinueWith(_ => { });
            }
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(balls);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(balls.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}